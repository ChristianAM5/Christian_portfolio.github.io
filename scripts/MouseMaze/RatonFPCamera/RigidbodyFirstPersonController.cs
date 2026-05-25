using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;
using UnityEngine.UI;

namespace UnityStandardAssets.Characters.FirstPerson
{
    // Hace que Unity obligue a tener un Rigidbody y un CapsuleCollider en el mismo GameObject.
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class RigidbodyFirstPersonController : MonoBehaviourPunCallbacks, IPunObservable
    {
        // Define el input del jugador
        public PlayerInput playerInput;
        public Camera cam;
        public int tnt = 0;
        public TextMeshProUGUI dinamitasText;
        public TextMeshProUGUI spectatorNickname;
        public TextMeshProUGUI dinamitasTextSpectator;
        public TextMeshProUGUI flechasTextSpectator;
        public MovementSettings movementSettings = new MovementSettings();
        public MouseLook mouseLook = new MouseLook();
        public AdvancedSettings advancedSettings = new AdvancedSettings();
        public CrossbowOptions crossbowOptions = new CrossbowOptions();
        public bool isChatOpen = false;
        private GameObject panelEspectador;
        private GameObject panelRaton;
        private ModoEspectador modoEspectador; 
        public bool isTutorialOpen = false; // <--- AÑADE ESTO
        // Comprobar si esta muerto para la salida y camara
        public bool isDead = false;

        // Define la accion de correr (para acceder a la funcion de Run() dentro de la clase MovementSettings)
        private InputAction runAction;

        private Rigidbody m_RigidBody;
        private CapsuleCollider m_Capsule;
        private Vector3 m_GroundContactNormal;
        private bool m_Jump, m_PreviouslyGrounded, m_Jumping, m_IsGrounded;
        private PlayerAnimatorController _animController;

        [Header("Death")]
        public VideoClip videoClip;
        private GameObject panelMuerte;     // Se busca automáticamente en Start
        private RawImage rawImage;
        private VideoPlayer videoPlayer;

        public Vector3 Velocity
        {
            get { return m_RigidBody.velocity; }
        }

        public bool Grounded
        {
            get { return m_IsGrounded; }
        }

        public bool Jumping
        {
            get { return m_Jumping; }
        }

        public bool Running
        {
            get
            {
#if !MOBILE_INPUT
                return movementSettings.Running;
#else
                return false;
#endif
            }
        }

        private void Start()
        {
            modoEspectador = gameObject.GetComponent<ModoEspectador>();

            // Al nacer, el jugador añade su cámara a la lista global
            if (cam != null)
            {
                GameManager_Network.camarasActivas.Add(cam);
            }
            else
            {
                Debug.Log("No meto camara");
            }

            // Parar aquí para los clones, antes de tocar nada de UI o input
            if (PhotonNetwork.IsConnected && !photonView.IsMine)
            {
                if (playerInput != null) Destroy(playerInput);
                if (cam != null) cam.gameObject.SetActive(false);
                if (modoEspectador != null) Destroy(modoEspectador);
                return; // El código se detiene aquí para los clones de otros jugadores
            }

            // IMPORTANTE: Solo el jugador local debe conectarse a la interfaz de la pantalla
            if (!PhotonNetwork.IsConnected || photonView.IsMine)
            {
                GameObject dinamitaTexto = GameObject.Find("contadorDinamitasText");
                GameObject flechaTexto = GameObject.Find("contadorFlechaText");
                GameObject espectadorNickname = GameObject.Find("NombreJugadorText");
                GameObject flechaTextoEspectador = GameObject.Find("contadorEspectadorFlechaText");
                GameObject dinamitaTextoEspectador = GameObject.Find("contadorEspectadorDinamitasText");

                if (dinamitaTexto != null)
                    dinamitasText = dinamitaTexto.GetComponent<TextMeshProUGUI>();
                else
                    Debug.LogWarning("No se encontró el objeto 'contadorDinamitasText' en la escena.");

                if (flechaTexto != null)
                    crossbowOptions.flechasText = flechaTexto.GetComponent<TextMeshProUGUI>();
                else
                    Debug.LogWarning("No se encontró el objeto 'contadorFlechaText' en la escena.");

                if (espectadorNickname != null)
                    spectatorNickname = espectadorNickname.GetComponent<TextMeshProUGUI>();
                else
                    Debug.LogWarning("No se encontró el objeto 'spectatorNicknameText' en la escena.");

                if (flechaTextoEspectador != null)
                    flechasTextSpectator = flechaTextoEspectador.GetComponent<TextMeshProUGUI>();
                else
                    Debug.LogWarning("No se encontró el objeto 'contadorEspectadorFlechaText' en la escena.");

                if (dinamitaTextoEspectador != null)
                    dinamitasTextSpectator = dinamitaTextoEspectador.GetComponent<TextMeshProUGUI>();
                else
                    Debug.LogWarning("No se encontró el objeto 'contadorEspectadorDinamitasText' en la escena.");

                crossbowOptions.arrows = GameManager_Network.BalanceoFlechas();
                crossbowOptions.flechasText.text = crossbowOptions.arrows.ToString();
                dinamitasText.text = tnt + "/" + SpawnManager.totalTNTsEnMapa;

                panelEspectador = GameObject.Find("PanelEspectador");
                if (panelEspectador != null)
                    panelEspectador.SetActive(false);
                else
                    Debug.LogWarning("No encuentro el panel de espectador");

                panelRaton = GameObject.Find("PanelRaton");

                // Inicializar cursor: bloqueado y oculto al empezar
                isChatOpen = false;
                mouseLook.lockCursor = true;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                StartCoroutine(EsperarYActualizarTNTCount());

                panelMuerte = GameObject.Find("SlimeKill");
                if (panelMuerte != null)
                {
                    panelMuerte.SetActive(false);
                    rawImage = panelMuerte.GetComponentInChildren<RawImage>(true);
                }
                else
                    Debug.LogWarning("No se encontró el panel SlimeKill");

                _animController = GetComponent<PlayerAnimatorController>();

            }

            dinamitasText.text = tnt + "/" + SpawnManager.totalTNTsEnMapa;

            PlayerInput playerInputComp = GetComponent<PlayerInput>();
            if (playerInputComp != null)
            {
                playerInputComp.neverAutoSwitchControlSchemes = false;
                var gamepads = UnityEngine.InputSystem.Gamepad.all;

                int playerIndex = PhotonNetwork.IsConnected
                    ? (PhotonNetwork.LocalPlayer.ActorNumber - 1)
                    : 0;

                try
                {
                    if (gamepads.Count > playerIndex)
                    {
                        playerInputComp.user.UnpairDevices();
                        playerInputComp.SwitchCurrentControlScheme("Gamepad", gamepads[playerIndex]);
                        Debug.Log($"Jugador {playerIndex} → Gamepad {gamepads[playerIndex].name}");
                    }
                    else
                    {
                        List<InputDevice> pcDevices = new List<InputDevice>();
                        if (Keyboard.current != null) pcDevices.Add(Keyboard.current);
                        if (Mouse.current != null) pcDevices.Add(Mouse.current);
                        playerInputComp.SwitchCurrentControlScheme("Keyboard Mouse", pcDevices.ToArray());
                        Debug.Log($"Jugador {playerIndex} → Teclado/Ratón");
                    }
                }
                catch (InvalidOperationException e)
                {
                    Debug.LogWarning($"[InputSystem] {e.Message}");
                }
            }

            m_RigidBody = GetComponent<Rigidbody>();
            m_Capsule = GetComponent<CapsuleCollider>();

            playerInput = GetComponent<PlayerInput>();
            runAction = playerInput.actions["Run"];
            crossbowOptions.flechasText.text = crossbowOptions.arrows.ToString();

            runAction.performed += movementSettings.Run;

            mouseLook.Init(transform, cam.transform);
            ActualizarSensibilidad();
            if (PhotonNetwork.IsConnected && photonView.IsMine)
            {
                mouseLook.lockCursor = true;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                isChatOpen = false;
            }
        }

        private void Update()
        {
            if (PhotonNetwork.IsConnected && !photonView.IsMine) return;

            // [!] CORRECCIÓN: Si el juego está pausado o el chat abierto, forzamos el cursor libre.
            // --- AÑADE isTutorialOpen A ESTA CONDICIÓN ---
    if (isChatOpen || PauseMenuManager.GameIsPaused || isDead || isTutorialOpen)
            {
                mouseLook.lockCursor = false;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                return; // Cortamos el Update para que no rote la cámara ni haga el UpdateCursorLock normal
            }

            // Si llegamos aquí, es gameplay normal. Permitimos que MouseLook gestione el cursor y la rotación.
            mouseLook.lockCursor = true;
            mouseLook.UpdateCursorLock();
            RotateView();
        }

        private void FixedUpdate()
        {
            if (PhotonNetwork.IsConnected && !photonView.IsMine) return;
            if (PauseMenuManager.GameIsPaused) return;
            if (isChatOpen) return;

            GroundCheck();
            Vector2 input = GetInput();

            if ((Mathf.Abs(input.x) > float.Epsilon || Mathf.Abs(input.y) > float.Epsilon) && (advancedSettings.airControl || m_IsGrounded))
            {
                Vector3 desiredMove = cam.transform.forward * input.y + cam.transform.right * input.x;
                desiredMove = Vector3.ProjectOnPlane(desiredMove, m_GroundContactNormal).normalized;

                desiredMove.x = desiredMove.x * movementSettings.CurrentTargetSpeed;
                desiredMove.z = desiredMove.z * movementSettings.CurrentTargetSpeed;
                desiredMove.y = desiredMove.y * movementSettings.CurrentTargetSpeed;

                if (m_RigidBody.velocity.sqrMagnitude < (movementSettings.CurrentTargetSpeed * movementSettings.CurrentTargetSpeed))
                {
                    m_RigidBody.AddForce(desiredMove * movementSettings.SlopeMultiplier(m_GroundContactNormal), ForceMode.Impulse);
                }
            }

            if (m_IsGrounded)
            {
                m_RigidBody.drag = 5f;

                if (m_Jump)
                {
                    m_RigidBody.drag = 0f;
                    m_RigidBody.velocity = new Vector3(m_RigidBody.velocity.x, 0f, m_RigidBody.velocity.z);
                    m_RigidBody.AddForce(new Vector3(0f, movementSettings.JumpForce, 0f), ForceMode.Impulse);
                    m_Jumping = true;
                }
            }
            else
            {
                m_RigidBody.drag = 0f;
                if (m_PreviouslyGrounded && !m_Jumping)
                {
                    advancedSettings.StickToGroundHelper(transform, m_Capsule, m_RigidBody);
                }
            }
            m_Jump = false;

            // Si el jugador tiene al menos una flecha, puede disparar 
            // crossbowOptions.ActualizarVisibilidad();
            if (crossbowOptions.arrows > 0 && crossbowOptions.canShoot)
            {
                crossbowOptions.Disparo(m_Capsule, cam);
                crossbowOptions.canShoot = false;
            }
        }

        public void RecogerFlecha()
        {
            crossbowOptions.Recoger();
            if (flechasTextSpectator != null)
                flechasTextSpectator.text = crossbowOptions.arrows.ToString();
        }

        public void RecogerTnt()
        {
            if (tnt < SpawnManager.totalTNTsEnMapa)
            {
                tnt++;
                dinamitasText.text = tnt + "/" + SpawnManager.totalTNTsEnMapa;
            }
        }

        public void SetChatOpen(bool open)
        {
            if (isChatOpen == open) return;
            isChatOpen = open;

            // [!] CORRECCIÓN: Sincronizamos con el MouseLook también
            mouseLook.lockCursor = !open;

            if (isChatOpen)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        public Vector2 GetInput()
        {
            Vector2 input = playerInput.actions["Move"].ReadValue<Vector2>();
            movementSettings.UpdateDesiredTargetSpeed(input);
            return input;
        }

        private void RotateView()
        {
            if (Mathf.Abs(Time.timeScale) < float.Epsilon) return;

            float oldYRotation = transform.eulerAngles.y;
            mouseLook.LookRotation(transform, cam.transform);

            if (m_IsGrounded || advancedSettings.airControl)
            {
                Quaternion velRotation = Quaternion.AngleAxis(transform.eulerAngles.y - oldYRotation, Vector3.up);
                m_RigidBody.velocity = velRotation * m_RigidBody.velocity;
            }
        }

        private void GroundCheck()
        {
            m_PreviouslyGrounded = m_IsGrounded;
            RaycastHit hitInfo;

            // 1. Calculamos el centro real de tu cápsula y la distancia hasta los pies
            Vector3 centroCapsula = m_Capsule.bounds.center;
            float distanciaAlSuelo = m_Capsule.bounds.extents.y;

            // 2. Hacemos la esfera detectora un poco más pequeña que el radio para no raspar paredes
            float radioEsfera = m_Capsule.radius * 0.8f;

            // 3. Calculamos cuánto debe viajar la esfera hacia abajo
            float distanciaViaje = distanciaAlSuelo - radioEsfera + advancedSettings.groundCheckDistance;

            // Seguro anti-bugs: si aplastas mucho la cápsula, evitamos que la distancia sea negativa
            if (distanciaViaje < 0f) distanciaViaje = 0.05f;

            // 4. Lanzamos el detector
            if (Physics.SphereCast(centroCapsula, radioEsfera, Vector3.down, out hitInfo, distanciaViaje, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                m_IsGrounded = true;
                m_GroundContactNormal = hitInfo.normal;
            }
            else
            {
                m_IsGrounded = false;
                m_GroundContactNormal = Vector3.up;
            }

            if (!m_PreviouslyGrounded && m_IsGrounded && m_Jumping)
            {
                m_Jumping = false;
            }
        }

        public void Jump(InputAction.CallbackContext callbackContext)
        {
            if (callbackContext.performed)
            {
                m_Jump = true;
            }
        }

        public void Shoot(InputAction.CallbackContext callbackContext)
        {
            if (callbackContext.performed)
            {
                // No disparar si la ballesta está guardada
                if (_animController != null && !_animController.CrossbowOut)
                    return;

                crossbowOptions.canShoot = true;

                _animController?.OnShoot();
            }
            else
            {
                crossbowOptions.canShoot = false;
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(tnt);
                stream.SendNext(crossbowOptions.arrows);
                stream.SendNext(isDead); // el raton local necesita saber si los demas estan muertos
                stream.SendNext(_animController != null ? _animController.CrossbowOut : false); // Enviar si tienen ballesta o no
            }
            else
            {
                tnt = (int)stream.ReceiveNext();
                crossbowOptions.arrows = (int)stream.ReceiveNext();
                isDead                 = (bool)stream.ReceiveNext();
                
                bool remoteCrossbowOut = (bool)stream.ReceiveNext();
                if (_animController != null)
                {
                    _animController.CrossbowOut = remoteCrossbowOut;
                }
            }
        }

        public void Morir()
        {
            if (isDead) return;
            isDead = true;

            if (panelMuerte != null && videoClip != null && rawImage != null)
                StartCoroutine(MostrarVideoYMorir());
            else
                CompletarMuerte();
        }

        private void CompletarMuerte()
        {
            if (!PhotonNetwork.IsConnected)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("Gana_Slime");
                return;
            }
            StartCoroutine(ComprobarTodosMuertos());
        }

        private IEnumerator ComprobarTodosMuertos()
        {
            yield return new WaitForSeconds(0.2f);

            var allPlayers = FindObjectsOfType<RigidbodyFirstPersonController>();
            bool todosMuertos = System.Array.TrueForAll(allPlayers, p => p.isDead);

            if (todosMuertos)
                photonView.RPC("RPC_CargarEscenaFinal", RpcTarget.MasterClient, "Gana_Slime");
            else
                Espectator();
        }

        public void Espectator()
        {
            if (cam != null && GameManager_Network.camarasActivas.Contains(cam) && isDead)
            {
                GameManager_Network.camarasActivas.Remove(cam);
                Debug.Log($"Cámara eliminada. Total en lista: {GameManager_Network.camarasActivas.Count}");
            }

            if (photonView.IsMine)
            {
                if (panelMuerte != null) panelMuerte.SetActive(false);
                if (playerInput != null) playerInput.SwitchCurrentActionMap("Espectador");
                if (cam != null) cam.gameObject.SetActive(false);
                panelEspectador.SetActive(true);
                panelRaton.SetActive(false);

                // [!] CORRECCIÓN: Liberar el ratón al ser espectador
                mouseLook.lockCursor = false;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            if (modoEspectador != null)
                modoEspectador.CambiarASiguienteJugador();
        }

        IEnumerator EsperarYActualizarTNTCount()
        {
            if (PhotonNetwork.IsConnected)
                yield return new WaitUntil(() => SpawnManager.totalTNTsEnMapa > 0);
            else
                yield return null;

            dinamitasText.text = tnt + "/" + SpawnManager.totalTNTsEnMapa;
            Debug.Log($"TNTs necesarias para ganar: {SpawnManager.totalTNTsEnMapa}");
        }

        private IEnumerator MostrarVideoYMorir()
        {
            videoPlayer = gameObject.AddComponent<VideoPlayer>();

            RenderTexture rt = new RenderTexture(1920, 1080, 24);
            rt.Create();

            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            videoPlayer.targetTexture = rt;
            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = false;
            videoPlayer.clip = videoClip;

            rawImage.texture = rt;

            panelMuerte.SetActive(true);
            rawImage.gameObject.SetActive(true);

            bool videoTerminado = false;
            videoPlayer.loopPointReached += (_) => videoTerminado = true;

            videoPlayer.Prepare();
            yield return new WaitUntil(() => videoPlayer.isPrepared);
            videoPlayer.Play();

            yield return new WaitUntil(() => videoTerminado);

            panelMuerte.SetActive(false);
            Destroy(rt);
            CompletarMuerte();
        }

        public void ActualizarSensibilidad()
        {
            if (PhotonNetwork.IsConnected && !photonView.IsMine) return;

            float sens = PlayerPrefs.GetInt("Sensibility", 20);

            mouseLook.XSensitivity = sens;
            mouseLook.YSensitivity = sens;

            Debug.Log($"Sensibilidad aplicada al jugador local: {sens}");
        }

        [PunRPC]
        public void RPC_Morir()
        {
            Morir();
        }

        [PunRPC]
        private void RPC_CargarEscenaFinal(string nombreEscena)
        {
            if (PhotonNetwork.IsMasterClient)
                PhotonNetwork.LoadLevel(nombreEscena);
        }
    }
}
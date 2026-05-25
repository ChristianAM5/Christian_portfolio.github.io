using System.Collections;
using UnityEngine;
using Photon.Pun;

namespace UnityStandardAssets.Characters.FirstPerson
{
    /// <summary>
    /// Controla las animaciones del ratón (local y remoto) y las sincroniza por red con Photon.
    ///
    /// ══════════════════════════════════════════════════
    ///  SETUP — PASOS OBLIGATORIOS
    /// ══════════════════════════════════════════════════
    ///
    /// 1. COMPONENTE
    ///    Añadir este script al GameObject raíz del Player
    ///    (el mismo que tiene RigidbodyFirstPersonController).
    ///
    /// 2. INSPECTOR
    ///    - localAnimator  → arrastrar el Animator de Camera/LocalRig
    ///    - remoteAnimator → arrastrar el Animator de RemoteRig
    ///
    /// 3. PHOTONVIEW
    ///    Añadir este componente a la lista "Observed Components" del PhotonView
    ///    (junto a RigidbodyFirstPersonController).
    ///
    /// 4. RIGIDBODYFIRSTPERSONCONTROLLER
    ///    En el método Shoot(), añadir: _animController?.OnShoot();
    ///    (ver el parche al final del archivo)
    ///
    /// ══════════════════════════════════════════════════
    ///  PARÁMETROS DEL ANIMATOR CONTROLLER
    ///  (crear estos en AMBOS animators: LocalRig y RemoteRig)
    /// ══════════════════════════════════════════════════
    ///
    ///  Tipo     Nombre          Descripción
    ///  ──────── ─────────────── ────────────────────────────────────
    ///  bool     HasArrows       Tiene al menos una flecha
    ///  bool     IsMoving        Está caminando (no corriendo)
    ///  bool     IsRunning       Está corriendo
    ///  bool     IsGrounded      Está en el suelo
    ///  bool     CrossbowOut     La ballesta está en mano / visible
    ///  Trigger  Shoot           Disparo → animación de recarga
    ///  Trigger  Idle2           Idle alternativo aleatorio
    ///
    /// ══════════════════════════════════════════════════
    ///  ESTADOS DEL ANIMATOR CONTROLLER
    /// ══════════════════════════════════════════════════
    ///
    ///  ESTADO INICIAL (Default State): Armature_Idle2_ballesta
    ///
    ///  ── SIN BALLESTA ──────────────────────────────────────────────
    ///  Armature_Idle            Idle normal sin ballesta
    ///  Armature_Idle2           Idle alternativo (no loop, vuelve a Idle)
    ///  Armature_Walk            Caminar sin ballesta
    ///  Armature_Correr          Correr a 4 patas (no loop recomendado: loop)
    ///  Armature_Salto           Salto (no loop)
    ///
    ///  ── CON BALLESTA ──────────────────────────────────────────────
    ///  Armature_Idle2_ballesta         Idle con ballesta (loop)
    ///  Armature_Walk_con_ballesta      Caminar con ballesta (loop)
    ///  Armature_Idle_ballesta_recarga  Recarga en idle (no loop)
    ///  Armature_Walk_Ballesta_Recharge Recarga caminando (no loop)
    ///
    ///  ── TRANSICIONES DE BALLESTA ──────────────────────────────────
    ///  Armature_Sacar_Ballesta_Idle    Sacar ballesta desde idle (no loop)
    ///  Armature_Guardar_Ballesta_Idle  Guardar ballesta desde idle (no loop)
    ///  Armature_Walk_sacar_ballesta    Sacar ballesta caminando (no loop)
    ///  Armature_Walk_guardar_ballesta  Guardar ballesta caminando (no loop)
    ///
    /// ══════════════════════════════════════════════════
    ///  TRANSICIONES DEL ANIMATOR CONTROLLER
    ///  (configurar Has Exit Time = false salvo donde se indique)
    /// ══════════════════════════════════════════════════
    ///
    ///  ── SIN BALLESTA ──────────────────────────────────────────────
    ///  Idle              → Walk                    [IsMoving=T, CrossbowOut=F]
    ///  Idle              → Idle2                   [Trigger: Idle2]
    ///  Idle              → Correr                  [IsRunning=T]
    ///  Idle2             → Idle                    [Has Exit Time = true, Exit=1.0]
    ///  Walk              → Idle                    [IsMoving=F, CrossbowOut=F]
    ///  Walk              → Correr                  [IsRunning=T]
    ///  Correr            → Idle                    [IsRunning=F, IsMoving=F]
    ///  Correr            → Walk                    [IsRunning=F, IsMoving=T]
    ///
    ///  ── SALTO (AnyState, orden de prioridad alta) ──────────────────
    ///  AnyState          → Salto                   [IsGrounded=F, CrossbowOut=F]
    ///                                               (no desde estados de transición,
    ///                                                usar "Can Transition To Self"=F)
    ///  Salto             → Idle                    [IsGrounded=T, IsMoving=F]
    ///  Salto             → Walk                    [IsGrounded=T, IsMoving=T]
    ///
    ///  ── HACIA CON BALLESTA ────────────────────────────────────────
    ///  Idle              → Sacar_Ballesta_Idle      [CrossbowOut=T, IsMoving=F]
    ///  Sacar_Ballesta_Idle → Idle2_ballesta         [Has Exit Time=T, Exit=1.0]
    ///  Walk              → Walk_sacar_ballesta      [CrossbowOut=T, IsMoving=T]
    ///  Walk_sacar_ballesta → Walk_con_ballesta      [Has Exit Time=T, Exit=1.0]
    ///
    ///  ── CON BALLESTA ──────────────────────────────────────────────
    ///  Idle2_ballesta    → Walk_con_ballesta        [IsMoving=T, CrossbowOut=T]
    ///  Walk_con_ballesta → Idle2_ballesta           [IsMoving=F, CrossbowOut=T]
    ///  Idle2_ballesta    → Idle_ballesta_recarga    [Trigger: Shoot]
    ///  Walk_con_ballesta → Walk_Ballesta_Recharge   [Trigger: Shoot]
    ///  Idle_ballesta_recarga → Idle2_ballesta       [Has Exit Time=T, Exit=1.0]
    ///  Walk_Ballesta_Recharge → Walk_con_ballesta   [Has Exit Time=T, Exit=1.0]
    ///
    ///  ── DESDE CON BALLESTA ────────────────────────────────────────
    ///  Idle2_ballesta    → Guardar_Ballesta_Idle    [CrossbowOut=F, IsMoving=F]
    ///  Guardar_Ballesta_Idle → Idle                 [Has Exit Time=T, Exit=1.0]
    ///  Walk_con_ballesta → Walk_guardar_ballesta    [CrossbowOut=F, IsMoving=T]
    ///  Walk_guardar_ballesta → Walk                 [Has Exit Time=T, Exit=1.0]
    ///
    ///  NOTA: Las transiciones Guardar/Sacar inducidas por correr o saltar
    ///  las gestiona directamente el script con Animator.Play(),
    ///  por eso NO hace falta transición desde esos estados en el controller.
    /// ══════════════════════════════════════════════════
    /// </summary>

public class PlayerAnimatorController : MonoBehaviourPunCallbacks, IPunObservable
{
        // ══════════════════════════════════════════════
        #region Inspector
        // ══════════════════════════════════════════════
 
        [Header("Rigs")]
        [Tooltip("Animator en Camera/LocalRig — solo visible para el jugador local")]
        public Animator localAnimator;
 
        [Tooltip("Animator en RemoteRig — visible para el resto de jugadores")]
        public Animator remoteAnimator;
 
        #endregion
 
        // ══════════════════════════════════════════════
        #region Hashes de parámetros (cacheados para rendimiento)
        // ══════════════════════════════════════════════
 
        private static readonly int PIsMoving    = Animator.StringToHash("IsMoving");
        private static readonly int PIsRunning   = Animator.StringToHash("IsRunning");
        private static readonly int PIsGrounded  = Animator.StringToHash("IsGrounded");
        private static readonly int PHasArrows   = Animator.StringToHash("HasArrows");
        private static readonly int PCrossbowOut = Animator.StringToHash("CrossbowOut");
        private static readonly int PShoot       = Animator.StringToHash("Shoot");
        private static readonly int PIdle2       = Animator.StringToHash("Idle2");
 
        #endregion
 
        // ══════════════════════════════════════════════
        #region Estado interno
        // ══════════════════════════════════════════════
 
        private RigidbodyFirstPersonController _fp;
 
        private bool _crossbowOut;
        private bool _wasRunning;
        private bool _wasGrounded  = true;
        private bool _wasJumping;
        private int  _prevArrows   = -1;
 
        private Coroutine _activeRoutine;

        [Header("Crossbows")]
        [SerializeField] private GameObject localCrossbow;
        [SerializeField] private GameObject remoteCrossbow;
 
        // Datos que se mandan por red
        private int   _netStateHash;
        private float _netNormalizedTime;
        private bool  _netCrossbowOut;
        private bool _netIsRunning;
        private bool _netIsMoving;
        private bool _netIsGrounded;

        [Header("Run Traveling")]
        [SerializeField] private Transform localRig;

        [Header("Traveling Offsets")]
        [SerializeField] private Vector3 runningOffset = new Vector3(0f, 0f, 1f);
        [SerializeField] private Vector3 idle2Offset = new Vector3(0f, 0f, -1f); // Nuevo offset para Idle2

        [Header("Smoothing")]
        [SerializeField] private float smoothSpeed = 8f;
        [SerializeField] private float idle2Smooth = 4f;

        private bool _idle2Activo;

        private Vector3 _localRigInitialPos;

        private Coroutine _idle2Coroutine;

        public bool CrossbowOut 
        { 
            get => _crossbowOut; 
            set => _crossbowOut = value; 
        }
 
        #endregion
 
        // ══════════════════════════════════════════════
        #region Unity Lifecycle
        // ══════════════════════════════════════════════
 
        private void Awake()
        {
            _fp = GetComponent<RigidbodyFirstPersonController>();
        }
 
        private void Start()
        {
            ConfigurarRigs();
 
            // Estado inicial según flechas al comenzar
            int initialArrows = _fp.crossbowOptions.arrows;
            _crossbowOut = initialArrows > 0;
            _prevArrows  = initialArrows;
 
            SetBool(PIsGrounded,  true);
            SetBool(PHasArrows,   _crossbowOut);
            SetBool(PCrossbowOut, _crossbowOut);
 
            if (localRig != null)
            _localRigInitialPos = localRig.localPosition;

        }
 
        private void Update()
        {
            bool esMio = !PhotonNetwork.IsConnected || photonView.IsMine;
            if (esMio)
            {
                ActualizarAnimacionesLocal();
                // 1. Obtenemos el estado actual del animator
                AnimatorStateInfo state = localAnimator.GetCurrentAnimatorStateInfo(0);

                // 2. Forzamos la variable basándonos en lo que el Animator REALMENTE está haciendo
                // Esto es más seguro que confiar solo en la corrutina
                if (state.IsName("Armature_Idle2")) 
                {
                    _idle2Activo = true;
                }
                else 
                {
                    _idle2Activo = false;
                }
                ActualizarIdle2IntentadoTraveling(); // Llamada simplificada
            }
            else
            {
                AplicarAnimacionRed();
            }
        }
 
        #endregion
 
        // ══════════════════════════════════════════════
        #region Configuración de Rigs
        // ══════════════════════════════════════════════
 
        private void ConfigurarRigs()
        {
            bool esMio = !PhotonNetwork.IsConnected || photonView.IsMine;
 
            // Local: jugador propio ve el LocalRig (dentro de la cámara), oculta RemoteRig
            SetRigActivo(localAnimator,  esMio);
            // Remoto: los demás jugadores ven el RemoteRig del clon
            SetRigActivo(remoteAnimator, !esMio);
        }
 
        private static void SetRigActivo(Animator anim, bool activo)
        {
            if (anim != null)
                anim.gameObject.SetActive(activo);
        }
 
        #endregion
 
        // ══════════════════════════════════════════════
        #region Lógica de animación local
        // ══════════════════════════════════════════════
 
        private void ActualizarAnimacionesLocal()
        {
            if (_fp == null) return;
 
            bool isRunning  = _fp.Running;
            AnimatorStateInfo state = localAnimator.GetCurrentAnimatorStateInfo(0);

            bool isGrounded = _fp.Grounded;
            bool isJumping  = _fp.Jumping;
            int  arrows     = _fp.crossbowOptions.arrows;
            bool hasArrows  = arrows > 0;
            
            Vector3 vel    = _fp.Velocity;
            bool isMoving  = (vel.x * vel.x + vel.z * vel.z) > 0.25f; // umbral ~0.5 m/s
 
            _netIsRunning = isRunning;
            _netIsMoving = isMoving;
            _netIsGrounded = isGrounded;

            // ── Cambio de flechas ──────────────────────────────────────────────
            if (arrows != _prevArrows)
            {
                // Pasó a 0 flechas → guardar ballesta
                if (arrows == 0 && _crossbowOut)
                {
                    IniciarRutina(RutinaGuardar(isMoving));
                }
                // Recogió flecha estando sin ballesta y en reposo → sacarla
                else if (arrows > 0 && _prevArrows == 0
                         && !_crossbowOut && !isRunning && isGrounded && !isJumping)
                {
                    IniciarRutina(RutinaSacar(isMoving));
                }
                _prevArrows = arrows;
            }
 
            // ── Comenzó a correr ──────────────────────────────────────────────
            if (isRunning && !_wasRunning)
            {
                if (_crossbowOut)
                    IniciarRutina(RutinaGuardarYCorrer());  // guarda primero, luego corre
                else
                    SetBool(PIsRunning, true);
            }
            // ── Dejó de correr ────────────────────────────────────────────────
            else if (!isRunning && _wasRunning)
            {
                SetBool(PIsRunning, false);
                // Si tiene flechas y la ballesta estaba guardada (porque corrió) → sacarla
                if (hasArrows && !_crossbowOut && isGrounded && _activeRoutine == null)
                    IniciarRutina(RutinaSacar(isMoving));
            }
 
            // ── Despegó del suelo (inicio del salto) ───────────────────────────
            if (!isGrounded && _wasGrounded && !isRunning)
            {
                SetBool(PIsGrounded, false);
                if (_crossbowOut)
                    IniciarRutina(RutinaGuardarYSaltar()); // guarda, luego el Animator pasa a Salto
            }
 
            // ── Aterrizó ──────────────────────────────────────────────────────
            if (isGrounded && !_wasGrounded)
            {
                SetBool(PIsGrounded, true);

                // Forzar finalización de rutina vieja si seguía viva
                if (_activeRoutine != null)
                {
                    StopCoroutine(_activeRoutine);
                    _activeRoutine = null;
                }

                // Sacar ballesta otra vez al aterrizar
                if (hasArrows && !_crossbowOut && !isRunning)
                {
                    IniciarRutina(RutinaSacar(isMoving));
                }
            }

            // ── Parámetros continuos ──────────────────────────────────────────
            SetBool(PIsMoving,  isMoving);
            SetBool(PHasArrows, hasArrows);
 
            // ── Guardar estado del frame para sincronización ──────────────────
            Animator anim = AnimatorActivo;
            if (anim != null)
            {
                AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);
                _netStateHash      = info.fullPathHash;
                _netNormalizedTime = info.normalizedTime;
            }
            _netCrossbowOut = _crossbowOut;

            // ── Actualizar flags del frame anterior ───────────────────────────
            _wasRunning  = isRunning;
            _wasGrounded = isGrounded;
            _wasJumping  = isJumping;

            // ── Idle2 aleatorio controlado por corrutina ─────────────────────
            if (!isMoving && !isRunning && isGrounded && !isJumping
                && _activeRoutine == null && _idle2Coroutine == null)
            {
                _idle2Coroutine = StartCoroutine(RutinaIdle2Aleatorio());
            }

            // Si el jugador se mueve/corre/salta → cancelar intento
            if (isMoving || isRunning)
            {
                if (_idle2Coroutine != null)
                {
                    StopCoroutine(_idle2Coroutine);
                    _idle2Coroutine = null;
                }
                _idle2Activo = false;
            }

            if (_idle2Activo && !state.IsName("Armature_Idle2"))
            {
                _idle2Activo = false;
            }
        }

        private void ActualizarIdle2IntentadoTraveling()
        {
            if (localRig == null) return;

            Vector3 targetPos = _localRigInitialPos;
            float currentSmooth = smoothSpeed;

            // Prioridad 1: Correr (Offset hacia adelante)
            if (_fp != null && _fp.Running)
            {
                targetPos += runningOffset;
                currentSmooth = smoothSpeed;
            }
            // Prioridad 2: Idle 2 (Offset hacia atrás/lado según tu diseño)
            else if (_idle2Activo)
            {
                targetPos += idle2Offset;
                currentSmooth = idle2Smooth;
            }

            // Aplicar movimiento
            localRig.localPosition = Vector3.Lerp(
                localRig.localPosition,
                targetPos,
                Time.deltaTime * currentSmooth
            );
        }

        /// <summary>
        /// Llamar desde RigidbodyFirstPersonController cuando el jugador dispara.
        /// Activa la animación de recarga.
        /// </summary>
        public void OnShoot()
        {
            SetTrigger(PShoot);
        }
 
        #endregion
 
        // ══════════════════════════════════════════════
        #region Corrutinas de transición con ballesta
        // ══════════════════════════════════════════════
 
        private void IniciarRutina(IEnumerator rutina)
        {
            if (_activeRoutine != null)
                StopCoroutine(_activeRoutine);
            _activeRoutine = StartCoroutine(rutina);
        }
 
        /// <summary>Guardar la ballesta (idle o walk).</summary>
       private IEnumerator RutinaGuardar(bool isMoving)
        {
            _crossbowOut = false;
            SetBool(PCrossbowOut, false);

            string estado = isMoving
                ? "Armature_Walk_guardar_ballesta"
                : "Armature_Guardar_Ballesta_Idle";

            AnimatorActivo?.Play(estado);

            // Espera a que la animación vaya avanzada
            yield return EsperarPorcentajeAnim(estado, 0.8f);

            // Oculta la ballesta físicamente
            MostrarBallesta(false);

            yield return EsperarFinAnim(estado);

            _activeRoutine = null;
        }
 
        /// <summary>Sacar la ballesta (idle o walk).</summary>
        private IEnumerator RutinaSacar(bool isMoving)
        {
            // Mostrar físicamente la ballesta
            MostrarBallesta(true);

            _crossbowOut = true;
            SetBool(PCrossbowOut, true);

            string estado = isMoving
                ? "Armature_Walk_sacar_ballesta"
                : "Armature_Sacar_Ballesta_Idle";

            AnimatorActivo?.Play(estado);

            yield return EsperarFinAnim(estado);

            _activeRoutine = null;
        }
 
        /// <summary>Guarda la ballesta y activa la carrera al terminar.</summary>
        private IEnumerator RutinaGuardarYCorrer()
        {
            _crossbowOut = false;
            SetBool(PCrossbowOut, false);

            const string guardar = "Armature_Guardar_Ballesta_Idle";


            AnimatorActivo?.Play(guardar);

            // 2. Esperamos menos tiempo (al 50% de la animación ya la ocultamos)
            yield return EsperarPorcentajeAnim(guardar, 0.1f);
            MostrarBallesta(false);

            // 3. Salimos de la animación casi de inmediato
            yield return EsperarFinAnim(guardar, 0.15f);

            // 4. RESTAURAR VELOCIDAD NORMAL y correr
            if (AnimatorActivo != null) AnimatorActivo.speed = 1.0f;
            SetBool(PIsRunning, true);

            _activeRoutine = null;
        }
 
        /// <summary>Guarda la ballesta y deja al Animator pasar a Salto al terminar.</summary>
        private IEnumerator RutinaGuardarYSaltar()
        {
            _crossbowOut = false;
            SetBool(PCrossbowOut, false);

            const string guardar = "Armature_Guardar_Ballesta_Idle";
            
           

            AnimatorActivo?.Play(guardar);

            // Ocultar casi al empezar (10%)
            yield return EsperarPorcentajeAnim(guardar, 0.1f);
            MostrarBallesta(false);

            // Liberar la rutina muy rápido (30%) para que el Update pueda detectar el IsGrounded=false
            yield return EsperarPorcentajeAnim(guardar, 0.3f);

            if (AnimatorActivo != null) AnimatorActivo.speed = 1.0f;
            _activeRoutine = null;
        }
 
        private IEnumerator EsperarPorcentajeAnim(string nombreEstado, float porcentaje)
        {
            Animator anim = AnimatorActivo;
            if (anim == null) yield break;

            while (true)
            {
                AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);

                if (info.IsName(nombreEstado) &&
                    info.normalizedTime >= porcentaje)
                {
                    yield break;
                }

                yield return null;
            }
        }

        /// <summary>
        /// Espera hasta que la animación indicada finalice (normalizedTime >= 0.9).
        /// Timeout por seguridad.
        /// </summary>
        private IEnumerator EsperarFinAnim(string nombreEstado, float maxEspera = 2f)
        {
            Animator anim = AnimatorActivo;
            if (anim == null) yield break;
 
            // Esperar a que el Animator entre en ese estado (máx 0.5 s)
            float t = 0f;
            while (!anim.GetCurrentAnimatorStateInfo(0).IsName(nombreEstado))
            {
                t += Time.deltaTime;
                if (t > 0.5f) yield break; // no entró a tiempo, abortar
                yield return null;
            }
 
            // Esperar a que la animación complete su ciclo
            t = 0f;
            while (t < maxEspera)
            {
                AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);
                // Salió del estado antes de tiempo (interrumpido externamente)
                if (!info.IsName(nombreEstado)) yield break;
                // Llegó al final
                if (info.normalizedTime >= 0.9f) yield break;
 
                t += Time.deltaTime;
                yield return null;
            }
        }
 
        #endregion
 
        private void MostrarBallesta(bool visible)
        {
            if (PhotonNetwork.IsConnected && !photonView.IsMine)
            {
                if (remoteCrossbow != null)
                    remoteCrossbow.SetActive(visible);
            }
            else
            {
                if (localCrossbow != null)
                    localCrossbow.SetActive(visible);
            }
        }

        // ══════════════════════════════════════════════
        #region Sincronización Photon
        // ══════════════════════════════════════════════
 
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                // stream.SendNext(_netStateHash);
                // stream.SendNext(_netNormalizedTime);
                stream.SendNext(_crossbowOut); // Usamos la variable real
                stream.SendNext(_netIsRunning);
                stream.SendNext(_netIsMoving);
                stream.SendNext(_netIsGrounded);
            }
            else
            {
                // _netStateHash      = (int)stream.ReceiveNext();
                // _netNormalizedTime = (float)stream.ReceiveNext();
                
                bool nuevoEstadoBallesta = (bool)stream.ReceiveNext();
                // Si el estado cambió por red, actualizamos la visibilidad física del clon
                if (nuevoEstadoBallesta != _crossbowOut)
                {
                    _crossbowOut = nuevoEstadoBallesta;
                    MostrarBallesta(_crossbowOut); 
                }

                _netIsRunning = (bool)stream.ReceiveNext();
                _netIsMoving = (bool)stream.ReceiveNext();
                _netIsGrounded = (bool)stream.ReceiveNext();
            }
        }
 
        /// <summary>
        /// Aplica el estado recibido por red al RemoteRig.
        /// Solo cambia de estado cuando cambia el hash para evitar stuttering.
        /// </summary>
        private void AplicarAnimacionRed()
        {
            if (remoteAnimator == null) return;

            remoteAnimator.SetBool(PIsRunning, _netIsRunning);
            remoteAnimator.SetBool(PIsMoving, _netIsMoving);
            remoteAnimator.SetBool(PIsGrounded, _netIsGrounded);
 
            //AnimatorStateInfo current = remoteAnimator.GetCurrentAnimatorStateInfo(0);
            //if (current.fullPathHash != _netStateHash)
            //    remoteAnimator.Play(_netStateHash, 0, _netNormalizedTime);
 
            remoteAnimator.SetBool(PCrossbowOut, _crossbowOut);
        }
 
        #endregion
 
        // ══════════════════════════════════════════════
        #region Utilidades
        // ══════════════════════════════════════════════

        /// <summary>Devuelve el Animator activo para este cliente.</summary>
        private Animator AnimatorActivo =>
            (!PhotonNetwork.IsConnected || photonView.IsMine) ? localAnimator : remoteAnimator;
 
        private void SetBool(int hash, bool value)
        {
            // Solo enviamos el parámetro si el Animator existe Y el GameObject está activo
            if (localAnimator != null && localAnimator.gameObject.activeInHierarchy) 
                localAnimator.SetBool(hash, value);
                
            if (remoteAnimator != null && remoteAnimator.gameObject.activeInHierarchy) 
                remoteAnimator.SetBool(hash, value);
        }
 
        private void SetTrigger(int hash)
        {
            // Solo disparar triggers en el animator activo para evitar triggers fantasma
            if (localAnimator  != null && localAnimator.gameObject.activeInHierarchy)
                localAnimator.SetTrigger(hash);
            if (remoteAnimator != null && remoteAnimator.gameObject.activeInHierarchy)
                remoteAnimator.SetTrigger(hash);
        }
 
        private IEnumerator RutinaIdle2Aleatorio()
        {
            // Espera aleatoria entre 8 y 20 segundos
            float espera = Random.Range(8f, 20f);

            yield return new WaitForSeconds(espera);

            if (_fp == null)
            {
                _idle2Coroutine = null;
                yield break;
            }

            bool isMoving =
                (_fp.Velocity.x * _fp.Velocity.x +
                _fp.Velocity.z * _fp.Velocity.z) > 0.25f;

            bool isRunning = _fp.Running;
            bool isGrounded = _fp.Grounded;

            // SI ESTÁ EN CONDICIONES DE IDLE
            if (!isMoving && !isRunning && isGrounded && _activeRoutine == null)
            {
                // --- PASO CRÍTICO: TELETRANSPORTE PREVIO ---
                _idle2Activo = true; 
                if (localRig != null) 
                    localRig.localPosition = _localRigInitialPos + idle2Offset;

                // Ahora que la cámara ya está en Z -1, disparamos la animación
                SetTrigger(PIdle2);

                // Esperamos a que la animación termine (Ajusta este tiempo a la duración de tu clip)
                // Si tu animación dura 3 segundos, pon 3.2f para dar margen.
                yield return new WaitForSeconds(3.5f);

                // --- REGRESO ---
                if (localRig != null) 
                    localRig.localPosition = _localRigInitialPos;
                    
                _idle2Activo = false;
            }

            _idle2Coroutine = null;
        }

        #endregion
    }
}
 
// ══════════════════════════════════════════════════════════════════════
//  PARCHE PARA RigidbodyFirstPersonController.cs
// ══════════════════════════════════════════════════════════════════════
//
//  Añadir en la zona de variables privadas:
//
//      private PlayerAnimatorController _animController;
//
//  Añadir al final del bloque local de Start() (justo antes del cierre del if):
//
//      _animController = GetComponent<PlayerAnimatorController>();
//
//  Modificar el método Shoot() así:
//
//      public void Shoot(InputAction.CallbackContext callbackContext)
//      {
//          if (callbackContext.performed)
//          {
//              crossbowOptions.canShoot = true;
//              _animController?.OnShoot();   // ← añadir esta línea
//          }
//          else
//          {
//              crossbowOptions.canShoot = false;
//          }
//      }
//
// ══════════════════════════════════════════════════════════════════════
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityStandardAssets.Characters.FirstPerson;

public class ModoEspectador : MonoBehaviourPunCallbacks
{
    private int indiceCamaraActual = 0;
    private RigidbodyFirstPersonController controller; // Player local

    // Variable para recordar a qui�n estamos observando en este momento
    private RigidbodyFirstPersonController jugadorObservadoActual;

    private void Awake()
    {
        controller = gameObject.GetComponent<RigidbodyFirstPersonController>();
    }

    // Actualizamos la UI constantemente mientras observamos a alguien
    private void Update()
    {
        if (controller != null && jugadorObservadoActual != null)
        {
            if (controller == null || jugadorObservadoActual == null) return;
            if (GameManager_Network.Instance == null) return; // guard offline

            // Si el jugador observado ha muerto, cambiar automáticamente
            if (jugadorObservadoActual.isDead)
            {
                EjecutarCambioDeCamara();
                return;
            }
            // Como OnPhotonSerializeView actualiza estas variables silenciosamente por red,
            // simplemente las leemos en cada frame y las pintamos en pantalla.
            controller.flechasTextSpectator.text = jugadorObservadoActual.crossbowOptions.arrows.ToString();
            controller.dinamitasTextSpectator.text = GameManager_Network.Instance.tntGlobal + "/" + SpawnManager.totalTNTsEnMapa;
        }
    }

    public void CambiarASiguienteJugador(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            EjecutarCambioDeCamara();
        }
    }

    public void CambiarASiguienteJugador()
    {
        EjecutarCambioDeCamara();
    }

   
    public void EjecutarCambioDeCamara()
    {
        // Limpiar entradas nulas antes de operar
        GameManager_Network.camarasActivas.RemoveAll(c => c == null);

        //Debug.Log("Espectador camaras: " + "INDICE " + indiceCamaraActual + "CANTIDAD" + GameManager_Network.camarasActivas.Count);
        // Verificamos que haya c�maras en la lista
        if (GameManager_Network.camarasActivas.Count == 0) return;

        // Reclamp por si el índice quedó fuera de rango tras eliminar cámaras
        indiceCamaraActual = Mathf.Clamp(indiceCamaraActual, 0, GameManager_Network.camarasActivas.Count - 1);

        // Apagamos la c�mara que estamos viendo actualmente
        GameManager_Network.camarasActivas[indiceCamaraActual].gameObject.SetActive(false);

        //if (controller != null)

        //    controller.spectatorNickname.text = "Carlos";
        //else
        //    Debug.Log("No");

        // Sumamos 1 al �ndice (y si llegamos al final, volvemos a 0 usando el m�dulo %)
        indiceCamaraActual = (indiceCamaraActual + 1) % GameManager_Network.camarasActivas.Count;
        Camera nuevaCamara = GameManager_Network.camarasActivas[indiceCamaraActual];

        // Encendemos la nueva c�mara
        nuevaCamara.gameObject.SetActive(true);


        // Buscamos el PhotonView en el objeto padre de la c�mara
        PhotonView viewObservado = nuevaCamara.GetComponentInParent<PhotonView>();
        // Buscamos el RigidbodyFirstPersonController en el objeto padre de la camara
        jugadorObservadoActual = nuevaCamara.GetComponentInParent<RigidbodyFirstPersonController>();

        if (controller != null && controller.spectatorNickname != null)
        {
            if (viewObservado != null && viewObservado.Owner != null)
            {
                // PhotonNetwork guarda autom�ticamente el nombre aqu� cuando el jugador se conecta
                string nombreReal = viewObservado.Owner.NickName;

                // Si el jugador no se puso nombre, Photon lo deja en blanco. Le ponemos uno por defecto.
                if (string.IsNullOrEmpty(nombreReal)) nombreReal = "Jugador " + viewObservado.Owner.ActorNumber;

                controller.spectatorNickname.text = nombreReal;
            }
            else
            {
                controller.spectatorNickname.text = "Desconocido";
            }

            //if (jugadorObservadoActual != null)
            //{
            //    // Asignamos las flechas y dinamita del jugador observado
            //    controller.flechasTextSpectator.text = jugadorObservadoActual.crossbowOptions.arrows.ToString();
            //    controller.dinamitasTextSpectator.text = jugadorObservadoActual.tnt + "/" + SpawnManager.totalTNTsEnMapa; ;
            //}
            //else
            //{
            //    Debug.Log("No dispongo del texto de la dinamita y flechas del jugador observado");
            //}
        }
    }
}
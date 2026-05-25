using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.Video;

public class UIVideoPlayer : MonoBehaviour
{
    public VideoClip videoClip;
    public RawImage rawImage;
    VideoPlayer videoPlayer;
    public GameObject panel;

    private void Awake()
    {
        if (GameConfig.singleScreenMode)
        {
            // Desactivar objetos del segundo display
            string[] objectsToDisable = { "CameraSlime", "AnimacionS", "DualScreenManager" };
            foreach (string objName in objectsToDisable)
            {
                GameObject obj = GameObject.Find(objName);
                if (obj != null)
                    obj.SetActive(false);
                else
                    Debug.LogWarning($"[SingleScreen] No se encontró el objeto: {objName}");
            }

            // Cambiar Main Camera (1) al Display 1 (índice 0)
            GameObject mainCam1 = GameObject.Find("CameraRaton");
            if (mainCam1 != null)
            {
                Camera cam = mainCam1.GetComponent<Camera>();
                if (cam != null)
                    cam.targetDisplay = 0;
                else
                    Debug.LogWarning("[SingleScreen] camera raton no tiene componente Camera!");
            }
            else
            {
                Debug.LogWarning("[SingleScreen] No se encontró el objeto: camera raton");
            }

            // ── Reasignar Canvas AnimacionR al Display 1 ──
            GameObject AnimacionRObj = GameObject.Find("AnimacionR");
            if (AnimacionRObj != null)
            {
                Canvas canvas = AnimacionRObj.GetComponent<Canvas>();
                if (canvas != null)
                    canvas.targetDisplay = 0; // Display 1 = índice 0
                else
                    Debug.LogWarning("[SingleScreen] AnimacionR no tiene componente Canvas!");
            }
            else
            {
                Debug.LogWarning("[SingleScreen] No se encontró el objeto: AnimacionR");
            }
        }
    }

    void Start()
    {
        // Crear Video Player
        videoPlayer = gameObject.AddComponent<VideoPlayer>();

        // Crear Render Texture
        RenderTexture renderTexture = new RenderTexture(1920, 1080, 24);
        rawImage.texture = renderTexture;

        // Configurar Video Player
        videoPlayer.playOnAwake = true;
        videoPlayer.isLooping = false;
        videoPlayer.clip = videoClip;
        videoPlayer.targetTexture = renderTexture;

        videoPlayer.Prepare();
        videoPlayer.prepareCompleted += (source) => {
            panel.SetActive(false);
            videoPlayer.Play();
        };

        videoPlayer.loopPointReached += (source) => {
            if (GameConfig.singleScreenMode)
            SceneManager.LoadScene("Nivel_1_Friendless");
            else
            SceneManager.LoadScene("Nivel_1");

        };
    }

    void Update()
    {
        //if(Input.GetKeyDown(KeyCode.Escape))
        //{
        //    SkipVideo();
        //}

        //if (Gamepad.current != null && (Gamepad.current.startButton.wasPressedThisFrame ||
        //                       Gamepad.current.buttonSouth.wasPressedThisFrame))
        //{
        //    SkipVideo();
        //}
    }



    public void SkipVideo(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            videoPlayer.Stop();
            if (GameConfig.singleScreenMode)
                SceneManager.LoadScene("Nivel_1_Friendless");
            else
                SceneManager.LoadScene("Nivel_1");
        }
    }
 
}
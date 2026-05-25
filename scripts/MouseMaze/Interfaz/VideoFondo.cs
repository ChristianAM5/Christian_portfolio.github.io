using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class UIVideoPlayerLoop : MonoBehaviour
{
    public VideoClip videoClip;
    public RawImage rawImage;
    VideoPlayer videoPlayer;
    public bool loop =  false;
    public GameObject panel;
    void Start()
    {
        // Crear Video Player
        videoPlayer = gameObject.AddComponent<VideoPlayer>();

        // Crear Render Texture
        RenderTexture renderTexture = new RenderTexture(1920, 1080, 24);
        rawImage.texture = renderTexture;

        // Configurar Video Player para loop infinito
        videoPlayer.playOnAwake = true;
        videoPlayer.isLooping = loop;
        videoPlayer.clip = videoClip;
        videoPlayer.targetTexture = renderTexture;

        videoPlayer.Prepare();
        videoPlayer.prepareCompleted += (source) => {
            panel.SetActive(false);
            videoPlayer.Play();
        };
    }
}
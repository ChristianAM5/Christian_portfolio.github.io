using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;

public class GameManager : MonoBehaviour
{
    [SerializeField] private int targetFPS = 60;
    public int level;
    public RigidbodyFirstPersonController RigidbodyFirstPersonController;
    public GameObject Raton;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Antes del if");
        
        Debug.Log("Despues");

        StartCoroutine(ActivateRaton());
        if (GameConfig.singleScreenMode)
        {
            Debug.Log("Dentro del if");
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // si esta en una pantalla llevar a animacion inicial

#if UNITY_EDITOR
        QualitySettings.vSyncCount = 0;  // Desactiva VSync en el editor
        Application.targetFrameRate = targetFPS;
#endif

    }




    private void Update()
    {
        
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            RigidbodyFirstPersonController = other.gameObject.GetComponent<RigidbodyFirstPersonController>();
        }

        if (RigidbodyFirstPersonController != null)
        {
            if (RigidbodyFirstPersonController.tnt >= SpawnManager.totalTNTsEnMapa)
            {
                SceneManager.LoadScene("Gana_Raton");
            }
        }
    }
    IEnumerator ActivateRaton()
    {
        yield return new WaitForEndOfFrame();

        Raton.SetActive(true);
    }
}

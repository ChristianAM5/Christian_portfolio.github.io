using UnityEngine;

public class MenuController : MonoBehaviour
{
    public GameObject menuCanvas;
    public GameObject hintPanel;

    private float hintTimer = 0f;
    private const float HINT_DURATION = 3f;
    private bool hintVisible = false;
    
    // Start is called once before the first execution of Update
    void Start()
    {
        menuCanvas.SetActive(false);

        // Mostrar el hint al inicio
        if (hintPanel != null)
        {
            hintPanel.SetActive(true);
            hintTimer = 0f;
            hintVisible = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Temporizador del hint
        if (hintVisible)
        {
            hintTimer += Time.deltaTime;
            if (hintTimer >= HINT_DURATION)
            {
                HideHint();
            }
        }

        // Edit > Poject Settins > Player > Other Settings > Active Input Handling
        if (Input.GetKeyDown(KeyCode.Tab))    
        {

            // Si el hint está visible, ocultarlo al presionar Tab
            if (hintVisible)
            {
                HideHint();
            }
            
            if(!menuCanvas.activeSelf && PauseController.IsGamePaused)
            {
                return; // Si estamos pausados por otra razón, no queremos alterar esta lógica
            }

            menuCanvas.SetActive(!menuCanvas.activeSelf);
            PauseController.SetPause(menuCanvas.activeSelf);
        }
        
    }
    
    private void HideHint()
    {
        if (hintPanel != null)
        {
            hintPanel.SetActive(false);
        }
        hintVisible = false;
    }
}
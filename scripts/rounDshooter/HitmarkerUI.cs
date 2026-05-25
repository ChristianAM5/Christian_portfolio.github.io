using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HitmarkerUI : MonoBehaviour
{
    [SerializeField] private float displayDuration = 0.1f;
    [SerializeField] private Image hitmarkerImage;

    private Coroutine hideCoroutine;

    private void Awake()
    {
        // Si no se arrastró en el Inspector, intentamos obtenerlo
        if (hitmarkerImage == null)
        {
            hitmarkerImage = GetComponent<Image>();
        }

        // Nos aseguramos de que empiece invisible apagando solo el componente Image
        if (hitmarkerImage != null)
        {
            hitmarkerImage.enabled = false;
        }
    }

    public void ShowHitmarker()
    {
        if (hitmarkerImage == null) return;

        // Si ya hay una cuenta atrás en marcha, la detenemos para reiniciarla
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }

        // Hacemos visible la imagen y arrancamos el temporizador
        hitmarkerImage.enabled = true;
        hideCoroutine = StartCoroutine(HideAfterTime());
    }

    private IEnumerator HideAfterTime()
    {
        yield return new WaitForSeconds(displayDuration);
        hitmarkerImage.enabled = false;
        hideCoroutine = null;
    }
}
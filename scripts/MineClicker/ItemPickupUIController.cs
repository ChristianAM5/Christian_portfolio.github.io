using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Se ha modificado ItemPickupUIController para notificaciones
// con los minerales y otras sin iconos en caso de que solo
// se quiera notificar texto

public class ItemPickupUIController : MonoBehaviour
{
    public static ItemPickupUIController Instance { get; private set; }

    [Header("Prefab del popup")]
    public GameObject popupPrefab;

    [Header("Configuración")]
    public int   maxPopups      = 5;
    public float popupDuration  = 3f;

    private readonly Queue<GameObject> activePopups = new Queue<GameObject>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Debug.LogError("Múltiples instancias de ItemPickupUIController.");
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Muestra un popup en la esquina inferior izquierda.
    /// Si itemIcon es null no muestra imagen.
    /// Si cantidad es 0 no muestra número.
    /// </summary>
    public void ShowItemPickup(string itemName, Sprite itemIcon, int cantidad = 1)
    {
        GameObject newPopup = Instantiate(popupPrefab, transform);

        // Texto: con cantidad si es mayor que 1, sin número si es 0
        string textoMostrado;
        if (cantidad <= 0)
            textoMostrado = itemName;
        else if (cantidad > 1)
            textoMostrado = $"{itemName} x{cantidad}";
        else
            textoMostrado = itemName;

        newPopup.GetComponentInChildren<TMP_Text>().text = textoMostrado;

        // Icono: lo ocultamos si no hay sprite
        Image itemImage = newPopup.transform.Find("ItemIcon")?.GetComponent<Image>();
        if (itemImage != null)
        {
            if (itemIcon != null)
            {
                itemImage.sprite = itemIcon;
                itemImage.gameObject.SetActive(true);
            }
            else
            {
                itemImage.gameObject.SetActive(false);
            }
        }

        activePopups.Enqueue(newPopup);

        // Si hay demasiados popups eliminamos el más antiguo
        if (activePopups.Count > maxPopups)
            Destroy(activePopups.Dequeue());

        StartCoroutine(FadeAndDestroy(newPopup));
    }

    private IEnumerator FadeAndDestroy(GameObject popup)
    {
        yield return new WaitForSeconds(popupDuration);

        if (popup == null) yield break;

        CanvasGroup canvasGroup = popup.GetComponent<CanvasGroup>();
        if (canvasGroup == null) yield break;

        for (float t = 0f; t < 1f; t += Time.deltaTime)
        {
            if (popup == null) yield break;
            canvasGroup.alpha = 1f - t;
            yield return null;
        }

        Destroy(popup);
    }
}
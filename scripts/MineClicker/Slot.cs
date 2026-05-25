using UnityEngine;
using TMPro;

// Se ha modificado para tener una capacidad mejorable de objetos iguales

public class Slot : MonoBehaviour
{
    [Header("Item actual en el slot")]
    public GameObject currentItem;

    [Header("UI de cantidad")]
    // Txt que gestiona la cantidad de espacio del slot
    public TextMeshProUGUI quantityText;

    // Capacidad máxima actual del slot se consulta a MineralUpgradeManager según el tipo de mineral.


    // Actualiza el texto de cantidad visible en el slot se llama cada vez que cambie la cantidad.
    public void UpdateQuantityDisplay()
    {
        if (quantityText == null) return;

        if (currentItem != null)
        {
            MineralItem mineral = currentItem.GetComponent<MineralItem>();

            // Si el objeto tiene MineralItem y la cantidad es mayor que 1
            if (mineral != null && mineral.quantity > 1)
            {
                // Activamos el texto de cantidad
                quantityText.gameObject.SetActive(true);

                // Mostramos la cantidad como texto
                quantityText.text = mineral.quantity.ToString();
            }
            else
            {
                // Si no hay mineral o la cantidad es 1 o menor, ocultamos el texto
                quantityText.gameObject.SetActive(false);
            }
        }
        else
        {
            // Si no hay objeto en el slot, ocultamos el texto
            quantityText.gameObject.SetActive(false);
        }
    }
}



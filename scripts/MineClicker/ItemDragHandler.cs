using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    Transform originalParent;
    CanvasGroup canvasGroup;
    public float minDroipDistance = 2f;
    public float maxDroipDistance = 3f;
    
    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent; // Guardasmos la posici�n inicial
        transform.SetParent(transform.root); //
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f; // El objeto es semitransparente durante el movimiento
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position=eventData.position; // Sigue el movimiento del rat�n 
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true; // Habilita raycast
        canvasGroup.alpha = 1.0f; // Al soltarlo se vuelve s�lido

        Slot dropSlot = eventData.pointerEnter?.GetComponent<Slot>(); // Slot donde se deja caer
        if(dropSlot == null)
        {
            GameObject dropItem =  eventData.pointerEnter;
            if(dropItem != null)
            {
                dropSlot= dropItem.GetComponentInParent<Slot>();
            }
        }
        Slot originalSlot = originalParent.GetComponent<Slot>();

        if (dropSlot != null)
        {
            if (dropSlot.currentItem != null) { 
                // El slot contiene otro objeto y debemos intercambiarlos
                dropSlot.currentItem.transform.SetParent(originalSlot.transform);
                originalSlot.currentItem = dropSlot.currentItem;
                dropSlot.currentItem.GetComponent<RectTransform>().anchoredPosition= Vector2.zero;
            }
            else
            {
                originalSlot.currentItem = null;
            }
            //Movemos el objeto al slot donde lo dejamos caer
            transform.SetParent(dropSlot.transform);
            dropSlot.currentItem = gameObject;
        }
        else
        {
            // No hay hueco donde estamos soltando el objeto:
            if (!IsWithinInventory(eventData.position))
            {
                // a) Si soltamos el objeto fuera del inventario lo dejamos caer en el escenario
                DropItem(originalSlot);
            }
            else
            {
                // b) Si dejamos caer el objeto en una posici�n donde no hay slot dentro del inventario
                transform.SetParent(originalParent);
            }                
        }
        GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
    }    

    bool IsWithinInventory(Vector2 mousePosition)
    {
        RectTransform inventoryRect = originalParent.parent.GetComponent<RectTransform>();
        return RectTransformUtility.RectangleContainsScreenPoint(inventoryRect, mousePosition);
    }

    void DropItem(Slot originalSlot)
    {
        originalSlot.currentItem = null;

        // Buscamos el objeto Player
        Transform playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (playerTransform == null)
        {
            Debug.Log("Falta la etiqueta 'Player'");
            return;
        }
        // Generamos una posici�n aleatoria para soltar el objeto pero cerca de la posici�n del jugador
        Vector2 dropOffset = Random.insideUnitCircle.normalized * Random.Range(minDroipDistance, maxDroipDistance);
        Vector2 dropPosition = (Vector2)playerTransform.position + dropOffset;

        // Instanciamos el objeto a soltar
        GameObject dropItem = Instantiate(gameObject, dropPosition, Quaternion.identity);
        dropItem.GetComponent<BounceEffect>().StartBounce();

        // Destruimos el objeto del UI
        Destroy(gameObject);
    }
}

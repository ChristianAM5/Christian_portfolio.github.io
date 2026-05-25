using UnityEngine;

// Representa un objeto mineral que puede guardarse en el inventario.
// Heredando de la clase Item 

public class MineralItem : Item
{
    [Header("Tipo de mineral")]
    public MineralType mineralType;

    [Header("Cantidad en este stack")]
    // Cuántas unidades representa este objeto en el inventario
    public int quantity = 1;
}


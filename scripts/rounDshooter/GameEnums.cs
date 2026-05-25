// Archivo: GameEnums.cs
// No lleva "public class", simplemente definimos el tipo: es mejor que no haya ninguna
// clase envolviéndolo para que sea accesible directamente.

/// <summary>
/// Define los diferentes tipos de llaves que existen en el juego.
/// Un 'enum' es una lista de constantes con nombre que facilita la lectura del código,
/// evitando el uso de números mágicos (0, 1, 2...) o strings que pueden tener erratas.
/// </summary>
public enum KeyType
{
    // Es una regla de oro en C# empezar con un valor 'None' o 'Default'.
    // Por defecto, los enums empiezan en valor 0. Si una variable no se asigna,
    // será 'None', lo que nos ayuda a detectar errores de configuración.
    None,

    // Tipos de llaves identificables por colores:
    Red,
    Blue,
    Green,

    // Tipos de llaves especiales:
    Gold,
    Master  // Una llave maestra que podría abrir cualquier cerradura
}

public enum AmmoType
{
    Pistol_9mm,
    Rifle_556mm
}
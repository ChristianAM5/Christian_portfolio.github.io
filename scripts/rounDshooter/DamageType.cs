using UnityEngine;

// El atributo CreateAssetMenu permite crear instancias de este objeto 
// directamente desde el menú de Unity (clic derecho en la carpeta Assets).
[CreateAssetMenu(fileName = "New Damage Type", menuName = "FPS Lab/Damage Type")]
public class DamageType : ScriptableObject
{
    [Header("Identificación")]
    // El nombre del tipo de daño (ej: "Fuego", "Explosión", "Veneno").
    public string typeName;

    [Header("Visualización")]
    // Color asociado para efectos visuales o depuración (ej: rojo para sangre, verde para ácido).
    public Color debugColor = Color.red;

    [Header("Ajustes de Combate")]
    // El multiplicador define cómo de fuerte es este tipo de daño.
    // [Range(0, 2)] crea un slider en el inspector que limita el valor entre 0 y 2.
    // 1f = daño normal | 0.5f = mitad de daño | 2f = doble de daño.
    [Range(0, 2)]
    public float damageMultiplier = 1f;
}

/* Conceptos clave:
1. ¿Qué es un ScriptableObject?
A diferencia de un MonoBehaviour, un ScriptableObject no se cuelga de un GameObject en la escena. Es un contenedor 
de datos que vive en la carpeta de Assets.
Analogía: Si el HealthSystem es el motor del coche, el DamageType es el tipo de combustible (Gasolina, Diesel, Eléctrico). 
Puedes tener muchos tipos de combustible sin cambiar el motor.

2. Ventajas del Flujo de Trabajo
Con este sistema, no necesitamos tocar el código cada vez que queramos crear un elemento nuevo.
Podemos crear un DamageType llamado "Lava" con un damageMultiplier de 2.0.
Luego, un "Dardo Tranquilizante" con un multiplicador de 0.1.
Todo se gestiona desde el Inspector de Unity, reduciendo errores de programación.

3. El Atributo [Range]
Es una excelente herramienta de UX para desarrolladores. Al usar [Range(0, 2)], evitamos que un alumno (o un diseñador de niveles)
ponga accidentalmente un valor negativo o un número demasiado alto que rompa el balance del juego.

¿Cómo se integra esto en vuestro sistema?
Como tenemos un HealthSystem desacoplado, este DamageType debería ser un parámetro en la función que resta vida. Por ejemplo:
public void TakeDamage(float amount, DamageType type)

Así, el sistema de feedback (Flash y Vignette) podría usar el debugColor del DamageType para cambiar el color del flash 
según el tipo de daño recibido.
*/

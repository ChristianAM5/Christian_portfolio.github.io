using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class Esfera1 : MonoBehaviour
{
    TutorialRatonController section;
    bool yaActivado = false; // Para que no se repita el SetSection

    void Start()
    {
        section = GameObject.Find("TutorialManager").GetComponent<TutorialRatonController>();
    }

    private void OnTriggerStay(Collider collision) // Cambiado a Stay
    {
        if (collision.gameObject.CompareTag("Player") && !yaActivado)
        {
            var player = collision.gameObject.GetComponent<RigidbodyFirstPersonController>();
            int tnt = player.tnt;
            Debug.Log("Dinamitas recogidas: " + tnt);

            if (tnt == 1 || tnt == 5)
            {
                yaActivado = true; // Bloqueamos para que solo pase una vez
                CheckDinamitas(tnt);
                
            }
        }
    }

    void CheckDinamitas(int tntRecogida)
    {
        Debug.Log("Dinamitas recogidas: " + tntRecogida);

        if (tntRecogida == 5) section.final = true;
        section.SetSection();
        // Opcional: Destroy(gameObject); // Si ya cumplió su función, bórralo
    }
}
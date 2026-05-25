using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PestaniaControles : MonoBehaviour
{
    private bool abierta = false;
    private bool puedeAlternar = true;
    public float tiempoEspera = 1.0f;

    public void Abrir()
    {
        // Nos aseguramos de que el objeto esté activo para ver la animación
        gameObject.SetActive(true);
        Debug.Log("Abriendo pestania controles");
        Animator animator = GetComponent<Animator>();
        animator.SetBool("arriba", true);
        animator.SetBool("abajo", false);
        abierta = true;
    }

    public void Cerrar()
    {
        Debug.Log("Cerrando pestania controles");
        Animator animator = GetComponent<Animator>();
        animator.SetBool("arriba", false);
        animator.SetBool("abajo", true);
        abierta = false;
    }

    // NUEVO MÉTODO: Lo usará el TutorialRatonController para poder liberar las teclas
    public void CerrarConRetraso(float tiempo)
    {
        Cerrar();
        StartCoroutine(DesactivarObjeto(tiempo));
    }

    private IEnumerator DesactivarObjeto(float t)
    {
        yield return new WaitForSeconds(t);
        gameObject.SetActive(false); // Aquí es cuando el Tutorial detecta que ya no hay nada abierto
    }

    public void Alternar()
    {
        if (!puedeAlternar) return;
        if (abierta) { Cerrar(); abierta = false; }
        else { Abrir(); abierta = true; }
        StartCoroutine(EsperarYReactivar());
    }

    private IEnumerator EsperarYReactivar()
    {
        puedeAlternar = false;
        yield return new WaitForSeconds(tiempoEspera);
        puedeAlternar = true;
    }
}
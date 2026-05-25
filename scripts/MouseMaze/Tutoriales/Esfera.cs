using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Esfera : MonoBehaviour
{
    Tutorial_Slime section;
    // Start is called before the first frame update
    void Start()
    {
        section = GameObject.Find("TutorialManager").GetComponent<Tutorial_Slime>();
    }
    private void OnTriggerEnter(Collider collision)
    {
        //Cuando el slime colisiona con esta esfera, llamamos al método SetSection() del script "Tutorial_Slime"
        if (collision.gameObject.CompareTag("Enemy"))
        {
            section.SetSection();
        }
    }
}

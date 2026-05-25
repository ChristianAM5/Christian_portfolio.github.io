using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.UI;

public class Tutorial_Slime : MonoBehaviour
{
    [Header("Interfaz del tutorial")]
    public RawImage[] textos;
    public Button btn;
    [Header("Tutorial Section")]    
    public List<GameObject> tutorialSection;
    public int sectionIndex;

    [Header("Esferas")]
    [SerializeField] List<GameObject> spheres;
    [Header("References")]
    [SerializeField] NavMeshSurface navMeshSurface;

    private void Start()
    {
        //Llamar a las referencias necesarias
        navMeshSurface = GameObject.Find("NavMesh Surface").GetComponent<NavMeshSurface>();

        //Llamar a la sección inicial
        SetSection();
    }

    void SetText()
    {
        //Mandamos leer todos los textos, para dejar activo el que nos interesa
        foreach(RawImage ri in textos)
        {
            if(ri == textos[sectionIndex])
                //Es el que queremos, lo activamos
            {
                ri.gameObject.SetActive(true);

                //Mostramos el texto nada más iniciar la sección
                ri.GetComponent<PestaniaControles>().Abrir();
            }
            else
                //No es el que toca, lo desactivamos
            {
                ri.gameObject.SetActive(false);
            }
        }
    }

    //Para cambiar de sección, llamamos al siguiente método desde el script "Esfera"
    public void SetSection()
    {
        //Solo activamos el código si el index no es superior a la cantidad de secciones
        if (sectionIndex < tutorialSection.Count)
        {
            //Actualizamos el texto a mostrar
            SetText();

            //Actualizamos el listener del botón que muestra el texto
            btn.onClick.AddListener(textos[sectionIndex].GetComponent<PestaniaControles>().Abrir);

            //Activar, en la escena, la sección que queremos. Las demás, las desactivamos
            foreach (GameObject section in tutorialSection)
            {
                if (section != tutorialSection[sectionIndex])
                //Esta sección no es la que queremos, la apagamos
                {
                    section.SetActive(false);
                }
                else
                //Esta sí la queremos, la encendemos
                {
                    section.SetActive(true);
                }
            }

            //Crear nav mesh nuevo
            navMeshSurface.BuildNavMesh();

            //Activamos las esferas, después del nav mesh
            spheres[sectionIndex].SetActive(true);

            //Incrementamos el index, para cuando toque crear la siguiente ronda
            sectionIndex++;
        }
        //De lo contrario, terminamos el tutorial
        else
        {
            //Debug.Log("Tutorial terminado");
            GetComponent<MasterManager>().MenuTutorial();
        }
    }

}

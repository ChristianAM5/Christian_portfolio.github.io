using UnityEngine;

public class SlimeKillsTutorial : MonoBehaviour
{
    private Animator slimeAttack;
    SlimeStatus slimeStatus;
    GameObject raton;
    //public Transform SpawnPoint;

    private void Awake()
    {
        slimeAttack = GetComponent<Animator>();
        slimeStatus = GetComponent<SlimeStatus>();
        raton = GameObject.Find("PlayerA_FPSController");
    }

    public void OnCollisionEnter(Collision collision)
    {

        if (collision.gameObject.tag != "Player" || slimeStatus.isStunned) return;

        var player = collision.gameObject.GetComponent<UnityStandardAssets.Characters.FirstPerson.RigidbodyFirstPersonController>();

        if (player == null || player.isDead) return;

        slimeAttack.SetTrigger("Attack");

        //realmente hace falta repetir esto?
        if (player == null) return;
        //la unica diferencia respecto de SlimeKills es q no mata al ratón, simplemente lo mueve a la posición inicial para q pueda terminar el tutorial
        raton.transform.position = new Vector3(-0.3f, 0.9784327f, 48.7f);
        //player.GetComponent<Transform>().position = SpawnPoint.GetComponent<Transform>().position;

    }
}
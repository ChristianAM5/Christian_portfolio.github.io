using UnityEngine;

// NPC enemigo que puede retar al jugador a un duelo.
// Implementa IInteractable para que el jugador pueda interactuar con él.

public class DuelEnemyNPC : MonoBehaviour, IInteractable
{
    private DuelEnemySpawner spawner;
    private bool interactuando = false;

    private void Start()
    {
        spawner = FindFirstObjectByType<DuelEnemySpawner>();
    }

    public bool CanInteract()
    {
        return !interactuando && !PauseController.IsGamePaused;
    }

    public void Interact()
    {
        if (!CanInteract()) return;
        interactuando = true;

        // Mostramos el diálogo de desafío
        // "this" = le pasamos este NPC al sistema de diálogo
        DuelDialogueUI.Instance.MostrarDialogo(this);
    }

    // El jugador acepta el duelo inicia el juego y le pasamos este npc como rival
    public void AceptarDuelo()
    {
        DuelMinigameController.Instance.IniciarDuelo(this);
    }


    // El jugador rechaza el duelo y el npc desaparece
    public void RechazarDuelo()
    {
        Despawn();
    }


    // Se llama cuando termina el combate y el jugador sale victorioso
    public void ResultadoDuelo(bool jugadorGano)
    {
        // Aplica un buff temporal aleatorio al jugador
        if (jugadorGano)
            TemporaryGlobalBuffSystem.Instance.AplicarBuffAleatorioConNombre();

        // Se despawnea el NPC
        Despawn();
    }


    // Se destruye el personaje de la escena y se notifica al spawner
    private void Despawn()
    {
        spawner?.NotificarDespawn();
        Destroy(gameObject);
    }
}

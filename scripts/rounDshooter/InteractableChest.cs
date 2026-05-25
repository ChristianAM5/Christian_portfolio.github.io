using UnityEngine;
using System.Collections.Generic;

public class InteractableChest : MonoBehaviour, IInteractable
{
   [Header("Configuración de Seguridad")]
    [SerializeField] private bool isLocked = true;
    [SerializeField] private List<KeyType> requiredKeys;

    [Header("Prompts")]
    [SerializeField] private string openPrompt = "Abrir Cofre";
    [SerializeField] private string closePrompt = "Cerrar Cofre";
    [SerializeField] private string lockedPrompt = "Cofre cerrado. Falta: ";

    [Header("Animación")]
    [SerializeField] private Transform lid;
    [SerializeField] private float openAngle = -120f;
    [SerializeField] private float smoothing = 5f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;
    [SerializeField] private AudioClip lockedSound;

    [Header("Loot")]
    [SerializeField] private GameObject lootPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private bool spawnOnlyOnce = true;

    [Header("Loot Aleatorio")]
    [SerializeField] private GameObject healthPickupPrefab;
    [SerializeField] private GameObject skillPointPickupPrefab;
    [SerializeField] private bool randomizeLoot = true;

    private bool _hasSpawned = false;

    private bool _isOpen = false;
    private Quaternion _targetRotation;
    private Quaternion _closedRotation;

    public string InteractionPrompt
    {
        get
        {
            if (_isOpen) return closePrompt;
            if (isLocked) return lockedPrompt + string.Join(", ", requiredKeys);
            return openPrompt;
        }
    }

    void Start()
    {
        if (lid == null) lid = transform;

        _closedRotation = lid.localRotation;
        _targetRotation = _closedRotation;
    }

    void Update()
    {
        lid.localRotation = Quaternion.Slerp(
            lid.localRotation,
            _targetRotation,
            Time.deltaTime * smoothing
        );
    }

    public void Interact()
    {
        // Si está bloqueado, comprobamos llaves
        if (isLocked)
        {
            PlayerInventory inventory = FindFirstObjectByType<PlayerInventory>();

            if (inventory != null)
            {
                bool hasMaster = inventory.HasKey(KeyType.Master);

                if (hasMaster || inventory.HasAllKeys(requiredKeys))
                {
                    isLocked = false;
                }
                else
                {
                    if (audioSource && lockedSound)
                        audioSource.PlayOneShot(lockedSound);

                    return;
                }
            }
        }

        // Abrir / cerrar
        _isOpen = !_isOpen;

        AudioClip clip = _isOpen ? openSound : closeSound;
        if (audioSource && clip)
            audioSource.PlayOneShot(clip);

        _targetRotation = _isOpen
            ? _closedRotation * Quaternion.Euler(openAngle, 0, 0)
            : _closedRotation;

        // Spawn de loot al abrir
        if (_isOpen)
        {
            TrySpawnLoot();
        }
    }

    private void TrySpawnLoot()
    {
        if (spawnOnlyOnce && _hasSpawned) return;

        Vector3 position = spawnPoint != null
            ? spawnPoint.position
            : transform.position + Vector3.up * 0.5f;

        GameObject prefabToSpawn = null;

        if (randomizeLoot)
        {
            // 50% probabilidad
            bool giveHealth = Random.value > 0.5f;

            if (giveHealth && healthPickupPrefab != null)
                prefabToSpawn = healthPickupPrefab;
            else if (skillPointPickupPrefab != null)
                prefabToSpawn = skillPointPickupPrefab;
        }
        else
        {
            prefabToSpawn = lootPrefab;
        }

        if (prefabToSpawn != null)
            Instantiate(prefabToSpawn, position, Quaternion.identity);

        _hasSpawned = true;
    }

    void OnEnable()
    {
        GameEvents.OnRoundStarted += ResetChest;
    }

    void OnDisable()
    {
        GameEvents.OnRoundStarted -= ResetChest;
    }

    void ResetChest(int round)
    {
        _hasSpawned = false;
        _isOpen = false;
        _targetRotation = _closedRotation;
    }
}
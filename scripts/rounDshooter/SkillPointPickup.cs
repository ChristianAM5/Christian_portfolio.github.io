using UnityEngine;

public class SkillPointPickup : MonoBehaviour
{
    [SerializeField] private int amount = 1;

    [Header("Efectos")]
    [SerializeField] private AudioClip pickupSound;

    [Range(0, 1)]
    [SerializeField] private float volume = 1f;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        RoundManager.Instance.AddBonusSkillPoints(amount);

        // Reproducir sonido
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position, volume);
        }

        Destroy(gameObject);
    }
}

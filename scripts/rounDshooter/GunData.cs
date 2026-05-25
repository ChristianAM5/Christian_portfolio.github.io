using UnityEngine;

[CreateAssetMenu(fileName = "NewGun", menuName = "FPS Lab/Gun Data")]
public class GunData : ScriptableObject
{
    [Header("Info")]
    public string gunName;

    [Header("Shooting")]
    public float damage;
    public float maxDistance;
    public float fireRate; // Balas por segundo
    public bool isAutomatic;

    [Header("Reloading")]
    public int magSize;
    public float reloadTime;
}
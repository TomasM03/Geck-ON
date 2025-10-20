using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapons/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Weapon Info")]
    public string weaponName = "Pistol";
    public Sprite weaponIcon;

    [Header("Combat Stats")]
    public float damage = 25f;
    public float range = 100f;
    public float fireRate = 0.5f;

    [Header("Bullet Behavior")]
    public int bulletsPerShot = 1;
    [Range(0f, 10f)]
    public float spread = 0f;

    [Header("Audio/Visual")]
    public AudioClip shootSound;
}

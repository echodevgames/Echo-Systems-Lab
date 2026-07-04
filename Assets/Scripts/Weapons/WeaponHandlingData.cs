//-----WeaponHandlingData.cs START-----

using UnityEngine;

[CreateAssetMenu(
    fileName = "WeaponHandlingData_NewWeapon",
    menuName = "Echo Systems Lab/Weapons/Weapon Handling Data")]
public class WeaponHandlingData : ScriptableObject
{
    [Header("Fire Kickback")]
    public Vector3 firePositionKick = new Vector3(0f, -0.015f, -0.08f);
    public Vector3 fireRotationKick = new Vector3(-4f, 0f, 0f);

    [Header("Fire Randomness")]
    public Vector3 randomRotationKick = new Vector3(1f, 0.75f, 1.25f);

    [Header("Limits")]
    public Vector3 maxPositionOffset = new Vector3(0.08f, 0.08f, 0.18f);
    public Vector3 maxRotationOffset = new Vector3(12f, 6f, 8f);

    [Header("Recovery")]
    public float positionSnappiness = 24f;
    public float rotationSnappiness = 24f;
    public float positionReturnSpeed = 12f;
    public float rotationReturnSpeed = 12f;

    [Header("Animator Triggers")]
    public bool useAnimatorTriggers = true;
    public string fireTriggerName = "Fire";
    public string reloadTriggerName = "Reload";
    public string equipTriggerName = "Equip";
}

//-----WeaponHandlingData.cs END-----
//-----DamageInfo.cs START-----
using UnityEngine;

public struct DamageInfo
{
    public int damageAmount;
    public string weaponId;
    public string weaponType;
    public GameObject sourceObject;
    public Vector3 hitPoint;

    public DamageInfo(
        int damageAmount,
        string weaponId,
        string weaponType,
        GameObject sourceObject,
        Vector3 hitPoint)
    {
        this.damageAmount = damageAmount;
        this.weaponId = weaponId;
        this.weaponType = weaponType;
        this.sourceObject = sourceObject;
        this.hitPoint = hitPoint;
    }
}
//-----DamageInfo.cs END-----
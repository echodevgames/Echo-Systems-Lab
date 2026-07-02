//-----TargetRangeMissionTarget.cs START-----

using UnityEngine;

public class TargetRangeMissionTarget : MonoBehaviour
{
    private TargetRangeTargetGroup owningGroup;

    public TargetRangeTargetGroup OwningGroup => owningGroup;

    public void Initialize(TargetRangeTargetGroup group)
    {
        owningGroup = group;
    }

    public void NotifyDestroyed(TargetHealth targetHealth, DamageInfo damageInfo)
    {
        if (owningGroup != null)
            owningGroup.HandleTargetDestroyed(this, targetHealth, damageInfo);
    }
}

//-----TargetRangeMissionTarget.cs END-----
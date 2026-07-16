//-----FootstepSurfaceTag.cs START-----

using UnityEngine;

public class FootstepSurfaceTag : MonoBehaviour
{
    [Header("Surface")]
    [SerializeField] private FootstepSurfaceData surfaceData;

    public FootstepSurfaceData SurfaceData => surfaceData;
}

//-----FootstepSurfaceTag.cs END-----
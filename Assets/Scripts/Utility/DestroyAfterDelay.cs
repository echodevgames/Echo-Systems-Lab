//-----DestroyAfterDelay.cs START-----

using UnityEngine;

public class DestroyAfterDelay : MonoBehaviour
{
    [SerializeField] private float delay = 2f;

    private void Start()
    {
        Destroy(gameObject, delay);
    }
}

//-----DestroyAfterDelay.cs END-----
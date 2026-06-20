using UnityEngine;

// Mark any object the fire head should be able to destroy. On Burn() the object is removed
// (after an optional short delay), clearing the path.
[RequireComponent(typeof(Collider))]
public class Burnable : MonoBehaviour
{
    [SerializeField] private float burnDelay = 0f;        // small delay before it disappears, if you want a beat
    [SerializeField] private GameObject burnEffectPrefab; // optional VFX spawned on burn

    private bool burning;

    public void Burn()
    {
        if (burning) return;
        burning = true;

        if (burnEffectPrefab != null)
            Instantiate(burnEffectPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject, burnDelay);
    }
}

using UnityEngine;

public class BurningEffect : MonoBehaviour
{
    [SerializeField] ParticleSystem burningParticles;

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.GetComponent<FireHead>() != null)
        {
            burningParticles.Play();
        }
    }
}

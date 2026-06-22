using UnityEngine;

public class Basketball : MonoBehaviour
{
    [SerializeField] GameObject movingPlatform;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<FireHead>() != null)
        {
            movingPlatform.GetComponent<MovingPlatform>().SetShouldMoving(true);
        }
    }
}

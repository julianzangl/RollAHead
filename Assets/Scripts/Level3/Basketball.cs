using UnityEngine;

public class Basketball : MonoBehaviour
{
    [SerializeField] GameObject movingPlatform;

    void OnTriggerEnter(Collider other)
    {
        if(!other.gameObject.CompareTag("Player")) return ;

        movingPlatform.GetComponent<MovingPlatform>().SetShouldMoving(true);
    }
}

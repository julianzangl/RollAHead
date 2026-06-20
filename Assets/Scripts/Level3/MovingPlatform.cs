using UnityEngine;
public class MovingPlatform : MonoBehaviour
{
    private bool shouldMoving = false;

    [SerializeField]
    private float platformSpeed;
    [SerializeField]
    private Vector3 start;
    [SerializeField]
    private Vector3 end;

    private Vector3 lastPosition;

    void FixedUpdate()
    {
        if (shouldMoving)
        {
            lastPosition = transform.position;

            float pingPong = Mathf.PingPong(Time.fixedTime * this.platformSpeed, 1.0f);

            var newPosition = Vector3.Lerp(this.start, this.end, pingPong);
            this.transform.localPosition = newPosition;
        }
    }

    public Vector3 GetVelocity()
    {
        if (shouldMoving)
        {
            return (transform.position - lastPosition) / Time.fixedDeltaTime;
        }
        else
        {
            return Vector3.zero;
        }
    }

    public void SetShouldMoving(bool shouldPlatformsMoving)
    {
        shouldMoving = shouldPlatformsMoving;
    }
}


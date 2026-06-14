using UnityEngine;

public class RespawnPoint : MonoBehaviour
{
    private static Vector3 currentPosition;
    private static bool hasCurrentPosition;

    public static Vector3 CurrentPosition => hasCurrentPosition ? currentPosition : Vector3.zero;

    void Awake()
    {
        SetCurrentPosition(transform.position);
    }

    public static void SetCurrentPosition(Vector3 position)
    {
        currentPosition = position;
        hasCurrentPosition = true;
    }
}

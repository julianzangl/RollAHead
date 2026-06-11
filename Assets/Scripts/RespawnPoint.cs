using UnityEngine;

public class RespawnPoint : MonoBehaviour
{
    private static RespawnPoint current;

    public static Vector3 CurrentPosition => current != null ? current.transform.position : Vector3.zero;

    void Awake()
    {
        current = this;
    }
}

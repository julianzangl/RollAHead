using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private Vector3 respawnOffset;
    [SerializeField] private bool hideOnCollect = true;

    private bool collected;

    void Awake()
    {
        Collider checkpointCollider = GetComponent<Collider>();
        if (checkpointCollider != null)
            checkpointCollider.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        TryCollect(other);
    }

    void OnTriggerStay(Collider other)
    {
        TryCollect(other);
    }

    private void TryCollect(Collider other)
    {
        if (collected || !IsActivator(other)) return;

        collected = true;
        RespawnPoint.SetCurrentPosition(transform.position + respawnOffset);

        if (hideOnCollect)
            gameObject.SetActive(false);
    }

    private bool IsActivator(Collider other)
    {
        return other.GetComponentInParent<Character>() != null;
    }
}

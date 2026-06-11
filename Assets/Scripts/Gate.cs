using UnityEngine;

public class Gate : MonoBehaviour
{
    [SerializeField] private Vector3 openOffset = Vector3.up * 4f;
    [SerializeField] private float openSpeed = 5f;
    [SerializeField] private bool deactivateOnOpen;

    private Vector3 closedPosition;
    private Vector3 openPosition;
    private bool isOpen;

    void Awake()
    {
        closedPosition = transform.position;
        openPosition = closedPosition + openOffset;
    }

    void Update()
    {
        if (!isOpen || deactivateOnOpen) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            openPosition,
            openSpeed * Time.deltaTime);
    }

    public void Open()
    {
        if (isOpen) return;

        isOpen = true;
        if (deactivateOnOpen)
            gameObject.SetActive(false);
    }
}

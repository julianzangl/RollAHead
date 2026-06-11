using System.Collections.Generic;
using UnityEngine;

public class BlockSnapPoint : MonoBehaviour
{
    private static readonly List<BlockSnapPoint> activePoints = new List<BlockSnapPoint>();

    public static IReadOnlyList<BlockSnapPoint> ActivePoints => activePoints;

    private PushableBlock occupyingBlock;

    public bool IsOccupied => occupyingBlock != null;

    void OnEnable()
    {
        if (!activePoints.Contains(this))
            activePoints.Add(this);
    }

    void OnDisable()
    {
        activePoints.Remove(this);
    }

    void Awake()
    {
        Collider snapCollider = GetComponent<Collider>();
        if (snapCollider != null)
            snapCollider.isTrigger = true;
    }

    public bool CanSnap(PushableBlock block)
    {
        return occupyingBlock == null || occupyingBlock == block;
    }

    public void Occupy(PushableBlock block)
    {
        occupyingBlock = block;
    }
}

using UnityEngine;

// Common contract for every thrown head type (normal, robot, slime, fire).
// Lets HeadThrow and world triggers (buttons, keys, respawn) treat any head uniformly.
public interface IThrowableHead
{
    void Initialize(Vector3 throwDirection, float throwForce, HeadThrow headThrow);
}

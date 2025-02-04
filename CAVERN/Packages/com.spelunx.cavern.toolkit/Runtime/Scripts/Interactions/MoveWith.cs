using UnityEngine;

public class MoveWith : MonoBehaviour
{
    public Transform targetToCopy;

    void Update()
    {
        transform.SetPositionAndRotation(targetToCopy.position, targetToCopy.rotation);
    }
}

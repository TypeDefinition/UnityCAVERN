using UnityEngine;

public class MoveWith : MonoBehaviour
{
    public Transform targetToCopy;

    void Update()
    {
        transform.position = targetToCopy.position;
    }
}

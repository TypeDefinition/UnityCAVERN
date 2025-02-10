using UnityEngine;

public class MotionMirrorer : MonoBehaviour
{
    [SerializeField] private Transform source; // The object to mirror (vivetracker)
    private Vector3 sourceOrigin;
    private Vector3 origin;

    void Start()
    {
        origin = transform.position;
        sourceOrigin = source.position;
    }

    void Update()
    {
        Vector3 relativeMovement = source.position - sourceOrigin;

        Vector3 mirroredMovement = new Vector3(relativeMovement.x, relativeMovement.y, -relativeMovement.z);

        transform.position = origin + mirroredMovement;

        transform.rotation = Quaternion.LookRotation(new Vector3(source.forward.x, source.forward.y, -source.forward.z));
    }
}

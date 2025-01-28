using UnityEngine;

public class MotionMirrorer : MonoBehaviour
{

    [SerializeField]
    private Transform source;
    private Vector3 sourceOrigin;

    private Vector3 origin;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        origin = transform.position;
        sourceOrigin = source.position;
    }
    void Update()
    {
        // Copy motion
        transform.position = sourceOrigin - source.position + origin;
        transform.localRotation = source.localRotation;
    }
}

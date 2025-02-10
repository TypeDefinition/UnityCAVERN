using UnityEngine;

public class EvasiveMotion : MonoBehaviour
{
    [SerializeField] private Transform source; // The transform the object reacts to
    [SerializeField] private float triggerDistance = 2.0f; // Distance at which it moves away
    [SerializeField] private float shyDistance = 3.0f; // How far it moves away
    [SerializeField] private float moveSpeed = 3.0f; // Speed of movement

    private Vector3 origin;

    void Start()
    {
        origin = transform.position; // Store the initial position
    }

    void Update()
    {
        float distance = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z),
                                          new Vector3(source.position.x, 0, source.position.z));

        if (distance < triggerDistance)
        {
            // Get the direction away from the source (XZ plane only)
            Vector3 directionAway = (new Vector3(transform.position.x, 0, transform.position.z) -
                                     new Vector3(source.position.x, 0, source.position.z)).normalized;

            // Calculate new target position, keeping Y the same
            Vector3 targetPosition = new Vector3(origin.x, transform.position.y, origin.z) + directionAway * shyDistance;

            // Move smoothly toward the target position
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        }
    }
}

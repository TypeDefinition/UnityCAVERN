using UnityEngine;

public class MovingCube : MonoBehaviour {
    public float zSpeed = 1.0f;
    public float xSpeed = 1.0f;
    Vector3 startPos;

    void Start() {
        startPos = transform.position;
    }

    void Update() {
        float x = Mathf.Sin(Time.timeSinceLevelLoad * xSpeed) * 10.0f;
        float z = Mathf.Sin(Time.timeSinceLevelLoad * zSpeed) * 10.0f;
        transform.position = startPos + new Vector3(x, 0.0f, z);
    }
}
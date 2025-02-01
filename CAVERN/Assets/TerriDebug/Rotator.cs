using UnityEngine;

public class Rotator : MonoBehaviour {
    public Vector3 angularVelocity = Vector3.zero;

    private void Start() {
    }

    private void Update() {
        transform.Rotate(angularVelocity * Time.deltaTime);
    }
}
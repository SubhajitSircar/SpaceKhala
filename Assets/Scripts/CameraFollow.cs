using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // Drag the player here
    public float smoothSpeed = 5f; // How quickly the camera catches up

    // The camera needs to sit back on the Z axis to see the 2D world
    public Vector3 offset = new Vector3(0f, 0f, -10f);

    // We use LateUpdate instead of Update for cameras
    void LateUpdate()
    {
        if (target != null)
        {
            // Where the camera wants to go
            Vector3 desiredPosition = target.position + offset;

            // Smoothly glide from current position to the desired position
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        }
    }
}
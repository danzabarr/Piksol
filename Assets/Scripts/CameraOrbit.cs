using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    public Transform target;

    public Vector3 pan;
    public float zoom;
    public float yaw;
    public float pitch;

    public Vector2 panSensitivity;
    public Vector2 rotateSensitivity;
    public float zoomSensitivity;
    public float zoomMin, zoomMax;

    private Vector3 follow;

    private void Update()
    {
        if (Input.GetMouseButton(2))
        {
            yaw += Input.GetAxis("Mouse X") * rotateSensitivity.x;
            pitch -= Input.GetAxis("Mouse Y") * rotateSensitivity.y;
        }

        if (Input.GetMouseButton(1))
        {
            pan -= transform.right * Input.GetAxis("Mouse X") * panSensitivity.x;
            pan -= transform.up * Input.GetAxis("Mouse Y") * panSensitivity.y;
        }

        zoom -= Input.mouseScrollDelta.y * zoomSensitivity;

        Validate();
    }

    private void OnValidate()
    {
        Validate();
    }

    private void Validate()
    {
        zoom = Mathf.Clamp(zoom, zoomMin, zoomMax);
        pitch = Mathf.Clamp(pitch, -89.99f, 89.99f);
        yaw = (yaw % 360 + 360) % 360;

        
        Vector3 position = pan;
        if (target)
        {
            follow = Vector3.Lerp(follow, target.position, Time.deltaTime * 5);
            position += follow;
        }

        Quaternion rotation = Quaternion.AngleAxis(yaw, Vector3.up) * Quaternion.AngleAxis(pitch, Vector3.right);
        position -= rotation * Vector3.forward * zoom;

        transform.position = position;
        transform.rotation = rotation;
    }
}

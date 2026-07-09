using UnityEngine;

public class Parallax : MonoBehaviour
{
    public Transform cameraTransform;

    [Range(0f, 1f)]
    public float parallaxEffect = 0.5f;
    public bool affectY;

    private Vector3 startPosition;
    private Vector3 startCameraPosition;

    private void Start()
    {
        if (cameraTransform == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
                cameraTransform = mainCamera.transform;
        }

        startPosition = transform.position;

        if (cameraTransform != null)
            startCameraPosition = cameraTransform.position;
    }

    private void LateUpdate()
    {
        if (cameraTransform == null)
            return;

        Vector3 cameraDelta = cameraTransform.position - startCameraPosition;
        float yOffset = affectY ? cameraDelta.y * parallaxEffect : 0f;

        transform.position = new Vector3(
            startPosition.x + cameraDelta.x * parallaxEffect,
            startPosition.y + yOffset,
            startPosition.z);
    }
}

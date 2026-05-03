using System;
using UnityEngine;

public class StartMenu : MonoBehaviour
{
    [SerializeField] Camera _camera;
    [SerializeField] Transform cameraView;
    [SerializeField] float followSpeed = 10f;

    void Update()
    {
        // Look at camera on y axis only
        Vector3 lookDirection = cameraView.position - transform.position;
        lookDirection.x = 0;
        lookDirection.z = 0;
        transform.LookAt(lookDirection);

        // Lazy follow
        Vector3 targetPosition = cameraView.position;
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
    }
}
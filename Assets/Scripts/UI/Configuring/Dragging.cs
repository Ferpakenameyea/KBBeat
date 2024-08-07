using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class Dragging : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private new Camera camera;
    [SerializeField] private float sensitivity = 0.2f;
    [SerializeField] private float zoomSensitivity = 1f;
    private bool MouseInRange
    {
        get
        {
            var vector = this.camera.ScreenToViewportPoint(Input.mousePosition);
            return (vector.x >= 0 && vector.y >= 0 && vector.x <= 1 && vector.y <= 1);
        }
    }

    private void Start()
    {
        this.camera = GetComponent<Camera>();
    }

    private void Update()
    {
        if (MouseInRange)
        {
            if (Input.GetMouseButton(0))
            {
                var axisX = Input.GetAxis("Mouse X") * sensitivity;
                var axisY = Input.GetAxis("Mouse Y") * sensitivity;

                this.target.transform.Rotate(Vector3.down, axisX);
                this.target.transform.Rotate(Vector3.right, axisY);
            }
            var wheel = Input.GetAxis("Mouse ScrollWheel") * zoomSensitivity;
            this.camera.fieldOfView -= wheel;
        } 
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControls : MonoBehaviour
{
    public float mouseSpeed = 1;
    public float moveSpeed = 0.5f;

    float xRot, yRot;

    private bool pausedMovement;

    void Start()
    {
        Vector3 currentRot = transform.rotation.eulerAngles;
        xRot = currentRot.x;
        yRot = currentRot.y;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && !pausedMovement) pausedMovement = true;
        else if(Input.GetKeyDown(KeyCode.Tab) && pausedMovement) pausedMovement = false;
            
        if (pausedMovement) return;

        float mx = Input.GetAxis("Mouse X");
        float my = Input.GetAxis("Mouse Y");

        xRot -= my * mouseSpeed;
        yRot += mx * mouseSpeed;

        transform.rotation = Quaternion.Euler(xRot, yRot, 0);

        float dx = Input.GetAxis("Horizontal");
        float dy = Input.GetAxis("Vertical");
        float dz = (Input.GetKey(KeyCode.Q) ? -1 : 0) + (Input.GetKey(KeyCode.E) ? 1 : 0);
        transform.position +=
            (transform.forward * (moveSpeed * dy) + transform.right * (moveSpeed * dx) +
             transform.up * (moveSpeed * dz)) * Time.deltaTime;
    }
}
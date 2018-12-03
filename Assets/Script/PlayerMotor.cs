using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMotor : MonoBehaviour
{
    public Camera cam;
    private Vector3 velocity = Vector3.zero;
    private Vector3 rotation = Vector3.zero;
    private float camRotationX = 0f;
    private float currentCamRotationX = 0f;
    private Vector3 thrusterForce = Vector3.zero;

    public float camRotationLimit = 85f;
    public float camRotationLimitNegativ = 40f;


    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    private void FixedUpdate()
    {
        PerformMovement();
        PerformRotation();
    }


    internal void Move(Vector3 _velocity)
    {
        velocity = _velocity;
    }

    private void PerformMovement()
    {
        if (velocity != Vector3.zero)
        {
            rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
        }

        if (thrusterForce != Vector3.zero)
        {
            rb.AddForce(thrusterForce * Time.fixedDeltaTime, ForceMode.Acceleration);
        }
    }

    internal void Rotate(Vector3 _rotation)
    {
        rotation = _rotation;
    }
    private void PerformRotation()
    {
        rb.MoveRotation(rb.rotation * Quaternion.Euler(rotation));
        if (cam != null)
        {
            currentCamRotationX -= camRotationX;
            currentCamRotationX = Mathf.Clamp(currentCamRotationX, -camRotationLimit, camRotationLimitNegativ);

            cam.transform.localEulerAngles = new Vector3(currentCamRotationX, 0f, 0f);

        }
    }

    internal void RotateCamera(float _camRotationX)
    {
        camRotationX = _camRotationX;
    }

}

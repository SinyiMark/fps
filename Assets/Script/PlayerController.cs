using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerMotor))]
public class PlayerController : MonoBehaviour {

    public float speed = 5f;
    public float rotationSpeed = 3f;
    public float thrusterForce = 1000f;

    [SerializeField]
    public Animator animator;

    [Header("Spring settings:")]

    private PlayerMotor motor;
    private ConfigurableJoint joint;

	void Start ()
    {
        motor = GetComponent<PlayerMotor>();
    }
	
	void Update ()
    {

        #region Movement
        float xMov = Input.GetAxisRaw("Horizontal");
        float zMov = Input.GetAxisRaw("Vertical");
        Vector3 moveHorizontal = transform.right * xMov;
        Vector3 moveVertical = transform.forward * zMov;
        
        Vector3 velocity = (moveHorizontal + moveVertical).normalized * speed;

        if (Input.GetKey(KeyCode.W))
        {
            animator.SetBool("MoveBack", false);
            animator.SetBool("MoveForvard", true);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            animator.SetBool("MoveForvard", false);
            animator.SetBool("MoveBack", true);
        }
        else
        {
            animator.SetBool("MoveForvard", false);
            animator.SetBool("MoveBack", false);
        }

        motor.Move(velocity);
        #endregion

        #region Rotation
        float yRot = Input.GetAxisRaw("Mouse X");
        Vector3 _rotation = new Vector3(0, yRot, 0) * rotationSpeed;

        motor.Rotate(_rotation);
        #endregion

        #region CamRotation
        float xRot = Input.GetAxisRaw("Mouse Y");
        float _camRotationX = xRot * rotationSpeed;

        motor.RotateCamera(_camRotationX);
        #endregion

    }
}

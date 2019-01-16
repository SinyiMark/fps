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

    public Rigidbody rigidbody;

    private bool CanDash = true;
    public float DashColdown = 3f;

    public int DashPower = 100;

	void Start ()
    {
        motor = GetComponent<PlayerMotor>();
    }
	
	void Update ()
    {
        if (PauseMenu.IsOn)
        {
            motor.Rotate(new Vector3(0, 0, 0));
            motor.RotateCamera(0);
            return;
        }


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

        if (Input.GetKeyDown(KeyCode.Space) && CanDash == true)
        {
            Dash(moveHorizontal, moveVertical);
        }

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
    private void Dash(Vector3 _hor, Vector3 _ver)
    {
        if (CanDash == true)
        {
            rigidbody.AddForce((_hor + _ver) * DashPower, ForceMode.Impulse);
            CanDash = false;
            StartCoroutine(DashColDown(DashColdown));
        }
    }

    IEnumerator DashColDown(float _coldown)
    {
        yield return new WaitForSeconds(_coldown);
        CanDash = true;
    }
}

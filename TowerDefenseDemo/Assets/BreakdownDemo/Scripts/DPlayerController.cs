using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DPlayerController : MonoBehaviour
{
    private CharacterController controller;
    public Vector3 playerVelocity;
    public bool groundedPlayer;
    public float playerSpeed = 2.0f;
    public float jumpHeight = 1.0f;
    public float jumpSpeed = -3.0f;
    public float gravityValue = -9.81f;

    private Vector2 turnAmount;
    public float turnSensitivity = 0.5f;

    //public float groundDelayCheck = 0;
    //private const float MAX_GROUND_DELAY = 0.2f;

    void Start()
    {
        controller = gameObject.GetComponent<CharacterController>();
        // Enable this if you are using the mouse for the input at all.
        //Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        /*if(controller.isGrounded)
        {
            groundDelayCheck = 0;
        } else
        {
            groundDelayCheck += Time.deltaTime;
        }
        groundedPlayer = groundDelayCheck < MAX_GROUND_DELAY;*/
        groundedPlayer = controller.isGrounded;

        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        turnUsingKeys();
        moveForward();

        // Changes the height position of the player..
        if (Input.GetButtonDown("Jump") && groundedPlayer)
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * jumpSpeed * gravityValue);
        }

        
        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
        
    }

    private void strafe()
    {
        // Strafing movement using left/right arrows and A/D
        controller.Move(gameObject.transform.right * Input.GetAxis("Horizontal") * Time.deltaTime * playerSpeed);
    }

    private void moveForward()
    {
        // Forward/Back Movement using up/down arrows and W/S
        controller.Move(gameObject.transform.forward * Input.GetAxis("Vertical") * Time.deltaTime * playerSpeed);
    }

    private void turnUsingKeys()
    {
        turnAmount.x += Input.GetAxis("Horizontal") * turnSensitivity * Time.deltaTime;
        transform.localRotation = Quaternion.Euler(-turnAmount.y, turnAmount.x, 0);
    }

    private void turnUsingMouse()
    {
        turnAmount.x += Input.GetAxis("Mouse X") * turnSensitivity * Time.deltaTime;
        transform.localRotation = Quaternion.Euler(-turnAmount.y, turnAmount.x, 0);
    }
}

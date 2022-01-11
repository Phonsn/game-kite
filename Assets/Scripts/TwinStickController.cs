using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(GunController))]
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class TwinStickController : MonoBehaviour
{
    [SerializeField] private float playerSpeed = 5f;
    [SerializeField] private float playerSprintSpeed = 15f;
    [SerializeField] private float playerSprintDuration = .5f;
    [SerializeField] private float playerSprintRechargeSpeed = 2f;

    [SerializeField] private float gravityValue = -9.18f;
    [SerializeField] private float controllerDeadzone = 0.1f;
    [SerializeField] private float gamepadRotateSmoothing = 1000f;

    [SerializeField] private bool isGamepad;

    private CharacterController playerController;
    private GunController gunController;

    private Vector2 movement;
    private Vector2 aim;

    private Vector3 playerVelocity;

    private InputMaster playerControls;
    private PlayerInput playerInput;

    //Sprint
    private float playerOriginalSpeed;
    private float sprintEndTime;
    private float nextSprintAvailable;
    private bool playerIsPressingSprint;
    private bool playerIsSprinting;
    private bool playerSprinted;

    private void Awake()
    {
        playerController = GetComponent<CharacterController>();
        gunController = GetComponent<GunController>();
        playerControls = new InputMaster();
        playerInput = GetComponent<PlayerInput>();

        playerOriginalSpeed = playerSpeed;
        playerIsPressingSprint = false;
        playerIsSprinting = false;
        playerSprinted = false;
        nextSprintAvailable = Time.time;
    }

    private void OnEnable()
    {
        playerControls.Enable();

        playerControls.Player.Shoot.started += ShootStart;
        playerControls.Player.Shoot.canceled += ShootStop;

        playerControls.Player.Sprint.started += SprintStart;
        playerControls.Player.Sprint.canceled += SprintStop;

        playerControls.Player.PlaceMine.performed += PlaceMine;
    }

    private void ShootStart(InputAction.CallbackContext obj)
    {
        gunController.OnTriggerHold();
    }

    private void ShootStop(InputAction.CallbackContext obj)
    {
        gunController.OnTriggerRelease();
    }

    private void PlaceMine(InputAction.CallbackContext obj)
    {
        gunController.PlaceMine();
    }

    private void SprintStart(InputAction.CallbackContext obj)
    {
        playerIsPressingSprint = true;
        if (Time.time >= nextSprintAvailable)
        {
            playerSprinted = true;
            sprintEndTime = Time.time + playerSprintDuration;
        } 
    }

    private void SprintStop(InputAction.CallbackContext obj)
    {
        playerIsPressingSprint = false;
        if(playerSprinted)
        {
            nextSprintAvailable = Time.time + playerSprintRechargeSpeed;
        }
        playerSprinted = false;
    }

    private void OnDisable()
    {
        playerControls.Disable();
        playerControls.Player.Shoot.started -= ShootStart;
        playerControls.Player.Shoot.canceled -= ShootStop;

        playerControls.Player.Sprint.started -= SprintStart;
        playerControls.Player.Sprint.canceled -= SprintStop;

        playerControls.Player.PlaceMine.performed -= PlaceMine;
    }

    void Update()
    {
        HandleInput();
        HandleSprint();
        HandleMovement();
        HandleRotation();
    }

    private void HandleInput()
    {
        movement = playerControls.Player.Movement.ReadValue<Vector2>();
        aim = playerControls.Player.Aim.ReadValue<Vector2>();
    }

    private void HandleSprint()
    {
        //Check if sprinting should be active
        if (playerIsPressingSprint && Time.time <= sprintEndTime)
        {
            playerIsSprinting = true;
        } else if (playerIsPressingSprint && Time.time > sprintEndTime)
        {
            nextSprintAvailable = sprintEndTime + playerSprintRechargeSpeed;
            playerIsSprinting = false;
        } else
        {
            playerIsSprinting = false;
        }

        // Adjust speed
        if (playerIsSprinting)
        {
            playerSpeed = playerSprintSpeed;
            gameObject.GetComponent<Player>().ControlTrailRenderer(true);
        } else
        {
            playerSpeed = playerOriginalSpeed;
            gameObject.GetComponent<Player>().ControlTrailRenderer(false);
        }
    }

    private void HandleMovement()
    {
        Vector3 move = new Vector3(movement.x, 0, movement.y);
        playerController.Move(move * Time.deltaTime * playerSpeed);

        playerVelocity.y += gravityValue * Time.deltaTime;
        playerController.Move(playerVelocity * Time.deltaTime);
    }

    private void HandleRotation()
    {
        if (isGamepad)
        {
            if (Mathf.Abs(aim.x)>controllerDeadzone || Mathf.Abs(aim.y) > controllerDeadzone)
            {
                Vector3 playerDirection = Vector3.right * aim.x + Vector3.forward * aim.y;
                if (playerDirection.sqrMagnitude > 0.0f)
                {
                    Quaternion newRotation = Quaternion.LookRotation(playerDirection, Vector3.up);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, newRotation, gamepadRotateSmoothing * Time.deltaTime);
                }
            }
        }
        else
        {
            Ray ray = Camera.main.ScreenPointToRay(aim);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            float rayDistance;

            if (groundPlane.Raycast(ray, out rayDistance))
            {
                Vector3 point = ray.GetPoint(rayDistance);
                LookAt(point);
            }
        }
    }

    private void LookAt(Vector3 lookPoint)
    {
        Vector3 heightCorrectedPoint = new Vector3(lookPoint.x, transform.position.y, lookPoint.z);
        transform.LookAt(heightCorrectedPoint);
    }

    public void OnDeviceChange (PlayerInput pi)
    {
        isGamepad = pi.currentControlScheme.Equals("Gamepad") ? true : false;
        
    }
}

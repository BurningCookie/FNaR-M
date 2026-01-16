using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Unity.Netcode;

public class FirstPersonController : NetworkBehaviour
{
    [Header("Movement Speeds")]
    [SerializeField] private float walkSpeed = 3.0f;
    [SerializeField] private float sprintMultiplier = 2.0f;

    [Header("Jump Parameters")]
    [SerializeField] private float jumpForce = 5.0f;
    [SerializeField] private float gravityMultiplier = 1.0f;
    [SerializeField] private bool canJump = true;

    [Header("Look Parameters")]
    [SerializeField] private float lookSensitivity = 0.1f;
    [SerializeField] private float maxLookAngle = 80.0f;

    [Header("References")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private PlayerInput playerInput;

    private Vector3 currentMovement;
    private float verticalRoatation;
    private float CurrentSpeed => walkSpeed * (playerInput.SprintTriggered ? sprintMultiplier : 1.0f);

    public override void OnNetworkSpawn()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (!IsOwner)
        {
            mainCamera.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (!IsOwner) return;

        HanldeMovement();
        HandleRotation();
        HandleInteraction();
    }

    private Vector3 CalculateWorldDirection()
    {
        Vector3 inputDirection = new Vector3(playerInput.MovementInput.x, 0, playerInput.MovementInput.y);
        Vector3 worldDirection = transform.TransformDirection(inputDirection);
        return worldDirection.normalized;
    }

    private void HandleJumping()
    {
        // Apply gravity every frame
        currentMovement.y += Physics.gravity.y * gravityMultiplier * Time.deltaTime;

        // If grounded, jump is enabled, and jump is pressed this frame, apply jump force
        if (characterController.isGrounded && canJump && playerInput.JumpPressedThisFrame)
        {
            currentMovement.y = jumpForce;
        }
    }

    private void HanldeMovement()
    {
        Vector3 worldDirection = CalculateWorldDirection();
        currentMovement.x = worldDirection.x * CurrentSpeed;
        currentMovement.z = worldDirection.z * CurrentSpeed;

        HandleJumping();
        characterController.Move(currentMovement * Time.deltaTime);
    }

    private void ApplyHorizontalRotation(float rotationAmount)
    {
        transform.Rotate(0, rotationAmount, 0);
    }

    private void ApplyVerticalRotation(float rotationAmount)
    {
        verticalRoatation = Mathf.Clamp(verticalRoatation - rotationAmount, -maxLookAngle, maxLookAngle);
        mainCamera.transform.localRotation = Quaternion.Euler(verticalRoatation, 0, 0);
    }

    private void HandleRotation()
    {
        float mouseXRotation = playerInput.RotationInput.x * lookSensitivity;
        float mouseYRotation = playerInput.RotationInput.y * lookSensitivity;
        ApplyHorizontalRotation(mouseXRotation);
        ApplyVerticalRotation(mouseYRotation);
    }

    private void HandleInteraction()
    {
        if (playerInput.InteractPressedThisFrame)
        {
            Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            if (Physics.Raycast(ray, out RaycastHit hit, 5f))
            {
                if (hit.collider.CompareTag("interactable"))
                {
                    hit.collider.SendMessage("Interact", SendMessageOptions.DontRequireReceiver);
                }
            }
        }
    }
}
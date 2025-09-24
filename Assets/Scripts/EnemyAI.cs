using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterController))]
public class EnemyAI : MonoBehaviour
{
    [Header("Patrol Settings")]
    [SerializeField] private List<Transform> patrolPoints = new List<Transform>();
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float waitTimeAtPoints = 3f;
    [SerializeField] private float reachedPointDistance = 0.5f;

    [Header("Detection Settings")]
    [SerializeField] private float playerFocusTime = 2f; // Time to keep looking after player leaves

    [Header("Pan Settings")]
    [SerializeField] private float panAngle = 90f;
    [SerializeField] private float panSpeed = 1f;
    [SerializeField] private bool panAtStops = true;

    [Header("Flashlight Settings")]
    [SerializeField] private Transform flashlightTransform;
    [SerializeField] private float flashlightConeAngle = 45f;
    [SerializeField] private float flashlightConeRange = 15f;
    [SerializeField] private float flashlightDamageCooldown = 1f;
    [SerializeField] private int flashlightDamage = 1;

    [Header("Gravity Settings")]
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float gravityMultiplier = 2f;

    // Private variables
    private CharacterController characterController;
    private Player playerController;
    private int currentPatrolIndex = 0;
    private float waitTimer = 0f;
    private float verticalVelocity = 0f;
    private float flashlightDamageTimer = 0f;
    private float playerFocusTimer = 0f;
    private Quaternion originalRotation;
    private Quaternion targetPlayerRotation;
    private bool isWaiting = false;
    private bool isPanning = false;
    private bool isPlayerDetected = false;
    private bool wasPlayerDetected = false;

    // Enemy states
    private enum EnemyState { Patrolling, Waiting, Alert, Focusing }
    private EnemyState currentState = EnemyState.Patrolling;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        originalRotation = transform.rotation;

        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerController = playerObj.GetComponent<Player>();
        }

        // Start at first patrol point if available
        if (patrolPoints.Count > 0)
        {
            transform.position = patrolPoints[0].position;
        }
    }

    void Update()
    {
        CheckForPlayer();
        HandleState();
        HandleFlashlightDamage();
        ApplyGravity();
    }

    private void CheckForPlayer()
    {
        wasPlayerDetected = isPlayerDetected;
        isPlayerDetected = IsPlayerInFlashlightCone();

        // If player just entered detection
        if (isPlayerDetected && !wasPlayerDetected)
        {
            currentState = EnemyState.Alert;
            playerFocusTimer = playerFocusTime;
            StopAllMovement();
        }
        // If player just left detection
        else if (!isPlayerDetected && wasPlayerDetected)
        {
            currentState = EnemyState.Focusing;
        }
    }

    private void HandleState()
    {
        switch (currentState)
        {
            case EnemyState.Patrolling:
                HandlePatrol();
                break;

            case EnemyState.Waiting:
                HandleWaiting();
                break;

            case EnemyState.Alert:
                HandleAlert();
                break;

            case EnemyState.Focusing:
                HandleFocusing();
                break;
        }
    }

    private void HandlePatrol()
    {
        if (patrolPoints.Count == 0) return;

        Vector3 targetPosition = patrolPoints[currentPatrolIndex].position;
        Vector3 direction = (targetPosition - transform.position).normalized;

        // Move horizontally only
        Vector3 move = new Vector3(direction.x, 0, direction.z) * moveSpeed;
        move.y = verticalVelocity;

        characterController.Move(move * Time.deltaTime);

        // Rotate to face movement direction
        if (direction.magnitude > 0.1f)
        {
            RotateTowards(direction);
        }

        // Check if reached patrol point
        if (Vector3.Distance(transform.position, targetPosition) <= reachedPointDistance)
        {
            currentState = EnemyState.Waiting;
            waitTimer = waitTimeAtPoints;
            isPanning = panAtStops;
            originalRotation = transform.rotation;
        }
    }

    private void HandleWaiting()
    {
        waitTimer -= Time.deltaTime;

        // Handle panning while waiting
        if (panAtStops && isPanning)
        {
            HandlePanning();
        }

        if (waitTimer <= 0f)
        {
            currentState = EnemyState.Patrolling;
            isPanning = false;
            MoveToNextPoint();
        }
    }

    private void HandleAlert()
    {
        // Stop all movement and focus on player
        if (playerController != null)
        {
            Vector3 directionToPlayer = (playerController.transform.position - transform.position).normalized;
            directionToPlayer.y = 0;
            RotateTowards(directionToPlayer);
        }
    }

    private void HandleFocusing()
    {
        // Keep looking at last known player position for a moment
        playerFocusTimer -= Time.deltaTime;

        if (playerFocusTimer <= 0f)
        {
            // Return to previous state
            if (isWaiting)
            {
                currentState = EnemyState.Waiting;
            }
            else
            {
                currentState = EnemyState.Patrolling;
            }
        }
    }

    private void StopAllMovement()
    {
        // Stop any current movement or waiting
        isWaiting = false;
        isPanning = false;
    }

    private void HandlePanning()
    {
        float pan = Mathf.Sin(Time.time * panSpeed) * panAngle;
        Quaternion targetRotation = Quaternion.Euler(0, pan, 0) * originalRotation;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, panSpeed * 100f * Time.deltaTime);
    }

    private void MoveToNextPoint()
    {
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Count;
    }

    private void RotateTowards(Vector3 direction)
    {
        direction.y = 0;
        if (direction.sqrMagnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 180f * Time.deltaTime);
        }
    }

    private void HandleFlashlightDamage()
    {
        // Only damage player if actively detected (in Alert state)
        if (currentState == EnemyState.Alert && IsPlayerInFlashlightCone() && playerController != null)
        {
            flashlightDamageTimer -= Time.deltaTime;
            if (flashlightDamageTimer <= 0f)
            {
                playerController.TakeDamage(flashlightDamage, "flashlight");
                flashlightDamageTimer = flashlightDamageCooldown;
            }
        }
        else
        {
            flashlightDamageTimer = 0f;
        }
    }

    private void ApplyGravity()
    {
        if (characterController.isGrounded)
            verticalVelocity = -1f;
        else
            verticalVelocity += gravity * gravityMultiplier * Time.deltaTime;
    }

    private bool IsPlayerInFlashlightCone()
    {
        if (flashlightTransform == null || playerController == null) return false;

        Vector3 toPlayer = (playerController.transform.position - flashlightTransform.position);
        float distance = toPlayer.magnitude;
        if (distance > flashlightConeRange) return false;

        float angle = Vector3.Angle(flashlightTransform.forward, toPlayer.normalized);
        return angle < flashlightConeAngle * 0.5f;
    }

    // Visual indicator of state (optional)
    private void OnDrawGizmos()
    {
        // Draw state indicator
        Gizmos.color = currentState switch
        {
            EnemyState.Alert => Color.red,
            EnemyState.Focusing => Color.yellow,
            EnemyState.Waiting => Color.blue,
            _ => Color.green
        };
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 2f, 0.3f);
    }

    private void OnDrawGizmosSelected()
    {
        // Draw patrol path
        if (patrolPoints.Count > 0)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < patrolPoints.Count; i++)
            {
                if (patrolPoints[i] == null) continue;

                Gizmos.DrawSphere(patrolPoints[i].position, 0.3f);
                if (i < patrolPoints.Count - 1 && patrolPoints[i + 1] != null)
                {
                    Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[i + 1].position);
                }
                else if (patrolPoints.Count > 1 && patrolPoints[0] != null)
                {
                    Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[0].position);
                }
            }
        }

        // Draw flashlight cone
        if (flashlightTransform != null)
        {
            Gizmos.color = isPlayerDetected ? Color.red : Color.yellow;
            Vector3 forward = flashlightTransform.forward * flashlightConeRange;
            Gizmos.DrawRay(flashlightTransform.position, forward);

            Vector3 left = Quaternion.Euler(0, -flashlightConeAngle * 0.5f, 0) * forward;
            Vector3 right = Quaternion.Euler(0, flashlightConeAngle * 0.5f, 0) * forward;

            Gizmos.DrawRay(flashlightTransform.position, left);
            Gizmos.DrawRay(flashlightTransform.position, right);
        }
    }
}
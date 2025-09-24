using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHP = 10;
    private int currentHP;

    [Header("Visual Settings")]
    [SerializeField] private SkinnedMeshRenderer[] meshRenderers;
    [SerializeField] private float dissolveSpeed = 1f;
    private float dissolveValue = 1f;
    private bool isDissolving = false;

    [Header("Movement Settings")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float rotationSpeed = 500f;

    [Header("Gravity Settings")]
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float gravityMultiplier = 3.0f;
    [SerializeField] private float glideMultiplier = 0.5f;
    [SerializeField] private float maxFallSpeed = -3f;

    [Header("Respawn Settings")]
    [SerializeField] private float respawnDelay = 2f;

    [Header("Ghost Light Settings")]
    [SerializeField] private Light ghostLight;

    [Header("Damage Settings")]
    [SerializeField] private float hitStunDuration = 0.5f;
    [SerializeField] private float damageCooldown = 1.5f;

    [Header("Damage Slow Settings")]
    [SerializeField] private float movementSlowFactor = 0.4f;
    [SerializeField] private float slowDuration = 2f;

    // Components
    private CharacterController controller;
    private Animator animator;
    private Camera mainCamera;

    // Movement variables
    private Vector2 input;
    private Vector3 direction;
    private float currentSpeed;
    private float verticalVelocity;

    // State variables
    private float hitStunTimer = 0f;
    private float damageCooldownTimer = 0f;
    private float slowTimer = 0f;
    private float respawnTimer;
    private bool isInHitStun = false;
    private bool isSlowed = false;
    private bool waitingForRespawn = false;
    private float originalSpeed;

    // Animation hashes
    private static readonly int SurprisedState = Animator.StringToHash("Base Layer.surprised");
    private static readonly int DissolveState = Animator.StringToHash("Base Layer.dissolve");

    public bool IsMovementLocked => isDissolving || waitingForRespawn || isInHitStun;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        mainCamera = Camera.main;
        currentHP = maxHP;
    }

    private void Start()
    {
        originalSpeed = speed;
    }

    private void Update()
    {
        HandleTimers();

        if (!IsMovementLocked)
        {
            HandleRotation();
            HandleGravity();
            HandleMovement();
        }

        HandleDissolve();
        HandleRespawn();
    }

    #region Input Handler
    public void OnMove(InputAction.CallbackContext context)
    {
        if (IsMovementLocked)
        {
            input = Vector2.zero;
            return;
        }
        input = context.ReadValue<Vector2>();
    }
    #endregion

    #region Movement
    private void HandleRotation()
    {
        if (input.sqrMagnitude == 0) return;

        direction = Quaternion.Euler(0.0f, mainCamera.transform.eulerAngles.y, 0.0f)
                   * new Vector3(input.x, 0.0f, input.y);

        Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation,
                            rotationSpeed * Time.deltaTime);
    }

    private void HandleGravity()
    {
        float currentGravityMultiplier = controller.isGrounded ? gravityMultiplier : glideMultiplier;

        if (controller.isGrounded && verticalVelocity < 0.0f)
            verticalVelocity = -1.0f;
        else
            verticalVelocity += gravity * currentGravityMultiplier * Time.deltaTime;

        verticalVelocity = Mathf.Max(verticalVelocity, maxFallSpeed);
        direction.y = verticalVelocity;
    }

    private void HandleMovement()
    {
        if (input.sqrMagnitude < 0.1f)
        {
            currentSpeed = 0f;
            return;
        }

        float targetSpeed = speed;
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.deltaTime);

        Vector3 movement = transform.forward * currentSpeed * Time.deltaTime;
        movement.y = verticalVelocity * Time.deltaTime;

        controller.Move(movement);
    }
    #endregion

    #region Health & Damage
    public void TakeDamage(int damage, string damageSource = "unknown")
    {
        if (damageCooldownTimer > 0f) return;
        if (isDissolving || waitingForRespawn) return;

        currentHP = Mathf.Max(0, currentHP - damage);

        // Flash floating health bar
        FloatingHealthBar healthBar = GetComponentInChildren<FloatingHealthBar>();
        if (healthBar != null)
        {
            healthBar.FlashHealthBar();
        }

        // Clear movement inputs
        input = Vector2.zero;
        direction = Vector3.zero;
        currentSpeed = 0f;

        // Start timers
        hitStunTimer = hitStunDuration;
        damageCooldownTimer = damageCooldown;
        slowTimer = slowDuration;
        isInHitStun = true;
        isSlowed = true;
        speed = originalSpeed * movementSlowFactor;

        // Play animation
        animator.CrossFade(SurprisedState, 0.1f, 0, 0);

        if (currentHP <= 0)
        {
            StartDissolve();
        }
    }

    public int GetCurrentHealth()
    {
        return currentHP;
    }

    public int GetMaxHealth()
    {
        return maxHP;
    }
    #endregion

    #region Timers & Status Effects
    private void HandleTimers()
    {
        // Hit stun timer
        if (hitStunTimer > 0f)
        {
            hitStunTimer -= Time.deltaTime;
            if (hitStunTimer <= 0f)
            {
                isInHitStun = false;
            }
        }

        // Damage cooldown timer
        if (damageCooldownTimer > 0f)
        {
            damageCooldownTimer -= Time.deltaTime;
        }

        // Slow timer
        if (slowTimer > 0f)
        {
            slowTimer -= Time.deltaTime;
            if (slowTimer <= 0f)
            {
                ResetMovementSpeed();
            }
        }
    }

    private void ResetMovementSpeed()
    {
        if (isSlowed)
        {
            speed = originalSpeed;
            isSlowed = false;
        }
    }
    #endregion

    #region Dissolve & Respawn
    private void StartDissolve()
    {
        animator.CrossFade(DissolveState, 0.1f, 0, 0);
        isDissolving = true;
        direction = Vector3.zero;
        verticalVelocity = 0f;
        currentSpeed = 0f;

        // Reset movement speed if slowed
        ResetMovementSpeed();

        // Destroy the ghost light
        if (ghostLight != null)
        {
            Destroy(ghostLight.gameObject);
        }

        // Hide the health bar immediately
        FloatingHealthBar healthBar = GetComponentInChildren<FloatingHealthBar>();
        if (healthBar != null && healthBar.canvasGroup != null)
        {
            healthBar.canvasGroup.alpha = 0f; // Instantly hide
        }

        // Alternative: Disable the entire canvas if you prefer
        Canvas healthBarCanvas = GetComponentInChildren<Canvas>();
        if (healthBarCanvas != null)
        {
            healthBarCanvas.enabled = false;
        }
    }

    private void HandleDissolve()
    {
        if (!isDissolving) return;

        dissolveValue -= dissolveSpeed * Time.deltaTime;
        foreach (var renderer in meshRenderers)
        {
            renderer.material.SetFloat("_Dissolve", dissolveValue);
        }

        if (dissolveValue <= 0 && !waitingForRespawn)
        {
            waitingForRespawn = true;
            respawnTimer = respawnDelay;
        }
    }

    private void HandleRespawn()
    {
        if (!waitingForRespawn) return;

        respawnTimer -= Time.deltaTime;
        if (respawnTimer <= 0f)
        {
            Respawn();
        }
    }

    private void Respawn()
    {
        ResetMovementSpeed();
        isInHitStun = false;
        hitStunTimer = 0f;
        damageCooldownTimer = 0f;
        slowTimer = 0f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    #endregion

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("Enemy"))
        {
            TakeDamage(1, "enemy_collision");
        }
    }
}
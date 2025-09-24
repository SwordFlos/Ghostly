using UnityEngine;

public class StaticLightDamage : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private int damageAmount = 1;
    [SerializeField] private float damageCooldown = 1f;
    [SerializeField] private bool isActive = true;

    [Header("Visual Settings")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private Color gizmoColor = new Color(1f, 0.5f, 0f, 0.3f); // Orange

    private SphereCollider damageCollider;
    private float damageTimer = 0f;

    void Start()
    {
        // Get or add Sphere Collider
        damageCollider = GetComponent<SphereCollider>();
        if (damageCollider == null)
        {
            damageCollider = gameObject.AddComponent<SphereCollider>();
            damageCollider.radius = 3f; // Default radius
        }

        damageCollider.isTrigger = true; // Ensure it's a trigger
    }

    void Update()
    {
        if (damageTimer > 0f)
        {
            damageTimer -= Time.deltaTime;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (!isActive || damageTimer > 0f) return;

        // Check if the colliding object is the player
        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(damageAmount, "static_light");
                damageTimer = damageCooldown;

                // Optional: Add visual/audio feedback
                Debug.Log($"Player took {damageAmount} damage from {gameObject.name}");
            }
        }
    }

    // Public methods to control the light
    public void SetActive(bool active)
    {
        isActive = active;
    }

    public void ToggleActive()
    {
        isActive = !isActive;
    }

    public void SetDamage(int newDamage)
    {
        damageAmount = newDamage;
    }

    public void SetRadius(float newRadius)
    {
        if (damageCollider != null)
        {
            damageCollider.radius = newRadius;
        }
    }

    // Visual debugging in Scene view
    void OnDrawGizmos()
    {
        if (!showGizmos) return;

        // Use the collider's radius if available, otherwise use a default
        float radius = damageCollider != null ? damageCollider.radius : 3f;

        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(transform.position, radius);

        // Draw a wireframe outline too
        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 1f);
        Gizmos.DrawWireSphere(transform.position, radius);

        // Draw an icon to make it easily identifiable
        Gizmos.DrawIcon(transform.position + Vector3.up * (radius + 0.5f), "Light Gizmo", true);
    }

    void OnDrawGizmosSelected()
    {
        float radius = damageCollider != null ? damageCollider.radius : 3f;

        // Draw a more visible version when selected
        Gizmos.color = new Color(1f, 0.3f, 0f, 0.5f);
        Gizmos.DrawSphere(transform.position, radius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);

        // Draw direction indicator
        Gizmos.DrawLine(transform.position, transform.position + transform.up * 1.5f);
    }
}
using System.Collections;
using UnityEngine;

/// <summary>
/// PlayerController — Todo lo relacionado con Mario/Jumpman.
///
/// Características implementadas:
///   • Movimiento horizontal
///   • Salto con verificación de suelo
///   • Subir / bajar escaleras
///   • Recoger martillo y golpear barriles
///   • Recibir daño (con invulnerabilidad temporal)
///   • Muerte y llamada al GameManager
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CapsuleCollider2D))]
public class PlayerController : MonoBehaviour
{
    // ──────────────────────────────────────────────
    // INSPECTOR — MOVIMIENTO
    // ──────────────────────────────────────────────
    [Header("Movimiento")]
    [Tooltip("Velocidad horizontal de desplazamiento")]
    public float moveSpeed = 4f;

    [Tooltip("Fuerza del salto")]
    public float jumpForce = 10f;

    [Tooltip("Layer que Unity usa para las plataformas (suelo)")]
    public LayerMask groundLayer;

    [Tooltip("Punto en los pies del personaje para detectar suelo")]
    public Transform groundCheck;

    [Tooltip("Radio del círculo de detección de suelo")]
    public float groundCheckRadius = 0.15f;

    // ──────────────────────────────────────────────
    // INSPECTOR — ESCALERAS
    // ──────────────────────────────────────────────
    [Header("Escaleras")]
    [Tooltip("Velocidad de movimiento en escalera")]
    public float climbSpeed = 3f;

    // ──────────────────────────────────────────────
    // INSPECTOR — MARTILLO
    // ──────────────────────────────────────────────
    [Header("Martillo")]
    [Tooltip("Tiempo en segundos que dura el efecto del martillo")]
    public float hammerDuration = 10f;

    [Tooltip("Área de golpe del martillo (BoxCollider2D trigger)")]
    public Transform hammerHitArea;

    [Tooltip("Radio del golpe del martillo")]
    public float hammerHitRadius = 0.5f;

    [Tooltip("Layer de los barriles para detectar golpes")]
    public LayerMask barrelLayer;

    // ──────────────────────────────────────────────
    // INSPECTOR — VIDA
    // ──────────────────────────────────────────────
    [Header("Vida")]
    [Tooltip("Tiempo de invulnerabilidad tras recibir daño")]
    public float invulnerabilityTime = 1.5f;

    // ──────────────────────────────────────────────
    // ESTADO INTERNO
    // ──────────────────────────────────────────────
    private Rigidbody2D rb;
    private Animator anim;

    // Suelo / salto
    private bool isGrounded;

    // Escaleras
    private bool isOnLadder;
    private bool isClimbing;
    private float originalGravity;

    // Martillo
    private bool hasHammer;
    private float hammerTimer;

    // Daño
    private bool isInvulnerable;
    private bool isDead;

    // Input
    private float horizontalInput;
    private float verticalInput;

    // ──────────────────────────────────────────────
    // UNITY LIFECYCLE
    // ──────────────────────────────────────────────
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        originalGravity = rb.gravityScale;
    }

    private void Update()
    {
        if (isDead) return;

        ReadInput();
        CheckGround();
        HandleJump();
        HandleHammer();
        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        if (isClimbing)
            HandleClimbing();
        else
            HandleHorizontalMovement();
    }

    // ──────────────────────────────────────────────
    // INPUT
    // ──────────────────────────────────────────────
    private void ReadInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal"); // -1, 0, 1
        verticalInput = Input.GetAxisRaw("Vertical");   // -1, 0, 1
    }

    // ──────────────────────────────────────────────
    // SUELO
    // ──────────────────────────────────────────────
    private void CheckGround()
    {
        // Dibuja un pequeño círculo en los pies y detecta la capa "Ground"
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    // ──────────────────────────────────────────────
    // MOVIMIENTO HORIZONTAL
    // ──────────────────────────────────────────────
    private void HandleHorizontalMovement()
    {
        // Sólo mueve horizontalmente si no está en escalera
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);

        // Voltear sprite según dirección
        if (horizontalInput > 0)
            transform.localScale = new Vector3(1, 1, 1);
        else if (horizontalInput < 0)
            transform.localScale = new Vector3(-1, 1, 1);
    }

    // ──────────────────────────────────────────────
    // SALTO
    // ──────────────────────────────────────────────
    private void HandleJump()
    {
        // Sólo puede saltar si está en el suelo y NO en escalera
        if (Input.GetButtonDown("Jump") && isGrounded && !isClimbing)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            anim.SetTrigger("Jump");
        }
    }

    // ──────────────────────────────────────────────
    // ESCALERAS
    // ──────────────────────────────────────────────

    /// <summary>Llamado por LadderZone al entrar en zona de escalera.</summary>
    public void EnterLadder()
    {
        isOnLadder = true;
    }

    /// <summary>Llamado por LadderZone al salir de zona de escalera.</summary>
    public void ExitLadder()
    {
        isOnLadder = false;
        StopClimbing();
    }

    private void HandleClimbing()
    {
        // Si hay input vertical y estamos en la escalera → trepar
        if (isOnLadder && Mathf.Abs(verticalInput) > 0.1f)
        {
            StartClimbing();
            rb.linearVelocity = new Vector2(0f, verticalInput * climbSpeed);
        }
        else if (isClimbing)
        {
            // Parado en escalera: sin gravedad pero quieto
            rb.linearVelocity = new Vector2(0f, 0f);
        }
    }

    private void StartClimbing()
    {
        if (!isClimbing)
        {
            isClimbing = true;
            rb.gravityScale = 0f; // Desactivar gravedad al trepar
        }
    }

    private void StopClimbing()
    {
        isClimbing = false;
        rb.gravityScale = originalGravity; // Restaurar gravedad
    }

    // Si el jugador pulsa un botón de movimiento horizontal en la escalera,
    // abandona la escalera (comportamiento arcade clásico)
    private void Update_LadderExit()
    {
        if (isClimbing && isGrounded && Mathf.Abs(horizontalInput) > 0.1f)
            StopClimbing();
    }

    // ──────────────────────────────────────────────
    // MARTILLO
    // ──────────────────────────────────────────────
    private void HandleHammer()
    {
        if (!hasHammer) return;

        hammerTimer -= Time.deltaTime;

        // Golpe al presionar "Fire1" (Ctrl izquierdo o botón A en gamepad)
        if (Input.GetButtonDown("Fire1"))
        {
            HammerSwing();
        }

        // El tiempo del martillo expiró
        if (hammerTimer <= 0f)
        {
            hasHammer = false;
            anim.SetBool("HasHammer", false);
        }
    }

    private void HammerSwing()
    {
        anim.SetTrigger("HammerSwing");

        // Detectar barriles en el área de golpe
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            hammerHitArea.position, hammerHitRadius, barrelLayer);

        foreach (Collider2D hit in hits)
        {
            Barrel barrel = hit.GetComponent<Barrel>();
            if (barrel != null)
                barrel.DestroyBarrel();
        }
    }

    /// <summary>Llamado por el trigger del Martillo (PickupHammer.cs).</summary>
    public void PickUpHammer()
    {
        hasHammer = true;
        hammerTimer = hammerDuration;
        anim.SetBool("HasHammer", true);
    }

    // ──────────────────────────────────────────────
    // DAÑO Y MUERTE
    // ──────────────────────────────────────────────

    /// <summary>
    /// Llamado por barriles u otros peligros al tocar al jugador.
    /// </summary>
    public void TakeDamage()
    {
        if (isInvulnerable || isDead) return;

        StartCoroutine(DieSequence());
    }

    private IEnumerator DieSequence()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;

        anim.SetTrigger("Die");

        // Pequeño salto de animación de muerte (clásico DK)
        yield return new WaitForSeconds(0.1f);
        rb.gravityScale = originalGravity;
        rb.linearVelocity = new Vector2(0f, 5f);

        yield return new WaitForSeconds(1f);

        GameManager.Instance.PlayerDied();
    }

    // ──────────────────────────────────────────────
    // ANIMADOR
    // ──────────────────────────────────────────────
    private void UpdateAnimator()
    {
        anim.SetFloat("Speed", Mathf.Abs(horizontalInput));
        anim.SetBool("IsGrounded", isGrounded);
        anim.SetBool("IsClimbing", isClimbing);
    }

    // ──────────────────────────────────────────────
    // GIZMOS (visibles sólo en el Editor)
    // ──────────────────────────────────────────────
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
        if (hammerHitArea != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(hammerHitArea.position, hammerHitRadius);
        }
    }
}
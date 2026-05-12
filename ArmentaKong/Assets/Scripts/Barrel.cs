using UnityEngine;

// Barrel — Barril con movimiento fluido y edge detection robusto.
//
// PROBLEMA ANTERIOR: el rayo de borde detectaba "no hay suelo" durante
// toda la caida, invirtiendo la direccion muchas veces seguidas.
//
// SOLUCION:
//   1. El edge check solo corre cuando el barril esta FIRMEMENTE en el suelo
//      (isGrounded = true) Y su velocidad vertical es casi cero (no esta cayendo).
//   2. Un cooldown impide que se vuelva a invertir por N segundos tras hacerlo.
//   3. Cuando el barril cae al vacio y aterriza en la plataforma de abajo,
//      el cooldown ya expiro y puede detectar el proximo borde normalmente.
//
// CONFIGURACION Rigidbody2D:
//   - Gravity Scale        : 3
//   - Collision Detection  : Continuous
//   - Interpolate          : Interpolate  (suavidad visual)
//   - Freeze Rotation Z    : DESACTIVADO
// Layer: Barrel  |  Tag: Barrel

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class Barrel : MonoBehaviour
{
    // ──────────────────────────────────────────────
    // INSPECTOR
    // ──────────────────────────────────────────────
    [Header("Movimiento")]
    public float rollSpeed = 3f;

    [Tooltip("Layer de plataformas/suelo")]
    public LayerMask groundLayer;

    [Tooltip("Debe coincidir con el radio del CircleCollider2D")]
    public float barrelRadius = 0.25f;

    [Tooltip("Segundos de espera antes de poder volver a invertir direccion")]
    public float flipCooldown = 0.4f;

    [Tooltip("Velocidad vertical maxima para considerar que el barril esta quieto en el suelo")]
    public float groundedVelocityThreshold = 0.1f;

    [Header("Efectos")]
    public GameObject breakEffect;

    // ──────────────────────────────────────────────
    // ESTADO INTERNO
    // ──────────────────────────────────────────────
    private Rigidbody2D rb;

    private float direction = 1f;      // 1 = derecha, -1 = izquierda
    private bool isGrounded;
    private float flipTimer;           // tiempo restante del cooldown

    // ──────────────────────────────────────────────
    // UNITY LIFECYCLE
    // ──────────────────────────────────────────────
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        CheckGround();
        ApplyHorizontalMovement();
        CheckEdge();
        ApplyVisualRotation();

        // Reducir cooldown
        if (flipTimer > 0f)
            flipTimer -= Time.fixedDeltaTime;
    }

    // ──────────────────────────────────────────────
    // SUELO
    // Tres rayos en abanico para detectar suelo de forma mas confiable
    // que un solo rayo central (especialmente en bordes).
    // ──────────────────────────────────────────────
    private void CheckGround()
    {
        float rayLen = barrelRadius + 0.1f;
        Vector2 center = transform.position;

        // Rayo central, izquierdo y derecho
        bool c = Physics2D.Raycast(center, Vector2.down, rayLen, groundLayer);
        bool l = Physics2D.Raycast(center + Vector2.left * barrelRadius * 0.6f, Vector2.down, rayLen, groundLayer);
        bool r = Physics2D.Raycast(center + Vector2.right * barrelRadius * 0.6f, Vector2.down, rayLen, groundLayer);

        isGrounded = c || l || r;
    }

    // ──────────────────────────────────────────────
    // MOVIMIENTO HORIZONTAL
    // Solo forzamos X cuando esta en el suelo; en el aire la inercia
    // mantiene la trayectoria y la gravedad hace el resto.
    // ──────────────────────────────────────────────
    private void ApplyHorizontalMovement()
    {
        if (isGrounded)
            rb.linearVelocity = new Vector2(direction * rollSpeed, rb.linearVelocity.y);
    }

    // ──────────────────────────────────────────────
    // BORDE DE PLATAFORMA
    //
    // Condiciones para ejecutar la deteccion:
    //   A) El barril esta en el suelo
    //   B) No esta cayendo (velocidad Y casi cero) — esto es lo clave:
    //      evita que el rayo se dispare durante la caida entre plataformas
    //   C) El cooldown de inversion ya expiro
    // ──────────────────────────────────────────────
    private void CheckEdge()
    {
        // Condicion B: si esta cayendo, no checar borde
        bool isFalling = rb.linearVelocity.y < -groundedVelocityThreshold;
        if (!isGrounded || isFalling || flipTimer > 0f)
            return;

        // Rayo desde el borde frontal del barril hacia abajo
        // El origen esta justo en el borde exterior del circulo
        Vector2 origin = (Vector2)transform.position
                         + new Vector2(direction * barrelRadius, 0f);

        // Longitud: un poco mas que el radio para asegurar deteccion
        float rayLen = barrelRadius + 0.15f;

        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, rayLen, groundLayer);

        if (hit.collider == null)
        {
            // No hay plataforma adelante → invertir y activar cooldown
            direction *= -1f;
            flipTimer = flipCooldown;
        }
    }

    // ──────────────────────────────────────────────
    // ROTACION VISUAL
    // Basada en la velocidad X real → proporcional y siempre fluida.
    // ──────────────────────────────────────────────
    private void ApplyVisualRotation()
    {
        float circumference = 2f * Mathf.PI * barrelRadius;
        float degreesPerSecond = (rb.linearVelocity.x / circumference) * 360f;
        transform.Rotate(0f, 0f, -degreesPerSecond * Time.fixedDeltaTime);
    }

    // ──────────────────────────────────────────────
    // COLISIONES
    // ──────────────────────────────────────────────
    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            PlayerController player = col.gameObject.GetComponent<PlayerController>();
            if (player != null)
                player.TakeDamage();
        }
    }

    // ──────────────────────────────────────────────
    // DESTRUCCION POR MARTILLO
    // ──────────────────────────────────────────────
    public void DestroyBarrel()
    {
        GameManager.Instance.AddScore(GameManager.Instance.pointsPerBarrel);

        if (breakEffect != null)
            Instantiate(breakEffect, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }

    // ──────────────────────────────────────────────
    // GIZMOS
    // ──────────────────────────────────────────────
    private void OnDrawGizmosSelected()
    {
        float rayLen = barrelRadius + 0.1f;
        Vector2 center = transform.position;

        // Rayos de suelo (azul)
        Gizmos.color = Color.blue;
        Gizmos.DrawRay((Vector3)center, Vector3.down * rayLen);
        Gizmos.DrawRay((Vector3)center + Vector3.left * barrelRadius * 0.6f, Vector3.down * rayLen);
        Gizmos.DrawRay((Vector3)center + Vector3.right * barrelRadius * 0.6f, Vector3.down * rayLen);

        // Rayo de borde (amarillo)
        Gizmos.color = Color.yellow;
        Vector3 edgeOrigin = (Vector3)center + new Vector3(direction * barrelRadius, 0f, 0f);
        Gizmos.DrawRay(edgeOrigin, Vector3.down * (barrelRadius + 0.15f));
    }
}

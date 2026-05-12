using System.Collections;
using UnityEngine;

/// <summary>
/// DonkeyKong — Controlador de Donkey Kong.
///
/// • Lanza barriles a intervalos de tiempo.
/// • Puede golpearse el pecho entre lanzamientos (animación de burla).
/// • Cuando el jugador llega a la cima, activa la animación de escape
///   y notifica al GameManager para avanzar de nivel.
/// </summary>
public class DonkeyKong : MonoBehaviour
{
    // ──────────────────────────────────────────────
    // INSPECTOR
    // ──────────────────────────────────────────────
    [Header("Lanzamiento de Barriles")]
    [Tooltip("Prefab del barril")]
    public GameObject barrelPrefab;

    [Tooltip("Punto desde el que aparecen los barriles (vacío hijo de DK)")]
    public Transform barrelSpawnPoint;

    [Tooltip("Tiempo mínimo entre lanzamientos (segundos)")]
    public float throwIntervalMin = 2f;

    [Tooltip("Tiempo máximo entre lanzamientos (segundos)")]
    public float throwIntervalMax = 4f;

    [Header("Escape")]
    [Tooltip("Punto de la escalera por donde escapa DK al ganar el nivel")]
    public Transform escapePoint;

    [Tooltip("Velocidad a la que DK se mueve hacia el punto de escape")]
    public float escapeSpeed = 3f;

    [Tooltip("Tiempo de espera antes de escapar (para que se vea la animación)")]
    public float escapeDelay = 1.5f;

    // ──────────────────────────────────────────────
    // ESTADO INTERNO
    // ──────────────────────────────────────────────
    private Animator anim;
    private bool isEscaping;
    private Coroutine throwRoutine;

    // ──────────────────────────────────────────────
    // UNITY LIFECYCLE
    // ──────────────────────────────────────────────
    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        // Comenzar ciclo de lanzamiento de barriles
        throwRoutine = StartCoroutine(ThrowBarrelRoutine());
    }

    private void Update()
    {
        // Si está escapando, mover hacia el punto de escape
        if (isEscaping && escapePoint != null)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                escapePoint.position,
                escapeSpeed * Time.deltaTime);
        }
    }

    // ──────────────────────────────────────────────
    // LANZAMIENTO DE BARRILES
    // ──────────────────────────────────────────────
    private IEnumerator ThrowBarrelRoutine()
    {
        while (true)
        {
            // Esperar un tiempo aleatorio entre lanzamientos
            float waitTime = Random.Range(throwIntervalMin, throwIntervalMax);
            yield return new WaitForSeconds(waitTime);

            if (!isEscaping)
                ThrowBarrel();
        }
    }

    private void ThrowBarrel()
    {
        if (barrelPrefab == null || barrelSpawnPoint == null) return;

        anim.SetTrigger("Throw");

        // Instanciar el barril en el punto de lanzamiento
        Instantiate(barrelPrefab, barrelSpawnPoint.position, Quaternion.identity);
    }

    // ──────────────────────────────────────────────
    // ESCAPE (llamado por WinZone cuando el jugador llega a la cima)
    // ──────────────────────────────────────────────

    /// <summary>
    /// Activa la secuencia de escape de DK y desencadena el avance de nivel.
    /// </summary>
    public void StartEscape()
    {
        if (isEscaping) return;

        // Parar de lanzar barriles
        if (throwRoutine != null)
            StopCoroutine(throwRoutine);

        StartCoroutine(EscapeSequence());
    }

    private IEnumerator EscapeSequence()
    {
        isEscaping = true;
        anim.SetTrigger("Escape");

        // Esperar animación de sorpresa / huida
        yield return new WaitForSeconds(escapeDelay);

        // Mover a DK hacia el punto de escape (la lógica está en Update)
        // Esperar a que llegue al punto de escape
        while (escapePoint != null &&
               Vector2.Distance(transform.position, escapePoint.position) > 0.1f)
        {
            yield return null;
        }

        // Avisar al GameManager que el nivel fue ganado
        GameManager.Instance.LevelWon();

        // Ocultar a DK
        gameObject.SetActive(false);
    }
}


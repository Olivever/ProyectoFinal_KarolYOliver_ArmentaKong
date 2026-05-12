using UnityEngine;

/// <summary>
/// DeathZone — Zona invisible debajo del nivel.
///
/// Si el jugador cae aquí (se cae de todas las plataformas) muere automáticamente.
///
/// SETUP en Unity:
///   1. Crea un GameObject vacío bien por debajo del nivel.
///   2. Añade BoxCollider2D ? activa "Is Trigger".
///   3. Hazlo muy ancho para cubrir todo el nivel.
///   4. Añade este script.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class DeathZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
                player.TakeDamage();
        }

        // Destruir barriles que se caigan del nivel
        if (other.CompareTag("Barrel"))
            Destroy(other.gameObject);
    }
}
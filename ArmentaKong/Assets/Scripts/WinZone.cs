using UnityEngine;

/// <summary>
/// WinZone — Trigger en la cima del nivel (junto a Pauline).
///
/// Cuando el jugador llega aquí:
///   1. Le avisa a DonkeyKong que empiece a escapar.
///   2. Desactiva al jugador para que no siga moviéndose.
///
/// El GameManager.LevelWon() es llamado desde DonkeyKong.EscapeSequence()
/// una vez que DK llega a su punto de escape.
///
/// SETUP en Unity:
///   1. Crea un GameObject vacío en la cima del escenario.
///   2. Añade BoxCollider2D ? activa "Is Trigger".
///   3. Ajusta su tamaño para cubrir la zona de victoria.
///   4. Arrastra la referencia de DonkeyKong en el inspector.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class WinZone : MonoBehaviour
{
    [Tooltip("Referencia al GameObject de Donkey Kong en la escena")]
    public DonkeyKong donkeyKong;

    private bool triggered;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;

        if (other.CompareTag("Player"))
        {
            triggered = true;

            // Detener al jugador
            other.GetComponent<PlayerController>().enabled = false;

            // DK escapa
            if (donkeyKong != null)
                donkeyKong.StartEscape();
            else
                // Si no hay referencia a DK, llamar al GameManager directamente
                GameManager.Instance.LevelWon();
        }
    }
}
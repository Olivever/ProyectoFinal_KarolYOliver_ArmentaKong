using UnityEngine;

/// <summary>
/// LadderZone — Componente que va en cada escalera.
///
/// Usa un Collider2D en modo Trigger para detectar cuándo el jugador
/// entra o sale de la zona de escalada.
///
/// SETUP en Unity:
///   1. Crea un GameObject vacío hijo de la escalera (o directamente en ella).
///   2. Añade BoxCollider2D ? activa "Is Trigger".
///   3. Ajusta el tamaño para cubrir toda la zona escalable.
///   4. Añade este script.
///   5. Asegúrate de que el jugador tiene el tag "Player".
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class LadderZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
                player.EnterLadder();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
                player.ExitLadder();
        }
    }
}
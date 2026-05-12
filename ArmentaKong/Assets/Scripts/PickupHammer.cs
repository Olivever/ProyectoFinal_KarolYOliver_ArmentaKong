using UnityEngine;

/// <summary>
/// PickupHammer — Martillo recogible del escenario.
///
/// Cuando el jugador lo toca:
///   1. Le otorga el estado "tiene martillo" a través de PlayerController.
///   2. Se destruye a sí mismo.
///
/// SETUP en Unity:
///   1. Crea un sprite del martillo.
///   2. Añade CircleCollider2D ? activa "Is Trigger".
///   3. Añade este script.
///   4. Asegúrate de que el jugador tiene el tag "Player".
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class PickupHammer : MonoBehaviour
{
    [Tooltip("Efecto visual/sonido al recoger el martillo (opcional)")]
    public GameObject pickupEffect;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.PickUpHammer();

                if (pickupEffect != null)
                    Instantiate(pickupEffect, transform.position, Quaternion.identity);

                Destroy(gameObject);
            }
        }
    }
}
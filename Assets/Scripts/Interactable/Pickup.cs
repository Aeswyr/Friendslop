using Mirror;
using UnityEngine;

public class Pickup : NetworkBehaviour
{
    [SerializeField] private ItemID item;
    
    public void OnPickup(PlayerController player)
    {
        if (player.AddItem(item))
            Cleanup();

        [Command(requiresAuthority = false)] void Cleanup() {
            NetworkServer.Destroy(gameObject);
        }
    }
}

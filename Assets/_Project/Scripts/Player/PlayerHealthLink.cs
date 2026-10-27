using UnityEngine;

// Extra forms share the normal player's health, inventory and respawn lifecycle.
public class PlayerHealthLink : MonoBehaviour
{
    public PlayerHealth Owner { get; private set; }
    public void Bind(PlayerHealth owner) => Owner = owner;
}

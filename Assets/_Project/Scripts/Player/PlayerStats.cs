using UnityEngine;

// Compatibility entry point for existing boss attacks and serialized components.
// PlayerHealth owns armor, invulnerability, death and respawn.
[RequireComponent(typeof(PlayerHealth))]
public class PlayerStats : MonoBehaviour
{
    private PlayerHealth health;
    private PlayerHealth Health => health != null ? health : health = GetComponent<PlayerHealth>();

    public int damage { get => Health.damage; set => Health.damage = value; }
    public int armor { get => Health.armor; set => Health.armor = value; }
    public float respawnDelay { get => Health.respawnDelay; set => Health.respawnDelay = value; }

    public void TakeDamage(int incomingDamage) => Health.TakeDamage(incomingDamage);

    public static PlayerStats Resolve(GameObject source) => source != null ? Resolve(source.transform) : null;

    public static PlayerStats Resolve(Component source)
    {
        var owner = PlayerHealth.Resolve(source);
        return owner != null ? owner.GetComponent<PlayerStats>() : null;
    }
}

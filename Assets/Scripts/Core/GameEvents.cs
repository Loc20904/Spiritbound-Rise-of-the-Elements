public static class GameEvents
{
    // --- Player Stat Events ---
    public static event System.Action<PlayerStatProfile> OnPlayerStatRevealed;

    public static void TriggerPlayerStatRevealed(PlayerStatProfile stat)
    {
        OnPlayerStatRevealed?.Invoke(stat);
    }
}
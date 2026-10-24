using UnityEngine;

public class PlayerStatManager : MonoBehaviour
{
    public static PlayerStatManager Instance { get; private set; }

    [Header("Database Reference")]
    [Tooltip("Drag the PlayerStatDatabase ScriptableObject here")]
    public PlayerStatDatabase database;

    /// <summary>
    /// The currently active tarot card (stat profile). Null if not assigned yet.
    /// </summary>
    public PlayerStatProfile ActiveStat { get; private set; }

    public bool HasReceivedStat => ActiveStat != null;

    public event System.Action<PlayerStatProfile> OnStatAssigned;

    private const string PREF_KEY = "PlayerStatIndex"; // Changed key for the new system

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        LoadSavedStat();
    }

    private void LoadSavedStat()
    {
        if (database == null) return;

        if (PlayerPrefs.HasKey(PREF_KEY))
        {
            int savedIndex = PlayerPrefs.GetInt(PREF_KEY);
            ActiveStat = database.GetStat(savedIndex);

            if (ActiveStat == null)
            {
                PlayerPrefs.DeleteKey(PREF_KEY);
            }
        }
    }

    public PlayerStatProfile AssignRandomStat()
    {
        if (HasReceivedStat) return ActiveStat;
        if (database == null || database.Count == 0) return null;

        int randomIndex = UnityEngine.Random.Range(0, database.Count);
        ActiveStat = database.GetStat(randomIndex);

        PlayerPrefs.SetInt(PREF_KEY, randomIndex);
        PlayerPrefs.Save();

        OnStatAssigned?.Invoke(ActiveStat);
        GameEvents.TriggerPlayerStatRevealed(ActiveStat);

        return ActiveStat;
    }

    // --- Modifier Getters ---
    // Base
    public float GetHPMultiplier() => ActiveStat?.GetHPMultiplier() ?? 1f;
    public float GetMPMultiplier() => ActiveStat?.GetMPMultiplier() ?? 1f;
    public float GetSpeedMultiplier() => ActiveStat?.GetSpeedMultiplier() ?? 1f;
    public float GetHPRegenMultiplier() => ActiveStat?.GetHPRegenMultiplier() ?? 1f;

    // Combat
    public float GetDamageMultiplier() => ActiveStat?.GetDamageMultiplier() ?? 1f;
    public float GetDamageTakenMultiplier() => ActiveStat?.GetDamageTakenMultiplier() ?? 1f;
    public float GetCritChanceMultiplier() => ActiveStat?.GetCritChanceMultiplier() ?? 1f;
    public float GetCritDamageMultiplier() => ActiveStat?.GetCritDamageMultiplier() ?? 1f;
    public float GetAttackSpeedMultiplier() => ActiveStat?.GetAttackSpeedMultiplier() ?? 1f;

    // Equipment
    public float GetWeaponDamageMultiplier() => ActiveStat?.GetWeaponDamageMultiplier() ?? 1f;
    public float GetArmorDefenseMultiplier() => ActiveStat?.GetArmorDefenseMultiplier() ?? 1f;
    public float GetAccessoryEffectMultiplier() => ActiveStat?.GetAccessoryEffectMultiplier() ?? 1f;

    // Skill
    public float GetSkillDamageMultiplier() => ActiveStat?.GetSkillDamageMultiplier() ?? 1f;
    public float GetSkillCooldownMultiplier() => ActiveStat?.GetSkillCooldownMultiplier() ?? 1f;
    public float GetSkillManaCostMultiplier() => ActiveStat?.GetSkillManaCostMultiplier() ?? 1f;
    public float GetSkillRangeMultiplier() => ActiveStat?.GetSkillRangeMultiplier() ?? 1f;

    [ContextMenu("DEBUG: Reset Player Tarot Card")]
    public void ResetStat()
    {
        PlayerPrefs.DeleteKey(PREF_KEY);
        PlayerPrefs.Save();
        ActiveStat = null;
        Debug.Log("[PlayerStatManager] Tarot Card reset. Next interaction will roll a new card.");
    }
}

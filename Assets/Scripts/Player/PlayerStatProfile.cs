using UnityEngine;

public enum StatType { Buff, Debuff, Mixed, Neutral }

[System.Serializable]
public class PlayerStatProfile
{
    [Header("Tarot Identity")]
    public string cardName;        // e.g. "The Magician"
    public string cardNumeral;     // e.g. "I"
    public string titleEmoji;      // e.g. "🃏"
    public StatType statType;
    public Sprite cardImage;

    [Header("Base Stats Modifiers (%)")]
    public float hpModifier;
    public float mpModifier;
    public float speedModifier;
    public float hpRegenModifier;

    [Header("Combat Modifiers (%)")]
    public float damageModifier;
    public float damageTakenModifier;
    public float critChanceModifier;
    public float critDamageModifier;
    public float attackSpeedModifier;

    [Header("Equipment Modifiers (%)")]
    public float weaponDamageModifier;
    public float armorDefenseModifier;
    public float accessoryEffectModifier;

    [Header("Skill Modifiers (%)")]
    public float skillDamageModifier;
    public float skillCooldownModifier; // negative is better
    public float skillManaCostModifier; // negative is better
    public float skillRangeModifier;

    [Header("Display")]
    [TextArea(2, 5)]
    public string description;     // Short description of the effect for UI display

    // --- Multiplier Helpers ---
    // Convert percentage to multiplier: 25 -> 1.25f, -20 -> 0.80f
    public float GetHPMultiplier() => 1f + (hpModifier / 100f);
    public float GetMPMultiplier() => 1f + (mpModifier / 100f);
    public float GetSpeedMultiplier() => 1f + (speedModifier / 100f);
    public float GetHPRegenMultiplier() => 1f + (hpRegenModifier / 100f);

    public float GetDamageMultiplier() => 1f + (damageModifier / 100f);
    public float GetDamageTakenMultiplier() => 1f + (damageTakenModifier / 100f);
    public float GetCritChanceMultiplier() => 1f + (critChanceModifier / 100f);
    public float GetCritDamageMultiplier() => 1f + (critDamageModifier / 100f);
    public float GetAttackSpeedMultiplier() => 1f + (attackSpeedModifier / 100f);

    public float GetWeaponDamageMultiplier() => 1f + (weaponDamageModifier / 100f);
    public float GetArmorDefenseMultiplier() => 1f + (armorDefenseModifier / 100f);
    public float GetAccessoryEffectMultiplier() => 1f + (accessoryEffectModifier / 100f);

    public float GetSkillDamageMultiplier() => 1f + (skillDamageModifier / 100f);
    public float GetSkillCooldownMultiplier() => 1f + (skillCooldownModifier / 100f);
    public float GetSkillManaCostMultiplier() => 1f + (skillManaCostModifier / 100f);
    public float GetSkillRangeMultiplier() => 1f + (skillRangeModifier / 100f);

    /// <summary>
    /// Returns a formatted string summarizing all active modifiers by category.
    /// </summary>
    public string GetEffectSummary()
    {
        var effects = new System.Collections.Generic.List<string>();

        // Base
        if (hpModifier != 0) effects.Add($"{(hpModifier > 0 ? "+" : "")}{hpModifier}% Max HP");
        if (mpModifier != 0) effects.Add($"{(mpModifier > 0 ? "+" : "")}{mpModifier}% Max MP");
        if (speedModifier != 0) effects.Add($"{(speedModifier > 0 ? "+" : "")}{speedModifier}% Speed");
        if (hpRegenModifier != 0) effects.Add($"{(hpRegenModifier > 0 ? "+" : "")}{hpRegenModifier}% HP Regen");

        // Combat
        if (damageModifier != 0) effects.Add($"{(damageModifier > 0 ? "+" : "")}{damageModifier}% Damage");
        if (damageTakenModifier != 0) effects.Add($"{(damageTakenModifier > 0 ? "+" : "")}{damageTakenModifier}% Damage Taken");
        if (critChanceModifier != 0) effects.Add($"{(critChanceModifier > 0 ? "+" : "")}{critChanceModifier}% Crit Chance");
        if (critDamageModifier != 0) effects.Add($"{(critDamageModifier > 0 ? "+" : "")}{critDamageModifier}% Crit Damage");
        if (attackSpeedModifier != 0) effects.Add($"{(attackSpeedModifier > 0 ? "+" : "")}{attackSpeedModifier}% Attack Speed");

        // Equipment
        if (weaponDamageModifier != 0) effects.Add($"{(weaponDamageModifier > 0 ? "+" : "")}{weaponDamageModifier}% Weapon Damage");
        if (armorDefenseModifier != 0) effects.Add($"{(armorDefenseModifier > 0 ? "+" : "")}{armorDefenseModifier}% Armor Defense");
        if (accessoryEffectModifier != 0) effects.Add($"{(accessoryEffectModifier > 0 ? "+" : "")}{accessoryEffectModifier}% Accessory Effect");

        // Skill
        if (skillDamageModifier != 0) effects.Add($"{(skillDamageModifier > 0 ? "+" : "")}{skillDamageModifier}% Skill Damage");
        if (skillCooldownModifier != 0) effects.Add($"{(skillCooldownModifier > 0 ? "+" : "")}{skillCooldownModifier}% Skill Cooldown");
        if (skillManaCostModifier != 0) effects.Add($"{(skillManaCostModifier > 0 ? "+" : "")}{skillManaCostModifier}% Skill Mana Cost");
        if (skillRangeModifier != 0) effects.Add($"{(skillRangeModifier > 0 ? "+" : "")}{skillRangeModifier}% Skill Range");

        return effects.Count > 0 ? string.Join("\n", effects) : "No fate alterations.";
    }
}

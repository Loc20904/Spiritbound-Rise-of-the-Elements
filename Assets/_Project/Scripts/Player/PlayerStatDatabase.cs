using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStatDatabase", menuName = "Spiritbound/Player Stat Database")]
public class PlayerStatDatabase : ScriptableObject
{
    public List<PlayerStatProfile> stats = new List<PlayerStatProfile>();

    public int Count => stats.Count;

    public PlayerStatProfile GetStat(int index)
    {
        if (index >= 0 && index < stats.Count)
        {
            return stats[index];
        }
        return null;
    }

#if UNITY_EDITOR
    [ContextMenu("Populate Default 22 Tarot Cards")]
    private void PopulateDefaultStats()
    {
        stats.Clear();

        // 0 The Fool
        stats.Add(new PlayerStatProfile { cardNumeral = "0", cardName = "The Fool", titleEmoji = "🃏", statType = StatType.Mixed,
            speedModifier = 20f, armorDefenseModifier = -15f, skillRangeModifier = 10f,
            description = "Beginnings, innocence, spontaneity." });

        // I The Magician
        stats.Add(new PlayerStatProfile { cardNumeral = "I", cardName = "The Magician", titleEmoji = "✨", statType = StatType.Buff,
            skillDamageModifier = 25f, skillManaCostModifier = -15f,
            description = "Manifestation, resourcefulness, power." });

        // II The High Priestess
        stats.Add(new PlayerStatProfile { cardNumeral = "II", cardName = "The High Priestess", titleEmoji = "🌙", statType = StatType.Buff,
            mpModifier = 30f, skillRangeModifier = 15f,
            description = "Intuition, sacred knowledge, divine feminine." });

        // III The Empress
        stats.Add(new PlayerStatProfile { cardNumeral = "III", cardName = "The Empress", titleEmoji = "👑", statType = StatType.Buff,
            hpModifier = 25f, hpRegenModifier = 10f, armorDefenseModifier = 10f,
            description = "Femininity, beauty, nature, abundance." });

        // IV The Emperor
        stats.Add(new PlayerStatProfile { cardNumeral = "IV", cardName = "The Emperor", titleEmoji = "🛡️", statType = StatType.Mixed,
            weaponDamageModifier = 20f, armorDefenseModifier = 15f, speedModifier = -10f,
            description = "Authority, establishment, structure, a father figure." });

        // V The Hierophant
        stats.Add(new PlayerStatProfile { cardNumeral = "V", cardName = "The Hierophant", titleEmoji = "📜", statType = StatType.Buff,
            skillCooldownModifier = -20f, skillManaCostModifier = -15f,
            description = "Spiritual wisdom, religious beliefs, conformity." });

        // VI The Lovers
        stats.Add(new PlayerStatProfile { cardNumeral = "VI", cardName = "The Lovers", titleEmoji = "❤️", statType = StatType.Mixed,
            critChanceModifier = 30f, hpModifier = -20f,
            description = "Love, harmony, relationships, values alignment." });

        // VII The Chariot
        stats.Add(new PlayerStatProfile { cardNumeral = "VII", cardName = "The Chariot", titleEmoji = "🎠", statType = StatType.Buff,
            speedModifier = 25f, attackSpeedModifier = 15f,
            description = "Control, willpower, success, action." });

        // VIII Strength
        stats.Add(new PlayerStatProfile { cardNumeral = "VIII", cardName = "Strength", titleEmoji = "🦁", statType = StatType.Buff,
            damageModifier = 20f, weaponDamageModifier = 15f,
            description = "Strength, courage, persuasion, influence." });

        // IX The Hermit
        stats.Add(new PlayerStatProfile { cardNumeral = "IX", cardName = "The Hermit", titleEmoji = "🏮", statType = StatType.Mixed,
            skillDamageModifier = 30f, attackSpeedModifier = -20f, speedModifier = -15f,
            description = "Soul-searching, introspection, being alone, inner guidance." });

        // X Wheel of Fortune
        stats.Add(new PlayerStatProfile { cardNumeral = "X", cardName = "Wheel of Fortune", titleEmoji = "🎡", statType = StatType.Mixed,
            hpModifier = 15f, speedModifier = 15f, damageModifier = -10f, armorDefenseModifier = -10f, critChanceModifier = 10f,
            description = "Good luck, karma, life cycles, destiny, a turning point." });

        // XI Justice
        stats.Add(new PlayerStatProfile { cardNumeral = "XI", cardName = "Justice", titleEmoji = "⚖️", statType = StatType.Neutral,
            damageModifier = 15f, damageTakenModifier = 15f,
            description = "Justice, fairness, truth, cause and effect." });

        // XII The Hanged Man
        stats.Add(new PlayerStatProfile { cardNumeral = "XII", cardName = "The Hanged Man", titleEmoji = "🙃", statType = StatType.Mixed,
            speedModifier = -25f, skillRangeModifier = 20f, skillDamageModifier = 15f,
            description = "Pause, surrender, letting go, new perspectives." });

        // XIII Death
        stats.Add(new PlayerStatProfile { cardNumeral = "XIII", cardName = "Death", titleEmoji = "💀", statType = StatType.Mixed,
            hpModifier = -30f, damageModifier = 35f, critDamageModifier = 20f,
            description = "Endings, change, transformation, transition." });

        // XIV Temperance
        stats.Add(new PlayerStatProfile { cardNumeral = "XIV", cardName = "Temperance", titleEmoji = "⏳", statType = StatType.Buff,
            damageTakenModifier = -15f, hpModifier = 10f, mpModifier = 10f,
            description = "Balance, moderation, patience, purpose." });

        // XV The Devil
        stats.Add(new PlayerStatProfile { cardNumeral = "XV", cardName = "The Devil", titleEmoji = "😈", statType = StatType.Mixed,
            damageModifier = 30f, critDamageModifier = 25f, armorDefenseModifier = -20f, damageTakenModifier = 20f,
            description = "Shadow self, attachment, addiction, restriction." });

        // XVI The Tower
        stats.Add(new PlayerStatProfile { cardNumeral = "XVI", cardName = "The Tower", titleEmoji = "🌩️", statType = StatType.Mixed,
            armorDefenseModifier = -20f, hpModifier = -15f, weaponDamageModifier = 30f,
            description = "Sudden change, upheaval, chaos, revelation." });

        // XVII The Star
        stats.Add(new PlayerStatProfile { cardNumeral = "XVII", cardName = "The Star", titleEmoji = "⭐", statType = StatType.Buff,
            mpModifier = 20f, skillCooldownModifier = -20f, accessoryEffectModifier = 15f,
            description = "Hope, faith, purpose, renewal, spirituality." });

        // XVIII The Moon
        stats.Add(new PlayerStatProfile { cardNumeral = "XVIII", cardName = "The Moon", titleEmoji = "🌕", statType = StatType.Mixed,
            damageModifier = -15f, damageTakenModifier = 25f, critChanceModifier = 20f,
            description = "Illusion, fear, anxiety, subconscious, intuition." });

        // XIX The Sun
        stats.Add(new PlayerStatProfile { cardNumeral = "XIX", cardName = "The Sun", titleEmoji = "☀️", statType = StatType.Buff,
            hpModifier = 15f, mpModifier = 15f, speedModifier = 15f, weaponDamageModifier = 10f,
            description = "Positivity, fun, warmth, success, vitality." });

        // XX Judgement
        stats.Add(new PlayerStatProfile { cardNumeral = "XX", cardName = "Judgement", titleEmoji = "🎺", statType = StatType.Mixed,
            skillDamageModifier = 25f, weaponDamageModifier = -25f, skillRangeModifier = 15f,
            description = "Judgement, rebirth, inner calling, absolution." });

        // XXI The World
        stats.Add(new PlayerStatProfile { cardNumeral = "XXI", cardName = "The World", titleEmoji = "🌍", statType = StatType.Buff,
            hpModifier = 10f, mpModifier = 10f, speedModifier = 10f, damageModifier = 10f, critChanceModifier = 10f,
            weaponDamageModifier = 10f, armorDefenseModifier = 10f, skillDamageModifier = 10f,
            description = "Completion, integration, accomplishment, travel." });

        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log("[PlayerStatDatabase] Updated with 22 Major Arcana Tarot Cards!");
    }
#endif
}

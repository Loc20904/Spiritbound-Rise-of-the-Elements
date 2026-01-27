using System;
using UnityEngine;

public class EnemyDropTable : MonoBehaviour
{
    [Serializable]
    public class DropEntry
    {
        public GameObject prefab;
        [Min(0f)] public float weight = 1f;
        [Range(0, 1f)] public float chance = 1f;
        [Min(1)] public int minAmount = 1;
        [Min(1)] public int maxAmount = 1;
    }

    [Header("Overall drop chance (can drop nothing)")]
    [Range(0, 1f)] public float dropChance = 0.5f;

    [Header("Drop entries (4-5 items...)")]
    public DropEntry[] drops;

    [Header("Spawn (optional random scatter)")]
    public Vector2 randomOffset = new Vector2(0.25f, 0.15f);

    public void TryDrop(Vector3 deadPos)
    {
        if (drops == null || drops.Length == 0) return;
        if (UnityEngine.Random.value > dropChance) return;

        int idx = PickWeightedIndex(drops);
        if (idx < 0) return;

        var entry = drops[idx];
        if (UnityEngine.Random.value > entry.chance) return;

        int amount = UnityEngine.Random.Range(entry.minAmount, entry.maxAmount + 1);

        for (int i = 0; i < amount; i++)
        {
            Vector3 pos = deadPos + new Vector3(
                UnityEngine.Random.Range(-randomOffset.x, randomOffset.x),
                UnityEngine.Random.Range(-randomOffset.y, randomOffset.y),
                0f
            );

            Instantiate(entry.prefab, pos, Quaternion.identity);
        }
    }

    private int PickWeightedIndex(DropEntry[] list)
    {
        float total = 0f;
        for (int i = 0; i < list.Length; i++)
            total += Mathf.Max(0f, list[i].weight);

        if (total <= 0f) return -1;

        float r = UnityEngine.Random.value * total;
        float acc = 0f;

        for (int i = 0; i < list.Length; i++)
        {
            acc += Mathf.Max(0f, list[i].weight);
            if (r <= acc) return i;
        }
        return list.Length - 1;
    }
}

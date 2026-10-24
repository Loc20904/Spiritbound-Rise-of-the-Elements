using UnityEngine;

public class SlotGenerator : MonoBehaviour
{
    public GameObject slotPrefab;

    void Start()
    {
        for (int i = 0; i < 20; i++)
        {
            Instantiate(slotPrefab, transform);
        }
    }
}
using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    public float delay = 1f; // Thời gian tồn tại (giây)

    void Start()
    {
        // Tự động hủy gameObject này sau 'delay' giây
        Destroy(gameObject, delay);
    }
}
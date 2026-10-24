using UnityEngine;

public class Lava : MonoBehaviour
{
    public static Lava Instance;

    [SerializeField] public Transform pointRespawn;

    private void Awake()
    {
        Instance = this;
    }
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.tag == "Player")
        {
            collision.transform.position = pointRespawn.position;
        }
    }
}

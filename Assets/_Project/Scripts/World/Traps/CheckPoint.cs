using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    public BoxCollider2D triggered;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.tag == "Player")
        {
            Lava.Instance.pointRespawn.position = transform.position;
            triggered.enabled = false;
        }
    }
}

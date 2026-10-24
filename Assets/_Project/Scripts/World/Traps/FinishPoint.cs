using UnityEngine;

public class FinishPoint : MonoBehaviour
{
    [SerializeField] string sceneToLoad;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            SceneController.Instance.LoadScene(sceneToLoad);

            Debug.Log("Player đã chạm Finish Point!");
        }
    }
}

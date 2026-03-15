using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

public class IntroChangeScene : MonoBehaviour
{

    public PlayableDirector introCutScene;

    void Start()
    {
        if (introCutScene != null)
        {
            introCutScene.Play();
        }
    }


    public IEnumerator playIntro()
    {
        if (introCutScene != null)
        {
            introCutScene.Play();
            yield return new WaitForSeconds((float)introCutScene.duration);
        }
        SceneController.Instance.LoadScene("FortuneScene");
    }

}

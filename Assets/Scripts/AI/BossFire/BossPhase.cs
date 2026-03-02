using UnityEngine;

public class BossPhase : MonoBehaviour
{
    public bool isPhase2 = false;
    public float phase2Threshold = 50f;

    Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void CheckPhase(float hpPercent)
    {
        if (!isPhase2 && hpPercent <= phase2Threshold)
        {
            EnterPhase2();
        }
    }

    void EnterPhase2()
    {
        isPhase2 = true;
        anim.SetTrigger("rage");
        Debug.Log("Boss entered Phase 2");
    }
}

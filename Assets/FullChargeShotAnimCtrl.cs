using UnityEngine;
using PlayerShot;

public class FullChargeShotAnimCtrl : IAnimatable
{
    private Animator anim;

    public void Construct(Animator animator)
    {
        this.anim = animator;
    }

    public void StartAnim()
    {
        //animator.SetTrigger("Start");
    }

    public void MoveAnim()
    {
        anim.SetTrigger("onFinishBigin");
    }

    public void TakeDamage()
    {
        anim.SetTrigger("isHit");
    }

    public void RefrectShell()
    {
        anim.SetTrigger("isRefrect");
    }
}

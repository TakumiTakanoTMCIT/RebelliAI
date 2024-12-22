using UnityEngine;
using PlayerShot;

public class FullChargeShotAnimCtrl : ShellAnimCtrlBase
{
    public override void StartAnim()
    {
        //animator.SetTrigger("Start");
    }

    public override void MoveAnim()
    {
        animator.SetTrigger("onFinishBigin");
    }

    public override void TakeDamage()
    {
        animator.SetTrigger("isHit");
    }

    public override void RefrectShell()
    {
        animator.SetTrigger("isRefrect");
    }
}

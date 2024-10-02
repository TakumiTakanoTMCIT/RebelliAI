using System.Collections;
using PlayerInfo;
using UnityEngine;

public class PlayerDashCtrl : MonoBehaviour
{
    PlayerStatus playerStatus;

    private bool isDashing = false;

    public bool isKeepDashSpeed = false;

    private void Start()
    {
        isDashing = false;
        playerStatus = this.GetComponent<PlayerStatus>();
    }

    public bool IsDashNow()
    {
        return isDashing;
    }

    public void KeepDash()
    {
        isKeepDashSpeed = true;
    }

    public void StopDash()
    {
        isKeepDashSpeed = false;
    }

    public IEnumerator DashTimeCounter()
    {
        isDashing = true;
        yield return new WaitForSeconds(playerStatus.DashTime);
        isDashing = false;
    }
}

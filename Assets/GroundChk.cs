using System.Collections;
using System.Collections.Generic;
using ActionStatusChk;
using PlayerState;
using UnityEngine;

public class GroundChk : MonoBehaviour
{
    private bool isGround;
    public bool IsGround
    {
        get { return isGround; }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            isGround = true;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            isGround = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            isGround = false;
        }
    }
}

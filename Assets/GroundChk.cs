using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundChk : MonoBehaviour
{
    private bool isGround;
    public bool IsGround
    {
        get { return isGround; }
        private set { isGround = value; }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            IsGround = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            IsGround = false;
        }
    }
}

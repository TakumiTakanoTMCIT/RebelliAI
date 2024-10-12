using System.Collections;
using System.Collections.Generic;
using ActionStatusChk;
using PlayerState;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class GroundChk : MonoBehaviour
{
    BoxCollider2D boxCollider;
    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        boxCollider.isTrigger = true;
    }

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

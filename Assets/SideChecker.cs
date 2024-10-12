using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class SideChecker : MonoBehaviour
{
    BoxCollider2D boxCollider;

    bool isEnteredWall = false;
    public bool IsEnteredWall { get { return isEnteredWall; } private set { isEnteredWall = value; } }

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        boxCollider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            IsEnteredWall = true;
        }
    }

    public void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            IsEnteredWall = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            IsEnteredWall = false;
        }
    }
}

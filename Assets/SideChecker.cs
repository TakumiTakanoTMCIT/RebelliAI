using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class SideChecker : MonoBehaviour
{
    BoxCollider2D boxCollider;

    [SerializeField]
    bool isEnteredWall = false;
    public bool IsEnteredWall { get { return isEnteredWall; } private set { isEnteredWall = value; } }

    private void Awake()
    {
        boxCollider = this.gameObject.MyGetComponent_NullChker<BoxCollider2D>();
        boxCollider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Ground")) isEnteredWall = true;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Ground")) isEnteredWall = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Ground")) isEnteredWall = false;
    }
}

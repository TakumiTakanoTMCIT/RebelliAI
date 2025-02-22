using System;
using UniRx;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class GroundChk : MonoBehaviour
{
    private Subject<Unit> onGround = new Subject<Unit>();
    public IObservable<Unit> OnGround => onGround;

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

            onGround.OnNext(Unit.Default);
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

using System;
using UnityEngine;
using Zenject;

public class MuzzlePositionDrawer : MonoBehaviour
{
    //Inject
    [SerializeField]
    private MuzzlePositions muzzlePositions;
    [SerializeField]
    private Transform playerTrans;

    private void OnDrawGizmos()
    {
        foreach (var muzzlePosition in muzzlePositions.muzzlePositions)
        {
            if (muzzlePosition.isCurrentState)
                Gizmos.color = new Color(0, 0, 1, 0.5f);
            else
                Gizmos.color = new Color(1, 0, 0, 0.5f);

            Vector2 pos = playerTrans.position;
            pos.x += muzzlePosition.pos.x;
            pos.y += muzzlePosition.pos.y;

            Gizmos.DrawSphere(pos, muzzlePosition.radius);
        }
    }
}

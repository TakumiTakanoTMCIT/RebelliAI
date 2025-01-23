using System;
using UnityEngine;

public class MuzzlePositionDrawer : MonoBehaviour
{
    //Inject
    private MuzzlePositions muzzlePositions;
    private Transform playerTrans;

    bool isToggle, force;

    public void Construct(MuzzlePositions muzzlePositions)
    {
        this.muzzlePositions = muzzlePositions;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            force = false;
            isToggle = !isToggle;
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            force = true;
            isToggle = false;
            Time.timeScale = 0;
            return;
        }

        if (force)
        {
            return;
        }

        if (isToggle)
        {
            Time.timeScale = 0.1f;
        }
        else
        {
            Time.timeScale = 1;
        }
    }

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

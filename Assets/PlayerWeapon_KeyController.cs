using KeyHandler;
using UnityEngine;

public class PlayerWeapon_KeyController : MonoBehaviour
{
    [SerializeField] private ChargeShot_TimeHandler chargeShot_TimeHandler;
    [SerializeField] private ShellManager mameManager;
    InputHandler inputHandler;

    private void Start()
    {
        inputHandler = this.GetComponent<InputHandler>();
    }

    private void Update()
    {
        if (inputHandler.IsShootKeyDown())
        {
            if(!chargeShot_TimeHandler.IsCharging)
            {
                chargeShot_TimeHandler.StartCharge();
            }
            mameManager.ShootMame();
            return;
        }

        if (inputHandler.IsShootKeyUp())
        {
            if (!chargeShot_TimeHandler.IsCharged)
            {
                if(chargeShot_TimeHandler.IsMinimumChargeTime)
                {
                    mameManager.ShootMame();
                }

                chargeShot_TimeHandler.InterruputChaging();
                return;
            }
            else
            {
                chargeShot_TimeHandler.ShootChargedShell();
            }
        }
    }
}

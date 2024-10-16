using KeyHandler;
using UnityEngine;

public class PlayerWeapon_KeyController : MonoBehaviour
{
    ChargeShot_Handler chargeShotHandler;
    AllShellManager mameManager;
    InputHandler inputHandler;

    //GameObject levelLower_EnergyBall, fullLevel_EnergyBall;

    public void Init(InputHandler inputHandler, ChargeShot_Handler chargeShotHandler, AllShellManager shellManager)
    {
        this.mameManager = shellManager;
        this.chargeShotHandler = chargeShotHandler;
        this.inputHandler = inputHandler;
    }

    private void Update()
    {
        if (inputHandler.IsShootKeyDown())
        {
            if (!chargeShotHandler.IsCharging)
            {
                chargeShotHandler.StartCharge();
            }
            mameManager.ShootMame();
            return;
        }

        if (inputHandler.IsShootKeyUp())
        {
            if (!chargeShotHandler.IsLowCharged)
            {
                if (chargeShotHandler.IsMinimumChargeTime)
                {
                    mameManager.ShootMame();
                }

                chargeShotHandler.InterruputChaging();
                return;
            }
            else if (chargeShotHandler.IsLowCharged && !chargeShotHandler.IsFullCharged)
            {
                chargeShotHandler.Shoot_Charged_Shell(chargeShotHandler.levelLower_EnergyBall);
            }
            else if (chargeShotHandler.IsFullCharged)
            {
                chargeShotHandler.Shoot_Charged_Shell(chargeShotHandler.fullLevel_EnergyBall);
            }
        }
    }
}

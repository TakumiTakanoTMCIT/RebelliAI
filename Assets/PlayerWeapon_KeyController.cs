using KeyHandler;
using UnityEngine;

public class PlayerWeapon_KeyController : MonoBehaviour
{
    ChargeShot_Handler chargeShotHandler;
    MameShellManager mameManager;
    InputHandler inputHandler;

    GameObject levelLower_EnergyBall, fullLevel_EnergyBall;

    public void Init(InputHandler inputHandler, ChargeShot_Handler chargeShotHandler, MameShellManager shellManager)
    {
        this.mameManager = shellManager;
        this.chargeShotHandler = chargeShotHandler;
        this.inputHandler = inputHandler;

        levelLower_EnergyBall = Resources.Load<GameObject>("LevelLowerShell");
        fullLevel_EnergyBall = Resources.Load<GameObject>("FullChargeBall");

        if (levelLower_EnergyBall == null)
            Debug.Log("LevelLowerShellがResourcesディレクトリにありません。確認してください!!");

        if (fullLevel_EnergyBall == null)
            Debug.Log("FullChargeBallがResourcesディレクトリにありません。確認してください!!");
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
                chargeShotHandler.Shoot_Charged_Shell(levelLower_EnergyBall);
            }
            else if (chargeShotHandler.IsFullCharged)
            {
                chargeShotHandler.Shoot_Charged_Shell(fullLevel_EnergyBall);
            }
        }
    }
}

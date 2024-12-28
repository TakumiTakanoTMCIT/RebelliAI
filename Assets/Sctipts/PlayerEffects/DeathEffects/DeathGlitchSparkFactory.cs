using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class DeathGlitchSparkFactory : MonoBehaviour
{
    [SerializeField] private float intervalTime = 0.01f, addYpos = 0.5f;
    [SerializeField] private Transform playerTransform;
    DeathGlitchSparkPoolHandler sparkPoolHandler;
    bool isEndAnimDeath = false;

    [SerializeField] bool PushThisBool_WillMakeEffects = false;

    public delegate void OnPlayerDeathEffectsInstanceDone();
    public static event OnPlayerDeathEffectsInstanceDone onPlayerDeathEffectsInstanceDone;

    private void Awake()
    {
        isEndAnimDeath = false;
        sparkPoolHandler = gameObject.MyGetComponent_NullChker<DeathGlitchSparkPoolHandler>();
        PushThisBool_WillMakeEffects = false;
    }

    private async void Update()
    {
        if (!PushThisBool_WillMakeEffects) return;
        PushThisBool_WillMakeEffects = false;
        await MakeDeathEffects();
    }

    async public UniTask MakeDeathEffects()
    {
        if (isEndAnimDeath) return;

        isEndAnimDeath = true;
        Vector3 instancePos = new Vector3(playerTransform.position.x, playerTransform.position.y + addYpos, playerTransform.position.z);
        for (int count = 0; count < sparkPoolHandler.maxInstanceCount; count++)
        {
            var spark = sparkPoolHandler.GetObject();
            spark.gameObject.MyGetComponent_NullChker<DeathGlitchSparkBody>().MyAwake(instancePos, this);
            await UniTask.Delay(TimeSpan.FromSeconds(intervalTime));
        }

        onPlayerDeathEffectsInstanceDone?.Invoke();
    }

    public void ReturnObject(GameObject obj)
    {
        sparkPoolHandler.ReturnObjct(obj);
    }
}

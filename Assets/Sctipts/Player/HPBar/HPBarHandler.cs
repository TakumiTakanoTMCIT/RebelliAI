using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Threading.Tasks;
using UniRx;

namespace HPBar
{
    public abstract class HPBarBase : MonoBehaviour
    {
        [SerializeField] protected GameObject midPrefab, BaseObj, hpUnitPrefab;
        [SerializeField] protected RectTransform topTransform;
        [SerializeField] protected float barHeight, addMidBarIntervalTime, intervalTime;
        [SerializeField] protected bool isDebugMode = false;
        [SerializeField] protected int currentLife, initMaxLife;
        [SerializeField] protected List<GameObject> hpUnitList;


        private int cacheMaxLife = 0;
        protected abstract void OnDead();
        protected abstract UniTask AddLife(int addAmount);
        protected abstract UniTask Heal(int healAmount);
        public abstract void Damage(int damageAmount);

        protected virtual void DamageLife(int damageAmount)
        {
            for (int count = 0; count < damageAmount; count++)
            {
                Destroy(hpUnitList[hpUnitList.Count - 1]);
                hpUnitList.RemoveAt(hpUnitList.Count - 1);

                currentLife--;
                if (currentLife <= 0)
                {
                    currentLife = 0;

                    OnDead();
                    return;
                }
            }
        }

        protected async Task MakeMidBar()
        {
            //中間のバーを作成
            var instance = Instantiate(midPrefab);
            instance.transform.SetParent(BaseObj.transform);
            instance.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, cacheMaxLife * barHeight);

            //蓋の位置を上げる
            Vector2 pos = topTransform.anchoredPosition;
            pos.y += barHeight;
            topTransform.anchoredPosition = pos;

            //キャッシュの値を増やす
            cacheMaxLife++;

            try
            {
                //インターバルだけ待つ
                if (isDebugMode) await UniTask.Yield();
                else await UniTask.Delay(TimeSpan.FromSeconds(addMidBarIntervalTime));
            }
            catch (Exception e)
            {
                Debug.LogError($"エラー発生しました、{e.Message}");
                //UnityEditor.EditorApplication.isPaused = true;
                return;
            }
        }

        protected async Task HealLife()
        {
            var instance = Instantiate(hpUnitPrefab);
            instance.transform.SetParent(BaseObj.transform);
            var rectTrasnform = instance.MyGetComponent_NullChker<RectTransform>();
            rectTrasnform.anchoredPosition = new Vector2(0, currentLife * barHeight);

            hpUnitList.Add(instance);
            currentLife++;
            SoundEffectCtrl.onPlayHealHPSound.OnNext(Unit.Default);

            try
            {
                if (isDebugMode) await UniTask.Yield();
                else await UniTask.Delay(TimeSpan.FromSeconds(intervalTime));
            }
            catch (Exception e)
            {
                Debug.LogError($"エラー発生しました : {e.Message}");
                //UnityEditor.EditorApplication.isPaused = true;
                return;
            }
        }
    }

    public class HPBarHandler : MonoBehaviour
    {
        [SerializeField] int InitialMaxLife = 1, InitialCurrentLife = 1;
        [SerializeField] int healAndDamageAmount = 5;
        [SerializeField] float intervalTime = 0.05f, addMidBarIntervalTime = 0.1f;
        [SerializeField] GameObject Base;
        [SerializeField] GameObject midPrefab;
        [SerializeField] GameObject Top;
        [SerializeField] GameObject hpUnit;
        [SerializeField] float barHeight = 6;
        [SerializeField] bool isDebugMode = false;
        [SerializeField] GamePlayerManager gamePlayerManager;

        int cacheMaxLife;

        bool isDead = false;

        RectTransform topTransform;

        [SerializeField] List<GameObject> hpUnitList;

        [SerializeField] private int playerMaxLife = 0, currentLife = 0;

        public static event Action onPlayerDeath, onPlayerDamage;

        public Subject<Unit> onPlayerInVoid = new Subject<Unit>();

        /// <summary>
        /// コンポーネントのインスタンスの取得のみを行う
        /// </summary>
        private void Awake()
        {
            onPlayerInVoid.Subscribe(_ =>
            {
                if (gamePlayerManager.isInGameArea) Damage(playerMaxLife);
            })
            .AddTo(this);

            topTransform = Top.MyGetComponent_NullChker<RectTransform>();
            hpUnitList = new List<GameObject>();
        }

        private async void Start()
        {
            topTransform.anchoredPosition = Vector2.zero;

            await AddLife(InitialMaxLife);
            Heal(InitialCurrentLife);
        }

        //デバッグ用です
        //isDebugModeがtrueの時のみ、以下の処理が行われます
        private async void Update()
        {
            if (!isDebugMode) return;

            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                Debug.LogWarning("Playerを死なせます: HPBarHandler");
                Damage(playerMaxLife);
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                Damage(healAndDamageAmount);
            }

            if (Input.GetKeyDown(KeyCode.X))
            {
                Heal(healAndDamageAmount);
            }

            if (Input.GetKeyDown(KeyCode.Z))
            {
                await AddLife(2);
            }

            if (Input.GetKeyDown(KeyCode.V))
            {
                Heal(playerMaxLife);
            }
        }

        public async void Heal(int healAmount)
        {
            if (healAmount <= 0)
                return;

            for (int i = 0; i < healAmount; i++)
            {
                if (currentLife >= playerMaxLife)
                {
                    return;
                }

                await HealLife();
            }
        }

        public void Damage(int damageAmount)
        {
            if (damageAmount <= 0)
            {
                Debug.Log("プラスの値を入力しないとダメージを受けません");
                return;
            }
            DamageLife(damageAmount);
        }

        public async UniTask AddLife(int addAmount)
        {
            if (addAmount <= 0)
                return;

            cacheMaxLife = playerMaxLife;

            //数値上だけ増やしたあとに、見た目の処理をします
            playerMaxLife += addAmount;

            for (int i = 0; i < addAmount; i++)
            {
                await MakeMidBar();
            }
        }

        /// <summary>
        /// これより下は、内部処理を行う関数です。決して外部から呼び出さないでください。
        /// 呼び出すときには、上記の関数を使用してください。
        /// </summary>

        void DamageLife(int damageAmount)
        {
            for (int count = 0; count < damageAmount; count++)
            {
                if (isDead) return;

                currentLife--;

                try
                {
                    Destroy(hpUnitList[hpUnitList.Count - 1]);
                }
                catch (Exception e)
                {
                    Debug.Log($"[hpUnitList.Count - 1] : {hpUnitList.Count - 1}");
                    Debug.Log($"currentLife : {currentLife}");
                    Debug.LogError($"エラー発生しました、{e.Message}");
                    //UnityEditor.EditorApplication.isPaused = true;
                    return;
                }

                try
                {
                    hpUnitList.RemoveAt(hpUnitList.Count - 1);
                    Debug.Log($"hpUnitList.Count : {hpUnitList.Count}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"エラー発生しました、{e.Message}");
                    //UnityEditor.EditorApplication.isPaused = true;
                    return;
                }

                if (hpUnitList.Count == 0)
                {
                    isDead = true;
                    Debug.LogWarning("HPUnitListが空です。");
                    onPlayerDeath?.Invoke();
                    SoundEffectCtrl.OnPlayDeathSE.OnNext(1);
                    return;
                    /*Debug.LogWarning("HPUnitListが空です。");
                    Debug.Log($"currentLife : {currentLife}");
                    //UnityEditor.EditorApplication.isPaused = true;
                    return;*/
                }

                //最初に体力を減らしたときのみ呼び出す
                if (count == 0)
                {
                    onPlayerDamage?.Invoke();
                    SoundEffectCtrl.OnPlayDamageSE.OnNext(0);
                }

                /*if (currentLife <= 0)
                {
                    currentLife = 0;
                    onPlayerDeath?.Invoke();
                    return;
                }*/
            }
        }

        async Task MakeMidBar()
        {
            //中間のバーを作成
            var instance = Instantiate(midPrefab);
            instance.transform.SetParent(Base.transform);
            instance.transform.position = new Vector3(Base.transform.position.x, Base.transform.position.y + (cacheMaxLife * barHeight));

            //蓋の位置を上げる
            Vector2 pos = topTransform.anchoredPosition;
            pos.y += barHeight;
            topTransform.anchoredPosition = pos;

            //キャッシュの値を増やす
            cacheMaxLife++;

            try
            {
                //インターバルだけ待つ
                if (isDebugMode) await UniTask.Yield();
                else await UniTask.Delay(TimeSpan.FromSeconds(addMidBarIntervalTime));
            }
            catch (Exception e)
            {
                Debug.LogError($"エラー発生しました、{e.Message}");
                //UnityEditor.EditorApplication.isPaused = true;
                return;
            }
        }

        async Task HealLife()
        {
            var instance = Instantiate(hpUnit);
            instance.transform.SetParent(Base.transform);
            var rectTrasnform = instance.MyGetComponent_NullChker<RectTransform>();
            rectTrasnform.anchoredPosition = new Vector2(0, currentLife * barHeight);

            hpUnitList.Add(instance);
            currentLife++;
            SoundEffectCtrl.onPlayHealHPSound.OnNext(Unit.Default);

            try
            {
                if (isDebugMode) await UniTask.Yield();
                else await UniTask.Delay(TimeSpan.FromSeconds(intervalTime));
            }
            catch (Exception e)
            {
                Debug.LogError($"エラー発生しました : {e.Message}");
                //UnityEditor.EditorApplication.isPaused = true;
                return;
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Threading.Tasks;
using UniRx;
using Zenject;

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
        [SerializeField] GamePlayerManager gamePlayerManager;

        public static event Action onPlayerDeath, onPlayerDamage;

        public Subject<Unit> onPlayerInVoid = new Subject<Unit>();

        //ここでPlayerHPのインスタンスを作成したくない、ZenjectでInjectするようにします。とにかく実装するためにここで作成します。
        PlayerHPVisual playerHPVisual;
        PlayerHP playerHP;

        [Inject]
        public void Construct(PlayerHP playerHP, PlayerHPVisual playerHPVisual)
        {
            this.playerHP = playerHP;
            this.playerHPVisual = playerHPVisual;
        }

        /// <summary>
        /// コンポーネントのインスタンスの取得のみを行う
        /// </summary>
        private void Awake()
        {
            onPlayerInVoid.Subscribe(_ =>
            {
                if (gamePlayerManager.isInGameArea) playerHP.Damage(playerHP.MaxLife);
            })
            .AddTo(this);
        }

        private void Start()
        {
            playerHPVisual.InitVisual();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                playerHPVisual.ChangeMaxLife(1);
            }

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                playerHPVisual.ChangeMaxLife(-1);
            }
        }

        /*public async UniTask AddLife(int addAmount)
        {
            if (addAmount <= 0)
            {
                Debug.LogError("プラスの値を入力しないと体力が増えません");
                return;
            }

            //TODO : ここで謎にキャッシュを取っているので、後で調べる
            cacheMaxLife = playerHP.MaxLife;

            //数値上の処理
            playerHP.Heal(addAmount);

            //見た目の処理
            for (int i = 0; i < addAmount; i++)
            {
                await MakeMidBar();
            }
        }

        /// <summary>
        /// これより下は、内部処理を行う関数です。決して外部から呼び出さないでください。
        /// 呼び出すときには、上記の関数を使用してください。
        /// </summary>

        //TODO : 描画の処理と数値の処理を分ける
        /*void DamageLife(int damageAmount)
        {
            playerHP.Damage(damageAmount);

            for (int count = 0; count < damageAmount; count++)
            {
                if (playerStats.IsDead) return;

                try
                {
                    Destroy(hpUnitList[hpUnitList.Count - 1]);
                }
                catch (Exception e)
                {
                    Debug.Log($"[hpUnitList.Count - 1] : {hpUnitList.Count - 1}");
                    Debug.Log($"currentLife : {playerHP.CurrentLife}");
                    Debug.LogError($"エラー発生しました、{e.Message}");
                    return;
                }

                try
                {
                    hpUnitList.RemoveAt(hpUnitList.Count - 1);
                }
                catch (Exception e)
                {
                    Debug.LogError($"エラー発生しました、{e.Message}");
                    return;
                }

                if (hpUnitList.Count == 0)
                {
                    playerStats.Dead();
                    onPlayerDeath?.Invoke();
                    SoundEffectCtrl.OnPlayDeathSE.OnNext(1);
                    return;
                }

                //最初に体力を減らしたときのみ呼び出す
                if (count == 0)
                {
                    onPlayerDamage?.Invoke();
                    SoundEffectCtrl.OnPlayDamageSE.OnNext(0);
                }
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
            rectTrasnform.anchoredPosition = new Vector2(0, playerHP.CurrentLife * barHeight);

            hpUnitList.Add(instance);
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
        }*/
    }

    /// <summary>
    /// 見た目の処理を行うクラスです
    /// </summary>
    public class PlayerHPVisual
    {
        //Injectするために必要な変数
        GameObject Base;
        GameObject midPrefab;
        GameObject TopObj;
        GameObject hpUnit;
        PlayerHP playerHP;


        //TODO : マジックナンバーはあぶないっす！
        int barHeight = 12;

        List<GameObject> hpUnitList = new List<GameObject>();
        List<GameObject> midBarList = new List<GameObject>();

        RectTransform topTransform;

        int cachedMaxLifeYPos = 0;

        [Inject]
        public PlayerHPVisual(PlayerHP playerHP, [Inject(Id = "Base")] GameObject Base, [Inject(Id = "Mid")] GameObject midPrefab, [Inject(Id = "Top")] GameObject TopObj, [Inject(Id = "HPUnit")] GameObject hpUnit)
        {
            this.playerHP = playerHP;

            this.Base = Base;
            this.midPrefab = midPrefab;
            this.TopObj = TopObj;
            this.hpUnit = hpUnit;
        }

        public void InitVisual()
        {
            //TopObjを初期化
            topTransform = TopObj.GetComponent<RectTransform>();
            Vector2 pos = topTransform.anchoredPosition;
            pos.y = 0f;
            topTransform.anchoredPosition = pos;

            Debug.Log("初期化できました");

            //初期最大HP分だけ初期化
            ChangeMaxLife(playerHP.MaxLife);
            /*ChangeCurrentLife(playerHP.MaxLife);*/
        }

        //amountの値分だけ最大体力を変更する
        public void ChangeMaxLife(int amount)
        {
            if (amount > 0)
            {
                for (int i = 0; i < amount; i++)
                {
                    //中間のバーを作成
                    var instance = GameObject.Instantiate(midPrefab);
                    var instanceTrans = instance.GetComponent<RectTransform>();
                    midBarList.Add(instance);

                    //中間のバーの位置を変更
                    instance.transform.SetParent(Base.transform);
                    instanceTrans.anchoredPosition = new Vector3(
                        0,
                        cachedMaxLifeYPos + (barHeight * i));
                    //なぜcachedMaxLifeYPosを使うのか？
                    //中間バーを作成するたびに基準点をcachedMaxLifeYPosに加算しているためです。

                    //TopObjを中間バーの子オブジェクトにして、座標を0にすると自動的に中間バーの上に配置される
                    //アンカーを正しく設定しているためです。
                    //子オブジェクトにしたら0の座標が中間バーの上となります
                    //毎回forで実行する必要が無い気がしますが、"最後のforのみ実行"ということができないので毎回実行するようにしています。
                    TopObj.transform.SetParent(instance.transform);
                    topTransform.anchoredPosition = Vector2.zero;
                }

                cachedMaxLifeYPos += barHeight * amount;

                return;
            }
            else if (amount < 0)
            {
                if(midBarList.Count == 0)
                {
                    return;
                }

                amount = -amount;

                //TopObjの親子関係をBaseに変更
                //これをしないと、上から中間バーを消してしまうとTopObjが消えてしまいます
                TopObj.transform.SetParent(Base.transform);

                for (int i = 0; i < amount; i++)
                {
                    //中間のバーを削除
                    GameObject.Destroy(midBarList[midBarList.Count - 1]);
                    midBarList.RemoveAt(midBarList.Count - 1);

                    //最上位の中間バーの位置を正す
                    cachedMaxLifeYPos -= barHeight;

                    //forが最後のループのときだけTopObjの位置を変更
                    //TopObjの親子関係を一番上の中間バーに変更
                    if (i == amount - 1)
                    {
                        if(midBarList.Count == 0)
                        {
                            //TopObjの位置を変更
                            topTransform.anchoredPosition = Vector2.zero;

                            //親子関係もBaseに変更
                            //明示的に戻しておきます
                            TopObj.transform.SetParent(Base.transform);
                            return;
                        }

                        //TopObjの位置を変更
                        Vector2 pos = topTransform.anchoredPosition;
                        pos.y = midBarList[midBarList.Count - 1].GetComponent<RectTransform>().anchoredPosition.y;
                        topTransform.anchoredPosition = pos;

                        //TopObjの親子関係を一番上の中間バーに変更
                        //座標も正しておく
                        TopObj.transform.SetParent(midBarList[midBarList.Count - 1].transform);
                        topTransform.anchoredPosition = Vector2.zero;
                    }
                }
                return;
            }
            else
            {
                Debug.LogError("0を入力することはできません");
                return;
            }
        }

        //amountの値分だけ現在の体力を変更する
        public void ChangeCurrentLife(int amount)
        {
            int cachedCurrentLife = playerHP.CurrentLife;

            if (amount > 0)
            {
                for (int i = 0; i > amount; i++)
                {
                    if (hpUnitList.Count == playerHP.MaxLife)
                    {
                        return;
                    }

                    //HPのユニットを作成
                    var instance = GameObject.Instantiate(hpUnit);
                    var instanceTrans = instance.GetComponent<RectTransform>();
                    hpUnitList.Add(instance);

                    instance.transform.SetParent(Base.transform);
                    instanceTrans.anchoredPosition = new Vector3(
                        0,
                        (cachedCurrentLife * barHeight) + (i * barHeight));
                }
            }
            else if (amount < 0)
            {
                for (int i = 0; i < -amount; i++)
                {
                    //HPのユニットを削除
                    GameObject.Destroy(hpUnitList[hpUnitList.Count - 1]);
                    hpUnitList.RemoveAt(hpUnitList.Count - 1);

                    if (hpUnitList.Count == 0)
                    {
                        return;
                    }
                }
            }
            else
            {
                Debug.LogError("0を入力することはできません");
                return;
            }
        }
    }

    public interface IPlayerHP
    {
        void Heal(int amount);
        void Damage(int amount);
        void UpdateMaxLife(int amount);
    }

    /// <summary>
    /// プレイヤーのHPを数値上で管理するクラスです
    /// </summary>
    public class PlayerHP : IPlayerHP
    {
        public int MaxLife => maxLife;
        public int CurrentLife => currentLife;
        private int maxLife, currentLife;

        public PlayerHP()
        {
            maxLife = 10;
            currentLife = maxLife;
        }

        public void Heal(int amount)
        {
            if (amount <= 0)
            {
                Debug.LogError("プラスの値を入力しないと体力が増えません");
                return;
            }

            currentLife += amount;
        }

        public void Damage(int amount)
        {
            if (amount <= 0)
            {
                Debug.LogError("プラスの値を入力しないとダメージを受けません");
                return;
            }

            currentLife -= amount;
            if (currentLife <= 0)
            {
                currentLife = 0;
            }
        }

        public void UpdateMaxLife(int amount)
        {
            if (amount == 0)
            {
                Debug.LogError("0を入力することはできません");
                return;
            }

            maxLife += amount;
            if (maxLife < 0)
            {
                maxLife = 0;
            }
        }
    }

    /// <summary>
    /// プレイヤーのステータスを管理するクラスです
    /// </summary>
    public class PlayerStats
    {
        public bool IsDead => isDead;
        private bool isDead = false;

        public void Dead()
        {
            isDead = true;
        }
    }
}

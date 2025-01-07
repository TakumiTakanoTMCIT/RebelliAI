using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ObjectPoolFactory;
using Zenject;

/// <summary>
/// このクラスの責務は、ワープエフェクトのアニメーションを制御することです。
/// </summary>
namespace Warp
{
    public class WarpEffectBody : MonoBehaviour
    {
        //エフェクトが表示される時間
        private float showingTime;

        //各クラスのインスタンスを生成
        private AnimatorCtrl animatorCtrl;
        private Timer timer;
        private PositionSetter positionSetter;

        //Injeect
        private Warp.PoolHandler poolHandler;

        [Inject]
        public void Construct(Warp.PoolHandler poolHandler)
        {
            this.poolHandler = poolHandler;
        }

        //オブジェクトプールで最初に生成されるので、このメソッドで初期化する
        private void Awake()
        {
            //各クラスのインスタンスを生成
            animatorCtrl = new AnimatorCtrl(gameObject.MyGetComponent_NullChker<Animator>());
            poolHandler.Init(gameObject);
            timer = new Timer(poolHandler);
            positionSetter = new PositionSetter(gameObject);
        }

        //ActiveSelfがfalseになったら、アニメーションをリセットする
        private void OnDisable()
        {
            animatorCtrl.ResetAnim();
        }

        //生成されたら、ポジションをXをプレイヤーの位置に、Yを指定の位置にする（YはFactoryで上から下になるように設定されます）
        //どのアニメーションにするのかここでランダムに指定する
        public void Init(float showingTime, float playerPosX, float PosY)
        {
            //TODO:ここの数値をfactoryiinfoから取得するようにする(Inject)
            this.showingTime = showingTime;
            positionSetter.SetPosition(playerPosX, PosY);
            animatorCtrl.StartAnim();
        }

        //アニメーションイベント
        void OnFinishStart()
        {
            animatorCtrl.FinishStart();
            timer.CountReleaseTime(showingTime).Forget();
        }
    }

    //アニメーションの制御をするクラス
    public class AnimatorCtrl
    {
        Animator animator;
        public AnimatorCtrl(Animator animator)
        {
            this.animator = animator;
        }

        //生成時
        public void StartAnim()
        {
            animator.SetInteger("RandColor", UnityEngine.Random.Range(1, 5));
        }

        //スタートアニメーション終了時
        public void FinishStart()
        {
            animator.SetBool("isStart", true);
        }

        //消滅時
        public void ResetAnim()
        {
            animator.SetBool("isStart", false);
            animator.SetInteger("RandColor", 0);
        }
    }

    //表示する時間を制御するクラス
    public class Timer
    {
        private Warp.PoolHandler poolHadnler;

        public Timer(Warp.PoolHandler poolHadnler)
        {
            this.poolHadnler = poolHadnler;
        }

        public async UniTask CountReleaseTime(float showingTime)
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(showingTime));
            }
            catch (Exception e)
            {
                Debug.Log(e);
                return;
            }
            finally
            {
                //エフェクトを消す
                poolHadnler.ReturnMeToPool();
            }
        }
    }

    //プールを制御するクラス
    //自分自身をいつプールに返すかを制御するクラスです！
    public class PoolHandler
    {
        private WarpPool _factory;
        private GameObject _mySelftObj;

        public void Init(GameObject mySelftObj)
        {
            _mySelftObj = mySelftObj;
        }

        [Inject]
        public PoolHandler(WarpPool factory)
        {
            this._factory = factory;
        }

        public void ReturnMeToPool()
        {
            _factory.ReturnObject(_mySelftObj);
        }
    }

    //自分の座標を制御するクラス
    public class PositionSetter
    {
        private GameObject mySelftObj;

        public PositionSetter(GameObject mySelftObj)
        {
            this.mySelftObj = mySelftObj;
        }

        public void SetPosition(float playerPosX, float PosY)
        {
            mySelftObj.transform.position = new Vector3(playerPosX, PosY, 0);
        }
    }
}

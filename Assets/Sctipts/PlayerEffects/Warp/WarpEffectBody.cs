using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

/// <summary>
/// このクラスの責務は、ワープエフェクトのアニメーションを制御することです。
/// </summary>
namespace Warp
{
    public class WarpEffectBody : MonoBehaviour
    {
        public class Factory : PlaceholderFactory<WarpEffectBody> { }

        //エフェクトが表示される時間
        private float showingTime;

        //Inject
        private Timer timer;
        private AnimatorCtrl animatorCtrl;
        private PositionSetter positionSetter;

        //ActiveSelfがfalseになったら、アニメーションをリセットする
        private void OnDisable()
        {
            animatorCtrl.ResetAnim();
        }

        [Inject]
        public void Construct(Timer timer, AnimatorCtrl animatorCtrl, PositionSetter positionSetter)
        {
            this.timer = timer;

            this.animatorCtrl = animatorCtrl;
            this.animatorCtrl.Construct(gameObject.MyGetComponent_NullChker<Animator>());

            this.positionSetter = positionSetter;
            this.positionSetter.Construct(gameObject);
        }

        //生成されたら、ポジションをXをプレイヤーの位置に、Yを指定の位置にする（YはFactoryで上から下になるように設定されます）
        //どのアニメーションにするのかここでランダムに指定する
        public void Init(float showingTime)
        {
            this.showingTime = showingTime;
        }

        public void StartDirection(float playerPosX, float PosY)
        {
            positionSetter.SetPosition(playerPosX, PosY);
            animatorCtrl.StartAnim();
        }

        //アニメーションイベント
        void OnFinishStart()
        {
            animatorCtrl.FinishStart();
            timer.CountReleaseTime(showingTime, gameObject).Forget();
        }
    }

    //アニメーションの制御をするクラス
    public class AnimatorCtrl
    {
        Animator animator;
        public void Construct(Animator animator)
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
    //Installerで登録します。
    public class Timer
    {
        private PoolHandler poolHandler;

        [Inject]
        public Timer(PoolHandler poolHandler)
        {
            this.poolHandler = poolHandler;
        }

        public async UniTask CountReleaseTime(float showingTime, GameObject myselfObj)
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
                //Debug.Log($"ReturnMeToPool : {poolHandler}");
                poolHandler.ReturnMeToPool(myselfObj);
            }
        }
    }

    //プールを制御するクラス
    //自分自身をいつプールに返すかを制御するクラスです！
    //Installerにてインスタンス化されます
    public class PoolHandler
    {
        private Action<GameObject> releaseObjCallBack;

        public void SetReleaseObjCallBack(Action<GameObject> releaseObjCallBack)
        {
            this.releaseObjCallBack = releaseObjCallBack;
        }

        public void ReturnMeToPool(GameObject myselfObj)
        {
            releaseObjCallBack?.Invoke(myselfObj);
        }
    }

    //自分の座標を制御するクラス
    public class PositionSetter
    {
        private GameObject mySelftObj;

        public void Construct(GameObject mySelftObj)
        {
            this.mySelftObj = mySelftObj;
        }

        public void SetPosition(float playerPosX, float PosY)
        {
            mySelftObj.transform.position = new Vector3(playerPosX, PosY, 0);
        }
    }
}

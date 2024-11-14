using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ObjectPoolFactory;

public class WarpEffectBody : MonoBehaviour
{
    Animator animator;
    WarpFactory factory;

    private void Awake()
    {
        animator = gameObject.MyGetComponent_NullChker<Animator>();
    }

    private void OnDisable()
    {
        animator.SetBool("isStart", false);
        animator.SetInteger("RandColor", 0);
    }

    //生成されたら、ポジションをXをプレイヤーの位置に、Yを指定の位置にする（YはFactoryで上から下になるように設定されます）
    //どのアニメーションにするのかここでランダムに指定する
    public void Init(WarpFactory factory, float playerPosX, float PosY)
    {
        this.factory = factory;
        transform.position = new Vector3(playerPosX, PosY, 0);
        animator.SetInteger("RandColor", UnityEngine.Random.Range(1, 5));
    }

    //アニメーションイベント
    void OnFinishStart()
    {
        animator.SetBool("isStart", true);
        CountReleaseTime().Forget();
    }

    //ある程度アニメーションしたら、エフェクトを消す
    async UniTask CountReleaseTime()
    {
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(factory.showingTime));
        }
        catch (Exception e)
        {
            Debug.Log(e);
            //UnityEditor.EditorApplication.isPaused = true;
            return;
        }

        factory.ReleaseWarpEffect(gameObject);
    }
}

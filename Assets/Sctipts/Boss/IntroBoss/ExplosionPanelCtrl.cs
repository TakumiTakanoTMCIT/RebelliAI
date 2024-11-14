using UnityEngine;
using UnityEngine.UI;
using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UniRx;

public class ExplosionPanelCtrl : MonoBehaviour
{
    [SerializeField] ExplodeDirection explodeDirection;
    [SerializeField] private GameObject explosionPanel;
    [SerializeField] float explosionFadeOutTime = 1f, explosionDelayTime = 1f;
    [SerializeField] PlayerAnimStateHandler playerAnimStateHandler;
    Image panelImage;

    public static event Action onFinishExplosionPanel;

    private Subject<Unit> onCompletedExplosionDirection = new Subject<Unit>();
    public IObservable<Unit> OnCompletedExplosionDirection => onCompletedExplosionDirection;

    private void Awake()
    {
        panelImage = explosionPanel.GetComponent<Image>();
        explosionPanel.SetActive(false);
    }

    private void OnEnable()
    {
        BossCutSceneHandler.onExplode += ShowExplosionPanel;
        ExplodeDirection.onFinishExplodeDirection += HideExplosionPanel;
    }

    private void OnDisable()
    {
        BossCutSceneHandler.onExplode -= ShowExplosionPanel;
        ExplodeDirection.onFinishExplodeDirection -= HideExplosionPanel;
    }

    private async void ShowExplosionPanel()
    {
        explosionPanel.SetActive(true);

        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(explosionDelayTime));
        }
        catch (Exception e)
        {
            Debug.Log($"awaitでエラーが起きました。{e}");
            //UnityEditor.EditorApplication.isPaused = true;
            return;
        }

        try
        {
            await panelImage.DOFade(endValue: 1f, duration: explodeDirection.explosionDirectionTime - explosionDelayTime);
        }
        catch (Exception e)
        {
            Debug.Log($"awaitでエラーが起きました。{e}");
            //UnityEditor.EditorApplication.isPaused = true;
            return;
        }

        //白飛びしたらプレイヤーのアニメーションをニュートラルにする
        playerAnimStateHandler.ChangeAnimState(playerAnimStateHandler.neutralIdleState);

        onFinishExplosionPanel?.Invoke();
    }

    async void HideExplosionPanel()
    {
        try
        {
            await panelImage.DOFade(endValue: 0f, duration: explosionFadeOutTime);
        }
        catch (Exception e)
        {
            Debug.Log($"awaitでエラーが起きました。{e}");
            //UnityEditor.EditorApplication.isPaused = true;
            return;
        }

        explosionPanel.SetActive(false);
        onCompletedExplosionDirection?.OnNext(Unit.Default);
    }
}

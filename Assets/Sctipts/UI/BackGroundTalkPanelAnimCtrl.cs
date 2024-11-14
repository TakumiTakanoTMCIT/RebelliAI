using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading;

public class BackGroundTalkPanelAnimCtrl : MonoBehaviour
{
    [SerializeField] private RectTransform talkBackGround;
    [SerializeField] private Vector2 defaultTalkBackGroundSize;
    [SerializeField] private float talkBackGroundShowTime = 1.0f;
    CancellationTokenSource cts;

    public static event Action onCompletedShowTalkPanel, onHideBackGround;

    private void Awake()
    {
        talkBackGround.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 100);
        talkBackGround.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 50);
    }

    private void OnEnable()
    {
        TalkSystem.onEndTalk += HideTalkPanel;
        TalkSystem.onShowTalkPanel += ShowTalkPanel;
    }

    private void OnDisable()
    {
        TalkSystem.onEndTalk -= HideTalkPanel;
        TalkSystem.onShowTalkPanel -= ShowTalkPanel;
    }

    private async void HideTalkPanel()
    {
        try
        {
            await talkBackGround.DOSizeDelta(Vector2.zero, talkBackGroundShowTime)
            .SetEase(Ease.Linear);
        }
        catch (Exception e)
        {
            Debug.LogError($"awaitでエラーが起きました。{e}");
            return;
        }

        onHideBackGround?.Invoke();
    }

    private async void ShowTalkPanel()
    {
        cts = new CancellationTokenSource();
        try
        {
            await talkBackGround.DOSizeDelta(defaultTalkBackGroundSize, talkBackGroundShowTime)
            .SetEase(Ease.Linear)
            .ToUniTask(tweenCancelBehaviour: TweenCancelBehaviour.Complete, cancellationToken: cts.Token);
        }
        catch (Exception e)
        {
            Debug.LogError($"awaitでエラーが起きました。{e}");
            return;
        }

        onCompletedShowTalkPanel?.Invoke();
    }

    private void OnDestroy()
    {
        if (cts != null)
        {
            cts.Cancel();
            cts.Dispose();
            cts = null;
        }
    }
}

using UnityEngine;
using TMPro;
using Cysharp.Threading.Tasks;
using System;
using KeyHandler;
using DG.Tweening;
using System.Threading;
using UniRx;

public enum Talker
{
    Player,
    Navi,
    Boss
}

[Serializable]
public class TalkBase
{
    [SerializeField][Multiline] public string messages;
    [SerializeField] public Talker talker;
}

public class TalkSystem : MonoBehaviour
{
    [SerializeField] private Sprite player, navi, boss;
    [SerializeField] private TalkBase[] talkBases;
    [SerializeField] private GameObject talkPanel, gyakuTriangle;
    [SerializeField] private TMP_Text textTMP;
    [SerializeField] private float textSpeed = 0.1f;

    [SerializeField] private bool isDebugMode = false;

    [SerializeField] int talkCount = 0;

    [SerializeField] bool skipTalk = false, isShowingChar = false, isWaitCharShow = false, isEndTalk = false;

    [SerializeField] internal bool isShowedTalkPanel = false, isStartShowTalkPanel = false;

    public static event Action onShowTalkPanel, onEndTalk;

    private void OnEnable()
    {
        InputHandler.onTalkKeyDown += OnTalkButton;
        BackGroundTalkPanelAnimCtrl.onCompletedShowTalkPanel += OnCompletedShowTalkPanel;
        BackGroundTalkPanelAnimCtrl.onHideBackGround += EndTalk;
        onEndTalk += HideTalkPanel;

        isShowedTalkPanel = false;

        gyakuTriangle.SetActive(false);
        talkPanel.SetActive(false);

        textTMP.text = "";
        talkCount = 0;

        skipTalk = false;
        isShowingChar = false;

        if (isDebugMode) Debug.Log($"会話数 :{talkBases.Length}");
    }

    private void OnDisable()
    {
        InputHandler.onTalkKeyDown -= OnTalkButton;
        BackGroundTalkPanelAnimCtrl.onCompletedShowTalkPanel -= OnCompletedShowTalkPanel;
        BackGroundTalkPanelAnimCtrl.onHideBackGround -= EndTalk;
        onEndTalk -= HideTalkPanel;
    }

    //イベントハンドラ
    void OnCompletedShowTalkPanel()
    {
        if (!isStartShowTalkPanel) return;
        if (isShowedTalkPanel) return;

        Debug.LogAssertion("会話パネル表示完了");
        ShowChar().Forget();
        isShowedTalkPanel = true;
    }

    void HideTalkPanel()
    {
        gyakuTriangle.SetActive(false);
        textTMP.text = "";
    }

    private async UniTask ShowChar()
    {
        Debug.LogAssertion("ShowCharが呼ばれました");
        isWaitCharShow = true;
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1));
        }
        catch (Exception e)
        {
            Debug.LogError($"awaitでエラーが起きました。{e}");
            return;
        }
        Debug.Log("ShowCharで文字表示");
        isWaitCharShow = false;
        CharShow();
    }

    public void StartTalk()
    {
        Debug.Log($"会話パネル表示 : {gameObject.name}");

        isStartShowTalkPanel = true;
        isShowedTalkPanel = false;
        isEndTalk = false;

        talkPanel.SetActive(true);
        onShowTalkPanel?.Invoke();
        return;
    }

    //イベントハンドラ
    public void OnTalkButton()
    {
        if (isEndTalk) return;
        if (!isStartShowTalkPanel) return;

        Debug.Log($"gameobject : {gameObject.name}");
        //会話が終了していたら会話終了のアニメーションをする
        if (talkCount == talkBases.Length)
        {
            Debug.Log("会話終了:isEndTalk");
            isEndTalk = true;
            onEndTalk?.Invoke();
            return;
        }

        //パネルをアニメーションで表示している途中なら、文字表示を待機する（トークボタンを押しても下↓の処理を実行しない）
        if (!isShowedTalkPanel) return;

        if (isWaitCharShow)
        {
            Debug.Log("文字表示待機中");
            return;
        }

        //文字の表示を開始
        if (!isShowingChar)
        {
            Debug.Log("文字表示開始");
            CharShow();
            return;
        }
        else
        {
            SkipCharShow();
        }
    }

    async void CharShow()
    {
        Debug.Log("１文字づつ表示開始");
        isShowingChar = true;
        gyakuTriangle.SetActive(false);
        textTMP.text = talkBases[talkCount].messages;

        for (int count = 0; count < talkBases[talkCount].messages.Length; count++)
        {
            if (skipTalk)
            {
                skipTalk = false;
                break;
            }

            textTMP.maxVisibleCharacters = count + 1;
            SoundEffectCtrl.onPlayCharShowSE.OnNext(Unit.Default);
            await UniTask.Delay(TimeSpan.FromSeconds(textSpeed));
        }

        Debug.Log("声をすべて表示完了");
        textTMP.maxVisibleCharacters = talkBases[talkCount].messages.Length;
        EndCharShow();
    }

    void SkipCharShow()
    {
        skipTalk = true;
    }

    void EndCharShow()
    {
        talkCount++;
        isShowingChar = false;
        gyakuTriangle.SetActive(true);

        Debug.Log($"talkCount : {talkCount} messages.Length : {talkBases[talkCount].messages.Length}");
    }

    void EndTalk()
    {
        talkPanel.SetActive(false);
        gyakuTriangle.SetActive(false);
    }
}

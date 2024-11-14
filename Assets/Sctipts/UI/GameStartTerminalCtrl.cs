using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class GameStartTerminalCtrl : MonoBehaviour
{
    [SerializeField] private Vector2 randomInstanceTime;
    [SerializeField] private float waitForChangeCheckIcon, waitForChangeTime, completeProcessWidth = 100f, posXZurasu = 25f, blinkingInterval = 0.03f;
    [SerializeField] private int makeProcessCount = 8;
    [SerializeField] private byte decreaseAlphaValue = 1;

    [SerializeField] private RectTransform coreModuleTransform, bootingTransform, completeProcessTransform;
    [SerializeField] private GameObject processStatusObj, cursolObj, standbyObj, terminalPanelObj;

    [SerializeField] private Sprite greenImage, CheckImage, bootingStopImage;

    [SerializeField] List<GameObject> processList;

    bool isFadeOutFinished = false;

    public Subject<Unit> onStartStandbyTerminal = new Subject<Unit>();

    private void Awake()
    {
        standbyObj.SetActive(false);
        terminalPanelObj.SetActive(false);

        isFadeOutFinished = false;

        processList = new List<GameObject>();
        for (int count = 0; count < makeProcessCount; count++)
        {
            var processObj = Instantiate(processStatusObj);
            processObj.transform.SetParent(terminalPanelObj.transform);
            processObj.gameObject.MyGetComponent_NullChker<RectTransform>().anchoredPosition = new Vector2(posXZurasu + (count * completeProcessWidth), 0);
            processObj.SetActive(false);
            processList.Add(processObj);
        }

        onStartStandbyTerminal.Subscribe(_ =>
        {
            AwakeTerminal().Forget();
        });
    }

    async public UniTask AwakeTerminal()
    {
        //Debug.Log("Awake Termianl");
        terminalPanelObj.SetActive(true);
        cursolObj.SetActive(true);

        //EditorApplication.isPaused = true;
        foreach (var process in processList)
        {
            process.SetActive(true);
            //Debug.Log("SetActive processor");
        }

        //Debug.Log("少し待機");
        await UniTask.Delay(TimeSpan.FromSeconds(waitForChangeTime));

        for (int count = 0; count < processList.Count; count++)
        {
            Image imageComponent;
            try
            {
                imageComponent = processList[count].MyGetComponent_NullChker<Image>();
            }
            catch (Exception e)
            {
                Debug.Log(e);
                //UnityEditor.EditorApplication.isPaused = true;
                return;
            }
            imageComponent.sprite = greenImage;
            UISoundCtrl.onPlayPassTheGreenSE.OnNext(Unit.Default);
            await UniTask.Delay(TimeSpan.FromSeconds(UnityEngine.Random.Range(randomInstanceTime.x, randomInstanceTime.y)));
        }

        //Debug.Log("更に少し待機");
        await UniTask.Delay(TimeSpan.FromSeconds(waitForChangeCheckIcon));
        for (int count = 0; count < processList.Count; count++)
        {
            var imageComponent = processList[count].MyGetComponent_NullChker<Image>();
            imageComponent.sprite = CheckImage;
        }
        completeProcessTransform.gameObject.SetActive(true);
        cursolObj.SetActive(false);

        Vector2 pos = bootingTransform.anchoredPosition;
        pos.y += completeProcessWidth;
        bootingTransform.anchoredPosition = pos;
        bootingTransform.gameObject.MyGetComponent_NullChker<Animator>().SetTrigger("OnComplete");
        bootingTransform.gameObject.MyGetComponent_NullChker<Image>().sprite = bootingStopImage;

        pos = coreModuleTransform.anchoredPosition;
        pos.y += completeProcessWidth;
        coreModuleTransform.anchoredPosition = pos;

        //フェードアウト開始
        FadeOutTerminalPanel();

        bool isOn = true;
        while (true)
        {
            if (isFadeOutFinished) break;

            isOn = !isOn;
            if (isOn)
            {
                standbyObj.SetActive(true);
                bootingTransform.gameObject.SetActive(true);
                completeProcessTransform.gameObject.SetActive(true);
                coreModuleTransform.gameObject.SetActive(true);

                foreach (var process in processList)
                {
                    process.SetActive(true);
                }

                UISoundCtrl.onCompletedProcessSE.OnNext(Unit.Default);
            }
            else
            {
                standbyObj.SetActive(false);
                bootingTransform.gameObject.SetActive(false);
                completeProcessTransform.gameObject.SetActive(false);
                coreModuleTransform.gameObject.SetActive(false);

                foreach (var process in processList)
                {
                    process.SetActive(false);
                }
            }
            await UniTask.Delay(TimeSpan.FromSeconds(blinkingInterval));
        }

        terminalPanelObj.SetActive(false);
        GameFlowManager.onCompletedShowStandbyTerminal.OnNext(Unit.Default);
    }

    async void FadeOutTerminalPanel()
    {
        Image myImage = terminalPanelObj.MyGetComponent_NullChker<Image>();
        Color32 color = myImage.color;
        while (true)
        {
            if (color.a <= 10)
            {
                isFadeOutFinished = true;
                break;
            }
            color.a -= decreaseAlphaValue;

            myImage.color = color;
            await UniTask.DelayFrame(1);
        }
    }
}

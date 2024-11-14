using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class DeathPanelCtrl : MonoBehaviour
{
    [SerializeField] byte whiteOutSpeed = 2;
    [SerializeField] GameObject deathPanel;

    public delegate void OnFinishDeathPanel();
    public static event OnFinishDeathPanel onFinishDeathPanel;

    Image panelImage;
    Color32 panelColor;

    private void Awake()
    {
        deathPanel.SetActive(false);
    }

    //イベント登録
    private void OnEnable()
    {
        DeathGlitchSparkFactory.onPlayerDeathEffectsInstanceDone += OnVisibleTime;
    }

    private void OnDisable()
    {
        DeathGlitchSparkFactory.onPlayerDeathEffectsInstanceDone -= OnVisibleTime;
    }

    //イベントハンドラー
    async void OnVisibleTime()
    {
        Debug.Log("WhiteOurPanelStart");
        deathPanel.SetActive(true);
        panelImage = deathPanel.MyGetComponent_NullChker<Image>();
        panelColor = panelImage.color;

        for (int count = 0; count < 255; count++)
        {
            panelColor.a = (byte)(count * whiteOutSpeed);
            if (panelColor.a >= 253) break;
            panelImage.color = panelColor;

            await UniTask.DelayFrame(1);
        }

        onFinishDeathPanel?.Invoke();
    }
}

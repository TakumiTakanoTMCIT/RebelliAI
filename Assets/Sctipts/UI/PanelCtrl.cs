using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UniRx;
using System;
using Cysharp.Threading.Tasks;

public class PanelCtrl : MonoBehaviour
{
    [SerializeField] private GameObject clearPanel;
    private void Awake()
    {
        clearPanel.SetActive(false);
        ClearPanelCtrl clearPanelCtrl = new ClearPanelCtrl(clearPanel, this);
    }
}

public class ClearPanelCtrl
{
    public static Subject<Unit> showPanel = new Subject<Unit>();
    public static Subject<Unit> onCompletedShowPanel = new Subject<Unit>();

    GameObject clearPanel;
    public ClearPanelCtrl(GameObject clearPanel, PanelCtrl panelCtrl)
    {
        this.clearPanel = clearPanel;

        showPanel.Subscribe(_ => ShowClearPanel().Forget());
    }

    private async UniTask ShowClearPanel()
    {
        Debug.LogWarning("ShowClearPanel");
        clearPanel.SetActive(true);
        var panelImage = clearPanel.GetComponent<Image>();

        await panelImage.DOFade(1, 1.0f).ToUniTask();

        onCompletedShowPanel.OnNext(Unit.Default);
    }
}

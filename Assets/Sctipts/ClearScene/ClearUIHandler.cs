using UnityEngine.SceneManagement;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine.UI;

public class ClearUIHandler : MonoBehaviour
{
    [SerializeField] private GameObject clearPanel;
    private async UniTask Start()
    {
        clearPanel.SetActive(true);
        await clearPanel.GetComponent<Image>().DOFade(0, 2.5f).ToUniTask();
        clearPanel.SetActive(false);
    }

    public void OnClickedButton()
    {
        Debug.Log("Clear Button Clicked");
        SceneManager.LoadScene("OpeningScene");
    }
}

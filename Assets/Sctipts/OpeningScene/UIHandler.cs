using UnityEngine;
using UnityEngine.SceneManagement;

public class UIHandler : MonoBehaviour
{
    private void Awake()
    {
        MyGameManager.resetSaveData = true;
    }

    public void OnClickStartButton()
    {
        Debug.Log("Start Button Clicked");
        SceneManager.LoadScene("SampleScene");
    }
}

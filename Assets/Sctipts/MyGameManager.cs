using UnityEngine;

public class MyGameManager : MonoBehaviour
{
    public static bool resetSaveData = false;

    public static MyGameManager instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
        DontDestroyOnLoad(this.gameObject);
    }
}

using UnityEngine;

[CreateAssetMenu(fileName = "SavePointSaver", menuName = "SavePointSaver")]
public class SavePointSaver : ScriptableObject
{
    [SerializeField] private int currentSavePoint = 0, nextSavePoint = 1;
    public int CurrentSavePoint => currentSavePoint;
    public int NextSavePoint => nextSavePoint;

    public void InitSavePoint()
    {
        currentSavePoint = 0;
        nextSavePoint = 1;
    }

    public void SaveNextPoint()
    {
        currentSavePoint++;
        nextSavePoint++;
    }
}

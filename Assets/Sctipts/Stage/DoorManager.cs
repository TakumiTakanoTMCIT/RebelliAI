using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DoorManager", menuName = "Game/DoorManager")]
public class DoorManager : ScriptableObject
{
    [System.Serializable]
    public class DoorState
    {
        public string doorID; // ドアID

        public DoorState(string doorID)
        {
            this.doorID = doorID;
        }
    }

    [SerializeField]
    private List<DoorState> doorStates = new List<DoorState>();

    // ドアを登録する（新しいドアの場合）
    public void RegisterDoor(string doorID)
    {
        if (!doorStates.Exists(door => door.doorID == doorID))
        {
            doorStates.Add(new DoorState(doorID));
        }
    }

    // ドアの状態を取得する
    public bool GetDoorState(string doorID)
    {
        DoorState door = doorStates.Find(d => d.doorID == doorID);
        if (door != null)
        {
            return true;
        }
        else
            return false;
    }

    public void ResetDoorStates()
    {
        doorStates.Clear();
    }
}

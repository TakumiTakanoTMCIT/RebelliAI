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

    // ドアが登録されているかどうか
    public bool IsResisteredDoor(string doorID)
    {
        if (doorStates.Exists(door => door.doorID == doorID))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // ドアを登録する
    public void RegisterDoor(string doorID)
    {
        doorStates.Add(new DoorState(doorID));
    }

    // ドアを削除する
    public void ResetDoorStates()
    {
        doorStates.Clear();
    }
}

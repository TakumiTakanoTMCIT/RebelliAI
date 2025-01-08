using UnityEngine;

[CreateAssetMenu(fileName = "WarpDirectionInfo", menuName = "Game/WarpDirectionInfo")]
public class WarpDirectionInfo : ScriptableObject
{
    [SerializeField] public GameObject Prefab;
    [SerializeField] public int MaxCapacity;
}

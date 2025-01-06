using UnityEngine;

[CreateAssetMenu(fileName = "FactoryInfo", menuName = "Game/FactoryInfo")]
public class FactoryInfo : ScriptableObject
{
    [SerializeField] public GameObject warpPrefab;
    [SerializeField] public int warpMaxCapacity;

    [SerializeField] public GameObject danboruPrefab;
    [SerializeField] public int danboruMaxCapacity = 10;
}

using UnityEngine;
using System;
using Muzzle;

[CreateAssetMenu(fileName = "MuzzlePositions", menuName = "MuzzlePos")]
public class MuzzlePositions : ScriptableObject
{
    //ポジションを格納するクラス
    [Serializable]
    public class MuzzlePositionDatas
    {
        [SerializeField] public State myState;
        [SerializeField] public Vector2 pos;
        [SerializeField] public float radius;
        [SerializeField] public bool isCurrentState;
    }

    public MuzzlePositionDatas[] muzzlePositions;
}

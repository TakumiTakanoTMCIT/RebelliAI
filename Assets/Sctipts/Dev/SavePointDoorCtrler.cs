using System;
using UnityEngine;
using Zenject;

public class SavePointDoorCtrler : MonoBehaviour
{
    //なぜZenjectを使わないのかというと、エディタ拡張しているので、Zenjectでインスタンスを取得できないため
    [SerializeField] public SavePointSaver pointSaver;
    [SerializeField] public DoorManager doorManager;
}

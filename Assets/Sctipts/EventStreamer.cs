using UnityEngine;
using UniRx;

public class EventStreamer
{
    public Subject<Unit> startBossDoorCutScene = new Subject<Unit>();
    public Subject<Unit> finishBossDoorCutScene = new Subject<Unit>();
}

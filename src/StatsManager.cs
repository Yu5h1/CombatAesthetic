using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Controller2D = Yu5h1Lib.Game.Character.Controller2D;
using PlayerHost = Yu5h1Lib.Game.Character.PlayerHost;

public class StatsManager : MonoBehaviour
{
    public static StatsManager instance => PoolManager.statsManager;
    public PoolManager poolManager => PoolManager.instance;
    public UI_Statbar UI_statbarSource => Resources.Load<UI_Statbar>("UI/BaseStatBar");

    void Start()
    {
        if (SceneController.IsLevelScene)
            poolManager.Add(UI_statbarSource,5);
    }
    public UI_Statbar SpawnStatbar() => poolManager.Spawn<UI_Statbar>(UI_statbarSource.name);

}

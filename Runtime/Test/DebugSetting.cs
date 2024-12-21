using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yu5h1Lib;

public class DebugSetting : ScriptableObject
{
    private static DebugSetting _instance;
    public static DebugSetting instance => ResourcesUtility.LoadAsInstance(ref _instance);

    public bool enable;
    public float GlobalDelayTimeMultiplier = 0.1f;
    public bool reloadCurrentSceneWhileLoadCheckPoint;
}

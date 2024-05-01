using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnscaleTimer : Timer
{
    protected override float GetTime() => Time.unscaledTime;
}

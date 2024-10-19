using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Yu5h1Lib;

public class GameManagementAgent : MonoBehaviour
{


    public void ContinueTheGameIfPause()
    {
        if (GameManager.IsGamePause)
            GameManager.IsGamePause = false;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILoadElement 
{
    int GetItemCount();
    IEnumerator Loading();
}

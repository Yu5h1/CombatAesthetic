using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Yu5h1Lib
{
    [System.Serializable]
    public class Vector2Event : UnityEvent<Vector2>, ISerializationCallbackReceiver { }
    [System.Serializable]
    public class Vector3Event : UnityEvent<Vector3>, ISerializationCallbackReceiver { }
}

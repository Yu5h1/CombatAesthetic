using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Editor<T> : Editor where T : Object
{
    public T targetObject => (T)target;
    public IEnumerable<T> targetObjects => targets.Cast<T>();
}

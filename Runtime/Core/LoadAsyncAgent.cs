using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yu5h1Lib
{
    [DisallowMultipleComponent]
    public class LoadAsyncAgent : MonoBehaviour
    {
        /// <summary>
        /// SetActive(true);
        /// SetAsLastSibling();
        /// </summary>
        protected internal virtual IEnumerator BeginLoad()
        {
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            yield return null;
        }
        protected internal virtual IEnumerator EndLoad()
        {
            gameObject.SetActive(false);
            yield return null;
        }
    } 
}

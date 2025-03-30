using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Yu5h1Lib
{
    [DisallowMultipleComponent]
    public class LoadAsyncAgent : BaseMonoBehaviour
    {
        public UnityEvent begin;
        public UnityEvent end;

        protected override void OnInitializing() {}

        /// <summary>
        /// SetActive(true);
        /// SetAsLastSibling();
        /// </summary>
        protected internal virtual IEnumerator BeginLoad()
        {
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            begin?.Invoke();            
            yield return null;
        }
        protected internal virtual IEnumerator EndLoad()
        {
            gameObject.SetActive(false);
            end?.Invoke();
            yield return null;
        }
    } 
}

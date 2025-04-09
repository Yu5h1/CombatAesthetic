using UnityEngine;
using UnityEngine.Events;
using Yu5h1Lib;

namespace Yu5h1Lib
{
    public class SimpleConsole : MonoBehaviour
    {
        [System.Serializable]
        public class Command
        {
            public string phrase;
            public UnityEvent action;
        }
        private string inputString;

        [SerializeField]
        private Command[] commands;

        void Start()
        {

        }
        void Update()
        {
            foreach (char c in Input.inputString)
            {
                if (c == '\b')
                {
                    if (!inputString.IsEmpty() && inputString.Length > 0)
                        inputString = inputString.Substring(0, inputString.Length - 1);
                }
                else if (c == '\n' || c == '\r')
                {

                    if (!inputString.IsEmpty() && commands.TryGet(c => inputString.StartsWith(c.phrase), out Command command))
                        command.action.Invoke();
                    inputString = "";
                }
                else
                    inputString += c;
            }
        }
        private void OnGUI()
        {
            //GUILayout.Label(inputString);
        }
    }
}
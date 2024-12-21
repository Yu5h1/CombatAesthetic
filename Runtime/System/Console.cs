using UnityEngine;
using UnityEngine.Events;

public class Console : MonoBehaviour
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
                if (inputString.Length > 0)
                    inputString = inputString.Substring(0, inputString.Length - 1);
            }
            else if (c == '\n' || c == '\r')
                inputString = "";
            else
                inputString += c;
        }
    }
}

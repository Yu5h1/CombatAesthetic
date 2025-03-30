using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Yu5h1Lib.Game;
using Serializable = System.SerializableAttribute;

namespace Yu5h1Lib
{
    [DisallowMultipleComponent]
    public class CheckPoint : PlayerEvent2D
    {
        public static void InitializeCheckPoints()
        {
            checkPoints = FindObjectsByType<CheckPoint>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var item in checkPoints)
                item.PerformIsChecked();
        }
        private static CheckPoint[] checkPoints = new CheckPoint[0];

        internal static int SceneIndex;
        public static Vector3? Position { get => SceneController.startPosition; private set => SceneController.startPosition = value; }

        [SerializeField]
        private bool doNotSave;

        public UnityEvent CheckedAction;
        public UnityEvent UncheckedAction;
        private void Reset() 
        {
            GetComponent<CircleCollider2D>().isTrigger = true;
        }
        private void PerformIsChecked()
        {
            if (gameObject.scene.buildIndex == SceneIndex && transform.position == Position)
                CheckedAction?.Invoke();
            else
                UncheckedAction?.Invoke();
        }
        private void OnTriggerEnter2D(Collider2D other)
        {            
            if (!Validate(other))
                return;
            if (SceneIndex == SceneController.ActiveSceneIndex && Position == transform.position)
                return;

            var activeScene = SceneManager.GetActiveScene();
            if (activeScene.buildIndex > 0)
            {
                SceneIndex = activeScene.buildIndex;
                Position = transform.position;

                if (!doNotSave)
                    Save();
                     
            }
                        
            foreach (var c in checkPoints)
                c.PerformIsChecked();
        }

        public static void Clear()
        {
            checkPoints = null;
            Position = null;
            SceneIndex = 0;
        }
        [ContextMenu("Save")]
        public void Save()
        {
            Records.Save(SceneManager.GetActiveScene().buildIndex , transform.position);
        }
    }
}


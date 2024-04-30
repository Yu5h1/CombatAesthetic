using UnityEngine;
using UIImage = UnityEngine.UI.Image;
using static Yu5h1Lib.GameManager.IDispatcher;
using Yu5h1Lib.Game.Character;
using static UnityEngine.UI.Image;

public class StatProperty_Deprecated
{
    public Controller2D Controller { get; private set; }
    public AttributeStatBehaviour statsBehaviour { get; private set; }
    public RectTransform Stat_UI;
    public VisualItem[] visualItems;
    public DefeatedReason DespawnReason = DefeatedReason.None;
    public StatProperty_Deprecated(AttributeStatBehaviour s,Controller2D controller)
    {
        statsBehaviour = s;
        Controller = controller;
        if (controller.tag == "Player")
        {
            cameraController.target = controller.transform;
            Controller.host = Resources.Load<PlayerHost>(nameof(PlayerHost));
            CreateDefaultVisualItems();
        }
        //if (stats = Controller.gameObject.GetComponent<AttributeStatBehaviour>())
        //    stats.StatDepleted += CheckHealthPointDepleted;

        
    }
    public void Destory()
    {
        if (statsBehaviour)
            statsBehaviour.StatDepleted -= CheckHealthPointDepleted;
        if (Stat_UI)
            GameObject.Destroy(Stat_UI.gameObject);
    }
    private void CheckHealthPointDepleted(AttributeType type)
    {
        if (type != AttributeType.Health)
            return;
        DespawnReason = DefeatedReason.Exhausted;
        OnCharacterDefeated();
    }
    public void UpdateVisualItems()
    {        
        if (!statsBehaviour || visualItems.IsEmpty())
            return;
        for (int i = 0; i < statsBehaviour.Keys.Length; i++)
            visualItems[i].Update(statsBehaviour.stats[i]);
        
    }
    public void OnCharacterDefeated()
    {
        if (Controller.tag == "Player")
        {
            Controller.enabled = false;
            Controller.animParam.animator.Play("Fail");
            Controller.rigidbody.simulated = false;
            //UIController.Fadeboard_UI.FadeIn(Color.black,true);
            PoolManager.canvas.sortingLayerName = "Back";
            cameraController.FadeIn("Back", 1);

            uiManager.LevelSceneMenu.FadeIn(5);

            //foreach (var item in CharactersForUpdate)
            //    item.characterController.enabled = false;
        }
        else
        {
            // npc ring out
            //Controller.gameObject.SetActive(false);
        }
        Controller.host = null;
        Debug.Log($"{Controller.gameObject.name} was defeated because of {DespawnReason}.");
    }

    public void CreateDefaultVisualItems()
    {
        if (Stat_UI != null)
        {
            Stat_UI.gameObject.SetActive(true);
            return;
        }
        Stat_UI = new GameObject($"{Controller.name}_UI").AddComponent<RectTransform>();
        Stat_UI.SetParent(StatsManager.instance.transform, false);
        visualItems = statsBehaviour.CreateVisualItems(Stat_UI, new Vector2(150, 15), false);
        Stat_UI.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 0);
        Stat_UI.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, 0);
        Stat_UI.pivot = new Vector2(-1, 1);
        Stat_UI.sizeDelta = new Vector2(80, 80);
    }

    public class VisualItem
    {
        public RectTransform root;
        public UIImage background;
        public UIImage fill;
        public VisualItem(RectTransform parent, AttributeType attributeType, int index,
            Vector2 size, bool UpDown)
        {
            root = new GameObject($"{attributeType}").AddComponent<RectTransform>();
            root.SetParent(parent, false);
            root.sizeDelta = size;

            var pos = root.localPosition;
            pos.y = index * root.sizeDelta.y * (UpDown ? 1 : -1);
            root.localPosition = pos;

            background = FindOrCreateImage(nameof(background), root);
            fill = FindOrCreateImage(nameof(fill), root,Type.Filled);

            Color.RGBToHSV(fill.color = attributeType.GetColor(), out float h, out float s, out float v);
            background.color = Color.HSVToRGB(h, s, v * 0.3f);

        }
        public void Update(AttributeStat status) => fill.fillAmount = status.normal;

        private static UIImage FindOrCreateImage(string name, RectTransform root,          
            UIImage.Type imgType = UIImage.Type.Simple, UIImage.FillMethod fillMethod = UIImage.FillMethod.Horizontal,
            AttributeType attributeType = AttributeType.None)
        {
            UIImage result = null;
            if (root.TryFind(name,out Transform parent))
                result = parent.GetComponent<UIImage>();
            if (result)
                return result;
            if (!parent)
                parent = new GameObject(name).transform;

            result = parent.gameObject.AddComponent<UIImage>();
            result.rectTransform.SetParent(root, false);
            result.rectTransform.anchorMin = Vector2.zero;
            result.rectTransform.anchorMax = Vector2.one;
            result.rectTransform.pivot = Vector2.one * 0.5f;
            result.rectTransform.offsetMin = Vector2.zero;
            result.rectTransform.offsetMax = Vector2.zero;
            result.type = imgType;
            result.fillMethod = fillMethod;
            result.sprite = Resources.Load<Sprite>("Texture/Square");
            return result;
        }
    }
}

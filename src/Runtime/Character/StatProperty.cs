using UnityEngine;
using UIImage = UnityEngine.UI.Image;
using static Yu5h1Lib.GameManager.IDispatcher;
using Yu5h1Lib.Game.Character;
using static UnityEngine.UI.Image;

public class StatProperty
{
    public Controller2D characterController { get; private set; }
    public AttributeStatBehaviour stats { get; private set; }
    public RectTransform Stat_UI;
    public VisualItem[] visualItems;
    public DespawnReason DespawnReason = DespawnReason.None;
    public StatProperty(Controller2D charactercontroller)
    {
        characterController = charactercontroller;
        
        if (stats = characterController.gameObject.GetComponent<AttributeStatBehaviour>())
            stats.StatDepletedEvent += CheckHealthPointDepleted;
    }
    public void Destory()
    {
        if (stats)
            stats.StatDepletedEvent -= CheckHealthPointDepleted;
        if (Stat_UI)
            GameObject.Destroy(Stat_UI.gameObject);
    }
    private void CheckHealthPointDepleted(AttributeType type)
    {
        if (type == AttributeType.Health)
            DespawnReason = DespawnReason.Exhausted;
    }
    public void SetUIVisible(bool visible) {
        if (!Stat_UI)
            return;
        Stat_UI.gameObject.SetActive(visible);
    }
    public void UpdateVisualItems()
    {        
        if (!stats || visualItems.IsEmpty())
            return;
        for (int i = 0; i < stats.Keys.Length; i++)
            visualItems[i].Update(stats.stats[i]);
        
    }
    public void CreateDefaultVisualItems()
    {
        if (Stat_UI != null)
        {
            Stat_UI.gameObject.SetActive(true);
            return;
        }
        Stat_UI = new GameObject($"{characterController.name}_UI").AddComponent<RectTransform>();
        Stat_UI.SetParent(gameManager.transform, false);
        visualItems = stats.CreateVisualItems(Stat_UI, new Vector2(150, 15), false);
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

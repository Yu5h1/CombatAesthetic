using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMenu : UI_Behaviour
{
    private Button _Submit;
    public Button Submit => _Submit ?? (TryFindButton(nameof(Submit), out _Submit) ? _Submit : null);
    private Button _Cancel;
    public Button Cancel => _Cancel ?? (TryFindButton(nameof(Cancel), out _Cancel) ? _Cancel : null);

    public bool TryFindButton(string name, out Button button)
    {
        button = null;
        if (!rectTransform.TryGetComponentInChildren(name, out button))
            throw new MissingReferenceException($"{name} name:{name} Botton could not be found !");
        return true;
    }
}

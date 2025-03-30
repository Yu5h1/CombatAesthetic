using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yu5h1Lib;

public abstract class EventMask2D : BaseEvent2D
{
    [SerializeField]
    private TagLayerMask _filter;
    public TagLayerMask Filter => _filter;

    protected virtual void Start() {}
    protected bool Validate(Component other)
        => _filter.Validate(this, other);

    protected bool NotAllowTriggerExit => GameManager.IsQuit || SceneController.IsSceneTransitioning;
}

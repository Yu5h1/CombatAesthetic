using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yu5h1Lib;

public class StoryManager : MonoBehaviour
{

    [SerializeField, ReadOnly]
    private StoryHandler _storyHandler;
    public StoryHandler story 
    { 
        get 
        { 
            if (_storyHandler == null)
            {
                if (ResourcesUtility.TryInstantiateFromResources(out _storyHandler, "Story", removeCloneSuffix: true))
                    GameObject.DontDestroyOnLoad(_storyHandler.gameObject);
                _storyHandler.animator.keepAnimatorStateOnDisable = true;
                _storyHandler.gameObject.SetActive(false);
                var c = _storyHandler.animator.runtimeAnimatorController;
            }
            return _storyHandler;
        }
    }

    public bool IsPerforming() => story.IsAvailable();

    public string loadingStory;

    public bool ValidateLoadingStory() => !loadingStory.IsEmpty() && story.animator.HasState(loadingStory);
    public bool TryPlayLoadingStory()
    {
        if (loadingStory.IsEmpty())
            return false;
        var storyName = loadingStory;
        loadingStory = string.Empty;
        if (Play(storyName,true))
        {
            
            UI_Manager.instance.LoadingBackGround.enabled = !SceneController.Isloading;
            UI_Manager.instance.RemoveMenu();
            return true;
        }
        return false;
    }
    public bool Play(string name,bool isLoading)
    {
        if ($"Story \"{name}\" does not exist.".printWarningIf(!story.animator.HasState(name)))
            return false;
        story.camera.cullingMask = isLoading ? story.LoadingCullingMask : story.cullingMask;
        story.gameObject.SetActive(true);
        story.animator.Play(name,0,0);
        return true;
    }
    public void Stop()
    {
        story.gameObject.SetActive(false);
    }
}

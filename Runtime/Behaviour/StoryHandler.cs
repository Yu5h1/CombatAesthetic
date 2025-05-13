using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Video;
using Yu5h1Lib;

public class StoryHandler : BaseMonoBehaviour
{
    [SerializeField,ReadOnly]
    private Animator _animator;
    public Animator animator => _animator;


    [SerializeField]
    private Camera _camera;
#pragma warning disable 0109
    public new Camera camera => _camera;
#pragma warning restore 0109
    [SerializeField]
    private VideoPlayer _player;

    [SerializeField]
    public LayerMask cullingMask;
    [SerializeField]
    public LayerMask LoadingCullingMask;

    [SerializeField]
    private UnityEvent onEnable;

    private void Reset()
    {
        Init();
    }
    protected override void OnInitializing()
    {
        
        this.GetComponent(ref _animator);
        UI_Manager.AddCancelAction(typeof(StoryHandler), () =>
        {
            if (!IsAvailable())
                return false;
            gameObject.SetActive(false);
            return true;
        });
    }
    private void OnEnable()
    {
        onEnable?.Invoke();
        UI_Manager.visible = SceneController.IsUnloading;
        GameManager.SetPlayerControllable(false);
        animator.speed = 1;
    }

    private void Update()
    {
        var speed = Input.GetMouseButton(0) ? 2 : 1; ;
        
        if (_animator)
            animator.speed = speed;
        if (_player)
            _player.playbackSpeed = speed;
    }

    private void OnDisable()
    {
        if (GameManager.IsQuit)
            return;
        UI_Manager.instance.LoadingBackGround.enabled = true;
        UI_Manager.visible = true;
        GameManager.SetPlayerControllable(true);
    }
    public void PlayVideo(VideoClip clip)
    {
        _player.clip = clip;
        _player.Play();
    }
    [ContextMenu("Test")]
    public void Test()
    {
        animator.HasState("story2").print();
        animator.HasState(0, Animator.StringToHash("Base Layer.story2")).print();
        animator.HasState("story2").print();
    }
}

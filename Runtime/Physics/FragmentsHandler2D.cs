using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Yu5h1Lib;

public class FragmentsHandler2D : MonoBehaviour
{
    [System.Serializable]
    public class FragmentInfo
    {
        public Vector2 offset;
        public Rigidbody2D rigidbody;
    }
    [SerializeField]
    private FragmentInfo[] infos;

    private void Start() {}

    private void OnEnable()
    {
        Restore();
    }

    private void OnDisable()
    {
    }


    [ContextMenu(nameof(ActiveFragments))]
    public void ActiveFragments()
        => SetFragmentsActive(true);
    [ContextMenu(nameof(DeactivateFragments))]
    public void DeactivateFragments()
        => SetFragmentsActive(false);
    public void SetFragmentsActive(bool active)
    {
        foreach (var info in infos)
            info.rigidbody.gameObject.SetActive(active);
    }

    [ContextMenu(nameof(Wake))]
    public void Wake() => SetSimulation(true);
    [ContextMenu(nameof(Sleep))]
    public void Sleep() => SetSimulation(true);

    public void SetSimulation(bool simulated)
    {
        foreach (var info in infos)
            info.rigidbody.simulated = simulated;
    }

    [ContextMenu(nameof(Record))]
    public void Record()
    {
        infos = GetComponentsInChildren<Rigidbody2D>(true).Select(r => new FragmentInfo()
        {
            offset = r.transform.localPosition,
            rigidbody = r
        }).ToArray();
    }
    [ContextMenu(nameof(Restore))]
    public void Restore() => Restore(false);
    [ContextMenu(nameof(RestoreAndSleep))]
    public void RestoreAndSleep() => Restore(true);

    public void Restore(bool sleep)
    {
        foreach (var info in infos)
        {
            info.rigidbody.simulated = false;
            info.rigidbody.Reset();
            info.rigidbody.transform.transform.localPosition = info.offset;
            info.rigidbody.transform.transform.localRotation = Quaternion.identity;
            info.rigidbody.simulated = !sleep;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Yu5h1Lib;
using Axis = System.CartesianCoordinate.Axis;

public class AimSolver : MonoBehaviour
{
	public Axis axis = Axis.X;
    public Vector3 direction => axis switch
    {
        Axis.X => transform.right,
        Axis.Y => transform.up,
        Axis.Z => transform.forward,
        _ => Vector3.zero
    };

    //[SerializeField,ReadOnly]
    //private Transform _target;
    //public Transform target 
    //{ 
    //    get => _target; 
    //    set 
    //    { 
    //        if (_target == value) return;
    //        _target = value;
    //        //if (value)
    //        //    OnFoundTarget();
    //        //else
    //        //    OnLostTarget();
    //    }
    //}

    public bool IsWithInRange(Vector3 targetPosition,float threshold)
		=> Vector3.Angle(direction, ((Vector2)targetPosition - (Vector2)transform.position).normalized) < threshold;
    public bool IsWithInRange(Vector2 targetPosition, float threshold)
        => Vector2.Angle(direction, (targetPosition - (Vector2)transform.position).normalized) < threshold;

    //public UnityEvent _foundTarget;
    //public UnityEvent _lostTarget;
    public IKController ikController;
    //public UnityEvent<Vector2> _aiming;

    public bool Aim(Vector3 targetPosition, float threshold)
    {
        if (!ikController)
            return false;
        //"Aim".print();
        var dir = (targetPosition - transform.position).normalized;
        ikController.direction = dir;
        ikController.Refresh();
        return Vector2.Angle(direction, dir) < threshold; 
    }
    public void StopAim()
    {
        ikController.RestoreDefaultPose();
    }
    //private void OnFoundTarget()
    //{
    //    _foundTarget?.Invoke();
    //}
    //private void OnLostTarget()
    //{
    //    _lostTarget?.Invoke();
    //}
}

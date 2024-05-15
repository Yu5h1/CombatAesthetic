using UnityEngine;

public static class CharacterSMBEx
{

	public static void GetMoveInfo(this CharacterSMB csmb,
        out bool Controllable, out Vector2 rootMotionWeight, out Vector2 VelocityWeight)
	{
        Controllable = false;
        rootMotionWeight = Vector2.zero;
        VelocityWeight = Vector2.one;
        if (csmb == null)
            return;
        Controllable = csmb.Controllable;
        rootMotionWeight = csmb.rootMotionWeight;
        VelocityWeight = csmb.rigidbodyVelocityWeight;
    }
}
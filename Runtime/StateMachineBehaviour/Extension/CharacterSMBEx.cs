using UnityEngine;

public static class CharacterSMBEx
{

	public static void GetMoveInfo(this CharacterSMB csmb,
        out bool Controllable, out Vector2 rootMotionWeight, out Vector2 VelocityWeight,out float fixAngleWeight)
	{
        Controllable = false;
        rootMotionWeight = Vector2.zero;
        VelocityWeight = Vector2.one;
        fixAngleWeight = 1f;
        if (csmb == null)
            return;
        Controllable = csmb.Controllable;
        rootMotionWeight = csmb.rootMotionWeight;
        VelocityWeight = csmb.rigidbodyVelocityWeight;
        fixAngleWeight = csmb.FixAngleWeight;
    }
}
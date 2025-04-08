using UnityEngine;
using Yu5h1Lib.Game.Character;

public static class CharacterSMBEx
{
	public static bool GetStateInfo(this CharacterSMB csmb,out StateInfo info)
	{
        info = StateInfo.Default;
        if (csmb == null)
            return false;
        info.controllable = csmb.Controllable;
        info.rootMotionWeight = csmb.rootMotionWeight;
        info.VelocityWeight = csmb.rigidbodyVelocityWeight;
        info.fixAngleWeight = csmb.FixAngleWeight;
        info.affectByMultiplier = csmb.affectByMultiplier;
        return true;
    }
}
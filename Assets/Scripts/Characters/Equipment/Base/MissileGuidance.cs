using UnityEngine;
using System.Collections;

[System.Serializable]
public abstract class MissileGuidance : ScriptableObject{

	public abstract void SetupGuidance(Missile missile);
	public abstract Quaternion GetNextRotation(Missile missile);
	public abstract void ClearGuidance(Missile missile);
}


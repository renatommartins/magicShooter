using UnityEngine;
using System.Collections;

[CreateAssetMenu]
public class DumbGuidance : MissileGuidance{
	
	public override void SetupGuidance (Missile missile){}

	public override Quaternion GetNextRotation (Missile missile){
		return missile.transform.rotation;
	}

	public override void ClearGuidance (Missile missile){}
}


using UnityEngine;
using System.Collections;

[CreateAssetMenu]
public class DirectLOSGuidance : MissileGuidance{

	public LayerMask BlockingLayer;

	public override void SetupGuidance (Missile missile){}

	public override Quaternion GetNextRotation (Missile missile){
		Quaternion result;
		if(!Physics.Linecast(missile.transform.position, missile.target.position, BlockingLayer.value)){
			Quaternion targetRotation = Quaternion.LookRotation(missile.target.position-missile.transform.position);
			result = Quaternion.RotateTowards(missile.transform.rotation,targetRotation,missile.angularSpeed*Time.fixedDeltaTime);
		}else{
			result = missile.transform.rotation;
		}

		return result;
	}

	public override void ClearGuidance (Missile missile){}
}


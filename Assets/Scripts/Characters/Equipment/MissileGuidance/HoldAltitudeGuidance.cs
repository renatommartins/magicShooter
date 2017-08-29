using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu]
public class HoldAltitudeGuidance : MissileGuidance{

	public override void SetupGuidance (Missile missile){
		missile.dynamicVar.Add("targetPositions",new List<Vector3>());
		missile.dynamicVar.Add("targetLastposition",missile.target.position);
		missile.dynamicVar.Add("turnAltitude", missile.holdAltitude - (missile.speed*(1f-Mathf.Cos(-Mathf.Deg2Rad*missile.transform.rotation.eulerAngles.x))/(Mathf.Deg2Rad*missile.angularSpeed)));
		Debug.Log(missile.dynamicVar["turnAltitude"]);
	}

	public override Quaternion GetNextRotation (Missile missile){
		Vector3 lastPosition = (Vector3) missile.dynamicVar["targetLastposition"];
		List<Vector3> lastPositionsList = (List<Vector3>) missile.dynamicVar["targetPositions"];

		Vector3 addedPosition = missile.target.position-lastPosition;
		lastPosition = missile.target.position;
		lastPositionsList.Add(addedPosition);
		if(lastPositionsList.Count>missile.predictionSamples){	lastPositionsList.RemoveAt(0);	}
		Vector3 positionSum = Vector3.zero;
		for(int i=0; i<lastPositionsList.Count && lastPositionsList.Count > 1; i++){
			positionSum += lastPositionsList[i];
		}
		float distance = (missile.target.position+positionSum/lastPositionsList.Count-missile.transform.position).magnitude;
		float timeToTarget = distance/missile.speed;
		Vector3 correction = (positionSum/lastPositionsList.Count/Time.fixedDeltaTime*timeToTarget);
		Vector3 targetPrediction = missile.target.position + correction - missile.transform.position;

		missile.dynamicVar["targetLastposition"] = missile.target.position;

		float diveDistance = (missile.holdAltitude - missile.target.position.y)/Mathf.Sin(Mathf.Deg2Rad*missile.diveAngle)*1.25f;
		if(missile.transform.position.y>=missile.holdAltitude-1f || distance < diveDistance){
			if(distance > diveDistance){
				targetPrediction.y = 0;
			}
			Quaternion targetRotation = Quaternion.LookRotation(targetPrediction);
			return Quaternion.RotateTowards(missile.transform.rotation,targetRotation,missile.angularSpeed*Time.fixedDeltaTime);
		}else{
			if(missile.transform.position.y>(float)missile.dynamicVar["turnAltitude"]){
				targetPrediction.y = 0;
				Quaternion targetRotation = Quaternion.LookRotation(targetPrediction);
				return Quaternion.RotateTowards(missile.transform.rotation,targetRotation,missile.angularSpeed*Time.fixedDeltaTime);
			}
			return missile.transform.rotation;
		}

	}

	public override void ClearGuidance (Missile missile){
		missile.dynamicVar.Remove("targetPositions");
		missile.dynamicVar.Remove("targetLastposition");
		missile.dynamicVar.Remove("turnAltitude");
	}
}


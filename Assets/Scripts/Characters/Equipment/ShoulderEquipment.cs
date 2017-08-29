//using UnityEngine;
//using System.Collections;
//
//[System.Serializable]
//public class ShoulderEquipment : BaseEquipment {
//
//	[Header("Launcher Settings")]
//	public float roundsPerMinute;
//	public Transform firingTransform;
//	[Header("Missile Settings")]
//	public GameObject missilePrefab;
//	public int damage;
//	public float radius;
//	public float speed;
//	public float angularSpeed;
//	public bool holdAltitude;
//	public float diveDistance;
//	public float armTime;
//	public float maxFlightTime;
//	public float triggerDistance;
//	public bool usePrediction;
//
//	private float reloadTime;
//	private float reloadCountdown;
//
//	private Missile activeMissile;
//
//	public override void EquipmentUpdate (Object obj, params float[] parameters){
//		cooldown = Mathf.Clamp01(reloadCountdown/reloadTime);
//
//		if(reloadTime == 0){	reloadTime = 60 / roundsPerMinute;	}
//		reloadCountdown += reloadCountdown > reloadTime? 0 : Time.deltaTime;
//		if(activeMissile != null){
//			activeMissile.targetPosition = activeMissile.transform.position + activeMissile.transform.forward * 50;
//		}
//		Debug.Log("updating");
//	}
//
//	public override bool Action (bool isRightSide){
//		if(reloadCountdown >= reloadTime){
//			reloadCountdown = 0;
//			Fire(0, firingTransform.position, firingTransform.forward);
//			return true;
//		}else{
//			return false;
//		}
//	}
//
//	public void Fire(int netId, Vector3 firingPoint, Vector3 firingDirection){
//		Vector3 originalPosition = firingTransform.position;
//		Quaternion originalRotation = firingTransform.rotation;
//
//		Missile newInstance = MissileManager.RequestInstance(missilePrefab);
//		newInstance.SetMissile(null, firingPoint, firingDirection, damage, radius, speed, angularSpeed, armTime, triggerDistance, usePrediction, holdAltitude, diveDistance);
//
//		activeMissile = newInstance;
//		newInstance.targetPosition = activeMissile.transform.position + activeMissile.transform.forward * 50;
//	}
//}

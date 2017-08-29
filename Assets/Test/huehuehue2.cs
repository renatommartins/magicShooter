using UnityEngine;
using System.Collections;

public class huehuehue2 : MonoBehaviour {
	
	[Header("Missile Settings")]
	public GameObject missilePrefab;
	public int damage;
	public float radius;
	public float speed;
	public float angularSpeed;
	public float holdAltitude;
	public float diveAngle;
	public float armTime;
	public float maxFlightTime;
	public float triggerDistance;
	public bool usePrediction;

	public Transform target;
	public DamageType damageType;
	public MissileGuidance guidance;

	void Start(){
		Missile newInstance = MissileManager.RequestInstance(missilePrefab);
		newInstance.SetMissile(target,transform.position,transform.forward,damage,damageType,radius,speed,angularSpeed,armTime, maxFlightTime,triggerDistance,usePrediction,holdAltitude,diveAngle,guidance);
	}
}

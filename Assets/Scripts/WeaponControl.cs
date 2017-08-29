using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WeaponControl : MonoBehaviour {

	public bool outOfRange = false;
	public Transform firingTransform;
	public LayerMask targetLayers;
	public Vector3 target;
	Vector3 distance;
	public Vector3 firingPoint;
	public Vector3 firingDirection;
	public float aimDistance;
	
	// Update is called once per frame

	public void AimGun (float projectileSpeed, Transform firingTransform) {
		Ray testRay = Camera.main.ScreenPointToRay(new Vector3(Camera.main.pixelWidth/2,Camera.main.pixelHeight/2,0));
		RaycastHit hitInfo;
		Physics.Raycast(testRay, out hitInfo, Mathf.Infinity, targetLayers.value);
		Vector3 target = hitInfo.point;
		distance = target-transform.position;
		aimDistance = distance.magnitude;
		Vector2 distance2D;
		distance2D.x = Mathf.Sqrt((distance.x*distance.x)+(distance.z*distance.z));
		distance2D.y = distance.y;
		//Debug.Log(target+"|"+distance+"|"+distance2D+"|"+CalculateAngle(distance2D.x,distance2D.y, true));

		float newAngle = PhysicsUtils.CalculateBallisticAngle(distance2D.x,distance2D.y, projectileSpeed, true);

		firingTransform.LookAt(target);
		firingTransform.localEulerAngles = new Vector3( (float.IsNaN(newAngle)? 0: -newAngle), firingTransform.localEulerAngles.y, 0);

		firingPoint = firingTransform.position;
		firingDirection = firingTransform.forward;
	}
}

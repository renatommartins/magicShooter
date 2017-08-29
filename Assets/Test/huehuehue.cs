using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class huehuehue : MonoBehaviour {

	public GameObject missileCamera;
	public GameObject staticCamera;

	public LayerMask collisionLayer;

	public Transform target;
	public Transform aim;
	public float maxSpeed;
	public float maxAngleSpeed;
	public float cruiseAltitude;
	public float diveDistance;
	private Rigidbody rigidbody;
	public float waitTime;
	public float countdown = 0;
	public float maxFlightTime;
	public float triggerDistance;
	public bool predictionOn;
	public int predictionSamples;
	public float predictionOffMod;

	public float distance;
	public float timeToTarget;

	public List<Vector3> latestTargetPositions = new List<Vector3>();
	public Vector3 lastPosition = Vector3.zero;
	public Vector3 correction;
	public float curveRadius;
	public List<Vector3> positions = new List<Vector3>();
	public List<Vector3> predictions = new List<Vector3>();

	private Vector3 initialPosition;
	private Quaternion initialRotation;

	void Start(){
		initialPosition = transform.position;
		initialRotation = transform.rotation;

		curveRadius = (maxSpeed*360)/(2*Mathf.PI*maxAngleSpeed);
	}

	void FixedUpdate(){
		//Target movement prediction
		Vector3 addedPosition = target.position-lastPosition;
		lastPosition = target.position;
		latestTargetPositions.Add(addedPosition);
		if(latestTargetPositions.Count>predictionSamples){	latestTargetPositions.RemoveAt(0);	}
		Vector3 positionSum = Vector3.zero;
		for(int i=0; i<latestTargetPositions.Count && latestTargetPositions.Count > 1; i++){
			positionSum += latestTargetPositions[i];
		}
		distance = (target.position+positionSum/latestTargetPositions.Count-transform.position).magnitude;
		timeToTarget = distance/maxSpeed;
		correction = positionSum/latestTargetPositions.Count/Time.fixedDeltaTime*timeToTarget;
		aim.position = target.position+(float.IsNaN(correction.x)? Vector3.zero : correction);

		//altitude hold
		//float curveRadius = (maxSpeed*360)/(2*Mathf.PI*maxAngleSpeed);
		if(countdown < maxFlightTime){
			if(countdown > waitTime){
				Vector3 targetPosition = target.position+(predictionOn? correction:Vector3.zero) - transform.position;
				if(distance > diveDistance){
					if(transform.position.y > cruiseAltitude - curveRadius){
						targetPosition.y = 0;
					}else{
						targetPosition.y = 10 * Mathf.Tan(Mathf.Deg2Rad*transform.eulerAngles.x*(-1));
					}
				}
				Quaternion targetRotation = Quaternion.LookRotation(targetPosition);
				transform.rotation = Quaternion.RotateTowards(transform.rotation,targetRotation,maxAngleSpeed*Time.fixedDeltaTime);

	//			Vector3 targetDelta = (target.position+correction) - transform.position;
	//			float angleDifference = Vector3.Angle(transform.forward,targetDelta);
	//			Vector3 cross = Vector3.Cross(transform.forward,targetDelta);
	//			rigidbody.AddTorque(cross*angleDifference*torqueForce);
			}
		}else{
			Quaternion targetRotation = Quaternion.LookRotation(new Vector3(0,-1,0));
			transform.rotation = Quaternion.RotateTowards(transform.rotation,targetRotation,10*Time.fixedDeltaTime);
		}

		RaycastHit hit;
		if(Physics.Raycast(transform.position,transform.forward, out hit,maxSpeed*Time.fixedDeltaTime,collisionLayer.value)){
			Debug.Log("hit something");
			if(hit.collider.gameObject.layer == Layers.TerrainLayer){	Debug.Log("Hit terrain");	Reset();	}
			if(hit.collider.gameObject.layer == Layers.BotPart || hit.collider.gameObject.layer == Layers.OwnBotPart){	Debug.Log("Direct hit!");	Reset();	}
			positions.Clear();
			predictions.Clear();
		}

		transform.position += transform.forward*maxSpeed*Time.fixedDeltaTime;

		if(distance < triggerDistance){
			Debug.Log("Boom");
//			missileCamera.SetActive(false);
//			staticCamera.SetActive(true);
			Reset();
		}
		countdown += Time.deltaTime;
//		if(countdown > timeOut){
//			constantForce.relativeForce = Vector3.zero;
//			rigidbody.drag = 0.1f;
//			rigidbody.angularDrag = 0.2f;
//			rigidbody.useGravity = true;
//		}
		//Debug \/
		positions.Add(transform.position);
		for(int i=0; i<positions.Count; i++){
			if(i>0){	Debug.DrawLine(positions[i-1],positions[i],Color.red);	}
			Debug.DrawLine(positions[i]+Vector3.left,positions[i]+Vector3.right,Color.blue);
			Debug.DrawLine(positions[i]+Vector3.down,positions[i]+Vector3.up,Color.blue);
			Debug.DrawLine(positions[i]+Vector3.back,positions[i]+Vector3.forward,Color.blue);
		}
		predictions.Add(aim.position);
		for(int i=0; i<predictions.Count; i++){
			if(i>0){	Debug.DrawLine(predictions[i-1],predictions[i],Color.yellow);	}
			Debug.DrawLine(predictions[i]+Vector3.left,predictions[i]+Vector3.right,Color.green);
			Debug.DrawLine(predictions[i]+Vector3.down,predictions[i]+Vector3.up,Color.green);
			Debug.DrawLine(predictions[i]+Vector3.back,predictions[i]+Vector3.forward,Color.green);
		}
	}

	void OnCollisionEnter(){
		Reset();
	}

	private void Reset(){
		countdown = 0;
		transform.position = initialPosition;
		transform.rotation = initialRotation;
		//rigidbody.velocity = Vector3.zero;
		curveRadius = (maxSpeed*360)/(2*Mathf.PI*maxAngleSpeed);
	}
}

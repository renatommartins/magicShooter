using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(WheelCollider))]
public class MovementControl : MonoBehaviour {

	private Character character;

	[Header("Movement Settings")]
	public float turnSpeedMax;
	public float brakeTorque;

	public float speed;
	public bool dashing;
	public bool grounded;
	public bool triedToMove;
	public float legAngle;

	//Testing only
	public Transform feetForwardTransform;

	[Header("Drag Settings")]
	public float dragCoefficient;
	public float crossSectionArea;

	private Rigidbody controlRigidbody;
	private WheelCollider wheelCollider;
	private CapsuleCollider feetCollider;

	void Awake(){
		character = GetComponent<Character>();
		controlRigidbody = GetComponent<Rigidbody>();
		wheelCollider = GetComponent<WheelCollider>();
		feetCollider = GetComponentsInChildren<CapsuleCollider>()[1];

		wheelCollider.enabled = false;
		feetCollider.enabled = false;
	}

	// Use this for initialization
	void Start(){
		controlRigidbody.drag = 0;//PhysicsUtils.CalculateDrag(controlRigidbody.velocity.magnitude,crossSectionArea,dragCoefficient);
	}

	// Update is called once per frame
	void Update () {
		if(character.legsState.isBroken){	dashing = false;	}
		Vector3 velocity = controlRigidbody.velocity;
		velocity = new Vector3(velocity.x,0,velocity.z);
		speed = velocity.magnitude;
	}

	void FixedUpdate(){
		controlRigidbody.drag = PhysicsUtils.CalculateDrag(controlRigidbody.velocity.magnitude,crossSectionArea,dragCoefficient);
	}

	void OnCollisionEnter(Collision collision){
		if(collision.gameObject.layer == 8){	grounded = true;	}
	}

	void OnCollisionStay(Collision collision){
		if(collision.gameObject.layer == 8){
			grounded = true;

		}
	}

	public void ToggleDash(){
		if(!character.legsState.isBroken){
			dashing = !dashing;
		}
	}

	public void Move(Vector3 translationInput, Vector2 rotationInput){
		Vector3 horizontalSpeed = controlRigidbody.velocity;
		horizontalSpeed.y = 0;

		/*	Walk Sequence		*/
		if(!dashing){
			if(horizontalSpeed.magnitude > character.legsPart.walkMaxSpeed || legAngle > 0.1f || legAngle < -0.1f){
				wheelCollider.brakeTorque = brakeTorque;
				wheelCollider.motorTorque = 0;
				legAngle = Mathf.MoveTowardsAngle(legAngle, 0, turnSpeedMax * Time.deltaTime);
			}else{
				legAngle = 0;
				wheelCollider.enabled = false;
				feetCollider.enabled = true;
				transform.eulerAngles = new Vector3(0,transform.eulerAngles.y,0);
			}

			Vector2 horizontalPlane = new Vector2(translationInput.x,translationInput.y);
			horizontalPlane.Normalize();

			float speedRatio = horizontalSpeed.magnitude/character.legsPart.walkMaxSpeed;
			float walkForce = character.legsPart.walkForceCurve.Evaluate(speedRatio) * character.legsPart.walkForceMax;

			controlRigidbody.AddRelativeForce(walkForce * horizontalPlane.x, 0, walkForce * horizontalPlane.y);

			/*	Turn Sequence		*/
			Quaternion deltaRotation = Quaternion.Euler(new Vector3(0, rotationInput.x, 0) * Time.fixedDeltaTime * turnSpeedMax);

			controlRigidbody.MoveRotation(controlRigidbody.rotation * deltaRotation);
		}

		/*	Dash Sequence		*/
		else{
			wheelCollider.enabled = true;
			feetCollider.enabled = false;

			if(translationInput.y > 0){
				triedToMove = true;
				wheelCollider.brakeTorque = 0;
			}
			else{	
				triedToMove = false;
				wheelCollider.brakeTorque = brakeTorque;
			}

			Vector2 horizontalPlane = new Vector2(translationInput.x,translationInput.y);
			horizontalPlane.Normalize();

			float speedRatio = horizontalSpeed.magnitude/character.legsPart.dashMaxSpeed;
			float dashForce = character.legsPart.dashForceCurve.Evaluate(speedRatio) * character.legsPart.dashForceMax;

			wheelCollider.motorTorque = dashForce * translationInput.y;

			/*	Turn Sequence		*/
			Quaternion deltaRotation = Quaternion.Euler(new Vector3(0, rotationInput.x, 0) * Time.fixedDeltaTime * turnSpeedMax);

			legAngle = Mathf.MoveTowardsAngle(legAngle, legAngle + turnSpeedMax * translationInput.x * Time.deltaTime, turnSpeedMax * Time.deltaTime);

			transform.rotation = transform.rotation * deltaRotation;
			legAngle -= deltaRotation.eulerAngles.y;
			if(Mathf.Abs(legAngle) > 360.1f){	legAngle = legAngle + 360*(Mathf.Sign(legAngle)*(-1));	}
			wheelCollider.steerAngle = legAngle;
		}
		wheelCollider.steerAngle = legAngle;
		feetForwardTransform.eulerAngles = new Vector3(0,transform.eulerAngles.y + legAngle,0);
	}
}

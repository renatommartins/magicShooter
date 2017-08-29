using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class LocalControl : Game.Network.NetworkBehaviour {

	[SerializeField]
	private bool forceLocal;
	private bool isLocal;

	public Camera playerCamera;
	public AudioListener playerListener;
	public Character character;
	public MovementControl movementControl;
	public WeaponControl gunScript;

	public float mouseSensitivity;
	private Vector3 translationInput;
	private Vector2 rotationInput;

	void Awake(){
		character = GetComponent<Character>();
		movementControl = GetComponent<MovementControl>();
		gunScript = GetComponent<WeaponControl>();



//		rightHandAction = character.rightHand.isEquiped? character.rightHand.Action as EquipmentAction : null;
//		leftHandAction = character.leftHand.isEquiped? character.leftHand.Action as EquipmentAction : null;

//		rightShoulderAction = character.rightShoulder.isEquiped? character.rightShoulder.Action as EquipmentAction : null;
//		leftShoulderAction = character.leftShoulder.isEquiped? character.leftShoulder.Action as EquipmentAction : null;
//
//		backpackAction = character.backpack.isEquiped? character.backpack.Action as EquipmentAction : null;
	}

	void Start(){
		if(forceLocal){	SetIsLocal(true);	}
	}

	// Update is called once per frame
	void Update () {
		if(isLocal){

			translationInput.y = Input.GetAxis(InputAxes.Vertical);
			translationInput.x = Input.GetAxis(InputAxes.Horizontal);
			translationInput.z = (Input.GetButton(InputAxes.Jump)? 1:0) + (Input.GetButton(InputAxes.Crouch)? -1:0);

			rotationInput.x = Input.GetAxis(InputAxes.MouseX) * mouseSensitivity * Time.deltaTime;
			rotationInput.y = Input.GetAxis(InputAxes.MouseY) * mouseSensitivity * Time.deltaTime;

			if(Input.GetButtonDown(InputAxes.DashToggle)){	movementControl.ToggleDash();	}

			if(Input.GetButton(InputAxes.FireRight)){
				if(character.rightHand != null){
					character.rightHand.Activate(character,character.rightHandState);
				}
			}

			if(Input.GetButton(InputAxes.FireLeft)){
				if(character.leftHand != null){
					character.leftHand.Activate(character,character.leftHandState);
				}
			}

			if(Input.GetButton(InputAxes.FireRightShoulder) && false){
				if(character.rightShoulder != null){
					Debug.Log("rockets away");
					character.rightShoulder.Activate(character,character.rightShoulderState);
				}
			}

			if(Input.GetButton(InputAxes.FireRightShoulderAlt)){}

			if(Input.GetButton(InputAxes.FireLeftShoulder)){}

			if(Input.GetButton(InputAxes.FireLeftShoulderAlt)){}

			if(Input.GetButton(InputAxes.BackpackAction)){}

			if(Input.GetButton(InputAxes.BackpackActionAlt)){}

			movementControl.Move(translationInput, rotationInput);

			character.cameraOffset = Mathf.Clamp(-rotationInput.y + character.cameraOffset
			                                                       ,character.minAngle,character.maxAngle);

			//GameSystem.Instance.speedText.text = movementControl.speed * 3.6f + " Km/h";
		}
	}

	public override void SetIsLocal (bool isLocal){
		this.isLocal = isLocal;
		character.isLocal = isLocal;
		Rigidbody rigidbody = GetComponent<Rigidbody>();
		if(isLocal){
			playerCamera.enabled = true;
			playerListener.enabled = true;
			character.enabled = true;
			gunScript.enabled = true;
			movementControl.enabled = true;
		}else{
			rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
			rigidbody.useGravity = false;
			rigidbody.isKinematic = true;
		}
		//gameObject.name = "Luke, I am your character";
	}


}

namespace UnityEngine{
	public enum EquipmentCommand{	RightHand, LeftHand, RightShoulder, LeftShoulder, Backpack	};
}
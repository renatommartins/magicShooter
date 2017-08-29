using UnityEngine;
using System.IO;
using System.Collections;

[System.Serializable]
public class ShoulderEquipment : BaseEquipment {

	[Header("Launcher Settings")]
	public float roundsPerMinute;
	public Transform firingTransform;
	[Header("Missile Settings")]
	public GameObject missilePrefab;
	public int damage;
	public float radius;
	public float speed;
	public float angularSpeed;
	public bool holdAltitude;
	public float diveDistance;
	public float armTime;
	public float maxFlightTime;
	public float triggerDistance;
	public bool usePrediction;
	[Header("Graphical Settings")]
	public GameObject meshGameObject;

	private float reloadTime;
	private float reloadCountdown;

	private Missile activeMissile;

	void OnValidate(){
		reloadTime = 60/roundsPerMinute;
	}

	public override void Load (Character attachedCharacter, EquipmentCommand equipmentCommand, EquipmentState state){
		state.equipmentCommand = equipmentCommand;
		string attachPointName = "";
		switch(equipmentCommand){
		case EquipmentCommand.Backpack:			attachPointName = "BackpackAttachment";	break;
		case EquipmentCommand.LeftShoulder:		attachPointName = "Shoulder.L";			break;
		case EquipmentCommand.RightShoulder:	attachPointName = "Shoulder.R";			break;
		case EquipmentCommand.LeftHand:			attachPointName = "Hand.L_end";			break;
		case EquipmentCommand.RightHand:		attachPointName = "Hand.R_end";			break;
		}
	}

	public override void EquipmentUpdate (Character character, EquipmentState state){
		
	}

	public override bool Activate (Character character, EquipmentState state){
		return false;
	}

	public override void LocalAction (Character character, EquipmentState state, Vector3 position, Vector3 forward, int randomSeed, float gameTime){
		
	}

	public override void RemoteAction (Character character, EquipmentState state, float gameTime, byte[] parameters){
		
	}
}

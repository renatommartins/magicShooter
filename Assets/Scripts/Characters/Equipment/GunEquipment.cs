using UnityEngine;
using System.IO;
using System.Collections;

[CreateAssetMenu]
public class GunEquipment : BaseEquipment{

	[Header("Weapon Settings")]
	public GameObject projectilePrefab;
	public float roundsPerMinute;
	public int projectileDamage;
	public DamageType damageType;
	public int projectileAmount;
	public float projectileSpeed;
	[Header("Accuracy Settings")]
	public float minimumSpread;
	public float maximumSpread;
	public float coneAngle;
	public AnimationCurve spreadDistribution;
	public float recoilSpread;
	public float spreadRecovery;
	[Header("Graphical Settings")]
	public GameObject meshGameObject;
	[SerializeField]
	private float reloadTime;

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
		Transform attachPoint = null;// = attachedCharacter.gameObject.transform.Find(attachPointName);
		foreach(Transform childTransform in attachedCharacter.GetComponentsInChildren<Transform>()){
			Debug.Log(childTransform.name);
			if(string.Equals(attachPointName,childTransform.name)){	attachPoint = childTransform;	break;	}
		}
		Debug.Log(attachPoint != null? "found "+attachPoint.name:"not found: "+attachPointName);
		GameObject equipmentMesh = (GameObject) Instantiate(meshGameObject,attachPoint.position,Quaternion.Euler(Vector3.down),attachPoint);
		equipmentMesh.transform.eulerAngles = new Vector3(0,attachedCharacter.transform.eulerAngles.y);
		equipmentMesh.transform.parent = attachedCharacter.transform;
		equipmentMesh.transform.localScale = Vector3.one;
		state.dynamicVar.Add("currentMaximumSpread", 0f);
		state.dynamicVar.Add("barrelEnd", equipmentMesh.transform.FindChild("BarrelEnd"));
	}

	public override void EquipmentUpdate (Character character, EquipmentState state){
		state.cooldownTimer += (state.cooldownTimer < reloadTime? Time.fixedDeltaTime : 0);
		state.isAvaliable = (state.cooldownTimer >= reloadTime? true : false);
		state.cooldown = Mathf.Clamp01(state.cooldownTimer/reloadTime);
		if(state.isAvaliable){	
			float currentMaximumSpread = (float) state.dynamicVar["currentMaximumSpread"];
			if(currentMaximumSpread < 0){	currentMaximumSpread = 0;	}
			else{	currentMaximumSpread -= spreadRecovery;	}
			currentMaximumSpread = currentMaximumSpread > maximumSpread? maximumSpread : currentMaximumSpread;
			state.dynamicVar["currentMaximumSpread"] = currentMaximumSpread;
		}
		Transform barrelEnd = (Transform) state.dynamicVar["barrelEnd"];
		//UnityEditor.EditorGUIUtility.PingObject(barrelEnd.gameObject);
		character.gunScript.AimGun(projectileSpeed,barrelEnd);
	}

	public override bool Activate (Character character, EquipmentState state){
		if(state.isAvaliable){
			int randomSeed = Time.frameCount;
			Transform barrelEnd = (Transform) state.dynamicVar["barrelEnd"];
			float currentMaximumSpread = (float) state.dynamicVar["currentMaximumSpread"];
			state.cooldownTimer = 0;
			state.isAvaliable = false;
			PlayerNetworkManager.PlayerCommand command = PlayerNetworkManager.PlayerCommand.BackpackAction;
			switch(state.equipmentCommand){
			case EquipmentCommand.Backpack:		command = PlayerNetworkManager.PlayerCommand.BackpackAction;	break;
			case EquipmentCommand.LeftHand:		command = PlayerNetworkManager.PlayerCommand.FireLeft;			break;
			case EquipmentCommand.RightHand:	command = PlayerNetworkManager.PlayerCommand.FireRight;			break;
			case EquipmentCommand.LeftShoulder:	command = PlayerNetworkManager.PlayerCommand.FireLeftShoulder;	break;
			case EquipmentCommand.RightShoulder:command = PlayerNetworkManager.PlayerCommand.FireRightShoulder;	break;
			}
			byte[] networkParameters;
			using(MemoryStream stream = new MemoryStream()){
				using(BinaryWriter writer = new BinaryWriter(stream)){
					writer.Write(barrelEnd.position.x);	writer.Write(barrelEnd.position.y);	writer.Write(barrelEnd.position.z);
					writer.Write(barrelEnd.forward.x);	writer.Write(barrelEnd.forward.y);	writer.Write(barrelEnd.forward.z);
					writer.Write(randomSeed);
					writer.Write(currentMaximumSpread);
				}
				networkParameters = stream.ToArray();
			}
			PlayerNetworkManager.SendPlayerAction(command, networkParameters);
			LocalAction(character, state, barrelEnd.position, barrelEnd.forward, randomSeed, Time.fixedTime);
			return true;
		}else{
			return false;
		}
	}

	public override void RemoteAction (Character character, EquipmentState state, float gameTime, byte[] parameters){
		Vector3 position;
		Vector3 forward;
		int randomSeed;
		float currentMaxSpread;
		using(MemoryStream stream = new MemoryStream(parameters)){
			using(BinaryReader reader = new BinaryReader(stream)){
				position = new Vector3(reader.ReadSingle(),reader.ReadSingle(),reader.ReadSingle());
				forward = new Vector3(reader.ReadSingle(),reader.ReadSingle(),reader.ReadSingle());
				randomSeed = reader.ReadInt32();
				currentMaxSpread = reader.ReadSingle();
			}
		}
		state.dynamicVar["currentMaximumSpread"] = currentMaxSpread;
		LocalAction(character, state, position,forward,randomSeed,0f);
	}

	public override void LocalAction (Character character, EquipmentState state, Vector3 position, Vector3 forward, int randomSeed, float gameTime){
		Transform barrelEnd = (Transform) state.dynamicVar["barrelEnd"];
		float currentMaximumSpread = (float) state.dynamicVar["currentMaximumSpread"];
		Vector3 originalPosition = barrelEnd.position;
		Vector3 originalRotation = barrelEnd.eulerAngles;
		Random.InitState(randomSeed);
		barrelEnd.position = position;
		barrelEnd.forward = forward;
		Vector3 recoilSpread = new Vector3(	spreadDistribution.Evaluate(Random.value) * currentMaximumSpread - currentMaximumSpread/2,
			spreadDistribution.Evaluate(Random.value) * currentMaximumSpread - currentMaximumSpread/2);
		currentMaximumSpread += this.recoilSpread;
		state.dynamicVar["currentMaximumSpread"] = currentMaximumSpread;
		#if (DEBUG_ALL || DEBUG_HANDWEAPON)
		Debug.Log(firingPoint);
		Debug.Log(firingDirection);
		Debug.Log(recoilSpread);
		#endif
		if(projectileAmount == 1){				
			barrelEnd.eulerAngles = barrelEnd.eulerAngles + recoilSpread;
			Projectile instance = ProjectileManager.RequestInstance(projectilePrefab);
			instance.SetProjectile(projectileDamage, damageType, position, barrelEnd.forward, projectileSpeed, character.ownerNetId);
		}else{
			Vector3 coneDirection = barrelEnd.eulerAngles + recoilSpread;
			#if (DEBUG_ALL || DEBUG_HANDWEAPON)
			Debug.Log(coneDirection.ToString());
			#endif
			for(int i=0; i<projectileAmount; i++){
				barrelEnd.eulerAngles = coneDirection;
				Vector3 pelletSpread =  new Vector3(	spreadDistribution.Evaluate(Random.value) * coneAngle - coneAngle/2,
					spreadDistribution.Evaluate(Random.value) * coneAngle - coneAngle/2);
				//Debug.Log(pelletSpread.ToString());
				barrelEnd.eulerAngles = barrelEnd.eulerAngles + pelletSpread;
				Projectile instance = ProjectileManager.RequestInstance(projectilePrefab);
				instance.SetProjectile(projectileDamage, damageType, position, barrelEnd.forward, projectileSpeed, character.ownerNetId);
			}
		}

		barrelEnd.position = originalPosition;
		barrelEnd.eulerAngles = originalRotation;
	}
}


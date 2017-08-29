using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Character. Base Class to derive all Characters in the game.
/// </summary>
public class Character : MonoBehaviour{

	public Vector3 nextPosition;
	public Vector3 pastPosition;
	public float nextRotation;
	public float pastRotation;
	private float smoothingTime;

	public bool isLocal;

	public int ownerNetId;
	public int instanceId;
	public int prefabId;

	static public Dictionary<List<Collider>,Character> allColliders = new Dictionary<List<Collider>, Character>();

	public Dictionary<List<Collider>, BaseBotPart> colliderDictionary = new Dictionary<List<Collider>, BaseBotPart>();
	private List<Collider> colliders = new List<Collider>();

	[System.Serializable]
	/// <summary>
	/// Temp collider reference class. Temporary measure to override the unique hitbox per part for testing and prototyping purposes
	/// </summary>
	private class TempColliderRefClass{
		public List<Collider> headColliders = new List<Collider>();
		public List<Collider> torsoColliders = new List<Collider>();
		public List<Collider> leftArmColliders = new List<Collider>();
		public List<Collider> rightArmColliders = new List<Collider>();
		public List<Collider> legsColliders = new List<Collider>();
	}

	[Header("Bot Parts")]
	public Head headPart;
	public PartState headState;	
	public Torso torsoPart;
	public PartState torsoState;
	public LeftArm lArmPart;
	public PartState lArmState;
	public RightArm rArmPart;
	public PartState rArmState;
	public Legs legsPart;
	public PartState legsState;

	[SerializeField]
	private TempColliderRefClass hitboxes = new TempColliderRefClass();

	[Header("Bot Equipment")]
	public BaseEquipment rightHand;
	public EquipmentState rightHandState = new EquipmentState();
	public BaseEquipment leftHand;
	public EquipmentState leftHandState = new EquipmentState();
	public BaseEquipment rightShoulder;
	public EquipmentState rightShoulderState = new EquipmentState();
	public BaseEquipment leftShoulder;
	public EquipmentState leftShoulderState = new EquipmentState();
	public BaseEquipment backpack;
	public EquipmentState backpackState = new EquipmentState();

	[Header("Camera Settings")]
	public Camera cameraView;
	public float maxAngle, minAngle;
	public Transform cameraPivot;
	public float cameraOffset;

	[HideInInspector]
	public Animator animator;
	[HideInInspector]
	public MovementControl movementControl;
	[HideInInspector]
	public WeaponControl gunScript;
	[HideInInspector]
	public LocalControl localControl;
	[HideInInspector]
	public Rigidbody localRigidbody;

	void Awake(){
		//Component reference caching
		animator = GetComponent<Animator>();
		cameraView = GetComponentInChildren<Camera>();
		movementControl = GetComponent<MovementControl>();
		gunScript = GetComponent<WeaponControl>();
		localControl = GetComponent<LocalControl>();
		localRigidbody = GetComponent<Rigidbody>();

		/*<Hitbox Assembly Algorithm>*/
		//Gets all children colliders
		colliders.AddRange(gameObject.GetComponentsInChildren<Collider>());

		//Removes the colliders that dont belong to the hitboxes
		List<Collider> ignoredCollider = new List<Collider>();
		foreach(Collider collider in colliders){
			if(collider.gameObject.layer != Layers.BotPart){	ignoredCollider.Add(collider);	}
		}
		colliders.RemoveAll(Collider => ignoredCollider.Contains(Collider));

		allColliders.Add(colliders,this);//Adds the hitbox colliders to a static dictionary, so the game knows whose character each collider belongs to
		#if (DEBUG_ALL || DEBUG_CHARACTER)
		foreach(KeyValuePair<List<Collider>, Character> pair in allColliders){
			Debug.Log(pair.Value.name);
			foreach(Collider collider in pair.Key){
				Debug.Log(collider.gameObject.name);
			}
		}
		#endif
		//Separes the colliders by part, used for applying damage correctly
		List<Collider> tempCollider = new List<Collider>();
		/*	Head	*/
		foreach(Collider collider in hitboxes.headColliders){	tempCollider.Add(collider);	}
		colliderDictionary.Add(tempCollider,headPart);
		tempCollider = new List<Collider>();

		/*	Torso	*/
		foreach(Collider collider in hitboxes.torsoColliders){	tempCollider.Add(collider);	}
		colliderDictionary.Add(tempCollider,torsoPart);
		tempCollider = new List<Collider>();

		/*	Left Arm	*/
		foreach(Collider collider in hitboxes.leftArmColliders){	tempCollider.Add(collider);	}
		colliderDictionary.Add(tempCollider,lArmPart);
		tempCollider = new List<Collider>();

		/*	Right Arm	*/
		foreach(Collider collider in hitboxes.rightArmColliders){	tempCollider.Add(collider);	}
		colliderDictionary.Add(tempCollider,rArmPart);
		tempCollider = new List<Collider>();
		/*	Legs	*/
		foreach(Collider collider in hitboxes.legsColliders){	tempCollider.Add(collider);	}
		colliderDictionary.Add(tempCollider,legsPart);
		tempCollider = new List<Collider>();

//		foreach(KeyValuePair<List<Collider>, BaseBotPart> pair in colliderDictionary){
//			Debug.Log(pair.Value.name);
//			foreach(Collider collider in pair.Key){
//				Debug.Log(collider.gameObject.name);
//			}
//		}

		/*</Hitbox Assembly Algorithm>*/

		//Initialization of each part

	}

	public void Load(){
		headPart.Load(this, headState);
		torsoPart.Load(this, torsoState);
		lArmPart.Load(this, lArmState);
		rArmPart.Load(this, rArmState);
		legsPart.Load(this, legsState);

		//If the equipment slot is used, the state is initialized
		if(rightHand != null)		{	rightHand.Load(this, EquipmentCommand.RightHand, rightHandState);				}
		if(leftHand != null)		{	leftHand.Load(this, EquipmentCommand.LeftHand, leftHandState);					}
		if(rightShoulder != null)	{	rightShoulder.Load(this, EquipmentCommand.RightShoulder, rightShoulderState);	}
		if(leftShoulder != null)	{	leftShoulder.Load(this, EquipmentCommand.LeftShoulder, leftShoulderState);		}
	}

	void Start(){
		//Changes own colliders layer to avoid own projectiles hitting the owner when firing
		//TODO: check if this step is still necessary as projectiles check the ID on collision independent of layer
		if(isLocal){
			foreach(Collider collider in colliders){
				{	collider.gameObject.layer = Layers.OwnBotPart;	}
			}
		}
	}

	void Update(){


		cameraView.transform.localEulerAngles = new Vector3(8 + cameraOffset,0,0);

		if(rightHand != null)		{	rightHand.EquipmentUpdate(this, rightHandState);		}
		if(leftHand != null)		{	leftHand.EquipmentUpdate(this, leftHandState);			}
		if(rightShoulder != null)	{	rightShoulder.EquipmentUpdate(this, rightShoulderState);}
		if(leftShoulder != null)	{	leftShoulder.EquipmentUpdate(this, leftShoulderState);	}
	}

	public void HandleHit(int damage, DamageType damageType, BotPartEnum partHit){
		if(Game.Network.NetworkManager.Instance.isServer){
			Game.Network.UniqueInstanceIdentifier uniqueId = new Game.Network.UniqueInstanceIdentifier(ownerNetId,instanceId,prefabId);
			TakeDamage(damage, partHit, damageType);
			if(!string.Equals(name,""))
				PlayerNetworkManager.SendPlayerHit(uniqueId, damage, partHit, damageType);
		}
	}

	public void TakeDamage(int damage, BotPartEnum partHit, DamageType damageType){
		#if (DEBUG_ALL || DEBUG_CHARACTER)
		Debug.Log("Hit in the "+partHit.ToString()+" for "+damage+" damage.");
		#endif
		switch(partHit){
		case BotPartEnum.Head:
			headPart.TakeDamage(headState, damage, damageType);
			break;
		case BotPartEnum.Torso:
			torsoPart.TakeDamage(torsoState, damage, damageType);
			break;
		case BotPartEnum.LeftArm:
			lArmPart.TakeDamage(lArmState, damage, damageType);
			break;
		case BotPartEnum.RightArm:
			rArmPart.TakeDamage(rArmState, damage, damageType);
			break;
		case BotPartEnum.Legs:
			legsPart.TakeDamage(legsState, damage, damageType);
			break;
		}
	}
}

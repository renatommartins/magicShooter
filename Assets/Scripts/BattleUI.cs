using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BattleUI : MonoBehaviour {

	#pragma warning disable 0649
	[SerializeField]
	private Canvas battleCanvas;
	private Character localCharacter;
	[SerializeField]
	private Character overrideCharacter;

	[SerializeField]
	private Color healthyColor = Color.green;
	[SerializeField]
	private Color brokenColor = Color.red;
	[SerializeField]
	private Color unusedColor = Color.gray;

	[SerializeField]
	private Text speedText;
	[SerializeField]
	private Text distanceText;
	[SerializeField]
	private Image crosshairCircle;
	[SerializeField]
	private Image crosshairCircle2;
	[SerializeField]
	private Image legDirection;
	[SerializeField]
	private EquipmentUI equipmentUI = new EquipmentUI();
	[SerializeField]
	private PartUI partUI = new PartUI();
	#pragma warning restore 0649

	public ArmorType CArmor;
	public ArmorType RArmor;
	public Vector2 imDebug = new Vector2();

	// Use this for initialization
	void Start () {
		battleCanvas.gameObject.SetActive(true);
		localCharacter = overrideCharacter == null? PlayerNetworkManager.Instance.localPlayer : overrideCharacter;

		if(localCharacter.headState.armorLayers.ContainsKey(CArmor)){	partUI.headHasCArmor = true;		}
		else{	partUI.headHasCArmor = false;		partUI.headCArmor.text = "-";		}

		if(localCharacter.torsoState.armorLayers.ContainsKey(CArmor)){	partUI.torsoHasCArmor = true;		}
		else{	partUI.torsoHasCArmor = false;		partUI.torsoCArmor.text = "-";		}
		if(localCharacter.torsoState.armorLayers.ContainsKey(RArmor)){	partUI.torsoHasRArmor = true;		}
		else{	partUI.torsoHasRArmor = false;		partUI.torsoRArmor.text = "-";		}

		if(localCharacter.lArmState.armorLayers.ContainsKey(CArmor)){	partUI.leftArmHasCArmor = true;		}
		else{	partUI.leftArmHasCArmor = false;	partUI.leftArmCArmor.text = "-";	}
		if(localCharacter.lArmState.armorLayers.ContainsKey(RArmor)){	partUI.leftHasRArmor = true;		}
		else{	partUI.leftHasRArmor = false;		partUI.leftArmRArmor.text = "-";	}

		if(localCharacter.rArmState.armorLayers.ContainsKey(CArmor)){	partUI.rightArmHasCArmor = true;	}
		else{	partUI.rightArmHasCArmor = false;	partUI.rightArmCArmor.text = "-";	}
		if(localCharacter.rArmState.armorLayers.ContainsKey(RArmor)){	partUI.rightHasRArmor = true;		}
		else{	partUI.rightHasRArmor = false;		partUI.rightArmRArmor.text = "-";	}

		if(localCharacter.legsState.armorLayers.ContainsKey(CArmor)){	partUI.legsHasCArmor = true;		}
		else{	partUI.legsHasCArmor = false;		partUI.legsCArmor.text = "-";		}
		if(localCharacter.legsState.armorLayers.ContainsKey(RArmor)){	partUI.legsHasRArmor = true;		}
		else{	partUI.legsHasRArmor = false;		partUI.legsRArmor.text = "-";		}

		if(localCharacter.leftHand == null){	equipmentUI.leftHandWeaponFill.color = unusedColor;	}
		if(localCharacter.rightHand == null){	equipmentUI.rightHandWeaponFill.color = unusedColor;	}
		if(localCharacter.leftShoulder == null){	equipmentUI.leftShoulderWeaponFill.color = unusedColor;	}
		if(localCharacter.rightShoulder == null){	equipmentUI.rightShoulderWeaponFill.color = unusedColor;	}
		if(!false){	equipmentUI.accessoryFill.color = unusedColor;	}
	}
	
	// Update is called once per frame
	void Update () {
		float headHitpoints = 		(float)localCharacter.headState.hitpoints			/(float)localCharacter.headPart.hitpoints;
		partUI.headHitpoints.text =		string.Join(string.Empty,new string[]{(headHitpoints*100).ToString("000."),"%"});
		float headStructure = 		(float)localCharacter.headState.structuralHitpoints	/(float)localCharacter.headPart.structuralHitpoints;
		partUI.headStructure.text =		string.Join(string.Empty,new string[]{(headStructure*100).ToString("000."),"%"});
		if(partUI.headHasCArmor){
			float headCArmor = 			(float)localCharacter.headState.armorLayers[CArmor]	
				/(float)localCharacter.headPart.armorLayers.Find(ArmorLayer => ArmorLayer.armorType == CArmor).hitPoints;
			partUI.headCArmor.text =		string.Join(string.Empty,new string[]{(headCArmor*100).ToString("000."),"%"});
		}

		float torsoHitpoints = 		(float)localCharacter.torsoState.hitpoints			/(float)localCharacter.torsoPart.hitpoints;
		partUI.torsoHitpoints.text =	string.Join(string.Empty,new string[]{(torsoHitpoints*100).ToString("000."),"%"});
		float torsoStructure = 		(float)localCharacter.torsoState.structuralHitpoints/(float)localCharacter.torsoPart.structuralHitpoints;
		partUI.torsoStructure.text =	string.Join(string.Empty,new string[]{(torsoStructure*100).ToString("000."),"%"});
		if(partUI.torsoHasCArmor){
			float torsoCArmor = 		(float)localCharacter.torsoState.armorLayers[CArmor]	
				/(float)localCharacter.torsoPart.armorLayers.Find(ArmorLayer => ArmorLayer.armorType == CArmor).hitPoints;
			partUI.torsoCArmor.text =		string.Join(string.Empty,new string[]{(torsoCArmor*100).ToString("000."),"%"});
		}
		if(partUI.torsoHasRArmor){
			int torsoRArmor = 			localCharacter.torsoState.armorLayers[RArmor];
			partUI.torsoRArmor.text =		string.Join("/",new string[]{(torsoRArmor).ToString(),
				localCharacter.torsoPart.armorLayers.Find(ArmorLayer => ArmorLayer.armorType == RArmor).hitPoints.ToString()});
		}

		float leftArmHitpoints = 	(float)localCharacter.lArmState.hitpoints			/(float)localCharacter.lArmPart.hitpoints;
		partUI.leftArmHitpoints.text = 	string.Join(string.Empty,new string[]{(leftArmHitpoints*100).ToString("000."),"%"});
		float leftArmStructure = 	(float)localCharacter.lArmState.structuralHitpoints	/(float)localCharacter.lArmPart.structuralHitpoints;
		partUI.leftArmStructure.text =	string.Join(string.Empty,new string[]{(leftArmStructure*100).ToString("000."),"%"});
		if(partUI.leftArmHasCArmor){
			float leftArmCArmor = 		(float)localCharacter.lArmState.armorLayers[CArmor]	
				/(float)localCharacter.lArmPart.armorLayers.Find(ArmorLayer => ArmorLayer.armorType == CArmor).hitPoints;
			partUI.leftArmCArmor.text =		string.Join(string.Empty,new string[]{(leftArmCArmor*100).ToString("000."),"%"});
		}
		if(partUI.leftHasRArmor){
			int leftArmRArmor = 		localCharacter.lArmState.armorLayers[RArmor];
			partUI.leftArmRArmor.text =		string.Join("/",new string[]{(leftArmRArmor).ToString(),
				localCharacter.lArmPart.armorLayers.Find(ArmorLayer => ArmorLayer.armorType == RArmor).hitPoints.ToString()});
		}

		float rightArmHitpoints = 	(float)localCharacter.rArmState.hitpoints			/(float)localCharacter.rArmPart.hitpoints;
		partUI.rightArmHitpoints.text =	string.Join(string.Empty,new string[]{(rightArmHitpoints*100).ToString("000."),"%"});
		float rightArmStructure = 	(float)localCharacter.rArmState.structuralHitpoints	/(float)localCharacter.rArmPart.structuralHitpoints;
		partUI.rightArmStructure.text =	string.Join(string.Empty,new string[]{(rightArmStructure*100).ToString("000."),"%"});
		if(partUI.rightArmHasCArmor){
			float rightArmCArmor = 		(float)localCharacter.rArmState.armorLayers[CArmor]	
				/(float)localCharacter.rArmPart.armorLayers.Find(ArmorLayer => ArmorLayer.armorType == CArmor).hitPoints;
			partUI.rightArmCArmor.text =	string.Join(string.Empty,new string[]{(rightArmCArmor*100).ToString("000."),"%"});
		}
		if(partUI.rightHasRArmor){
			int rightArmRArmor = 		localCharacter.rArmState.armorLayers[RArmor];
			partUI.rightArmRArmor.text =	string.Join("/",new string[]{(rightArmRArmor).ToString(),
				localCharacter.rArmPart.armorLayers.Find(ArmorLayer => ArmorLayer.armorType == RArmor).hitPoints.ToString()});
		}

		float legsHitpoints = 		(float)localCharacter.legsState.hitpoints			/(float)localCharacter.legsPart.hitpoints;
		partUI.legsHitpoints.text =		string.Join(string.Empty,new string[]{(legsHitpoints*100).ToString("000."),"%"});
		float legsStructure = 		(float)localCharacter.legsState.structuralHitpoints	/(float)localCharacter.legsPart.structuralHitpoints;
		partUI.legsStructure.text =		string.Join(string.Empty,new string[]{(legsStructure*100).ToString("000."),"%"});
		if(partUI.legsHasCArmor){
			float legsCArmor = 			(float)localCharacter.legsState.armorLayers[CArmor]	
				/(float)localCharacter.legsPart.armorLayers.Find(ArmorLayer => ArmorLayer.armorType == CArmor).hitPoints;
			partUI.legsCArmor.text =		string.Join(string.Empty,new string[]{(legsCArmor*100).ToString("000."),"%"});
		}
		if(partUI.legsHasRArmor){
			int legsRArmor = 			localCharacter.legsState.armorLayers[RArmor];
			partUI.legsRArmor.text =		string.Join("/",new string[]{(legsRArmor).ToString(),
				localCharacter.legsPart.armorLayers.Find(ArmorLayer => ArmorLayer.armorType == RArmor).hitPoints.ToString()});
		}

		partUI.headFill.fillAmount = headHitpoints > 0? headHitpoints:1;
		partUI.torsoFill.fillAmount = torsoHitpoints > 0? torsoHitpoints:1;
		partUI.leftArmFill.fillAmount = leftArmHitpoints > 0? leftArmHitpoints:1;
		partUI.rightArmFill.fillAmount = rightArmHitpoints > 0? rightArmHitpoints:1;
		partUI.legsFill.fillAmount = legsHitpoints > 0? legsHitpoints:1;

		partUI.headFill.color = Color.Lerp(brokenColor,healthyColor,headHitpoints);
		partUI.torsoFill.color = Color.Lerp(brokenColor,healthyColor,torsoHitpoints);
		partUI.leftArmFill.color = Color.Lerp(brokenColor,healthyColor,leftArmHitpoints);
		partUI.rightArmFill.color = Color.Lerp(brokenColor,healthyColor,rightArmHitpoints);
		partUI.legsFill.color = Color.Lerp(brokenColor,healthyColor,legsHitpoints);

		speedText.text = string.Join(string.Empty, new string[]{ (localCharacter.movementControl.speed*3.6f).ToString("000.")," Km/h"});

		distanceText.text = string.Join(string.Empty, new string[]{ localCharacter.gunScript.aimDistance.ToString("0000."),"m" });
		if(localCharacter.rightHand != null){
			if(localCharacter.rightHand.GetType() == typeof(GunEquipment)){
				GunEquipment rightHandEquipment = localCharacter.rightHand as GunEquipment;
				float crosshairMultiplier = Mathf.Tan(Mathf.Deg2Rad*((float)localCharacter.rightHandState.dynamicVar["currentMaximumSpread"]+rightHandEquipment.coneAngle)/2)/
					Mathf.Tan(Mathf.Deg2Rad*localCharacter.localControl.playerCamera.fieldOfView/2);
		//		crosshairCircle.rectTransform.sizeDelta = new Vector2(1000f*localCharacter.localControl.playerCamera.aspect*crosshairMultiplier,1500f*crosshairMultiplier);
	//			int crosshairSize = Mathf.RoundToInt(crosshairMultiplier*Screen.height);
				crosshairCircle.rectTransform.sizeDelta = new Vector2(-1600+1600*crosshairMultiplier,-900+1600*crosshairMultiplier);
			}
		}

		if(localCharacter.leftHand != null){
			if(localCharacter.rightHand.GetType() == typeof(GunEquipment)){
				GunEquipment leftHandEquipment = localCharacter.leftHand as GunEquipment;
				float crosshairMultiplier2 = Mathf.Tan(Mathf.Deg2Rad*((float)localCharacter.leftHandState.dynamicVar["currentMaximumSpread"]+leftHandEquipment.coneAngle)/2)/
					Mathf.Tan(Mathf.Deg2Rad*localCharacter.localControl.playerCamera.fieldOfView/2);
				//		crosshairCircle.rectTransform.sizeDelta = new Vector2(1000f*localCharacter.localControl.playerCamera.aspect*crosshairMultiplier,1500f*crosshairMultiplier);
	//			int crosshairSize2 = Mathf.RoundToInt(crosshairMultiplier2*Screen.height);
				crosshairCircle2.rectTransform.sizeDelta = new Vector2(-1600+1600*crosshairMultiplier2,-900+1600*crosshairMultiplier2);
			}
		}

		Vector3 legDirectionVector = legDirection.rectTransform.eulerAngles;
		legDirectionVector.z = 360 - localCharacter.movementControl.legAngle;
		legDirection.rectTransform.eulerAngles = legDirectionVector;

		if(localCharacter.leftHand != null){	equipmentUI.leftHandWeaponFill.fillAmount = localCharacter.leftHandState.cooldown;	}
		if(localCharacter.rightHand != null){	equipmentUI.rightHandWeaponFill.fillAmount = localCharacter.rightHandState.cooldown;	}
		if(localCharacter.leftShoulder != null){	equipmentUI.leftShoulderWeaponFill.fillAmount = localCharacter.leftShoulderState.cooldown;	}
		if(localCharacter.rightShoulder != null){	equipmentUI.rightShoulderWeaponFill.fillAmount = localCharacter.rightShoulderState.cooldown;	}
	}

	[System.Serializable]
	private class EquipmentUI{
		#pragma warning disable 0649
		public GameObject leftHandWeapon;
		public GameObject rightHandWeapon;
		public GameObject leftShoulderWeapon;
		public GameObject rightShoulderWeapon;
		public GameObject accessory;

		public Image leftHandWeaponFill;
		public Image rightHandWeaponFill;
		public Image leftShoulderWeaponFill;
		public Image rightShoulderWeaponFill;
		public Image accessoryFill;
		#pragma warning restore 0649
	}

	[System.Serializable]
	private class PartUI{
		#pragma warning disable 0649
		[Header("Head")]
		public Text headHitpoints;
		public Text headStructure;
		public Text headCArmor;
		public Image headFill;
		[HideInInspector]
		public bool headHasCArmor;

		[Header("Torso")]
		public Text torsoHitpoints;
		public Text torsoStructure;
		public Text torsoCArmor;
		public Text torsoRArmor;
		public Image torsoFill;
		[HideInInspector]
		public bool torsoHasCArmor;
		[HideInInspector]
		public bool torsoHasRArmor;

		[Header("Left Arm")]
		public Text leftArmHitpoints;
		public Text leftArmStructure;
		public Text leftArmCArmor;
		public Text leftArmRArmor;
		public Image leftArmFill;
		[HideInInspector]
		public bool leftArmHasCArmor;
		[HideInInspector]
		public bool leftHasRArmor;

		[Header("Right Arm")]
		public Text rightArmHitpoints;
		public Text rightArmStructure;
		public Text rightArmCArmor;
		public Text rightArmRArmor;
		public Image rightArmFill;
		[HideInInspector]
		public bool rightArmHasCArmor;
		[HideInInspector]
		public bool rightHasRArmor;

		[Header("Legs")]
		public Text legsHitpoints;
		public Text legsStructure;
		public Text legsCArmor;
		public Text legsRArmor;
		public Image legsFill;
		[HideInInspector]
		public bool legsHasCArmor;
		[HideInInspector]
		public bool legsHasRArmor;
		#pragma warning restore 0649
	}
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class BaseBotPart : ScriptableObject {

	static public float baseDefense = 50;

	public string partName;

	public List<ArmorLayer> armorLayers = new List<ArmorLayer>();

	public ArmorType structureArmorType;
	public int hitpoints;
	public int structuralHitpoints;
	public float weight;
	public int powerUse;

	public Material testMaterial;

	public List<Collider> colliders;

	public abstract BotPartEnum partType{	get; 	}

	public void Load(Character attachedCharacter, PartState state){
		state.partType = partType;
		for(int i=0; i<armorLayers.Count; i++){
			state.armorLayers.Add(armorLayers[i].armorType,armorLayers[i].hitPoints);
		}
		state.hitpoints = hitpoints;
		state.structuralHitpoints = structuralHitpoints;
		state.isBroken = false;
	}

	public void TakeDamage(PartState state,int damage, DamageType damageType){
		List<int> damages = new List<int>();
		int remainingDamage = damage;
		ArmorType armorType;
		for(int i=0; i<armorLayers.Count && remainingDamage > 0; i++){
			armorType = armorLayers[i].armorType;
			switch(armorLayers[i].armorType.type){
			case ArmorType.ArmorTypeEnum.Discardable:
				if(state.armorLayers[armorLayers[i].armorType] > 0){
					state.armorLayers[armorLayers[i].armorType] = state.armorLayers[armorLayers[i].armorType] - 1;
					remainingDamage = 0;
				}
				break;
			case ArmorType.ArmorTypeEnum.Destructable:
				if(state.armorLayers[armorLayers[i].armorType] > 0){
					float damageMod = damageType.damageModifiers.Find(DamageType => DamageType.armorType == armorType).damageModfier;
					int modifiedDamage = Mathf.RoundToInt(remainingDamage*damageMod);
					state.armorLayers[armorType] -= modifiedDamage;
					if(state.armorLayers[armorType] <= 0){
						remainingDamage = Mathf.RoundToInt(Mathf.Abs(state.armorLayers[armorType])/damageMod);
						state.armorLayers[armorType] = 0;
					}else{
						remainingDamage = 0;
					}
				}
				break;
			case ArmorType.ArmorTypeEnum.Absolute:
				remainingDamage = Mathf.CeilToInt(((float)remainingDamage)*baseDefense/(baseDefense+armorLayers[i].hitPoints));
				break;
			}
		}
		state.hitpoints -= remainingDamage;
		if(state.hitpoints < 0){	state.hitpoints = 0;	}
		state.structuralHitpoints -= remainingDamage;
		if(state.structuralHitpoints < 0){	state.structuralHitpoints = 0;	}
		if(state.hitpoints > state.structuralHitpoints){	state.hitpoints = state.structuralHitpoints;	}
	}
}

namespace UnityEngine{
	public enum BotPartEnum{	Head, Torso, LeftArm, RightArm, Legs	}

	[System.Serializable]
	public class ArmorLayer{
		public ArmorType armorType;
		public int hitPoints;
	}

	[System.Serializable]
	public class PartState{
		//TODO: dictionary?
		public Dictionary<ArmorType,int> armorLayers = new Dictionary<ArmorType, int>();

		public int hitpoints;
		public int structuralHitpoints;

		public BotPartEnum partType;

		public bool isBroken;
	}
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public abstract class BaseEquipment : ScriptableObject {
	
	public string equipmentName;
	public float weight;
	public int powerUse;

	public abstract void EquipmentUpdate(Character character, EquipmentState state);
	public abstract bool Activate		(Character character, EquipmentState state);
	public abstract void LocalAction	(Character character, EquipmentState state, Vector3 position, Vector3 forward, int randomSeed, float gameTime);
	public abstract void RemoteAction	(Character character, EquipmentState state, float gameTime, byte[] parameters);
	public abstract void Load			(Character attachedCharacter, EquipmentCommand equipmentCommand, EquipmentState state);
}

namespace UnityEngine{
	[System.Serializable]
	public class EquipmentState{
		public bool isEquiped;

		public bool isAvaliable;
		public float cooldown;
		public float cooldownTimer;

		public Dictionary<string,object> dynamicVar = new Dictionary<string, object>();

		public EquipmentCommand equipmentCommand;
	}
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu]
public class DamageType : ScriptableObject{

	private static Dictionary<int,DamageType> hashDictionary = new Dictionary<int, DamageType>();
	public static DamageType FindTypeByHash(int hash){
		if(hashDictionary.Count <= 0){
			foreach(DamageType type in Resources.FindObjectsOfTypeAll<DamageType>()){
				int typeHash = type.name.GetStableHash();
				if(!hashDictionary.ContainsKey(typeHash)){
					hashDictionary.Add(typeHash,type);
				}
			}
		}
		if(hashDictionary.ContainsKey(hash)){	return hashDictionary[hash];	}
		else{	return null;	}
	}

	[System.Serializable]
	public class DamageModifier{
		public ArmorType armorType;
		public float damageModfier;
	}

	public List<DamageModifier> damageModifiers = new List<DamageModifier>();
}


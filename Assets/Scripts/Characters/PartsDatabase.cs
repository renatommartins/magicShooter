using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu]
public class PartsDatabase : ScriptableObject{

	[SerializeField]
	private static PartsDatabase _instance;
	public static PartsDatabase Instance{
		get{
			if(_instance == null){	_instance = Resources.FindObjectsOfTypeAll<PartsDatabase>()[0];	}
			return _instance;
		}
	}

	public List<Head>		headParts = new List<Head>();
	public List<Torso>		torsoParts = new List<Torso>();
	public List<LeftArm>	leftArmParts = new List<LeftArm>();
	public List<RightArm>	rightArmParts = new List<RightArm>();
	public List<Legs>		legsParts = new List<Legs>();

	public List<BaseEquipment>	equipments = new List<BaseEquipment>();

	public static int GetObjectID(object obj){
		int returnValue = -1;
		if(obj != null){
			System.Type objectType = obj.GetType();

			if(objectType == typeof(Head))		{	returnValue = GameSystem.partsDatabase.headParts.FindIndex(Head => Head == (Head)obj);					}
			if(objectType == typeof(Torso))		{	returnValue = GameSystem.partsDatabase.torsoParts.FindIndex(Torso => Torso == (Torso)obj);				}
			if(objectType == typeof(LeftArm))	{	returnValue = GameSystem.partsDatabase.leftArmParts.FindIndex(LeftArm => LeftArm == (LeftArm)obj);		}
			if(objectType == typeof(RightArm))	{	returnValue = GameSystem.partsDatabase.rightArmParts.FindIndex(RightArm => RightArm == (RightArm)obj);	}
			if(objectType == typeof(Legs))		{	returnValue = GameSystem.partsDatabase.legsParts.FindIndex(Legs => Legs == (Legs)obj);					}

			if(objectType.IsSubclassOf(typeof(BaseEquipment)))
			{	returnValue = GameSystem.partsDatabase.equipments.FindIndex(BaseEquipment => BaseEquipment == (BaseEquipment)obj);	}
		}
		//Debug.Log(returnValue);
		return returnValue;
	}

	#if UNITY_EDITOR || UNITY_EDITOR_64
	void OnValidate(){
		_instance = this;

		headParts.Clear();
		torsoParts.Clear();
		leftArmParts.Clear();
		rightArmParts.Clear();
		legsParts.Clear();

		equipments.Clear();

		string[] guids = UnityEditor.AssetDatabase.FindAssets("t:BaseBotPart");
		BaseBotPart[] allParts = new BaseBotPart[guids.Length];
		for(int i=0; i<allParts.Length; i++){
			allParts[i] = UnityEditor.AssetDatabase.LoadAssetAtPath<BaseBotPart>(UnityEditor.AssetDatabase.GUIDToAssetPath(guids[i]));
		}
		for(int i=0; i<allParts.Length; i++){
			if(allParts[i].GetType() == typeof(Head)){		headParts.Add((Head)allParts[i]);			}
			if(allParts[i].GetType() == typeof(Torso)){		torsoParts.Add((Torso)allParts[i]);			}
			if(allParts[i].GetType() == typeof(LeftArm)){	leftArmParts.Add((LeftArm)allParts[i]);		}
			if(allParts[i].GetType() == typeof(RightArm)){	rightArmParts.Add((RightArm)allParts[i]);	}
			if(allParts[i].GetType() == typeof(Legs)){		legsParts.Add((Legs)allParts[i]);			}
		}

		guids = UnityEditor.AssetDatabase.FindAssets("t:BaseEquipment");
		for(int i=0; i<guids.Length; i++){
			equipments.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<BaseEquipment>(UnityEditor.AssetDatabase.GUIDToAssetPath(guids[i])));
		}
	}
	#endif
}


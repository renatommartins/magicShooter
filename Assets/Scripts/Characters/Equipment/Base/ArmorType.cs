using UnityEngine;
using System.Collections;

[CreateAssetMenu]
public class ArmorType : ScriptableObject{

	public enum ArmorTypeEnum{	Absolute, Destructable, Discardable	}

	public ArmorTypeEnum type;
}


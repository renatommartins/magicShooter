using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraDamage : MonoBehaviour {

	private Character character;

	public List<MonoBehaviour> damageEffects = new List<MonoBehaviour>();

	void Awake(){
		character = GetComponent<Character>();
	}
	
	// Update is called once per frame
	void Update () {
		if(character.headState.isBroken){
			foreach(MonoBehaviour monoBehaviour in damageEffects){
				monoBehaviour.enabled = true;
			}
		}else{
			foreach(MonoBehaviour monoBehaviour in damageEffects){
				monoBehaviour.enabled = false;
			}
		}
	}
}

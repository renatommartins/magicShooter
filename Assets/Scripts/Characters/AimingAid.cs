using UnityEngine;
using System.Collections;

public class AimingAid : MonoBehaviour{

	private bool isLocalPlayer;
	public Transform aimQuad;

	// Use this for initialization
	void Start (){
		isLocalPlayer = GetComponent<Character>().isLocal;
		if(isLocalPlayer){	aimQuad.gameObject.SetActive(false);	}
	}
	
	// Update is called once per frame
	void Update (){
		if(!isLocalPlayer){
			aimQuad.LookAt(PlayerNetworkManager.Instance.localPlayer.transform);
		}
	}
}


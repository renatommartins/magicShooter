using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// Game system. Manages the state of the game in general.
/// Layer tracking, score update, restarting game...
/// </summary>
public class GameSystem : Singleton<GameSystem> {

	public delegate void CallbackHook();
	private static event CallbackHook UpdateHook;
	private static event CallbackHook LateUpdateHook;
	private static event CallbackHook FixedUpdateHook;

	[SerializeField]
	private PartsDatabase _partsDatabase;
	public static PartsDatabase partsDatabase{
		get{	return Instance._partsDatabase;	}
	}

	[SerializeField]
	private bool lockMouse = true;
	[SerializeField]
	private Canvas networkCanvas;
	[SerializeField]
	private BattleUI battleCanvas;
	public Text speedText;

	public void switchCanvas(){
		networkCanvas.gameObject.SetActive(false);
		battleCanvas.enabled = true;
		if(lockMouse){
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
	}

	void Awake(){
		GameObject.DontDestroyOnLoad(this);
	}

	public static void SubscribeToUpdate(CallbackHook subscriber){	UpdateHook += subscriber;	}
	public static void UnsubscribeFromUpdate(CallbackHook subscriber){	UpdateHook -= subscriber;	}
	void Update()		{	if(UpdateHook != null)		{	UpdateHook();		}	}

	public static void SubscribeToLateUpdate(CallbackHook subscriber){	LateUpdateHook += subscriber;	}
	public static void UnsubscribeFromLateUpdate(CallbackHook subscriber){	LateUpdateHook -= subscriber;	}
	void LateUpdate()	{	if(LateUpdateHook != null)	{	LateUpdateHook();	}	}

	public static void SubscribeToFixedUpdate(CallbackHook subscriber){	FixedUpdateHook += subscriber;	}
	public static void UnsubscribeFromFixedUpdate(CallbackHook subscriber){	FixedUpdateHook -= subscriber;	}
	void FixedUpdate()	{	if(FixedUpdateHook != null)	{	FixedUpdateHook();	}	}

	void OnApplicationFocus(bool focus){
		if(true && Game.Network.NetworkManager.networkReady && lockMouse){
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}else{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
	}
}

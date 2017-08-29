using UnityEngine;
using System.Collections;

public class TransformFollow : MonoBehaviour{

	public Camera mainCamera;
	private Camera camera;
	// Use this for initialization
	void Start (){
		camera = GetComponent<Camera>();
	}
	
	// Update is called once per frame
	void Update (){
		camera.enabled = mainCamera.enabled;
		transform.position = mainCamera.transform.position;
		transform.rotation = mainCamera.transform.rotation;
	}
}


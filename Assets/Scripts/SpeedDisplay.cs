using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SpeedDisplay : MonoBehaviour {

	public Rigidbody controlRigidbody;

	private Text text;

	// Use this for initialization
	void Awake () {
		text = GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {
		text.text = "Speed: " + (controlRigidbody.velocity.magnitude *3.6f).ToString("0.0") + " Km/h";
	}
}

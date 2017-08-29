using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class ParticleEffect : PoolObject {

	private ParticleSystem particleComponent;
	private Transform followTransform;

	void Awake(){
		particleComponent = GetComponent<ParticleSystem>();
	}

	void Update(){
		if(followTransform != null){	transform.position = followTransform.position;	}
		if(!particleComponent.IsAlive() && gameObject.activeSelf){
			particleComponent.Stop();
			followTransform = null;
			gameObject.SetActive(false);
//			NetworkServer.UnSpawn(gameObject);
		}
	}

	public void SetParticle(Vector3 position, Vector3 facing){
		transform.position = position;
		transform.forward = facing;
		particleComponent.Play();
	}

	public void SetParticle(Transform followedTranform, Vector3 facing){
		followTransform = followedTranform;
		transform.forward = facing;
		particleComponent.Play();
	}
}

using UnityEngine;
//using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class Projectile : PoolObject {

	public LayerMask collisionLayer;
	public GameObject hitEffectPrefab;
	public GameObject hitEffectPrefab2;

	public int damage;
	public DamageType damageType;

	public float time;
	public Vector3 startPosition;
	public Vector3 direction;
	private Vector3 lastPosition; 
	public Vector3 currentPosition;

	public float startSpeed;
	public Vector3 startSpeedVector;
	public Vector3 currentSpeed;

	TrailRenderer trailRenderer;
	float trailTime;
	int activateTrail;

	public int ownerId;


	// Use this for initialization
	void Awake () {
		trailRenderer = GetComponent<TrailRenderer>();
		trailTime = trailRenderer.time;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		RaycastHit hitInfo;
		Vector3 newPosition;
		Vector3 rotation;
		time += Time.fixedDeltaTime;
		newPosition = CalculatePosition(time, startSpeedVector, Physics.gravity, out rotation);
		currentPosition = startPosition + newPosition;
		transform.LookAt(currentPosition);
		#if (DEBUG_ALL || DEBUG_PROJECTILES)
		Debug.DrawLine(lastPosition,currentPosition);
		#endif

		if(Physics.Raycast(lastPosition, transform.forward, out hitInfo, Vector3.Distance(lastPosition,currentPosition), collisionLayer.value)){		
			Character characterHit = null;
			foreach(KeyValuePair<List<Collider>, Character> pair in Character.allColliders){
				if(pair.Key.Contains(hitInfo.collider)){	characterHit = pair.Value;	break;	}
			}
			if(characterHit != null){
				if(characterHit.ownerNetId != ownerId){
					foreach(KeyValuePair<List<Collider>, BaseBotPart> pair in characterHit.colliderDictionary){
						if(pair.Key.Contains(hitInfo.collider)){
							#if (DEBUG_ALL || DEBUG_PROJECTILES)
							Debug.Log(ownerId+"|"+characterHit.ownerNetId);
							if(string.Equals(characterHit.name,"TestTarget")){	Debug.Log("TestTarget Hit");	}
							#endif
							if(ownerId != characterHit.ownerNetId){	characterHit.HandleHit(damage, damageType,pair.Value.partType);	}
							break;	
						}
					}
					ParticleEffect particleEffect = ParticleManager.RequestInstance(hitEffectPrefab);
					particleEffect.SetParticle(hitInfo.point, (hitInfo.normal+transform.forward).normalized);
					gameObject.SetActive(false);
				}
			}else{
				#if (DEBUG_ALL || DEBUG_PROJECTILES)
				Debug.Log(hitInfo.point.ToString());
				#endif
				ParticleEffect particleEffect = ParticleManager.RequestInstance(hitEffectPrefab2);
				particleEffect.SetParticle(hitInfo.point, (hitInfo.normal+transform.forward).normalized);
				gameObject.SetActive(false);
			}
//			NetworkServer.UnSpawn(gameObject);
		}

		transform.position = currentPosition;
		lastPosition = currentPosition;
		transform.eulerAngles = rotation;
	}

	//Trail renderer pool fix
	void Update(){
		if(activateTrail < 2){	activateTrail ++;	}
		if(activateTrail == 2){	
			trailRenderer.time = trailTime;
			activateTrail ++;
		}
	}

	Vector3 CalculatePosition(float t, Vector3 initialSpeed, Vector3 acceleration, out Vector3 rotation){
		Vector3 result, speed;

		speed.x = initialSpeed.x + acceleration.x * t;
		speed.y = initialSpeed.y + acceleration.y * t;
		speed.z = initialSpeed.z + acceleration.z * t;

		result.x = initialSpeed.x * t + acceleration.x * t * t * 0.5f;
		result.y = initialSpeed.y * t + acceleration.y * t * t * 0.5f;
		result.z = initialSpeed.z * t + acceleration.z * t * t * 0.5f;

		rotation.x = Mathf.Atan2(-speed.y,speed.z)*Mathf.Rad2Deg;
		rotation.y = Mathf.Atan2(speed.x,speed.z)*Mathf.Rad2Deg;
		rotation.z = 0;

		return result;
	}

	public void SetProjectile(int damage, Transform parentTransform, float initialSpeed){
		time = 0;

		this.damage = damage;
		direction = parentTransform.forward;
		startPosition = parentTransform.position;
		startSpeedVector = direction * initialSpeed;
		currentSpeed = startSpeedVector;
		lastPosition = startPosition;
		transform.position = startPosition;
	}

	public void SetProjectile(int damage, DamageType damageType, Vector3 position, Vector3 direction, float initialSpeed, int ownerId){
		time = 0;

		this.damage = damage;
		this.damageType = damageType;
		this.direction = direction;
		startPosition = position;
		startSpeedVector = direction * initialSpeed;
		currentSpeed = startSpeedVector;
		lastPosition = startPosition;
		transform.position = startPosition;

		this.ownerId = ownerId;
	}

	void OnDisable(){
		trailRenderer.time = -1;
	}

	void OnEnable(){
		activateTrail = 0;
	}
}

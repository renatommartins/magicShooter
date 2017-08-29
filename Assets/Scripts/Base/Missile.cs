using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Missile : PoolObject {

	GameObject explosionPrefab;

	TrailRenderer trailRenderer;
	float trailTime;
	int activateTrail;

	public LayerMask collisionLayer;

	public Transform target;
	private int damage;
	private DamageType damageType;
	private float radius;

	public float speed;
	public float angularSpeed;
	public float holdAltitude;
	public float diveDistance = 0;
	public float diveAngle;
	public float armTime;
	public float timer = 0;
	public float maxFlightTime;
	public float triggerDistance;
	public bool usePrediction;
	public int predictionSamples;

	public float distance;
	public float timeToTarget;

	public MissileGuidance guidance;
	public Dictionary<string,object> dynamicVar = new Dictionary<string, object>();

	public List<Vector3> latestTargetPositions = new List<Vector3>();
	public Vector3 lastPosition = Vector3.zero;
	public Vector3 correction;

	public void SetMissile(Transform target, Vector3 position, Vector3 direction, int damage, DamageType damageType, float radius, float speed, float angularSpeed, float armTime, float maxFlightTime, float triggerDistance, bool usePrediction, float holdAltitude, float diveAngle, MissileGuidance guidance){
		transform.position = position;
		transform.forward = direction;

		timer = 0;

		this.damage = damage;
		this.damageType = damageType;
		this.radius = radius;

		this.target = target;
		this.speed = speed;
		this.angularSpeed = angularSpeed;
		this.armTime = armTime;
		this.maxFlightTime = maxFlightTime;
		this.triggerDistance = triggerDistance;
		this.usePrediction = usePrediction;
		this.holdAltitude = holdAltitude;
		this.diveAngle = diveAngle;

		this.guidance = guidance;

		guidance.SetupGuidance(this);
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if(timer < maxFlightTime){
			transform.rotation = guidance.GetNextRotation(this);
		}else{
			transform.rotation = Quaternion.RotateTowards(transform.rotation,Quaternion.LookRotation(Vector3.down),angularSpeed*Time.fixedDeltaTime);
		}

//		if(triggerDistance > distance){
//			RaycastHit[] explosionHit;
//			explosionHit = Physics.SphereCastAll(transform.position, 10, transform.forward);
//			foreach(RaycastHit hit in explosionHit){
//				
//			}
//		}

		RaycastHit hit;
		if(Physics.Raycast(transform.position,transform.forward, out hit,speed*Time.fixedDeltaTime,collisionLayer.value)){
			if(hit.collider.gameObject.layer == Layers.TerrainLayer){		}
			if(hit.collider.gameObject.layer == Layers.BotPart || hit.collider.gameObject.layer == Layers.OwnBotPart){	
				Character characterHit = null;
				foreach(KeyValuePair<List<Collider>, Character> pair in Character.allColliders){
					if(pair.Key.Contains(hit.collider)){	characterHit = pair.Value;	break;	}
				}
				foreach(KeyValuePair<List<Collider>, BaseBotPart> pair in characterHit.colliderDictionary){
					if(pair.Key.Contains(hit.collider)){
						#if (DEBUG_ALL || DEBUG_MISSILES)
						Debug.Log(characterHit.ownerNetId);
						if(string.Equals(characterHit.name,"TestTarget")){	Debug.Log("TestTarget Hit");	}
						#endif
						characterHit.HandleHit(damage, damageType, pair.Value.partType);
					}
				}
			}
			guidance.ClearGuidance(this);
			gameObject.SetActive(false);
		}

		transform.position += transform.forward*speed*Time.fixedDeltaTime;
		timer += Time.fixedDeltaTime;
	}

	//Trail pool fix
	void Awake () {
		trailRenderer = GetComponent<TrailRenderer>();
		trailTime = trailRenderer.time;
	}
	void Update(){
		if(activateTrail < 2){	activateTrail ++;	}
		if(activateTrail == 2){	
			trailRenderer.time = trailTime;
			activateTrail ++;
		}
	}
	void OnDisable(){	trailRenderer.time = -1;	}
	void OnEnable()	{	activateTrail = 0;			}
}


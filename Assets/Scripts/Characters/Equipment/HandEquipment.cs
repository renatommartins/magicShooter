//using UnityEngine;
//using UnityEngine.Networking;
//using System.Collections;
//using System.Collections.Generic;
//
//[System.Serializable]
//public class HandEquipment : BaseEquipment {
//
//	public enum WeaponType {	Melee, Ranged	};
//
//	public WeaponType weaponType;
//	[Header("Weapon Settings")]
//	public GameObject projectilePrefab;
//	public float roundsPerMinute;
//	public int projectileDamage;
//	public int projectileAmount;
//	public float projectileSpeed;
//	[Header("Accuracy Settings")]
//	public float minimumSpread;
//	public float maximumSpread;
//	public float coneAngle;
//	public AnimationCurve spreadDistribution;
//	public float maximumSpreadTime;
//	public float currentMaximumSpread;
//	public Vector3 currentSpread;
//
//	public float reloadTime;
//	public float lastTimeFired;
//	public float firingTime;
//
//	private Transform aimTransform;
//	public Transform barrelEnd;
//
//	public override void EquipmentUpdate (Object obj, params float[] parameters){
//		if(reloadTime == 0){	reloadTime = 60 / roundsPerMinute;	}
//
//		float timeSinceLastFire = Time.time-lastTimeFired;
//		cooldown = Mathf.Clamp01(timeSinceLastFire/reloadTime);
//		if(timeSinceLastFire > reloadTime){	isAvaliable = true;	}else{	isAvaliable = false;	}
//
//		if(!(timeSinceLastFire > reloadTime+0.1f)){
//			firingTime = Mathf.Clamp(firingTime + Time.deltaTime,0,maximumSpreadTime);
//		}else{
//			firingTime = Mathf.Clamp(firingTime - Time.deltaTime,0,maximumSpreadTime);
//		}
//		currentMaximumSpread = (((maximumSpread - minimumSpread)*Mathf.Clamp(firingTime,0,maximumSpreadTime)
//			/maximumSpreadTime)+minimumSpread);
//
//		WeaponControl weaponControl = obj as WeaponControl;
//		weaponControl.AimGun(projectileSpeed,barrelEnd);
//	}
//
//	public override bool Action (bool isRightSide){
//		if(Time.time-lastTimeFired > reloadTime){
//			int randomSeed = Time.frameCount;
//			PlayerNetworkManager.SendPlayerAction((isRightSide? PlayerNetworkManager.PlayerCommand.FireRight:PlayerNetworkManager.PlayerCommand.FireLeft), randomSeed,
//				barrelEnd.position.x,barrelEnd.position.y,barrelEnd.position.z,
//				barrelEnd.forward.x,barrelEnd.forward.y,barrelEnd.forward.z,
//				currentMaximumSpread);
//			
//			lastTimeFired = Time.time;
//
//			Fire(Game.Network.NetworkManager.netId, barrelEnd.position, barrelEnd.forward, randomSeed, currentMaximumSpread);
//
//			return true;
//		}else{
//			return false;
//		}
//	}
//
//	public void Fire(int netId, Vector3 firingPoint, Vector3 firingDirection, int randomSeed, float currentMaximumSpread){
//		Vector3 originalPosition = barrelEnd.position;
//		Vector3 originalRotation = barrelEnd.eulerAngles;
//		Random.InitState(randomSeed);
//		barrelEnd.position = firingPoint;
//		barrelEnd.forward = firingDirection;
//		Vector3 recoilSpread = new Vector3(	spreadDistribution.Evaluate(Random.value) * currentMaximumSpread - currentMaximumSpread/2,
//			spreadDistribution.Evaluate(Random.value) * currentMaximumSpread - currentMaximumSpread/2);
//		#if (DEBUG_ALL || DEBUG_HANDWEAPON)
//		Debug.Log(firingPoint);
//		Debug.Log(firingDirection);
//		Debug.Log(recoilSpread);
//		#endif
//		if(projectileAmount == 1){				
//			barrelEnd.eulerAngles = barrelEnd.eulerAngles + recoilSpread;
//			Projectile instance = ProjectileManager.RequestInstance(projectilePrefab);
//			instance.SetProjectile(projectileDamage, firingPoint, barrelEnd.forward, projectileSpeed, netId);
//		}else{
//			Vector3 coneDirection = barrelEnd.eulerAngles + recoilSpread;
//			#if (DEBUG_ALL || DEBUG_HANDWEAPON)
//			Debug.Log(coneDirection.ToString());
//			#endif
//			for(int i=0; i<projectileAmount; i++){
//				barrelEnd.eulerAngles = coneDirection;
//				Vector3 pelletSpread =  new Vector3(	spreadDistribution.Evaluate(Random.value) * coneAngle - coneAngle/2,
//					spreadDistribution.Evaluate(Random.value) * coneAngle - coneAngle/2);
//				//Debug.Log(pelletSpread.ToString());
//				barrelEnd.eulerAngles = barrelEnd.eulerAngles + pelletSpread;
//				Projectile instance = ProjectileManager.RequestInstance(projectilePrefab);
//				instance.SetProjectile(projectileDamage, firingPoint, barrelEnd.forward, projectileSpeed, netId);
//			}
//		}
//
//		barrelEnd.position = originalPosition;
//		barrelEnd.eulerAngles = originalRotation;
//	}
//}

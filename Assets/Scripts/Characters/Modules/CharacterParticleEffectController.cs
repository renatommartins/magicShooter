using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using System.Linq;

public class CharacterParticleEffectController : MonoBehaviour {

	private Character character;

	public GameObject damageParticlePrefab;

	private PartEffectHolder headParticleEffect = new PartEffectHolder(BotPartEnum.Head);
	private PartEffectHolder torsoParticleEffect = new PartEffectHolder(BotPartEnum.Torso);
	private PartEffectHolder leftArmParticleEffect = new PartEffectHolder(BotPartEnum.LeftArm);
	private PartEffectHolder rightArmParticleEffect = new PartEffectHolder(BotPartEnum.RightArm);
	private PartEffectHolder legsParticleEffect = new PartEffectHolder(BotPartEnum.Legs);

	void Awake(){
		character = GetComponent<Character>();
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		CheckIfDestroyed(character.headState);
		CheckIfDestroyed(character.torsoState);
		CheckIfDestroyed(character.lArmState);
		CheckIfDestroyed(character.rArmState);
		CheckIfDestroyed(character.legsState);
	}

	void CheckIfDestroyed(PartState part){
//		PartEffectHolder particleEffectHolder;
//
//		switch(part.partType){
//		case BotPartEnum.Head:
//			particleEffectHolder = headParticleEffect;
//			break;
//		case BotPartEnum.Torso:
//			particleEffectHolder = torsoParticleEffect;
//			break;
//		case BotPartEnum.LeftArm:
//			particleEffectHolder = leftArmParticleEffect;
//			break;
//		case BotPartEnum.RightArm:
//			particleEffectHolder = rightArmParticleEffect;
//			break;
//		case BotPartEnum.Legs:
//			particleEffectHolder = legsParticleEffect;
//			break;
//		default: 
//			particleEffectHolder = null;
//			break;
//		}
//		if(part.isBroken){	
//			if(!particleEffectHolder.particleEffectList.Exists(ParticleEffect => ParticleEffect.originalPrefab == damageParticlePrefab)){
//				ParticleEffect newInstance = ParticleManager.RequestInstance(damageParticlePrefab);
//				newInstance.SetParticle(part.effectTransform, Vector3.up);
//				particleEffectHolder.particleEffectList.Add(newInstance);
//			}
//		}else{
//			ParticleEffect particleEffect = particleEffectHolder.particleEffectList.Find(ParticleEffect => ParticleEffect.originalPrefab == damageParticlePrefab);
//			if(particleEffect != null){
//				particleEffectHolder.particleEffectList.Remove(particleEffect);
//				particleEffect.gameObject.SetActive(false);
//			}
//		}
	}

	private class PartEffectHolder{
		public BotPartEnum partType;

		public List<ParticleEffect> particleEffectList;

		public PartEffectHolder(BotPartEnum partType){
			this.partType = partType;
			particleEffectList = new List<ParticleEffect>();
		}
	}
}

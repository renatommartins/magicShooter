using UnityEngine;
using System.Collections;

[CreateAssetMenu]
public class Legs : BaseBotPart {

	public int weightLimit;

	public AnimationCurve walkForceCurve;
	public float walkForceMax;
	public float walkMaxSpeed;

	public AnimationCurve dashForceCurve;
	public float dashForceMax;
	public float dashMaxSpeed;

	public override BotPartEnum partType {
		get {
			return BotPartEnum.Legs;
		}
	}
}

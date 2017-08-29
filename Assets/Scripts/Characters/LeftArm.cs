using UnityEngine;
using System.Collections;

[CreateAssetMenu]
public class LeftArm : Arm {

	public override BotPartEnum partType {
		get {
			return BotPartEnum.LeftArm;
		}
	}
}

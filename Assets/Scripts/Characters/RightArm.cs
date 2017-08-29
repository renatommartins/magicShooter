using UnityEngine;
using System.Collections;

[CreateAssetMenu]
public class RightArm : Arm {

	public override BotPartEnum partType {
		get {
			return BotPartEnum.RightArm;
		}
	}
}

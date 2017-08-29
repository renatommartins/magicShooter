using UnityEngine;
using System.Collections;

[CreateAssetMenu]
public class Torso : BaseBotPart {

	public int power;

	public override BotPartEnum partType {
		get {
			return BotPartEnum.Torso;
		}
	}
}

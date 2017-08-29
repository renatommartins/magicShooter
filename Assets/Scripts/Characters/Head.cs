using UnityEngine;
using System.Collections;

[CreateAssetMenu]
public class Head : BaseBotPart {

	public float radarRange;

	public override BotPartEnum partType {
		get {
			return BotPartEnum.Head;
		}
	}
}

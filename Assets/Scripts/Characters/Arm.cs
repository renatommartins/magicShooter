using UnityEngine;
using System.Collections;

public abstract class Arm : BaseBotPart {

	public float accuracy;
	public bool shoulderMount;
	public bool handMount;

	public override abstract BotPartEnum partType {	get;	}
}

using UnityEngine;
using System.Collections;

[System.Serializable]
public class WeaponBase {

	[Header("Weapon Settings")]
	public GameObject projectilePrefab;
	public float roundsPerMinute;
	public int projectileDamage;
	public float projectileSpeed;

	[Header("Accuracy Settings")]
	public float minimumSpread;
	public float maximumSpread;
	public AnimationCurve spreadDistribution;
	public float maximumSpreadTime;
	private float currentSpread;

	public float reloadTimeLeft;
}

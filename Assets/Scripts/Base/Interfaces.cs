using UnityEngine;
using System.Collections;

public interface IMoveable{	void Move(Vector2 velocity);	}

public interface IFireable{	void Fire(int damage, float speed, float angle);	}

public interface IDamageable{	void TakeDamage(int damage);	}
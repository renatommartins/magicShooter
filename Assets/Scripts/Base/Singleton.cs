using UnityEngine;
using System.Collections;

/// <summary>
/// Singleton.
/// Generic Class to inherit classes that needs only one instance each time
/// </summary>
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
	protected static T instance;

	/// <summary>
	/// Returns the instance of this singleton.
	/// </summary>
	/// <value>The instance.</value>
	public static T Instance{
		get{
			if(instance == null){
				//Checks if there is one and only one instance of a singleton
				T[] objects = (T[]) FindObjectsOfType(typeof(T));
				instance = objects[0];

				if(objects.Length > 1){
					Debug.LogError("More than one instance of " + typeof(T) + " exists in the scene, undesired behaviour may occur.\nEnsure that only one instance exists.");
				}

				if (instance == null){
					Debug.LogError("An instance of " + typeof(T) + " is needed in the scene, but there is none.");
				}
			}
			return instance;
		}
	}
}
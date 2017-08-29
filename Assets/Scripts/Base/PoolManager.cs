using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Collections;

public abstract class PoolManager<T> : Singleton<PoolManager<T>> where T:PoolObject {

	[SerializeField]
	List<Pool<T>> poolList = new List<Pool<T>>();
	[SerializeField]
	Transform organizeTransform;

	GameObject prefab;

//	public NetworkHash128 assetId{	get; set;	}


	void Awake(){
		InitializePool();
	}

	void InitializePool(){
		foreach(T objectType in Resources.FindObjectsOfTypeAll<T>()){
			poolList.Add(new Pool<T>(objectType.gameObject,organizeTransform));
//			ClientScene.RegisterPrefab(objectType.gameObject, objectType.GetComponent<NetworkIdentity>().assetId);
//			ClientScene.RegisterSpawnHandler(objectType.GetComponent<NetworkIdentity>().assetId, NetworkRequestInstance, NetworkReturnInstance);
		}
	}

	void Update(){
		foreach(Pool<T> pool in poolList){
			List<T> unusedInstances = new List<T>();

			foreach(T inUseObject in pool.inUse){
				if(!inUseObject.gameObject.activeSelf){	unusedInstances.Add(inUseObject);	}
			}
			foreach(T instance in unusedInstances){
				pool.inUse.Remove(instance);
				pool.pool.Enqueue(instance);
			}
		}

	}

//	public delegate GameObject SpawnDelegate(Vector3 position, NetworkHash128 assetId);
//	public delegate void UnSpawnDelegate(GameObject spawned);
//
//	public GameObject NetworkRequestInstance(Vector3 position, NetworkHash128 assetID){
//		foreach(Pool<T> pool in poolList){
//			if(NetworkHash128.Equals(pool.assetID, assetId)){
//				return RequestInstance(pool.prefab).gameObject;
//			}
//		}
//		return null;
//	}
//
//	public void NetworkReturnInstance(GameObject inUse){
//		inUse.SetActive(false);
//	}
//
	public static T RequestInstance(GameObject prefab){
		Instance.prefab = prefab;
		// Finds the correct pool
		Pool<T> pool = Instance.poolList.Find(Instance.FindPool);
		T instance;

		if(pool.pool.Count != 0){
			instance = pool.pool.Dequeue();
		}else{
		// If none is avaliable, instantiate another five and pick the first one
			pool.Instantiate(5);
			instance = pool.pool.Dequeue();
		}
		// takes the Enemy out of the pool
		instance.gameObject.SetActive(true);
		pool.inUse.Add(instance);

		return instance;
	}

	[System.Serializable]
	public class Pool<U>{

		public GameObject prefab;
//		public NetworkHash128 assetID;
		public Queue<U> pool = new Queue<U>();
		public HashSet<U> inUse = new HashSet<U>();
		public Transform holderTransform;

		public Pool(GameObject prefab, Transform parent){
			this.prefab = prefab;
//			assetID = prefab.GetComponent<NetworkIdentity>().assetId;
			GameObject holder = new GameObject(prefab.name);
			holder.transform.parent = parent;
			holderTransform = holder.transform;
			Instantiate(5);
		}

		public void Instantiate(int quantity){
			PoolObject instancePoolObject = null;
			for(int i=0; i<quantity; i++){
				GameObject newInstance = GameObject.Instantiate(prefab);
				newInstance.SetActive(false);
				newInstance.transform.parent = holderTransform;
				instancePoolObject = newInstance.GetComponent<PoolObject>();
				instancePoolObject.originalPrefab = prefab;
				pool.Enqueue(newInstance.GetComponent<U>());
			}
		}
	}

	private bool FindPool(Pool<T> pool){
		if(pool.prefab == prefab){	return true;	}
		else{	return false;	}
	}
}


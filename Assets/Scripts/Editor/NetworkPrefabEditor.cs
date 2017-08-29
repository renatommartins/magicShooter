using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class NetworkPrefabEditor {

	[MenuItem("Databases/Update Network Prefabs")]
	static public void UpdateNetworkPrefabs(){
		string path = string.Concat("Assets/", "NetworkPrefabsDatabase.asset" );
		GameObject[] netPrefabs = GetNetworkedPrefabs();

		NetworkPrefabDatabase prefabDatabase = (NetworkPrefabDatabase)AssetDatabase.LoadAssetAtPath(path, typeof(NetworkPrefabDatabase));

		if(prefabDatabase == null){
			prefabDatabase = ScriptableObject.CreateInstance<NetworkPrefabDatabase>();
			AssetDatabase.CreateAsset(prefabDatabase, path);
		}

		prefabDatabase.networkPrefabs = new List<GameObject>();
		Debug.Log("Network Prefab Database Contents");
		for(int i = 0; i<netPrefabs.Length; i++){
			prefabDatabase.networkPrefabs.Add(netPrefabs[i]);
			netPrefabs[i].GetComponent<NetworkInstance>().prefabId = i;
			Debug.Log("Prefab name: "+netPrefabs[i].name+"\nPrefab Instance ID: "+netPrefabs[i].GetInstanceID()+"\nNetwork Prefab ID: "+i);
		}
		AssetDatabase.SaveAssets();

		AssetDatabase.Refresh();
	}

	private static GameObject[] GetNetworkedPrefabs(){
		List<GameObject> prefabs = new List<GameObject>();
		foreach(NetworkInstance prefab in  Resources.FindObjectsOfTypeAll<NetworkInstance>()){
			prefabs.Add(prefab.gameObject);
		}
		return prefabs.ToArray();
	}
}

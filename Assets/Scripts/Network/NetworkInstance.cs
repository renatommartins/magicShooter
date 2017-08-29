using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class NetworkInstance : MonoBehaviour{
	private static HashSet<int> usedInstanceId = new HashSet<int>();

	public int netId;
	private int _instanceId;
	private bool isInstanceIdSet = false;
	public int instanceId{	get{
			if(!isInstanceIdSet){	_instanceId = GetNextAvaliableId();	}
			return _instanceId;
		}
	}

	public int prefabId;
//	private bool isPrefabIdSet;
//	public int prefabId{
//		get{	return _prefabId;	}
//		set{	if(!isPrefabIdSet){	isPrefabIdSet = true;	_prefabId = value; 	}	}
//	}

	private static int GetNextAvaliableId(){
		for(int i = 0; i < int.MaxValue; i++){
			if(!usedInstanceId.Contains(i)){	return i;	}
		}
		return -1;
	}
}
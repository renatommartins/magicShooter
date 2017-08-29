using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Game.Network;
using System.Linq;
using System.Reflection;

//public class NetworkInstanceManager : NetworkSingleton<NetworkInstanceManager> {
//
//	private enum MessageType {	InstanceCreated, InstanceDestroyed, InstanceSync, RemoteProcedureCall	};
//
//	//private enum PrimitiveType {	Byte, Char, String, Int32, Float, Double	};
//
//	public NetworkPrefabDatabase prefabDatabase;
//
//	private Dictionary<UniqueInstanceIdentifier,NetworkInstance> localInstances = new Dictionary<UniqueInstanceIdentifier,NetworkInstance>();
//	private Dictionary<UniqueInstanceIdentifier,NetworkInstance> remoteInstances = new Dictionary<UniqueInstanceIdentifier,NetworkInstance>();
//
//	private Dictionary<System.Type,List<FieldInfo>> syncPropertiesDictionary = new Dictionary<System.Type, List<FieldInfo>>();
//	private Dictionary<NetworkComponent,List<NetworkSyncVariable>>  localSyncInstancesDictionary = new Dictionary<NetworkComponent, List<NetworkSyncVariable>>();
//
//	private Dictionary<System.Type,List<MethodInfo>> rpcMethodsDictionary = new Dictionary<System.Type, List<MethodInfo>>();
//	//private Dictionary<NetworkComponent,List<NetworkSyncVariable>>  remoteSyncInstancesDictionary = new Dictionary<NetworkComponent, List<NetworkSyncVariable>>();
//
//	public new readonly int componentId = 5;
//
//	private class SyncValueComparer : IComparer<FieldInfo>{
//		public int Compare(FieldInfo x, FieldInfo y){
//			if(x == null && y == null){	return 0;	}
//			return string.Compare(x.Name, y.Name);
//		}
//	}
//
//	void Awake(){
//
//		//Finds all classes that inherits from NetworkComponent
//		foreach(var netComponent in Resources.FindObjectsOfTypeAll<NetworkComponent>()){
//			System.Type compType = netComponent.GetType();
//			Debug.Log("Found Component of type: "+ compType.Name);
//			//Caches the class and initializes a list for all its SyncVar tagged members
//			if(!syncPropertiesDictionary.ContainsKey(compType)){
//				syncPropertiesDictionary.Add(compType,new List<FieldInfo>());
//				//Goes thru each member of the class
//				foreach(FieldInfo property in compType.GetFields()){
//					Debug.Log("Reading Property: " + property);
//					//Gets the attributes of the member
//					object[] attributes = property.GetCustomAttributes(false);
//					//Checks each attribute for the SyncVar attribute
//					foreach(object obj in attributes){
//						System.Type type = obj.GetType();
//						Debug.Log(type.Name);
//						//If the member is tagged with SyncVar, add to the list
//						if(type == typeof(SyncVar)){
//							Debug.Log("Is SyncVar");
//							syncPropertiesDictionary[compType].Add(property);
//						}
//					}
//				}
//
//				rpcMethodsDictionary.Add(compType,new List<MethodInfo>());
//				foreach(MethodInfo method in compType.GetMethods()){
//					Debug.Log("Reading method: " + method);
//
//					object[] attributes = method.GetCustomAttributes(false);
//
//					foreach(object obj in attributes){
//						System.Type type = obj.GetType();
//
//						if(type == typeof(Game.Network.RPC)){
//							Debug.Log("Is RPC");
//							rpcMethodsDictionary[compType].Add(method);
//						}
//					}
//				}
//				//Sort the list so all cliets are in sync with the member ordering
//				syncPropertiesDictionary[compType].OrderBy(FieldInfo => FieldInfo.Name);
//				rpcMethodsDictionary[compType].OrderBy(MethodInfo => MethodInfo.Name);
//			}
//		}
//		foreach(KeyValuePair<System.Type,List<FieldInfo>> pair in syncPropertiesDictionary){
//			Debug.Log("Type: "+pair.Key.Name);
//			foreach(FieldInfo property in pair.Value){
//				Debug.Log("Property"+property.Name);
//			}
//		}
//
//	}
//
//	// Use this for initialization
//	void Start () {
//		NetworkManager.Instance.StartListening(componentId, ReceiveMessage, OnConnect, OnDisconnect, OnRemoteConnection, OnRemoteDisconnection);
//
//		Debug.Log("Network Prefab Database Contents");
//		for(int i=0; i<prefabDatabase.networkPrefabs.Count; i++){
//			Debug.Log("Prefab name: "+prefabDatabase.networkPrefabs[i].name+"\nPrefab Instance ID: "+prefabDatabase.networkPrefabs[i].GetInstanceID()+"\nNetwork Prefab ID: "+i);
//		}
//	}
//	
//	// Update is called once per frame
//	void FixedUpdate () {
//		foreach(KeyValuePair<NetworkComponent,List<NetworkSyncVariable>> pair in localSyncInstancesDictionary){
//			for(int i=0; i< pair.Value.Count; i++){
//				if(pair.Value[i].refreshCounter > pair.Value[i].refreshTime){
//					pair.Value[i].refreshCounter += (Time.fixedDeltaTime-pair.Value[i].refreshTime);
//
//					var oldValue = System.Convert.ChangeType(pair.Value[i].variable,pair.Value[i].variable.GetType());
//					var newValue = System.Convert.ChangeType(pair.Value[i].fieldInfo.GetValue(pair.Key),pair.Value[i].type);
//					if(!object.Equals(oldValue, newValue)){
//						/*Debug.Log(pair.Value[i].fieldInfo.Name + "Value has changed, old: "+oldValue.ToString()+"| new: "+newValue.ToString());*/
//						pair.Value[i].variable = newValue;
//						byte[] buffer;
//						using(MemoryStream stream = new MemoryStream()){
//							using(BinaryWriter writer = new BinaryWriter(stream)){
//								writer.Write((int) MessageType.InstanceSync);
//								writer.Write(myNetId);
//								writer.Write(pair.Key.GetComponent<NetworkInstance>().instanceId);
//								writer.Write(pair.Key.GetType().ToString());
//								writer.Write(i);
//								writer.Write(TypeToInt(pair.Value[i].type));
//								if(pair.Value[i].type == typeof(System.Byte))	{	writer.Write((byte)newValue);	/*Debug.Log("Sending Sync of type Byte");*/		}
//								if(pair.Value[i].type == typeof(System.Char))	{	writer.Write((char)newValue);	/*Debug.Log("Sending Sync of type Char");*/		}
//								if(pair.Value[i].type == typeof(System.Double))	{	writer.Write((double)newValue);	/*Debug.Log("Sending Sync of type Double");*/	}
//								if(pair.Value[i].type == typeof(System.Single))	{	writer.Write((float)newValue);	/*Debug.Log("Sending Sync of type Single");*/	}
//								if(pair.Value[i].type == typeof(System.Int32))	{	writer.Write((int)newValue);	/*Debug.Log("Sending Sync of type Int32");*/	}
//								if(pair.Value[i].type == typeof(System.String))	{	writer.Write((string)newValue);	/*Debug.Log("Sending Sync of type String");*/	}
//								if(pair.Value[i].type == typeof(Vector3))		{
//									Vector3 newVector3 = (Vector3) newValue;
//									writer.Write(newVector3.x);
//									writer.Write(newVector3.y);
//									writer.Write(newVector3.z);
//								}
//							}
//							buffer = stream.ToArray();
//						}
//						NetworkManager.SendNetworkMessage(new ComponentMessage(componentId,-1,myNetId,ChannelType.ReliableStateUpdate,buffer),false);
//					}
//				}else{	pair.Value[i].refreshCounter += Time.fixedDeltaTime;	}
//			}
//		}
//	}
//
//	public override void ReceiveMessage (ComponentMessage message){
//		MessageType messageType;
//		using(MemoryStream stream = new MemoryStream(message.buffer)){
//			using(BinaryReader reader = new BinaryReader(stream)){
//				messageType = (MessageType) reader.ReadInt32();
//				switch(messageType){
//
//				case MessageType.InstanceCreated:
//					int createdOwnerNetId = reader.ReadInt32();
//					int createdInstance = reader.ReadInt32();
//					int createdPrefabId = reader.ReadInt32();
//					if(createdOwnerNetId != myNetId){
//						UniqueInstanceIdentifier identifier = new UniqueInstanceIdentifier(createdOwnerNetId,createdInstance,0);
//						NetworkInstance instance = GameObject.Instantiate(prefabDatabase.networkPrefabs[createdPrefabId]).GetComponent<NetworkInstance>();
//						remoteInstances.Add(identifier,instance);
//						instance.gameObject.name += createdOwnerNetId+":"+createdInstance+"-"+createdPrefabId;
//						instance.SendMessage("SetIsLocal", false);
//					}
//					break;
//
//				case MessageType.InstanceDestroyed:
//					
//					break;
//
//				case MessageType.InstanceSync:
//					int syncOwnerNetId = reader.ReadInt32();
//					int syncInstance = reader.ReadInt32();
//					if(syncOwnerNetId != myNetId){
//						UniqueInstanceIdentifier syncInstanceId = new UniqueInstanceIdentifier(syncOwnerNetId,syncInstance,0);
//						string syncComponentString = reader.ReadString();
//						System.Type syncComponentType = System.Type.GetType(syncComponentString);
//						/*Debug.Log("Updating Component of type: "+syncComponentType.ToString());*/
//						if(!remoteInstances.ContainsKey(syncInstanceId)){	
//							Debug.LogWarning("Instance " + syncInstanceId.ToString() + " not found!");
//							break;
//						}
//						NetworkComponent syncNetworkComponent = (NetworkComponent) remoteInstances[syncInstanceId].GetComponent(syncComponentType);
//						int syncWichVariable = reader.ReadInt32();
//						FieldInfo syncedVariable = syncPropertiesDictionary[syncComponentType][syncWichVariable];
//						/*Debug.Log("Syncing variable: "+syncedVariable.fieldInfo.Name);*/
//						int syncVariableType = reader.ReadInt32();
//						object syncValue = null;
//						switch(syncVariableType){
//						case 0:	syncValue = reader.ReadByte();		/*Debug.Log("Received Byte: "+(syncValue.ToString()));*/	break;
//						case 1:	syncValue = reader.ReadChar();		/*Debug.Log("Received Char: "+(syncValue.ToString()));*/	break;
//						case 2:	syncValue = reader.ReadDouble();	/*Debug.Log("Received Double: "+(syncValue.ToString()));*/	break;
//						case 3:	syncValue = reader.ReadSingle();	/*Debug.Log("Received Single: "+(syncValue.ToString()));*/	break;
//						case 4:	syncValue = reader.ReadInt32();		/*Debug.Log("Received Int32: "+(syncValue.ToString()));*/	break;
//						case 5:	syncValue = reader.ReadString();	/*Debug.Log("Received String: "+(syncValue.ToString()));*/	break;
//						case 6:
//							Vector3 syncVector3 = new Vector3();
//							syncVector3.x = reader.ReadSingle();
//							syncVector3.y = reader.ReadSingle();
//							syncVector3.z = reader.ReadSingle();
//							syncValue = syncVector3;
//							break;
//						}
//						syncedVariable.SetValue(syncNetworkComponent, syncValue);
//						//syncPropertiesDictionary[syncComponentType].syncFields[syncVariableType].SetValue(remoteInstances[syncInstanceId],syncValue);
//					}
//					break;
//
//				case MessageType.RemoteProcedureCall:
//					int rpcNetId = reader.ReadInt32();
//					int rpcInstanceId = reader.ReadInt32();
//					UniqueInstanceIdentifier rpcUniqueId = new UniqueInstanceIdentifier(rpcNetId, rpcInstanceId,0);
//					System.Type rpcCompType = System.Type.GetType(reader.ReadString());
//					string methodName = reader.ReadString();
//					MethodInfo rpcMethodInfo = rpcMethodsDictionary[rpcCompType].Find(MethodInfo => string.Equals(methodName,MethodInfo.Name));
//					ParameterInfo[] rpcParametersInfo = rpcMethodInfo.GetParameters();
//					object[] rpcParameters;
//					{
//						List<object> parameterList = new List<object>();
//						for(int i = 0; i < rpcParametersInfo.Length; i++){
//							if(rpcParametersInfo[i].ParameterType == typeof(System.Byte))	{	parameterList.Add(reader.ReadByte());		}
//							if(rpcParametersInfo[i].ParameterType == typeof(System.Char))	{	parameterList.Add(reader.ReadChar());		}
//							if(rpcParametersInfo[i].ParameterType == typeof(System.Double))	{	parameterList.Add(reader.ReadDouble());	}
//							if(rpcParametersInfo[i].ParameterType == typeof(System.Single))	{	parameterList.Add(reader.ReadSingle());	}
//							if(rpcParametersInfo[i].ParameterType == typeof(System.Int32))	{	parameterList.Add(reader.ReadInt32());		}
//							if(rpcParametersInfo[i].ParameterType == typeof(System.String))	{	parameterList.Add(reader.ReadString());	}
//							if(rpcParametersInfo[i].ParameterType == typeof(Vector3)){
//								Vector3 vector3 = new Vector3();
//								vector3.x = reader.ReadSingle();
//								vector3.y = reader.ReadSingle();
//								vector3.z = reader.ReadSingle();
//								parameterList.Add(vector3);
//							}
//						}
//						rpcParameters = parameterList.ToArray();
//					}
//					NetworkComponent rpcInstance = null;
//					if(remoteInstances.ContainsKey(rpcUniqueId)){	rpcInstance = (NetworkComponent) remoteInstances[rpcUniqueId].GetComponent(rpcCompType);	}
//					else{
//						if(localInstances.ContainsKey(rpcUniqueId)){	rpcInstance = (NetworkComponent) localInstances[rpcUniqueId].GetComponent(rpcCompType);	}
//						else{	Debug.LogWarning("Remote Procedure Call failed");	break;	}
//					}
//					rpcMethodInfo.Invoke(rpcInstance, rpcParameters);
//					break;
//				}
//			}
//		}
//	}
//
//	public void RemoteProcedureCall(NetworkComponent component, int instanceId, int netId, string methodName, object[] parameters){
//		System.Type componentType = component.GetType();
//		MethodInfo methodInfo = rpcMethodsDictionary[componentType].Find(MethodInfo => string.Equals(MethodInfo.Name,methodName));
//		if(methodInfo == null){
//			Debug.LogWarning("Method " + methodName + " in type " + componentType.ToString() + " not registered!");
//			return;
//		}
//
//		ComponentMessage outgoingMessage = new ComponentMessage(componentId, netId, myNetId, ChannelType.ReliableSequenced, null);
//		byte[] buffer;
//
//		using(MemoryStream stream = new MemoryStream()){
//			using(BinaryWriter writer = new BinaryWriter(stream)){
//				writer.Write((int) MessageType.RemoteProcedureCall);
//				writer.Write(myNetId);
//				writer.Write(instanceId);
//				writer.Write(componentType.ToString());
//				writer.Write(methodInfo.Name);
//				ParameterInfo[] parametersInfo = methodInfo.GetParameters();
//				for(int i = 0; i < parametersInfo.Length; i++){
//					if(parametersInfo[i].ParameterType == typeof(System.Byte))	{	writer.Write((byte)parameters[i]);		}
//					if(parametersInfo[i].ParameterType == typeof(System.Char))	{	writer.Write((char)parameters[i]);		}
//					if(parametersInfo[i].ParameterType == typeof(System.Double)){	writer.Write((double)parameters[i]);	}
//					if(parametersInfo[i].ParameterType == typeof(System.Single)){	writer.Write((float)parameters[i]);		}
//					if(parametersInfo[i].ParameterType == typeof(System.Int32))	{	writer.Write((int)parameters[i]);		}
//					if(parametersInfo[i].ParameterType == typeof(System.String)){	writer.Write((string)parameters[i]);	}
//					if(parametersInfo[i].ParameterType == typeof(Vector3)){
//						Vector3 vector3 = (Vector3) parameters[i];
//						writer.Write((float)vector3.x);
//						writer.Write((float)vector3.y);
//						writer.Write((float)vector3.z);
//					}
//				}
//			}
//			buffer = stream.ToArray();
//		}
//
//		outgoingMessage.buffer = buffer;
//		NetworkManager.SendNetworkMessage(outgoingMessage, false);
//	}
//
//	public override void OnConnect (){
//		ComponentMessage outgoingMessage = new ComponentMessage(componentId,-1, myNetId, ChannelType.ReliableSequenced, null);
//
//		NetworkInstance newInstance = GameObject.Instantiate(prefabDatabase.networkPrefabs[0]).GetComponent<NetworkInstance>();
//		UniqueInstanceIdentifier identifier = new UniqueInstanceIdentifier(myNetId,newInstance.instanceId,0);
//		localInstances.Add(identifier,newInstance);
//		newInstance.prefabId = 0;
//		newInstance.gameObject.name += myNetId+":"+newInstance.instanceId+"-"+newInstance.prefabId;
//		newInstance.SendMessage("SetIsLocal", true);
//
//		foreach(NetworkComponent component in newInstance.gameObject.GetComponents<NetworkComponent>()){
//			List<FieldInfo> syncVars = syncPropertiesDictionary[component.GetType()];
//			localSyncInstancesDictionary.Add(component, new List<NetworkSyncVariable>());
//			Debug.Log("Found "+component.name);
//			for(int i=0; i<syncVars.Count; i++){
//				object[] attributes = syncVars[i].GetCustomAttributes(typeof(SyncVar),false);
//				localSyncInstancesDictionary[component].Add(new NetworkSyncVariable(syncVars[i].FieldType,syncVars[i].GetValue(component),syncVars[i],((SyncVar)attributes[0]).refreshTime));
//				Debug.Log("Tracking "+syncVars[i].Name);
//			}
//			localSyncInstancesDictionary[component].OrderBy(NetworkSyncVariable => NetworkSyncVariable.type.Name);
//		}
//
//		byte[] buffer;
//		using(MemoryStream stream = new MemoryStream()){
//			using(BinaryWriter writer = new BinaryWriter(stream)){
//				writer.Write(((int)MessageType.InstanceCreated));
//				writer.Write(myNetId);
//				writer.Write(newInstance.instanceId);
//				writer.Write(newInstance.prefabId);
//			}
//			buffer = stream.ToArray();
//		}
//		outgoingMessage.buffer = buffer;
//
//		NetworkManager.SendNetworkMessage(outgoingMessage, false);
//	}
//
//	public override void OnDisconnect (){}
//
//	public override void OnRemoteConnection (int connectedNetId){
//		if(connectedNetId == myNetId){	return;	}
//		Debug.Log("Sending local instances to Net ID: "+connectedNetId);
//		foreach(KeyValuePair<UniqueInstanceIdentifier, NetworkInstance> pair in localInstances){
//			ComponentMessage outgoingMessage = new ComponentMessage(componentId, connectedNetId, myNetId, ChannelType.ReliableSequenced, null);
//
//			byte[] buffer;
//			using(MemoryStream stream = new MemoryStream()){
//				using(BinaryWriter writer = new BinaryWriter(stream)){
//					writer.Write(((int)MessageType.InstanceCreated));
//					writer.Write(myNetId);
//					writer.Write(pair.Value.instanceId);
//					writer.Write(pair.Value.prefabId);
//				}
//				buffer = stream.ToArray();
//			}
//			outgoingMessage.buffer = buffer;
//
//			NetworkManager.SendNetworkMessage(outgoingMessage, false);
//		}
//	}
//
//	public override void OnRemoteDisconnection(int disconnectedNetId){
//		List<UniqueInstanceIdentifier> toRemove = new List<UniqueInstanceIdentifier>();
//		foreach(KeyValuePair<UniqueInstanceIdentifier,NetworkInstance> pair in remoteInstances){
//			if(pair.Key.netId == disconnectedNetId){
//				toRemove.Add(pair.Key);
//			}
//		}
//		for(int i=toRemove.Count; i>0; i++){
//			GameObject.Destroy(remoteInstances[toRemove[i]].gameObject);
//			remoteInstances.Remove(toRemove[i-1]);
//		}
//		toRemove = null;
//		/*List<UniqueInstanceIdentifier> toRemove = new List<UniqueInstanceIdentifier>();
//		List<GameObject> toDestroy = new List<GameObject>();
//		foreach(KeyValuePair<UniqueInstanceIdentifier,NetworkInstance> pair in remoteInstances){
//			if(pair.Key.netId == disconnectedNetId){
//				toRemove.Add(pair.Key);
//				toDestroy.Add(pair.Value.gameObject);
//			}
//		}
//		foreach(UniqueInstanceIdentifier instance in toRemove){
//			List<NetworkComponent> toRemoveComponents = new List<NetworkComponent>();
//			foreach(KeyValuePair<NetworkComponent,List<NetworkSyncVariable>> pair in remoteSyncInstancesDictionary){
//				if(pair.Key.GetUniqueId() == instance){
//					toRemoveComponents.Add(pair.Key);
//					pair.Value = null;
//				}
//			}
//			foreach(NetworkComponent component in toRemoveComponents){
//				remoteSyncInstancesDictionary.Remove(component);
//			}
//			remoteInstances.Remove(instance);
//		}
//		foreach(GameObject go in toDestroy){
//			toDestroy.Remove(go);
//			GameObject.Destroy(go);
//		}*/
//	}
//
//
//
//	private int TypeToInt(System.Type primitiveType){
//		if(primitiveType == typeof(System.Byte))	{	return 0;	}
//		if(primitiveType == typeof(System.Char))	{	return 1;	}
//		if(primitiveType == typeof(System.Double))	{	return 2;	}
//		if(primitiveType == typeof(System.Single))	{	return 3;	}
//		if(primitiveType == typeof(System.Int32))	{	return 4;	}
//		if(primitiveType == typeof(System.String))	{	return 5;	}
//		if(primitiveType == typeof(Vector3))		{	return 6;	}
//		Debug.LogError("Type not supported for network sync - "+primitiveType.ToString());
//		return -1;
//	}
//
//	private System.Type IntToType(int primitiveInt){
//		System.Type primitiveType = null;
//		switch(primitiveInt){
//		case 0:	primitiveType = typeof(System.Byte);	break;
//		case 1:	primitiveType = typeof(System.Char);	break;
//		case 2:	primitiveType = typeof(System.Double);	break;
//		case 3:	primitiveType = typeof(System.Single);	break;
//		case 4:	primitiveType = typeof(System.Int32);	break;
//		case 5:	primitiveType = typeof(System.String);	break;
//		default:
//			Debug.LogError("Unknown type received - "+primitiveInt);
//			break;
//		}
//		return primitiveType;
//	}
//
//	private class NetworkSyncVariable{
//		public System.Type type;
//		public System.Object variable;
//		public FieldInfo fieldInfo;
//
//		public float refreshTime;
//		public float refreshCounter;
//
//		public NetworkSyncVariable(System.Type type, System.Object variable, FieldInfo fieldInfo, float refreshTime){
//			this.type = type;
//			this.variable = variable;
//			this.fieldInfo = fieldInfo;
//			this.refreshTime = refreshTime;
//			this.refreshCounter = 0;
//		}
//	}
//
//
//}

public static class NetworkInstanceManagerExtensions{
	public static UniqueInstanceIdentifier GetUniqueId(this NetworkComponent component){
		NetworkInstance instance = component.GetComponent<NetworkInstance>();
		return new UniqueInstanceIdentifier(instance.netId,instance.instanceId,instance.prefabId);
	}

//	public static void RemoteProcedureCall(this NetworkComponent component, int instanceId, int netId, string methodName, params object[] parameters){
//		NetworkInstanceManager.Instance.RemoteProcedureCall(component, instanceId, netId, methodName, parameters);
//	}
}

namespace Game.Network{
	[System.AttributeUsage(System.AttributeTargets.Field)]
	public class SyncVar : System.Attribute {
		public float refreshTime;
		public SyncVar(float refreshTime){
			this.refreshTime = refreshTime;
		}
	}
	[System.AttributeUsage(System.AttributeTargets.Method)]
	public class RPC : System.Attribute {
	}

	[System.Serializable]
	public struct UniqueInstanceIdentifier{
		public int netId;
		public int instanceId;
		public int prefabId;

		public UniqueInstanceIdentifier(int netId, int instanceId, int prefabId){
			this.netId = netId;
			this.instanceId = instanceId;
			this.prefabId = prefabId;
		}

		public static bool operator ==(UniqueInstanceIdentifier f1, UniqueInstanceIdentifier f2){
			return (f1.netId == f2.netId && f1.instanceId == f2.instanceId);
		}

		public static bool operator !=(UniqueInstanceIdentifier f1, UniqueInstanceIdentifier f2){
			return (f1.netId != f2.netId || f1.instanceId != f2.instanceId);
		}

		public override string ToString (){
			return netId + ":" + instanceId;
		}
	}
}
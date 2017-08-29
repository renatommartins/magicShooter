using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Game.Network;

public class PlayerNetworkManager : NetworkSingleton<PlayerNetworkManager> {
	public new readonly int componentId = -225354773;

	private enum MessageType {	InstanceCreated, InstanceDestroyed, InstanceSync, RemotePlayerAction, PlayerHit	};
	public enum PlayerCommand	{	FireLeft, FireLeftShoulder, FireRight, FireRightShoulder, BackpackAction, ToggleDash	};

	public NetworkPrefabDatabase prefabDatabase;

	public UniqueInstanceIdentifier localIdentifier;
	public Character localPlayer;
	public Dictionary<UniqueInstanceIdentifier, Character> remotePlayers = new Dictionary<UniqueInstanceIdentifier, Character>();

	// Use this for initialization
	void Start () {
		NetworkManager.StartListening(componentId,this);
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if(NetworkManager.networkReady){
			ComponentMessage updateMessage = new ComponentMessage(componentId,-1,myNetId,ChannelType.StateUpdate,null);

			using(MemoryStream stream = new MemoryStream()){
				using(BinaryWriter writer = new BinaryWriter(stream)){
					writer.Write((int) MessageType.InstanceSync);
					writer.Write(myNetId);
					writer.Write(localIdentifier.instanceId);
					writer.Write(localIdentifier.prefabId);
					//Syncs transforms
					writer.Write(localPlayer.transform.position.x);
					writer.Write(localPlayer.transform.position.y);
					writer.Write(localPlayer.transform.position.z);

					writer.Write(localPlayer.transform.eulerAngles.y);
				}
				updateMessage.buffer = stream.ToArray();
			}
			#if (DEBUG_ALL || DEBUG_SYNC)
			Debug.Log("Sending Sync|"+localPlayer.transform.position.ToString());
			#endif
			NetworkManager.SendNetworkMessage(updateMessage);
			while(netMessages.Count > 0){
				ReceiveMessage(netMessages.Dequeue());
			}
		}
	}

	private void ReceiveMessage (ComponentMessage message){
		MessageType messageType;

		using(MemoryStream stream = new MemoryStream(message.buffer)){
			using(BinaryReader reader = new BinaryReader(stream)){
				messageType = (MessageType)reader.ReadInt32();
				int netId = reader.ReadInt32();
				int instanceId = reader.ReadInt32();

				switch(messageType){
				case MessageType.InstanceCreated:
					#if (DEBUG_ALL || DEBUG_SYNC)
					Debug.Log("Net ID: "+netId+" requested instantiation of IID: "+instanceId);
					#endif
					if(netId != myNetId){
						int createdPrefabId = reader.ReadInt32();
						UniqueInstanceIdentifier uniqueId = new UniqueInstanceIdentifier(netId,instanceId,createdPrefabId);
						NetworkInstance newInstance = GameObject.Instantiate(prefabDatabase.networkPrefabs[createdPrefabId].GetComponent<NetworkInstance>());

						Character newInstanceCharacter = newInstance.GetComponent<Character>();
						remotePlayers.Add(uniqueId, newInstanceCharacter);
						newInstance.gameObject.name += netId+":"+newInstance.instanceId+"-"+newInstance.prefabId;
						newInstance.SendMessage("SetIsLocal", false);
						remotePlayers[uniqueId].ownerNetId = netId;
						remotePlayers[uniqueId].instanceId = instanceId;
						remotePlayers[uniqueId].prefabId = createdPrefabId;

						int headID = reader.ReadInt32();
						newInstanceCharacter.headPart = GameSystem.partsDatabase.headParts[headID];
						int torsoID = reader.ReadInt32();
						newInstanceCharacter.torsoPart = GameSystem.partsDatabase.torsoParts[torsoID];
						int lArmID = reader.ReadInt32();
						newInstanceCharacter.lArmPart = GameSystem.partsDatabase.leftArmParts[lArmID];
						int rArmID = reader.ReadInt32();
						newInstanceCharacter.rArmPart = GameSystem.partsDatabase.rightArmParts[rArmID];
						int legsID = reader.ReadInt32();
						newInstanceCharacter.legsPart = GameSystem.partsDatabase.legsParts[legsID];

						int leftHandID = reader.ReadInt32();
						if(leftHandID != 0){	newInstanceCharacter.leftHand = GameSystem.partsDatabase.equipments[leftHandID-1];	}
						int rightHandID = reader.ReadInt32();
						if(rightHandID != 0){	newInstanceCharacter.rightHand = GameSystem.partsDatabase.equipments[rightHandID-1];	}
						int leftShoulderID = reader.ReadInt32();
						if(leftShoulderID != 0){	newInstanceCharacter.leftShoulder = GameSystem.partsDatabase.equipments[leftShoulderID-1];	}
						int rightShoulderID = reader.ReadInt32();
						if(rightShoulderID != 0){	newInstanceCharacter.rightShoulder = GameSystem.partsDatabase.equipments[rightShoulderID-1];	}
						int accessoryID = reader.ReadInt32();
						if(accessoryID != 0){	newInstanceCharacter.backpack = GameSystem.partsDatabase.equipments[accessoryID-1];	}

						newInstanceCharacter.Load();

						#if (DEBUG_ALL || DEBUG_SYNC)
						Debug.Log("Created Player Instance: NID: "+netId+"|IID: "+instanceId);
						#endif
					}
					break;

				case MessageType.InstanceSync:
					int syncPrefabId = reader.ReadInt32();

					Vector3 syncVector = new Vector3();
					UniqueInstanceIdentifier syncUniqueId = new UniqueInstanceIdentifier(netId,instanceId,syncPrefabId);
					if(!remotePlayers.ContainsKey(syncUniqueId)){	return;	}
					//Syncs transforms
					syncVector.x = reader.ReadSingle();
					syncVector.y = reader.ReadSingle();
					syncVector.z = reader.ReadSingle();

					remotePlayers[syncUniqueId].localRigidbody.MovePosition(syncVector);
					//remotePlayers[syncUniqueId].transform.position = syncVector;
					#if (DEBUG_ALL || DEBUG_SYNC)
					Debug.Log("Synced "+netId+":"+instanceId+"|"+syncVector.ToString());
					#endif
					Quaternion syncQuaternion = Quaternion.Euler(new Vector3(0, reader.ReadSingle(), 0));
					//remotePlayers[syncUniqueId].transform.eulerAngles = new Vector3(0, reader.ReadSingle(), 0);
					remotePlayers[syncUniqueId].localRigidbody.MoveRotation(syncQuaternion);
					break;

				case MessageType.RemotePlayerAction:
					int rpcPrefabId = reader.ReadInt32();
					PlayerCommand syncCommand = (PlayerCommand) reader.ReadInt32();

					List<byte> parameters = new List<byte>();
					while(reader.BaseStream.Position != reader.BaseStream.Length){
						parameters.Add(reader.ReadByte());
					}
					UniqueInstanceIdentifier rpcUniqueIdentifier = new UniqueInstanceIdentifier(netId,instanceId,rpcPrefabId);
					RemoteAction(remotePlayers[rpcUniqueIdentifier],syncCommand,parameters.ToArray());
					break;

				case MessageType.PlayerHit:
					int hitPrefabId = reader.ReadInt32();
					UniqueInstanceIdentifier hitUniqueId = new UniqueInstanceIdentifier(netId,instanceId,hitPrefabId);

					int damage = reader.ReadInt32();
					BotPartEnum partHit = (BotPartEnum) reader.ReadInt32();
					DamageType damageType = DamageType.FindTypeByHash(reader.ReadInt32());
					if(localIdentifier == hitUniqueId){	localPlayer.TakeDamage(damage, partHit, damageType);	}
					else{	remotePlayers[hitUniqueId].TakeDamage(damage, partHit, damageType);	}
					break;
				default:
					Debug.LogWarning("Dropping message.\nUndefined message type:"+(int)messageType+" received");
					break;	
				}
			}
		}
	}

	public static void SendPlayerAction(PlayerCommand action, byte[] parametersBuffer){
		ComponentMessage message = new ComponentMessage(PlayerNetworkManager.Instance.componentId,-1,NetworkManager.netId,ChannelType.ReliableSequenced,null);
		using(MemoryStream stream = new MemoryStream()){
			using(BinaryWriter writer = new BinaryWriter(stream)){
				writer.Write((int) MessageType.RemotePlayerAction);
				writer.Write(PlayerNetworkManager.Instance.myNetId);
				writer.Write(PlayerNetworkManager.Instance.localIdentifier.instanceId);
				writer.Write(PlayerNetworkManager.Instance.localIdentifier.prefabId);

				writer.Write((int) action);
				writer.Write(parametersBuffer);
			}
			message.buffer = stream.ToArray();
		}
		NetworkManager.SendNetworkMessage(message);
	}

	public static void SendPlayerHit(UniqueInstanceIdentifier uniqueId, int damage, BotPartEnum partHit, DamageType damageType){
		ComponentMessage message = new ComponentMessage(PlayerNetworkManager.Instance.componentId,-1,NetworkManager.netId,ChannelType.ReliableSequenced,null);
		using(MemoryStream stream = new MemoryStream()){
			using(BinaryWriter writer = new BinaryWriter(stream)){
				writer.Write((int) MessageType.PlayerHit);
				writer.Write(uniqueId.netId);
				writer.Write(uniqueId.instanceId);
				writer.Write(uniqueId.prefabId);

				writer.Write(damage);
				writer.Write((int) partHit);
				writer.Write(damageType.name.GetStableHash());
			}
			message.buffer = stream.ToArray();
		}

		NetworkManager.SendNetworkMessage(message);
	}

	private void RemoteAction(Character character, PlayerCommand action, byte[] parameters){
		switch(action){
		case PlayerNetworkManager.PlayerCommand.FireLeft:
			character.leftHand.RemoteAction(character, character.leftHandState, 0, parameters);
			break;
		case PlayerNetworkManager.PlayerCommand.FireRight:
			character.rightHand.RemoteAction(character, character.rightHandState, 0, parameters);
			break;
		}
	}

	public override void OnConnect (){
		ComponentMessage outgoingMessage = new ComponentMessage(componentId, -1, myNetId, ChannelType.ReliableSequenced, null);

		//Instantiates the local player and assigns it to a unique InstanceID
		NetworkInstance newInstance = GameObject.Instantiate(prefabDatabase.networkPrefabs[0].GetComponent<NetworkInstance>());
		//Ties the instance to the local NetID
		UniqueInstanceIdentifier uniqueId = new UniqueInstanceIdentifier(myNetId,newInstance.instanceId,newInstance.prefabId);

		//Renames the Instance to include its UniqueID on the inspector
		newInstance.gameObject.name += myNetId+":"+newInstance.instanceId+"-"+newInstance.prefabId;
		//Sets the Instance as local
		newInstance.SendMessage("SetIsLocal", true);

		localPlayer = newInstance.GetComponent<Character>();
		localPlayer.ownerNetId = myNetId;
		localPlayer.instanceId = newInstance.netId;
		localPlayer.prefabId = newInstance.prefabId;
		localIdentifier = uniqueId;

		localPlayer.headPart = GameSystem.partsDatabase.headParts[PartSelectorUI.Instance.headSelection.value];
		localPlayer.torsoPart = GameSystem.partsDatabase.torsoParts[PartSelectorUI.Instance.torsoSelection.value];
		localPlayer.lArmPart = GameSystem.partsDatabase.leftArmParts[PartSelectorUI.Instance.leftArmSelection.value];
		localPlayer.rArmPart = GameSystem.partsDatabase.rightArmParts[PartSelectorUI.Instance.rightArmSelection.value];
		localPlayer.legsPart = GameSystem.partsDatabase.legsParts[PartSelectorUI.Instance.legsSelection.value];

		localPlayer.leftHand = GameSystem.partsDatabase.equipments[PartSelectorUI.Instance.leftHandSelection.value];
		localPlayer.rightHand = GameSystem.partsDatabase.equipments[PartSelectorUI.Instance.rightHandSelection.value];

		localPlayer.Load();

		byte[] buffer;

		//Writes the instance creation message for broadcasting
		using(MemoryStream stream = new MemoryStream()){
			using(BinaryWriter writer = new BinaryWriter(stream)){
				writer.Write((int)MessageType.InstanceCreated);
				writer.Write(myNetId);
				writer.Write(newInstance.instanceId);
				writer.Write(newInstance.prefabId);

				writer.Write(PartsDatabase.GetObjectID(localPlayer.headPart));
				writer.Write(PartsDatabase.GetObjectID(localPlayer.torsoPart));
				writer.Write(PartsDatabase.GetObjectID(localPlayer.lArmPart));
				writer.Write(PartsDatabase.GetObjectID(localPlayer.rArmPart));
				writer.Write(PartsDatabase.GetObjectID(localPlayer.legsPart));

				writer.Write(PartsDatabase.GetObjectID(localPlayer.leftHand)+1);
				writer.Write(PartsDatabase.GetObjectID(localPlayer.rightHand)+1);
				writer.Write(PartsDatabase.GetObjectID(localPlayer.leftShoulder)+1);
				writer.Write(PartsDatabase.GetObjectID(localPlayer.rightShoulder)+1);
				writer.Write(PartsDatabase.GetObjectID(localPlayer.backpack)+1);
			}
			buffer = stream.ToArray();
		}

		outgoingMessage.buffer = buffer;

		#if (DEBUG_ALL || DEBUG_SYNC)
		Debug.Log("Requested Instantiation on all clients");
		#endif
		//Queues the message
		NetworkManager.SendNetworkMessage(outgoingMessage);
		GameSystem.Instance.switchCanvas();
	}

	public override void OnDisconnect (){
		
	}

	public override void OnRemoteConnection (int connectedNetId){
		if(connectedNetId == myNetId){	return;	}
		#if (DEBUG_ALL || DEBUG_SYNC)
		Debug.Log("Sending local instances to Net ID: "+connectedNetId);
		#endif
		ComponentMessage outgoingMessage = new ComponentMessage(componentId, connectedNetId, myNetId, ChannelType.ReliableSequenced, null);

		byte[] buffer;
		using(MemoryStream stream = new MemoryStream()){
			using(BinaryWriter writer = new BinaryWriter(stream)){
				writer.Write(((int)MessageType.InstanceCreated));
				writer.Write(myNetId);
				writer.Write(localIdentifier.instanceId);
				writer.Write(localIdentifier.prefabId);

				writer.Write(PartsDatabase.GetObjectID(localPlayer.headPart));
				writer.Write(PartsDatabase.GetObjectID(localPlayer.torsoPart));
				writer.Write(PartsDatabase.GetObjectID(localPlayer.lArmPart));
				writer.Write(PartsDatabase.GetObjectID(localPlayer.rArmPart));
				writer.Write(PartsDatabase.GetObjectID(localPlayer.legsPart));

				writer.Write(PartsDatabase.GetObjectID(localPlayer.leftHand)+1);
				writer.Write(PartsDatabase.GetObjectID(localPlayer.rightHand)+1);
				writer.Write(PartsDatabase.GetObjectID(localPlayer.leftShoulder)+1);
				writer.Write(PartsDatabase.GetObjectID(localPlayer.rightShoulder)+1);
				writer.Write(PartsDatabase.GetObjectID(localPlayer.backpack)+1);
			}
			buffer = stream.ToArray();
		}
		outgoingMessage.buffer = buffer;

		NetworkManager.SendNetworkMessage(outgoingMessage);

	}

	public override void OnRemoteDisconnection (int disconnectedNetId){
		List<UniqueInstanceIdentifier> toRemoveInstances = new List<UniqueInstanceIdentifier>();
		foreach(KeyValuePair<UniqueInstanceIdentifier,Character> pair in remotePlayers){
			if(pair.Key.netId == disconnectedNetId){	toRemoveInstances.Add(pair.Key);	}
		}
		for(int i=0; i<toRemoveInstances.Count; i++){
			Character instance = remotePlayers[toRemoveInstances[i]];
			remotePlayers.Remove(toRemoveInstances[i]);
			GameObject.Destroy(instance.gameObject);
		}
	}
}

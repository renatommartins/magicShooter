using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

namespace Game.Network{
	public class NetworkManager : NetworkSingleton<NetworkManager>{

		private enum NetworkTaskType{	AssignNetID, SendConnectedClients, RelayConnectEvent, RelayDisconnectEvent	}

		private struct NetworkTask{
			public NetworkTaskType taskType;
			public int destinationNetId;
			public int netId;
			public NetworkTask(NetworkTaskType taskType, int destinationNetId, int netId){
				this.taskType = taskType;
				this.destinationNetId = destinationNetId;
				this.netId = netId;
			}
		}

		public new readonly int componentId = -357347597;

		private Queue<NetworkTask> tasks = new Queue<NetworkTask>();

		public InputField address;
		public Text statusText;
		public Button serverStart;
		public Button connectStart;

		[SerializeField]
		GlobalConfig netWorkConfiguraion = new GlobalConfig();

		[SerializeField]
		ConnectionConfig connectionConfiguration = new ConnectionConfig();

		[SerializeField]
		private int maxConnections;

		[SerializeField]
		private int maxMessagesPerUpdate;

		public int socketId;
		private int connectionId;

		private bool isInitialized = false;
		public bool _isServer = false;
		private bool receivedNetIds = false;

		private static bool _networkReady = false;
		public static bool networkReady{	get{	return _networkReady;	}	}

		public static int netId;
		public Dictionary<int,int> convertNIDToCID = new Dictionary<int, int>();
		public Dictionary<int,int> convertCIDToNID = new Dictionary<int, int>();

		public Dictionary<int,int> convertNIDToNIDRemote = new Dictionary<int, int>();
		public int serverConnectionId;

		private HashSet<int> assignedNetId = new HashSet<int>();

		private Dictionary<int, NetworkComponent> listeningReceivers = new Dictionary<int, NetworkComponent>();

		public delegate void localNetworkDelegate();
		public event localNetworkDelegate OnConnectEvent;
		public event localNetworkDelegate OnDisconnectEvent;

		public delegate void RemoteNetworkDelegate(int netId);
		public event RemoteNetworkDelegate OnRemoteConnectEvent;
		public event RemoteNetworkDelegate OnRemoteDisconnectEvent;


		public static void StartListening(int componentId,NetworkComponent newListener){
			Instance.listeningReceivers.Add(componentId,newListener);
			Instance.OnConnectEvent += newListener.OnConnect;
			Instance.OnDisconnectEvent += newListener.OnDisconnect;
			Instance.OnRemoteConnectEvent += newListener.OnRemoteConnection;
			Instance.OnRemoteDisconnectEvent += newListener.OnRemoteDisconnection;
		}
		public static void StopListening(int componentId){
			NetworkComponent listener = Instance.listeningReceivers[componentId];
			Instance.listeningReceivers.Remove(componentId);
			Instance.OnConnectEvent -= listener.OnConnect;
			Instance.OnDisconnectEvent -= listener.OnDisconnect;
			Instance.OnRemoteConnectEvent -= listener.OnRemoteConnection;
			Instance.OnRemoteDisconnectEvent -= listener.OnRemoteDisconnection;
		}

		public override void OnConnect(){}
		public override void OnDisconnect (){}
		public override void OnRemoteConnection (int connectedNetId){}
		public override void OnRemoteDisconnection (int disconnectedNetId){}

		void StartServer(){
			if(isInitialized){		Debug.LogError("Network Already Started");	return;	}
			isInitialized = true;
			receivedNetIds = true;
			_isServer = true;

			NetworkTransport.Init(netWorkConfiguraion);

			string[] splitAddress = address.text.Split(':');
			int port = Convert.ToInt32(splitAddress[1]);

			HostTopology topology = new HostTopology(connectionConfiguration,maxConnections);
			int newSocket = NetworkTransport.AddHost(topology,port);
			_isServer = true;
			serverConnectionId = -1;

			netId = GetNextFreeNetId();
			socketId = newSocket;

			if(OnConnectEvent.GetInvocationList().Length > 0){	OnConnectEvent();	}
			_networkReady = true;
			#if (DEBUG_ALL || DEBUG_NETWORK)
			Debug.Log("Started Server. NetID: "+netId+" SocketID: "+newSocket);
			#endif
		}

		void ConnectToServer(){
			if(isInitialized){		Debug.LogError("Network Already Started");	return;	}

			_isServer = false;

			NetworkTransport.Init(netWorkConfiguraion);

			HostTopology topology = new HostTopology(connectionConfiguration,maxConnections);

			int newSocket = NetworkTransport.AddHost(topology,0);
			socketId = newSocket;

			string[] splitAddress = address.text.Split(':');
			string ipAdress = splitAddress[0];
			int port = Convert.ToInt32(splitAddress[1]);
			#if (DEBUG_ALL || DEBUG_NETWORK)
			Debug.Log("Connecting to "+ ipAdress+":"+port);
			#endif
			byte error;
			int exceptionConnectionId = 0;
			connectionId = NetworkTransport.Connect(socketId, ipAdress, port, exceptionConnectionId, out error);
			_isServer = false;
			serverConnectionId = connectionId;

			if(connectionId != exceptionConnectionId){	isInitialized = true;	}
			else{	Debug.LogError("Failed to connect");	}
		}

		void Awake(){
			//GameObject.DontDestroyOnLoad(this);

			connectionConfiguration.Channels.Clear();
			connectionConfiguration.Channels.Add(new ChannelQOS(QosType.ReliableSequenced));
			connectionConfiguration.Channels.Add(new ChannelQOS(QosType.UnreliableSequenced));
			connectionConfiguration.Channels.Add(new ChannelQOS(QosType.ReliableStateUpdate));
			connectionConfiguration.Channels.Add(new ChannelQOS(QosType.StateUpdate));

			serverStart.onClick.AddListener(StartServer);
			connectStart.onClick.AddListener(ConnectToServer);

			StartListening(componentId,this);
		}

		void FixedUpdate(){
			int port = new int();
			ulong network = new ulong();
			ushort dstNode = new ushort();
			byte error = new byte();

			if(isInitialized){
				NetworkTransport.GetConnectionInfo(socketId, connectionId, out port, out network, out dstNode, out error);
			}
			statusText.text = "Socket ID - " + socketId + "|Connection ID - " + connectionId + "|Port - " + port
				+ "|Network - " + network + "|DST Node - " + dstNode + "|Error - " + error;



			if(isInitialized){
				NetworkEventType networkEventType;
				int messageSocketId; 
				int messageConnectionId; 
				int messageChannelId; 
				byte[] recBuffer = new byte[1024]; 
				int bufferSize = 1024;
				int dataSize;
				byte msgError;


				
				for(int i=0; i<maxMessagesPerUpdate; i++){
					networkEventType = NetworkTransport.Receive(out messageSocketId, out messageConnectionId, out messageChannelId
						, recBuffer, bufferSize, out dataSize, out msgError);
					
					if(networkEventType == NetworkEventType.Nothing){	break;	}
					else{
						switch(networkEventType){
						case NetworkEventType.Nothing:
							break;

						case NetworkEventType.ConnectEvent:
							if(messageConnectionId == connectionId){
								#if (DEBUG_ALL || DEBUG_NETWORK)
								Debug.Log(String.Format("Connect in host {0} to connection {1}", messageSocketId, messageConnectionId));
								#endif
								//convertNIDToCID.Add(0, messageConnectionId);
							}else{
								#if (DEBUG_ALL || DEBUG_NETWORK)
								Debug.Log(String.Format("Connect in host {0} from connection {1}", messageSocketId, messageConnectionId));
								#endif
								int newNetId = GetNextFreeNetId();
								convertNIDToCID.Add(newNetId, messageConnectionId);
								convertCIDToNID.Add(messageConnectionId, newNetId);

								HandleNewConnection(newNetId);
							}

							break;

						case NetworkEventType.DisconnectEvent:
							if(_isServer){
								int disconnectedNetId = convertCIDToNID[messageConnectionId];
								convertNIDToCID.Remove(disconnectedNetId);
								convertCIDToNID.Remove(messageConnectionId);
								assignedNetId.Remove(disconnectedNetId);
								tasks.Enqueue(new NetworkTask(NetworkTaskType.RelayDisconnectEvent,-1,disconnectedNetId));
								#if (DEBUG_ALL || DEBUG_NETWORK)
								Debug.Log("Disconnect in host "+ messageSocketId +" from connection "+messageConnectionId+" (Net ID: "+disconnectedNetId+")");
								#endif
								if(OnRemoteDisconnectEvent.GetInvocationList().Length > 0){	OnRemoteDisconnectEvent(disconnectedNetId);	}
							}else{
								receivedNetIds = false;
								isInitialized = false;
								convertNIDToNIDRemote.Clear();
								#if (DEBUG_ALL || DEBUG_NETWORK)
								Debug.Log("Disconnect in host "+ messageSocketId +" from connection "+messageConnectionId);
								#endif
							}
							break;

						case NetworkEventType.DataEvent:
							ComponentMessage incomingMessage = ReadDataEvent(recBuffer);
							#if (DEBUG_ALL || DEBUG_NETWORK)
							Debug.Log("Received Message to Net ID: "+ incomingMessage.netId);
							#endif

							if(!receivedNetIds){
								listeningReceivers[incomingMessage.componentId].netMessages.Enqueue(incomingMessage);
								break;
							}
							if(incomingMessage.netId == -1){
								if(_isServer){
									//Debug.Log("Received broadcast message");
									listeningReceivers[incomingMessage.componentId].netMessages.Enqueue(incomingMessage);
									SendNetworkMessage(incomingMessage);
								}else{
									listeningReceivers[incomingMessage.componentId].netMessages.Enqueue(incomingMessage);
								}
							}else{
								if(incomingMessage.netId == myNetId){
									listeningReceivers[incomingMessage.componentId].netMessages.Enqueue(incomingMessage);
								}
								else{	SendNetworkMessage(incomingMessage);	}
							}
							break;
						}
					}
				}
				while(netMessages.Count > 0){
					ReceiveMessage(netMessages.Dequeue());
				}

				ExecuteTasks();

			}
		}

		private ComponentMessage ReadDataEvent(byte[] buffer){
			ComponentMessage dataMessage = new ComponentMessage();
			using(MemoryStream stream = new MemoryStream(buffer)){
				using(BinaryReader reader = new BinaryReader(stream)){
					dataMessage.netId = reader.ReadInt32();
					dataMessage.senderNetId = reader.ReadInt32();
					dataMessage.componentId = reader.ReadInt32();
					dataMessage.buffer = new byte[(int)reader.BaseStream.Length-sizeof(int)*3];
					reader.BaseStream.Read(dataMessage.buffer,0,(int)reader.BaseStream.Length-sizeof(int)*3);
				}
			}
			return dataMessage;
		}

		public static void SendNetworkMessage(ComponentMessage outgoingMessage){
			if(!Instance.isInitialized){	Debug.LogWarning("Send Network Message failed.\nNetwork not initialized.");	return;	}
			byte[] sendBuffer;
			using(MemoryStream stream = new MemoryStream()){
				using(BinaryWriter writer = new BinaryWriter(stream)){
					writer.Write(outgoingMessage.netId);
					writer.Write(outgoingMessage.senderNetId);
					writer.Write(outgoingMessage.componentId);
					writer.Write(outgoingMessage.buffer);
				}
				sendBuffer = stream.ToArray();
			}
			byte error;
			if(outgoingMessage.netId == -1){
				if(Instance._isServer){
					foreach(KeyValuePair<int,int> pair in Instance.convertNIDToCID){
						if(outgoingMessage.senderNetId == pair.Key){	continue;	}
						#if (DEBUG_ALL || DEBUG_NETWORK)
						Debug.Log("Sending new messages for broadcast to clients ("+outgoingMessage.netId+")");
						#endif
						if(!NetworkTransport.Send(instance.socketId,pair.Value,(int)outgoingMessage.channelType,sendBuffer,sendBuffer.Length,out error)){
							Debug.LogWarning("Message failed to send! Error: "+error);
						}
					}
				}
				else{
					#if (DEBUG_ALL || DEBUG_NETWORK)
					Debug.Log("Sending new broadcast to server");
					#endif
					if(!NetworkTransport.Send(instance.socketId,instance.serverConnectionId,(int)outgoingMessage.channelType,sendBuffer,sendBuffer.Length,out error)){
						Debug.LogWarning("Message failed to send! Error: "+error);
					}
				}
			}else{
				#if (DEBUG_ALL || DEBUG_NETWORK)
				Debug.Log("Processing message to Net ID: "+outgoingMessage.netId);
				#endif
				int newConnectionId;
				if(Instance.convertNIDToCID.ContainsKey(outgoingMessage.netId)){
					newConnectionId = Instance.convertNIDToCID[outgoingMessage.netId];
					#if (DEBUG_ALL || DEBUG_NETWORK)
					Debug.Log("Direct Connection ID:"+newConnectionId+" to Net ID:"+outgoingMessage.netId);
					#endif
				}else{
					int relayNetId = Instance.convertNIDToNIDRemote[outgoingMessage.netId];
					newConnectionId = Instance.convertNIDToCID[relayNetId];
					#if (DEBUG_ALL || DEBUG_NETWORK)
					Debug.Log("Remote Connection ID:"+newConnectionId+" to Net ID:"+outgoingMessage.netId);
					#endif
				}
				if(!NetworkTransport.Send(instance.socketId,newConnectionId,(int)outgoingMessage.channelType,sendBuffer,sendBuffer.Length,out error)){
					Debug.LogWarning("Message failed to send! Error: "+error);
				}
			}
		}

		void OnApplicationQuit(){
			NetworkTransport.Shutdown();
		}

		private int GetNextFreeNetId(){
			int newNetId;
			do{
				newNetId = UnityEngine.Random.Range(int.MinValue,int.MaxValue);
			}while(newNetId == -1 || assignedNetId.Contains(newNetId));
			assignedNetId.Add(newNetId);
			return newNetId;
		}

		private enum MessageType {	AssingNetID, RemoteConnection, RemoteDisconnection, RemoteClients	};

		private void ReceiveMessage(ComponentMessage message){
			MessageType messageType;
			using(MemoryStream stream = new MemoryStream(message.buffer)){
				using(BinaryReader reader = new BinaryReader(stream)){
					messageType = (MessageType) reader.ReadInt32();
					switch(messageType){
					case MessageType.AssingNetID://0 - message from server reporting NetID
						convertNIDToCID.Add(message.senderNetId,connectionId);
						netId = reader.ReadInt32();
						receivedNetIds = true;
						#if (DEBUG_ALL || DEBUG_NETWORK)
						Debug.Log("Assigned to Net ID: "+ myNetId);
						#endif
						if(OnConnectEvent.GetInvocationList().Length > 0){	OnConnectEvent();	}
						_networkReady = true;
						break;
					case MessageType.RemoteConnection://1 - relayed remote connection
						if(_isServer){	break;	}
						int remoteNetId = reader.ReadInt32();
						int relayNetId = reader.ReadInt32();
						if(remoteNetId != myNetId){
							convertNIDToNIDRemote.Add(remoteNetId,relayNetId);
						}
						#if (DEBUG_ALL || DEBUG_NETWORK)
						Debug.Log("Client with Net ID: "+remoteNetId+" connected to Server of Net ID:"+relayNetId);
						foreach(KeyValuePair<int,int> pair in convertNIDToNIDRemote){
							Debug.Log(pair.Key+":"+pair.Value);
						}
						#endif
						if(OnRemoteConnectEvent.GetInvocationList().Length > 0){	OnRemoteConnectEvent(remoteNetId);	}
						break;
					case MessageType.RemoteDisconnection://2 - relayed remote disconnection
						if(_isServer){	break;	}
						int remoteDisconnectNetId = reader.ReadInt32();
						int relayDisconnectNetId = reader.ReadInt32();
						convertNIDToNIDRemote.Remove(remoteDisconnectNetId);
						#if (DEBUG_ALL || DEBUG_NETWORK)
						Debug.Log("Client with Net ID: "+remoteDisconnectNetId+" disconnected from Server of Net ID:"+relayDisconnectNetId);
						#endif
						if(OnRemoteDisconnectEvent.GetInvocationList().Length > 0){	OnRemoteDisconnectEvent(remoteDisconnectNetId);	}
						break;
					case MessageType.RemoteClients://3 - remote clients list
						if(_isServer){	break;	}
						int clientRelayNetId = reader.ReadInt32();
						int clientCount = reader.ReadInt32();
						#if (DEBUG_ALL || DEBUG_NETWORK)
						Debug.Log("Received remote clients list");
						#endif
						for(int i=0; i<clientCount; i++){
							int remoteClientNetId = reader.ReadInt32();
							if(remoteClientNetId != myNetId){
								convertNIDToNIDRemote.Add(remoteClientNetId,clientRelayNetId);
								#if (DEBUG_ALL || DEBUG_NETWORK)
								Debug.Log("Remote client with Net ID: "+remoteClientNetId+" on Server of Net ID: "+clientRelayNetId+" registered");
								#endif
							}
						}
						break;
					}
				}
			}
		}

		private void HandleNewConnection(int newNetID){
			ComponentMessage message = new ComponentMessage(componentId,newNetID,myNetId,ChannelType.ReliableSequenced,null);

			using(MemoryStream stream = new MemoryStream()){
				using(BinaryWriter writer = new BinaryWriter(stream)){
					writer.Write((int)MessageType.AssingNetID);
					writer.Write(newNetID);
				}
				message.buffer = stream.ToArray();
			}
			#if (DEBUG_ALL || DEBUG_NETWORK)
			Debug.Log("Assigned new client the Net ID: "+ newNetID);
			#endif
			SendNetworkMessage(message);

			using(MemoryStream stream = new MemoryStream()){
				using(BinaryWriter writer = new BinaryWriter(stream)){
					writer.Write((int)MessageType.RemoteClients);
					writer.Write(myNetId);
					writer.Write(convertNIDToCID.Count);
					foreach(KeyValuePair<int,int> pair in convertNIDToCID){
						writer.Write(pair.Key);
					}
				}
				message.buffer = stream.ToArray();
			}
			#if (DEBUG_ALL || DEBUG_NETWORK)
			Debug.Log("Relaying all connected clients to Net ID: "+newNetID);
			#endif
			SendNetworkMessage(message);

			message.netId = -1;
			using(MemoryStream stream = new MemoryStream()){
				using(BinaryWriter writer = new BinaryWriter(stream)){
					writer.Write((int)MessageType.RemoteConnection);
					writer.Write(newNetID);
					writer.Write(myNetId);
				}
				message.buffer = stream.ToArray();
			}
			#if (DEBUG_ALL || DEBUG_NETWORK)
			Debug.Log("Relaying new connection of Net ID: "+newNetID+" to Net ID: "+(-1));
			#endif
			SendNetworkMessage(message);
			if(OnRemoteConnectEvent.GetInvocationList().Length > 0){	OnRemoteConnectEvent(newNetID);	}
		}

		private void ExecuteTasks(){
			while(tasks.Count > 0){
				NetworkTask task = tasks.Dequeue();
				int destinationNetId = 0;
				byte[] buffer;
				using(MemoryStream stream = new MemoryStream()){
					using(BinaryWriter writer = new BinaryWriter(stream)){
						switch(task.taskType){
						case NetworkTaskType.AssignNetID:
							writer.Write(0);
							writer.Write(task.netId);
							destinationNetId = task.netId;
							#if (DEBUG_ALL || DEBUG_NETWORK)
							Debug.Log("Assigned new client the Net ID: "+ task.netId);
							#endif
							break;
						case NetworkTaskType.RelayConnectEvent:
							writer.Write(1);
							writer.Write(task.netId);
							writer.Write(myNetId);
							destinationNetId = task.destinationNetId;
							#if (DEBUG_ALL || DEBUG_NETWORK)
							Debug.Log("Relaying new connection of Net ID: "+task.netId+" to Net ID: "+task.destinationNetId);
							#endif
							break;
						case NetworkTaskType.RelayDisconnectEvent:
							writer.Write(2);
							writer.Write(task.netId);
							writer.Write(myNetId);
							destinationNetId = task.destinationNetId;
							#if (DEBUG_ALL || DEBUG_NETWORK)
							Debug.Log("Relaying disconnection of Net ID: "+task.netId+" to Net ID: "+task.destinationNetId);
							#endif
							break;
						case NetworkTaskType.SendConnectedClients:
							writer.Write(3);
							writer.Write(myNetId);
							writer.Write(convertNIDToCID.Count);
							foreach(KeyValuePair<int,int> pair in convertNIDToCID){
								writer.Write(pair.Key);
							}
							destinationNetId = task.destinationNetId;
							#if (DEBUG_ALL || DEBUG_NETWORK)
							Debug.Log("Relaying all connected clients to Net ID: "+task.destinationNetId);
							#endif
							break;
						}
						buffer = stream.ToArray();
						SendNetworkMessage(new ComponentMessage(componentId,destinationNetId,myNetId,ChannelType.ReliableSequenced,buffer));
					}
				}
			}
		}
	}

	public enum ChannelType {	ReliableSequenced, UnreliableSequenced, ReliableStateUpdate, StateUpdate	}

	public struct ComponentMessage{
		public int componentId;
		public byte[] buffer;
		public int netId;
		public int senderNetId;
		public ChannelType channelType;
		public ComponentMessage(int componentId, int netId, int senderNetId, ChannelType channelType, byte[] buffer){
			this.componentId = componentId;
			this.netId = netId;
			this.senderNetId = senderNetId;
			this.buffer = buffer;
			this.channelType = channelType;
		}
	}

	public abstract class NetworkBehaviour : MonoBehaviour{
		public int myNetId{	get{	return NetworkManager.netId;	}	}
		public bool isServer{	get{	return NetworkManager.Instance._isServer;	}	}

		public abstract void SetIsLocal(bool isLocal);
	}

	public abstract class NetworkComponent : MonoBehaviour {
		public readonly int componentId;

		public int myNetId{	get{	return NetworkManager.netId;	}	}
		public bool isServer{	get{	return NetworkManager.Instance._isServer;	}	}

		public Queue<ComponentMessage> netMessages = new Queue<ComponentMessage>();
		public Queue<ComponentMessage> connectionMessage = new Queue<ComponentMessage>();

		abstract public void OnConnect();
		abstract public void OnDisconnect();
		abstract public void OnRemoteConnection(int connectedNetId);
		abstract public void OnRemoteDisconnection(int disconnectedNetId);
	}

	public abstract class NetworkSingleton<U> : NetworkComponent where U : NetworkComponent{
		protected static U instance;
		public static U Instance{
			get{
				if(instance == null){
					//Checks if there is one and only one instance of a singleton
					U[] objects = (U[]) FindObjectsOfType(typeof(U));
					instance = objects[0];

					if(objects.Length > 1){
						Debug.LogError("More than one instance of " + typeof(U) + " exists in the scene, undesired behaviour may occur.\nEnsure that only one instance exists.");
					}

					if (instance == null){
						Debug.LogError("An instance of " + typeof(U) + " is needed in the scene, but there is none.");
					}
				}
				return instance;
			}
		}
	}
}
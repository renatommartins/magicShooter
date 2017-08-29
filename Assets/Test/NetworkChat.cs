using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using Game.Network;

public class NetworkChat : NetworkComponent {

	public new readonly int componentId = 1;

	public Text textOutput;
	public InputField input;

	public string pending;

	void Awake(){
	}

	void Start(){
		Game.Network.NetworkManager.StartListening(componentId,this);
	}

	void Update(){
		if(Input.GetKeyDown(KeyCode.Return)){
			pending = input.text;
			input.text = "";
		}
	}

	void FixedUpdate(){
		ExecuteTask();
		if(netMessages.Count > 0){
			ReceiveMessage(netMessages.Dequeue());
		}
	}

	public void ReceiveMessage (ComponentMessage message){
		string receivedMessage;
		using(MemoryStream stream = new MemoryStream(message.buffer)){
			using(BinaryReader reader = new BinaryReader(stream)){
				receivedMessage = reader.ReadString();
				Debug.Log(receivedMessage);
			}
		}
		textOutput.text += "\n"+receivedMessage;
	}

	public override void OnConnect (){}

	public override void OnDisconnect (){}

	public override void OnRemoteConnection (int connectedNetId){}

	public override void OnRemoteDisconnection (int disconnectedNetId){}

	public void ExecuteTask (){
		if(pending != ""){
			ComponentMessage networkMessage = new ComponentMessage(componentId,-1,myNetId,ChannelType.ReliableSequenced,null);

			using(MemoryStream stream = new MemoryStream()){
				using(BinaryWriter writer = new BinaryWriter(stream)){
					writer.Write(pending);
				}
				networkMessage.buffer = stream.ToArray();
			}
			NetworkManager.SendNetworkMessage(networkMessage);
			textOutput.text += "\n"+pending;
			Debug.Log("''"+pending+"'' sent on chat");
			pending = "";
		}
	}
}

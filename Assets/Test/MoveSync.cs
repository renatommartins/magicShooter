using UnityEngine;
using System.Collections;
using Game.Network;
using System.IO;

public class MoveSync : NetworkComponent {

	public new readonly int componentId = 2;

	public Transform syncTransform;
	public float speed;

	public float minimumDelta;
	private Vector3 lastPosition;

	// Use this for initialization
	void Awake () {
	}

	void Start(){
		NetworkManager.StartListening(componentId,this);
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if(NetworkManager.Instance._isServer){
			Vector3 oldPosition = syncTransform.position;
			syncTransform.position = new Vector3(oldPosition.x+Input.GetAxis(InputAxes.Horizontal)*speed*Time.deltaTime
				,oldPosition.y+Input.GetAxis(InputAxes.Vertical)*speed*Time.deltaTime,0);
			Vector3 difference = syncTransform.position - lastPosition;
			difference.x = Mathf.Abs(difference.x);
			difference.y = Mathf.Abs(difference.y);
			difference.z = Mathf.Abs(difference.z);
			if(difference.x > minimumDelta || difference.y > minimumDelta || difference.z > minimumDelta){
				lastPosition = syncTransform.position;
				ExecuteTask();
			}
		}else{
			if(netMessages.Count > 0){
				ReceiveMessage(netMessages.Dequeue());
				netMessages.Clear();
			}
		}
	}

	public void ReceiveMessage (ComponentMessage message){
		if(isServer){	return;	}
		Vector3 newPosition = new Vector3();
		using(MemoryStream stream = new MemoryStream(message.buffer)){
			using(BinaryReader reader = new BinaryReader(stream)){
				newPosition.x = reader.ReadSingle();
				newPosition.y = reader.ReadSingle();
				newPosition.z = 0;
			}
		}
		syncTransform.position = newPosition;
	}

	public override void OnConnect (){}

	public override void OnRemoteConnection (int connectedNetId){
		if(isServer){
			byte[] buffer;
			using(MemoryStream stream = new MemoryStream()){
				using(BinaryWriter writer = new BinaryWriter(stream)){
					writer.Write(syncTransform.position.x);
					writer.Write(syncTransform.position.y);
				}
				buffer = stream.ToArray();
			}
			NetworkManager.SendNetworkMessage(new ComponentMessage(componentId,connectedNetId,myNetId,ChannelType.UnreliableSequenced,buffer));
		}
	}

	public override void OnDisconnect (){}

	public override void OnRemoteDisconnection (int disconnectedNetId){}

	private void ExecuteTask(){
		byte[] buffer;
		using(MemoryStream stream = new MemoryStream()){
			using(BinaryWriter writer = new BinaryWriter(stream)){
				writer.Write(syncTransform.position.x);
				writer.Write(syncTransform.position.y);
			}
			buffer = stream.ToArray();
		}
		NetworkManager.SendNetworkMessage(new ComponentMessage(componentId,-1,myNetId,ChannelType.UnreliableSequenced,buffer));
	}
}

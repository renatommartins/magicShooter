//using UnityEngine;
//using UnityEditor;
//using System.Collections;
//
//public class EquipmentDatabaseEditor {
//
//	[MenuItem("Databases/Create Equipment Database")]
//	static public void CreateEquipmentDatabase(){
//		string path = string.Concat("Assets/", "EquipmentDatabase.asset" );
//
//		EquipmentDatabase database = (EquipmentDatabase)AssetDatabase.LoadAssetAtPath(path, typeof(EquipmentDatabase));
//
//		if(database == null){
//			database = ScriptableObject.CreateInstance<EquipmentDatabase>();
//
//			database.handEquipments = new System.Collections.Generic.List<HandEquipment>();
//
//			AssetDatabase.CreateAsset(database, path);
//			EquipmentDatabase.database = database;
//		}else{
//			EquipmentDatabase.database = database;
//			Debug.LogWarning("There is an Equipment Database already! No database created.");
//		}
//	}
//}

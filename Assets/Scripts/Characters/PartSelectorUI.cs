using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PartSelectorUI : Singleton<PartSelectorUI>{

	public Dropdown headSelection;
	public Dropdown torsoSelection;
	public Dropdown leftArmSelection;
	public Dropdown rightArmSelection;
	public Dropdown legsSelection;
	[Space(10)]
	public Dropdown leftHandSelection;
	public Dropdown leftShoulderSelection;
	public Dropdown rightHandSelection;
	public Dropdown rightShoulderSelection;

	void Awake(){
		List<string> options = new List<string>();

		headSelection.ClearOptions();
		options.Clear();
		for(int i=0; i<GameSystem.partsDatabase.headParts.Count; i++){	options.Add(GameSystem.partsDatabase.headParts[i].partName);	}
		headSelection.AddOptions(options);

		torsoSelection.ClearOptions();
		options.Clear();
		for(int i=0; i<GameSystem.partsDatabase.torsoParts.Count; i++){	options.Add(GameSystem.partsDatabase.torsoParts[i].partName);	}
		torsoSelection.AddOptions(options);

		leftArmSelection.ClearOptions();
		options.Clear();
		for(int i=0; i<GameSystem.partsDatabase.leftArmParts.Count; i++){	options.Add(GameSystem.partsDatabase.leftArmParts[i].partName);	}
		leftArmSelection.AddOptions(options);

		rightArmSelection.ClearOptions();
		options.Clear();
		for(int i=0; i<GameSystem.partsDatabase.rightArmParts.Count; i++){	options.Add(GameSystem.partsDatabase.rightArmParts[i].partName);	}
		rightArmSelection.AddOptions(options);

		legsSelection.ClearOptions();
		options.Clear();
		for(int i=0; i<GameSystem.partsDatabase.legsParts.Count; i++){	options.Add(GameSystem.partsDatabase.legsParts[i].partName);	}
		legsSelection.AddOptions(options);

		leftHandSelection.ClearOptions();
		options.Clear();
		for(int i=0; i<GameSystem.partsDatabase.equipments.Count; i++){	options.Add(GameSystem.partsDatabase.equipments[i].equipmentName);	}
		leftHandSelection.AddOptions(options);

		rightHandSelection.ClearOptions();
		options.Clear();
		for(int i=0; i<GameSystem.partsDatabase.equipments.Count; i++){	options.Add(GameSystem.partsDatabase.equipments[i].equipmentName);	}
		rightHandSelection.AddOptions(options);
	}
}


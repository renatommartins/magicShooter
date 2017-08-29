using UnityEngine;
using System.Collections;
using MoonSharp.Interpreter;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.IO;

namespace ScriptSystem{
	public class LuaConsole : Singleton<LuaConsole> {

		public Text output;
		public InputField input;
		public GameObject holder;
		private string command;
		private Script script;
		private List<string> history = new List<string>();
		private int historyIndex;
		private bool set = true;

		void Start () {
			//System.Console.SetOut(new ConsoleWriter());
			Script.DefaultOptions.DebugPrint = ConsolePrint;
			Application.logMessageReceived += RedirectLog;
			script = new Script();
			InitializeAPI ();
			Clear ();
			GetFocus();
		}

		void Update () {
			//if(input.isFocused && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))){	ExecuteCommand();	}
			if(input.isFocused && Input.GetKeyDown(KeyCode.UpArrow) && historyIndex > 0){	historyIndex --;	set = false;}
			if(input.isFocused && Input.GetKeyDown(KeyCode.DownArrow) && historyIndex < history.Count){	historyIndex ++;	set = false;	}
			if(!set){	input.text = historyIndex!=history.Count? history[historyIndex] : ""; set = true;	input.MoveTextEnd(false);	}
			if (Input.GetKeyDown (KeyCode.F12) ) {
				holder.SetActive(!holder.activeSelf);
				if(!holder.activeSelf)
					input.DeactivateInputField();
				else
					GetFocus();
			}
			if(output.text.Length > 800){
				Clear();
			}
		}

		public void ExecuteCommand(){
			command = input.text;
			output.text += "\n" + command;
			input.text = "";
			history.Add(command);
			historyIndex = history.Count;
			GetFocus();
			try{	script.DoString(command);	}
			catch(InterpreterException exception){	output.text += "\n" + exception.DecoratedMessage+"\nUse Help() function for a list of useful functions";	}
		}

		private void InitializeAPI(){
			script.Globals["Help"] = (HelpDelegate)Help;
			script.Globals["Laurinha"] = ((LaurinhaDelegate)Laurinha);
			//script.Globals["RollDice"] = (RollDice)Dice.Roll;
			script.Globals["LoadScript"] = (LoadScriptDelegate)LoadScript;
			script.Globals["Clear"] = (ClearDelegate)Clear;
		}

		public static void ConsolePrint(string content){
			if (Instance.output.text.Length > 50){	Instance.output.text = Instance.output.text.Remove (0, 50);	}
			Instance.output.text += "\n"+content;
		}

		public static void ConsolePrint(params string[] strings){
			foreach(string text in strings){
				Instance.output.text += "\n"+text;
			}
		}

		public static void RedirectLog(string log, string stackTrace, LogType logType){
			ConsolePrint("\n",log+"\n"+stackTrace+logType.ToString());
		}
		
		private void GetFocus(){	input.Select();	input.ActivateInputField();	}

		delegate void HelpDelegate();
		void Help(){
			ConsolePrint ("Some useful functions:\nRollDice(int x, int dy)\nClear()\nLoadScript(string path)");
		}

		delegate string LaurinhaDelegate();
		string Laurinha(){	ConsolePrint("eh a que eu Amo muito muito e eh linda!!!!");	return "eh a que eu Amo muito muito e eh linda!!!!";	}

		delegate int RollDice(int x, int y);

		delegate void ClearDelegate();
		void Clear(){	output.text = "";	}

		delegate bool LoadScriptDelegate(string path);
		bool LoadScript(string path){
			UnityEngine.Object obj = Resources.Load(path);
			string command;

			if (obj == null) {
				try{
					using(System.IO.StreamReader externalText = new System.IO.StreamReader(path)){
						command = externalText.ReadToEnd();
					}
				}catch(Exception exception){	ConsolePrint(exception.Message);	return false;	}
			}else{	command = ((TextAsset) obj).text;	}

			try{	script.DoString(command);	}
			catch(InterpreterException exception){	output.text += "\n" + exception.DecoratedMessage;	return false;	}

			ConsolePrint("Script Loaded");
			return true;
		}
	}
}
//Version 0.1.1
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

[KSPAddon(KSPAddon.Startup.Flight, false)]
class CLS_FlightGui : MonoBehaviour{
	#region CLASS VARIABLES
	[KSPField(isPersistant = true)]
	private static bool doShowStatusWindow = false;
	[KSPField(isPersistant = true)]
	private static bool doShowSettingsWindow = false;
	private static bool HideUI = false;

	private static Vessel ControlledVessel;
	#endregion


	#region INIT FUNCTIONS
	/// <summary>
	/// Primary role is registering all hooks that will come into play later.
	/// </summary>
	private void Awake() {
		GameEvents.onVesselChange.Add(HOOK_VesselChange);
		GameEvents.onCrewOnEva.Add(HOOK_EVA_Start);
		GameEvents.onCrewBoardVessel.Add(HOOK_EVA_Start);
		GameEvents.onHideUI.Add(HOOK_HideUI);
		GameEvents.onShowUI.Add(HOOK_ShowUI);
		GameEvents.onFlightReady.Add(HOOK_FlightReady);
	}


	/// <summary>
	/// 
	/// </summary>
	private void Start() {
		CDebug.verbose("CLS GUI Starting.");
	}
	#endregion


	/// <summary> Anything I want to only happen once per frame goes here.
	/// </summary>
	private void Update() {
		//Manual show/hide for the Status panel. Only 'P' is listening for keydown, 
		//otherwise user would have to press both keys during the same frame.
		if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.P)) {
			doShowStatusWindow = !doShowStatusWindow;
			CDebug.log("Ctrl + P chord pressed.");
		}
	}


	#region GUI CONTROLS
	private static Rect StatusPanelBox = new Rect(150, 0, 350, 250);
	private static Rect SettingsBox = new Rect(145, 0, 400, 300);
	private static int statusToolbarSelection = 0;
	private static GUIStyle centeredText;
#if (DEBUG == false)
	private static string[] statusToolbarButtons = { "Overview", "Subsys", "Damage", "EVAs" };
#else
	private static string[] statusToolbarButtons = { "Overview", "Subsys", "Damage", "EVAs", "DEBUG" };
#endif
	/// <summary>
	/// 
	/// </summary>
	private void OnGUI() {
		//CDebug.log("GUI point 1");
		if (centeredText == null)	//using this because it was the first GUI variable I made. 
			InitGUIVariables();		//If it's null this is the GUI's first run.
		else {
			if (!HideUI && doShowStatusWindow)
				StatusPanelBox = GUI.Window("CLSStatus".GetHashCode(), StatusPanelBox, drawStatusWindow, "ClearScreen Life Support");
			if (!HideUI && doShowSettingsWindow)
				SettingsBox = GUI.Window("CLSSettings".GetHashCode(), SettingsBox, drawSettingsWindow, "CLS Settings");
		}
	}


	/// <summary>Init for GUI-based variables
	/// </summary>
	private void InitGUIVariables() {
		//CDebug.log("GUI point 2.1");
		centeredText = new GUIStyle(GUI.skin.label){
			alignment = TextAnchor.MiddleCenter
		};
	}


	/// <summary>Draw the Status Panel window.
	/// </summary>
	/// <param name="id"></param>
	private void drawStatusWindow(int id) {
		//CDebug.log("GUI point 2.2");
		if (GUI.Button(new Rect(StatusPanelBox.width - 15, 5, 10, 10), "X")) { doShowStatusWindow = false; }
		//Toolbar. (Left side, probably)
		GUILayout.BeginHorizontal();
		statusToolbarSelection =  GUILayout.SelectionGrid(statusToolbarSelection, statusToolbarButtons, 1, GUILayout.Width(75));
		GUILayout.Space(10);
		GUILayout.BeginVertical();

		switch (statusToolbarSelection) {
			case 0:
				StatusOverview();
				break;
			//case 1:
			//	Subsystems();
			//	break;
			case 2:
				BrokenParts();
				break;
			case 4:
				DEBUGTAB();
				break;
			case 1:
			case 3:
			default:
				GUILayout.Label("Either something went wrong, or the tab is not implemented.");
				break;
		}

		GUILayout.EndVertical();
		GUILayout.EndHorizontal();

		if (GUI.Button(new Rect(10, StatusPanelBox.height - 25, 75, 20), "Settings"))
			doShowSettingsWindow = true;
		GUI.DragWindow();
	}

	 
	/// <summary>Display function for the Overview tab of the status window 
	/// </summary>
	private void StatusOverview() {
		//CDebug.log("GUI point 2.2.1");
		//Vessel name, centered
		GUILayout.Label(ControlledVessel.GetName(), centeredText);
		//Crew present/capacity
		GUILayout.Label(ControlledVessel.GetCrewCount() + "/" + ControlledVessel.GetCrewCapacity() + " Kerbals");

		//ETTLs, Resources left/max
		//GUILayout.Label("ETTL display not implemented yet.");
		//GUILayout.Label("Res count not implemented yet.");
		foreach (string resName in ConfigSettings.CLSResourceNames) {
			GUILayout.BeginHorizontal();
			GUILayout.Label(String.Format("{0, -10}", resName + ":"),GUILayout.Width(50));
			GUILayout.Label(String.Format("\t{0, 15}",
				(Backend.ETTLs[resName] == 0) ? "--:--:--:--" : Backend.ETTLs[resName].ToString()));
			GUILayout.Label(String.Format("{0}/{1}",
				(Backend.ResourceMaximums[resName] == 0) ? "--" : "-0-",
				(Backend.ResourceMaximums[resName] == 0) ? "--" : Backend.ResourceMaximums[resName].ToString()));
			GUILayout.EndHorizontal();
		}

		//Stable/Warning for most urgent ETTL
		GUILayout.Label("\nETTL Warning not implemented.");
		//Subsystem damage status
		GUILayout.Label("Damage status not implemented.");
	}


	/// <summary>Display function for the Subsystems tab.
	/// </summary>
	private void Subsystems() {
		//CDebug.log("GUI point 2.2.2");
	}


	/// <summary>Display function for the broken parts tab.
	/// </summary>
	private void BrokenParts() {
		//CDebug.log("GUI point 2.2.3");
		if (Backend.BrokenParts.Count == 0)
			GUILayout.Label("No broken parts! Awesome!");
		else
			foreach (KeyValuePair<int, BrokenPart> kvp in Backend.BrokenParts) {
				GUILayout.Label(kvp.Value.partName);
			}
	}


	/// <summary>Display function for the Debug tab.
	/// </summary>
	private void DEBUGTAB() {
		//CDebug.log("GUI point 2.2.4");
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Break a part")) {
			Backend.BreakableParts[UnityEngine.Random.Range(0, Backend.BreakableParts.Count - 1)].BreakRandom();
		}
		if (GUILayout.Button("Reset tanks")) {
			foreach (KeyValuePair<string, List<PartResource>> kvp in Backend.Resources) {
				if (ConfigSettings.ratesPerKerbal.ContainsKey(kvp.Key)) {
					CDebug.log("RESET RESOURCE: " + kvp.Key);
					if (ConfigSettings.ratesPerKerbal[kvp.Key] > 0)
						foreach (PartResource pRes in kvp.Value)
							pRes.amount = pRes.maxAmount;
					else
						foreach (PartResource pRes in kvp.Value)
							pRes.amount = 0;
				}
				else
					CDebug.log("SKIPPING RESOURCE: " + kvp.Key + " (Not in rates dictionary)");
			}
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Kill all Kerbals")) {
			foreach (ProtoCrewMember unluckyBastard in ControlledVessel.GetVesselCrew())
				Backend.KillKerbal(unluckyBastard);
		}
		if (GUILayout.Button("Kill a Kerbal")) {
			Backend.KillKerbal(ControlledVessel.GetVesselCrew()[UnityEngine.Random.Range(0, ControlledVessel.GetVesselCrew().Count - 1)]);
		}
		GUILayout.EndHorizontal();

		GUILayout.Space(5);
		GUILayout.Label("Rates:");
		foreach (KeyValuePair<string, double> kvp in ConfigSettings.ratesPerKerbal)
			GUILayout.Label(kvp.Key + ": " + kvp.Value, GUILayout.MaxHeight(10));
	}


	private void drawSettingsWindow(int id) {
		CDebug.log("GUI point 2.3");
		if (GUI.Button(new Rect(SettingsBox.width - 15, 5, 10, 10), "X")) { doShowSettingsWindow = false; }
		foreach (string resName in ConfigSettings.CLSResourceNames) {
			GUILayout.BeginHorizontal();
			GUILayout.Label(resName + " warning level:", GUILayout.Width(70));
			GUILayout.EndHorizontal();
		}
		ConfigSettings.partsBreak = GUILayout.Toggle(ConfigSettings.partsBreak, "Parts can break");
		GUILayout.BeginHorizontal();
		GUILayout.Label("[Overall timescale edit]");
		GUILayout.EndHorizontal();

		GUI.DragWindow();
	}


	/// <summary> 
	/// </summary>
	internal static void showStatusWindow() {
		CDebug.verbose("Showing status window.");
		doShowStatusWindow = true;
	}

	/// <summary>
	/// </summary>
	internal static void hideStatusWindow() {
		doShowStatusWindow = false;
		CDebug.verbose("Hiding status window.");
	}
	#endregion


	#region HOOKS
	private void HOOK_VesselChange(Vessel v) { }


	//Grouped because inverse.
	//probably offload these to either the backend or the parts themselves.
	private void HOOK_EVA_Start(GameEvents.FromToAction<Part, Part> FtA) { }
	private void HOOK_EVA_End(GameEvents.FromToAction<Part, Part> FtA) { }


	//Grouped because inverse.
	private void HOOK_HideUI() { HideUI = true; }
	private void HOOK_ShowUI() { HideUI = false; }


	private void HOOK_FlightReady() {
		CDebug.verbose("Flight ready Event.");
		ControlledVessel = FlightGlobals.ActiveVessel;
		Backend.InitializeFlight(ControlledVessel);
		CDebug.log("Flight ready Event finished.");
	}
	#endregion
}

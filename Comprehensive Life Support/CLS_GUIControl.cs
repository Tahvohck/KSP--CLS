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
	private static Rect StatusPanelBox = new Rect(145, 0, 300, 400);
	private static Rect SettingsBox = new Rect(145, 0, 300, 400);
	[KSPField(isPersistant = true)]
	private static bool VisibleStatusWindow = false;
	[KSPField(isPersistant = true)]
	private static bool VisibleSettingsWindow = false;
	private static bool HideUI = false;

	private static Vessel ControlledVessel;
	//private static List<PartResource>
	//	connectedOxygen = new List<PartResource>(),
	//	connectedCO2 = new List<PartResource>(),
	//	connectedSnacks = new List<PartResource>(),
	//	connectedWater = new List<PartResource>();

	//This is used in place of the above lists.
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
			VisibleStatusWindow = !VisibleStatusWindow;
			CDebug.log("Ctrl + P chord pressed.");
		}
	}


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


	#region GUI CONTROLS
	/// <summary>
	/// 
	/// </summary>
	private void OnGUI() {
		//try {
			if (!HideUI && VisibleStatusWindow)
				StatusPanelBox = GUI.Window("CLSStatus".GetHashCode(), StatusPanelBox, drawStatusWindow, "ClearScreen Life Support");
			if (!HideUI && VisibleSettingsWindow)
				SettingsBox = GUI.Window("CLSSettings".GetHashCode(), SettingsBox, drawSettingsWindow, "CLS Settings");
		//}
		//catch (NullReferenceException nullE) {
		//	CDebug.log(nullE.Message + "\n" + nullE.StackTrace);
		//}
	}


	private int statusToolbarSelection = 0;
	private string[] statusToolbarButtons = {"Overview", "Generators", "EVAs" };
	/// <summary>Draw the Status Panel window.
	/// </summary>
	/// <param name="id"></param>
	private void drawStatusWindow(int id) {
		if (GUI.Button(new Rect(StatusPanelBox.width - 15, 5, 10, 10), "")) { VisibleStatusWindow = false; }
		//Toolbar. (Left side, probably)

		StatusOverview();

		GUI.DragWindow();
	}


	//private static GUIStyle centeredText = new GUIStyle(GUI.skin.label) {
	//	alignment = TextAnchor.MiddleCenter
	//};
	/// <summary>Display function for the Overview tab of the status window 
	/// </summary>
	private void StatusOverview() {
		//Vessel name, centered
		GUILayout.Label(ControlledVessel.GetName());//, centeredText);
		//Crew present/capacity
		GUILayout.Label(ControlledVessel.GetCrewCount() + "/" + ControlledVessel.GetCrewCapacity() + " Kerbals");

		//ETTLs, Resources left/max
		//GUILayout.Label("ETTL display not implemented yet.");
		//GUILayout.Label("Res count not implemented yet.");
		foreach (string resName in CLS_Configuration.CLSResourceNames) {
			GUILayout.BeginHorizontal();
			GUILayout.Label(String.Format("{0, -10}", resName + ":"));
			GUILayout.Label(String.Format("\t{0, 10}", Backend.ETTLs[resName]));
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


	private void drawSettingsWindow(int id) { }


	/// <summary> 
	/// </summary>
	internal static void showStatusWindow() {
		CDebug.verbose("Showing status window.");
		VisibleStatusWindow = true;
	}


	/// <summary>
	/// </summary>
	internal static void hideStatusWindow() {
		VisibleStatusWindow = false;
		CDebug.verbose("Hiding status window.");
	}
	#endregion
}

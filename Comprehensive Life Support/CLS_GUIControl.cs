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
	private static bool VisibleStatusWindow = true;
	[KSPField(isPersistant = true)]
	private static bool VisibleSettingsWindow = false;
	private static bool HideUI = false;

	private static Vessel ActiveVessel;
	private static List<Vessel> EVAKerbals;
	//private static List<PartResource>
	//	connectedOxygen = new List<PartResource>(),
	//	connectedCO2 = new List<PartResource>(),
	//	connectedSnacks = new List<PartResource>(),
	//	connectedWater = new List<PartResource>();

	//This is used in place of the above lists.
	private Dictionary<String, List<PartResource>> Resources;
	#endregion




	#region INIT FUNCTIONS
	/// <summary>
	/// 
	/// </summary>
	private void Awake() {
		GameEvents.onVesselChange.Add(HOOK_VesselChange);
		GameEvents.onCrewOnEva.Add(HOOK_EVA_Start);
		GameEvents.onCrewBoardVessel.Add(HOOK_EVA_Start);
		GameEvents.onHideUI.Add(HOOK_HideUI);
		GameEvents.onShowUI.Add(HOOK_ShowUI);
	}


	/// <summary>
	/// 
	/// </summary>
	private void Start() {
		//buildResourceTankLists();
	}
	#endregion


	#region HOOKS
	private void HOOK_VesselChange(Vessel v) { }


	//Grouped because inverse.
	private void HOOK_EVA_Start(GameEvents.FromToAction<Part, Part> FtA) { }
	private void HOOK_EVA_End(GameEvents.FromToAction<Part, Part> FtA) { }


	//Grouped because inverse.
	private void HOOK_HideUI() { HideUI = true; }
	private void HOOK_ShowUI() { HideUI = false; }

	#endregion


	#region GUI CONTROLS
	/// <summary>
	/// 
	/// </summary>
	private void OnGUI() {
		if (!HideUI && VisibleStatusWindow)
			StatusPanelBox = GUI.Window("CLSStatus".GetHashCode(), StatusPanelBox, drawStatusWindow, "ClearScreen Life Support");
		if (!HideUI && VisibleSettingsWindow)
			SettingsBox = GUI.Window("CLSSettings".GetHashCode(), SettingsBox, drawSettingsWindow, "CLS Settings");
	}


	private int statusToolbarSelection = 0;
	private string[] statusToolbarButtons = {"Overview", "Generators", "EVAs" };
	private void drawStatusWindow(int id) {
		if (GUI.Button(new Rect(StatusPanelBox.width - 15, 5, 10, 10), "")) { VisibleStatusWindow = false; }
		statusToolbarSelection = GUI.Toolbar(new Rect(5, 20, StatusPanelBox.width - 10, 15),statusToolbarSelection, statusToolbarButtons);

		GUILayout.Label("Test. {" + GUI.skin.label.fontSize + "}");
		GUI.skin.label.alignment = TextAnchor.MiddleCenter;
		//GUILayout.Label(ActiveVessel.vesselName);
		GUI.skin.label.alignment = TextAnchor.MiddleLeft;

		GUI.DragWindow();
	}


	private void drawSettingsWindow(int id) { }

	#endregion


	/// <summary>Pulse through vessel, discover resource tanks. Add them to appropriate list.
	/// </summary>
	private void buildResourceTankLists() {
		foreach (Part p in ActiveVessel.Parts)
			foreach (PartResource res in p.Resources)
				if (Resources.ContainsKey(res.resourceName))
					Resources[res.resourceName].Add(res);
				else
					Resources[res.resourceName] = new List<PartResource> { res };
	}


	/// <summary>
	/// Anything I want to only happen once per frame goes here.
	/// </summary>
	private void Update() {
		//Manual show/hide for the Status panel. Only 'P' is listening for keydown, 
		//otherwise user would have to press both keys during the same frame.
		if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.P)) {
			VisibleStatusWindow = !VisibleStatusWindow;
			CDebug.log("Ctrl + P chord pressed.");
		}
	}
}

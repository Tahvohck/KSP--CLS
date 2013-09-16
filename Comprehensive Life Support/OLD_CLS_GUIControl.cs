using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using UnityEngine;

/// <summary>
/// This class controls the GUI element for the flight scene. I think I should make it threaded at some point.
/// </summary>
[KSPAddon(KSPAddon.Startup.Flight, false)]
public class OLD_CLS_FlightGui : MonoBehaviour
{
	private static Rect StatusPanel = new Rect(145, 0, 300, 400);

	[KSPField(isPersistant = true)]
	private static bool StatusPanelVisible = false;
	private static bool HideUI = false;

	private static Vessel loadedVessel = null;
	private static Part masterLivablePart = null;
	private static List<PartResource>
		connectedOxygen	= new List<PartResource>(),
		connectedCO2	= new List<PartResource>(),
		connectedSnacks	= new List<PartResource>(),
		connectedWater	= new List<PartResource>();
	private static double
		sumOxygen	= 0,
		OxygenIn	= 0,
		OxygenOut	= 0,
		sumCO2		= 0,
		CO2In		= 0,
		CO2Out		= 0,
		sumWater	= 0,
		WaterIn		= 0,
		WaterOut	= 0,
		sumSnacks	= 0,
		SnacksIn	= 0,
		SnacksOut	= 0;
	private static string
		ETTLOxygen	= "",
		ETTLCO2		= "",
		ETTLWater	= "",
		ETTLSnacks	= "";
	public static ushort crewSize = 0;
	public static ushort maxCrewSize = 0;


	/****************
	*BEGIN FUNCTIONS*
	****************/
	/*******
	* INIT *
	*******/
	/// <summary>Initialize variables here, to make sure the whole active vessel is loaded. 
	/// </summary>
	private void Start() {
		buildResourceList();

		GameEvents.onVesselChange.Add(HOOK_VesselChange);
		GameEvents.onCrewOnEva.Add(HOOK_CrewEva);
		GameEvents.onCrewBoardVessel.Add(HOOK_CrewBoardVessel);
		GameEvents.onHideUI.Add(HOOK_HideUI);
		GameEvents.onShowUI.Add(HOOK_ShowUI);
		
	}


	/******************
	*CONTROL FUNCTIONS*
	******************/
	/// <summary> GUI Control
	/// </summary>
	void OnGUI() {
		if (StatusPanelVisible && !HideUI) //That is, only show if the UI is visible and the panel is toggled on.
			StatusPanel = GUILayout.Window("CLSLifesupport".GetHashCode(), StatusPanel, drawLifeSupport, "ClearScreen Life support");
	}

	/// <summary>Set the scene's active vessel.
	/// </summary><param name="v"></param>
	/// <returns>True if vessel was able to be set (that is, didn't match the already loaded vessel.)</returns>
	public static bool setActiveVessel(Vessel v, Part caller) {
		if (loadedVessel != v) {
			loadedVessel = v; masterLivablePart = caller;
			return true;
		}
		else
			return false;
	}

	internal static void toggleData() { StatusPanelVisible = !StatusPanelVisible; }


	/****************
	*POWER FUNCTIONS*
	****************/
	/// <summary> GUI Draw for flight GUI. Try to avoid too much crosstalk between this and Modules. </summary>
	/// <param name="id"></param>
	private void drawLifeSupport(int id) {
		if (GUI.Button(new Rect(StatusPanel.width - 15, 5, 10, 10), "")) { StatusPanelVisible = false; }

		GUILayout.Label(loadedVessel.vesselName);
		GUILayout.Label(String.Format("Crew: {0}/{1}", crewSize, maxCrewSize));
		GUILayout.BeginHorizontal();
		GUILayout.BeginVertical();
		GUILayout.Label("");
		GUILayout.Label("Oxygen:");
		GUILayout.Label("CO2:");
		GUILayout.Label("Water:");
		GUILayout.Label("Snacks:");
		GUILayout.EndVertical();
		GUILayout.BeginVertical();
		GUILayout.Label("ETTL");
		GUILayout.Label(ETTLOxygen);
		GUILayout.Label(ETTLCO2);
		GUILayout.Label(ETTLWater);
		GUILayout.Label(ETTLSnacks);
		GUILayout.Label("");
		GUILayout.EndVertical();
		GUILayout.BeginVertical();
		GUILayout.Label("Raw amount");
		GUILayout.Label(String.Format("{0,7:F2} liters", sumOxygen));
		GUILayout.Label(String.Format("{0,7:F2} liters", sumCO2));
		GUILayout.Label(String.Format("{0,7:F2} liters", sumWater));
		GUILayout.Label(String.Format("{0,7:F2} meals", sumSnacks));
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();

		GUI.DragWindow();
	}


	/// <summary> Build the Resource lists, so that I don't have to pulse through the vessel every
	/// time OnGUI() is getting called.
	/// </summary>
	private static void buildResourceList() {
		foreach (Part p in loadedVessel.parts) {
			foreach (PartResource pr in p.Resources) {
				if (pr.resourceName == "Oxygen")
					connectedOxygen.Add(pr);
				else if (pr.resourceName == "CO2")
					connectedCO2.Add(pr);
				else if (pr.resourceName == "Water")
					connectedWater.Add(pr);
				else if (pr.resourceName == "Snacks")
					connectedSnacks.Add(pr);
			}
		}
	}


	/// <summary>
	/// </summary>
	private static void sumResources() {
		crewSize = (ushort)loadedVessel.GetCrewCount();
		maxCrewSize = (ushort)loadedVessel.GetCrewCapacity();
			//I would like to use these, but it breaks the module loading for some reason.
			//sumOxygen = connectedOxygen.Sum(res => res.amount);
			//sumCO2 = connectedCO2.Sum(res => res.amount);
			//sumWater = connectedWater.Sum(res => res.amount);
			//sumSnacks = connectedSnacks.Sum(res => res.amount);
		sumOxygen = 0;
		sumCO2 = 0;
		sumWater = 0;
		sumSnacks = 0;
		foreach (PartResource res in connectedOxygen){
			sumOxygen += res.amount;}
		foreach (PartResource res in connectedCO2) {
			sumCO2 += res.amount;}
		foreach (PartResource res in connectedWater) {
			sumWater += res.amount;}
		foreach (PartResource res in connectedSnacks) {
			sumSnacks += res.amount;}
	}


	/// <summary>Build ETTLs. At the moment, this does not factor in resource reclamation or production.
	/// CO2 is also not built right now, as it is not a simple check of RawAmount/rate seconds.
	/// CO2 will have to pulse to find the LivableArea closest to filling with CO2.
	/// 
	/// !TODO! Rewrite this, make it actually use a Rate value.
	/// </summary>
	private static void buildETTLs() {
		double
			tmpTotalSecs = sumOxygen / (OLD_CLS_LivableArea.PerKerbal_inOxygen * crewSize),
			tmpDays = Math.Floor(tmpTotalSecs / 86400),
			tmpHrs = Math.Floor((tmpTotalSecs - tmpDays * 86400) / 3600),
			tmpMins = Math.Floor((tmpTotalSecs - tmpDays * 86400 - tmpHrs * 3600) / 60),
			tmpSecs = Math.Floor((tmpTotalSecs - tmpDays * 86400 - tmpHrs * 3600 - tmpMins * 60));
		ETTLOxygen = String.Format("{0:F0}:{1:F0}:{2:F0}:{3:F0}", tmpDays, tmpHrs, tmpMins, tmpSecs);

		tmpTotalSecs = sumWater / (OLD_CLS_LivableArea.PerKerbal_inWater * crewSize);
		tmpDays = Math.Floor(tmpTotalSecs / 86400);
		tmpHrs = Math.Floor((tmpTotalSecs - tmpDays * 86400) / 3600);
		tmpMins = Math.Floor((tmpTotalSecs - tmpDays * 86400 - tmpHrs * 3600) / 60);
		tmpSecs = Math.Floor((tmpTotalSecs - tmpDays * 86400 - tmpHrs * 3600 - tmpMins * 60));
		ETTLWater = String.Format("{0:F0}:{1:F0}:{2:F0}:{3:F0}", tmpDays, tmpHrs, tmpMins, tmpSecs);

		tmpTotalSecs = sumSnacks / (OLD_CLS_LivableArea.PerKerbal_inFood * crewSize);
		tmpDays = Math.Floor(tmpTotalSecs / 86400);
		tmpHrs = Math.Floor((tmpTotalSecs - tmpDays * 86400) / 3600);
		tmpMins = Math.Floor((tmpTotalSecs - tmpDays * 86400 - tmpHrs * 3600) / 60);
		tmpSecs = Math.Floor((tmpTotalSecs - tmpDays * 86400 - tmpHrs * 3600 - tmpMins * 60));
		ETTLSnacks = String.Format("{0:F0}:{1:F0}:{2:F0}:{3:F0}", tmpDays, tmpHrs, tmpMins, tmpSecs);
	}


	private static short FixedCycles = 0;
	public static void UpdateGUIData() {
		FixedCycles++;
		if (FixedCycles % (.2 * 50) == 0) {
			sumResources();
			buildETTLs();}

		if (FixedCycles % (120 * 50) == 0)
			FixedCycles = 0;
	}

	public void FixedUpdate() {
	}


	/************
	*EVENT HOOKS*
	************/
	public void HOOK_CrewEva(GameEvents.FromToAction<Part, Part> FTA) {
		print("Crew EVA out.\n\tFrom:" + FTA.from.name + "\n\tTo:" + FTA.to.name);
	}

	public void HOOK_CrewBoardVessel(GameEvents.FromToAction<Part, Part> FTA) {
		print("Crew EVA in.\n\tFrom:" + FTA.from.name + "\n\tTo:" + FTA.to.name);
	}

	public void HOOK_VesselChange(Vessel V) {
		print("Vessel changed to " + V.vesselName + ".\n\t" + V.vesselName +
			(V.isEVA ? " is on an EVA." : " is a ship."));
	}

	public void HOOK_HideUI() { HideUI = true; }
	public void HOOK_ShowUI() { HideUI = false; }
}
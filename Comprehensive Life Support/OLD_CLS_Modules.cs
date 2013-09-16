using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

// TODO Problem: Time warp locks out the last few dregs in a tank because it can't draw the full amount.
public class OLD_CLS_LivableArea : PartModule
{
	private static bool isFlight = false;
	private static bool isEditor = false;
	private static bool initialized = false;

	#region Consumption/Production Constants
	internal static double PerKerbal_inOxygen;
	internal static double PerKerbal_inFood;
	internal static double PerKerbal_inWater;
	internal static double PerKerbal_prCarbonDioxide;
	internal static double PerKerbal_prHumidity;
	#endregion

	private byte crewSize;
	private byte maxCrewSize;
	private bool IAmMaster;

	#region Region: KSPFields
	#endregion
	#region Region: KSPEvents
	[KSPEvent(guiName = "Toggle Debug Data", active = true, guiActive = true)]	//Event to show/hide StatusPanel
	private void showDataDebug() { OLD_CLS_FlightGui.toggleData(); }
#if DEBUG
	[KSPEvent(guiName = "Reset Resources", active = true, guiActive = true)]
	private void refill() {
		foreach (PartResource resource in part.Resources) {
			print("Trying to load Resource: " + resource.info.name);
			if (resource.resourceName == "CO2")
				part.RequestResource(resource.resourceName, 1000000);
			else
				part.RequestResource(resource.resourceName, -1000000);
	}}
#endif
	#endregion
	#region Region: KSPActions
	#endregion


	/****************
	*BEGIN FUNCTIONS*
	****************/

	/// <summary>When KSP starts the part:
	/// IF FLIGHT-
	/// Pulse through vessel to check for other LivableAreas.
	/// Check crew count in self.
	/// Check max crew count in self.</summary>
	/// <param name="state"></param>
	public override void OnStart(PartModule.StartState state) {
		crewSize = (byte)part.protoModuleCrew.Count;
		maxCrewSize = (byte)part.CrewCapacity;
	}


	/// <summary>When KSP loads the part:
	/// If this is the first time a crewable part has been loaded, initialize default values.</summary>
	/// <param name="node"></param>
	public override void OnLoad(ConfigNode node) {
		if (!initialized) { loadSettings(); initialized = true; } //Only do this once, on first load.

		isEditor = HighLogic.LoadedSceneIsEditor;
		isFlight = HighLogic.LoadedSceneIsFlight;

		if (isFlight) {
			IAmMaster = OLD_CLS_FlightGui.setActiveVessel(part.vessel, part);
			print("I have been loaded, I am " + (IAmMaster ? "" : "not ") + "master.");
		}
	}


	/// <summary>
	/// On every Fixed update (more stable than a standard update), do resource consumption/production. </summary>
	public override void OnFixedUpdate() {
		float dTime = TimeWarp.fixedDeltaTime;
		crewSize = (byte)part.protoModuleCrew.Count;
		//request Oxygen
		part.RequestResource("Oxygen", crewSize * PerKerbal_inOxygen * dTime);
		//if can't request Oxygen, start checking levels for kill effect
		//create CO2
		part.RequestResource("CO2", crewSize * PerKerbal_prCarbonDioxide * dTime);
		//Check CO2 levels
		//create humidity (auto dehumidifier?)

		//request food
		part.RequestResource("Snacks", crewSize * PerKerbal_inFood * dTime);

		//request water
		part.RequestResource("Water", crewSize * PerKerbal_inWater * dTime);

		//Update GUI Data, because it doesn't seem to have an OnFixedUpdate of it's own.
		if (IAmMaster)
			OLD_CLS_FlightGui.UpdateGUIData();

		
	}


	/// <summary>
	/// Load all default values from file.</summary>
	private void loadSettings() {
		//Just some test values for now.
		PerKerbal_inFood = 3.0 / 21600.0;		//3 per day
		PerKerbal_inOxygen = 2.0 / 300.0;		//144L/day
		PerKerbal_inWater = .00005;
		PerKerbal_prCarbonDioxide = -2.0 / 300.0;
		PerKerbal_prHumidity = .00005;

		//foreach (Part p in vessel.parts) {
		//	if (p.CrewCapacity > 0 && !part.Modules.Contains("OLD_CLS_LivableArea")) {
		//		p.AddModule("OLD_CLS_LivableArea");
		//		Debug.Log("[CLS][ALERT]: Adding OLD_CLS_LivableArea to part " + p.partName + "!");
		//	}
		//}
	}
}
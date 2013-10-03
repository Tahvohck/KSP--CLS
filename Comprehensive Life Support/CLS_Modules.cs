using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

class CLS_LivableArea : PartModule
{
	//[KSPEvent(active=true, guiActive=true,guiName="Hide CLS")]
	//private void hideGUI() {
	//	Events["showGUI"].active = true;
	//	Events["hideGUI"].active = false;
	//	CLS_FlightGui.hideStatusWindow();
	//}
	//[KSPEvent(active = true, guiActive = true, guiName = "Show CLS")]
	//private void showGUI() {
	//	Events["showGUI"].active = false;
	//	Events["hideGUI"].active = true;
	//	CLS_FlightGui.showStatusWindow();
	//}

	private static void Awake() {
		CDebug.log("LivableArea Awoken.");
	}
	private static void Start() {
		CDebug.log("LivableArea started.");
	}

	public override void OnLoad(ConfigNode node) {
		CDebug.log(node.ToString());
	}


	public void FixedUpdate() {
		CDebug.log(part.GetHashCode().ToString());
		int secondsPerDayFactor = 86400;
		double o2Got, co2Made, snacksGot, waterGot;
		float dTime = TimeWarp.fixedDeltaTime;
		byte crewSize = (byte)part.protoModuleCrew.Count;
		
		//get oxygen
		double wantedO2 = crewSize * dTime * ConfigSettings.ratesPerKerbal["Oxygen"] /
			(secondsPerDayFactor / ConfigSettings.timeScale);
		o2Got = part.RequestResource("Oxygen", wantedO2);
		if (o2Got == 0) { //if this returned 0 it means there wasn't the amount we wanted.
			double amountLeft = Backend.getCurrentAmount("Oxygen");
			if (amountLeft != 0)	//As long as we have anything left in the system
				o2Got = part.RequestResource("Oxygen", amountLeft);
			else { }	//start killing Kerbals
		}
		//add CO2
		co2Made = part.RequestResource("CO2", crewSize * dTime * ConfigSettings.ratesPerKerbal["CO2"] * (o2Got / wantedO2) /
			(secondsPerDayFactor / ConfigSettings.timeScale));
		//(o2Got / wantedO2) is to ensure that we only produce an equivalent amount of CO2 to the O2 we consumed.

		//get food
		snacksGot = part.RequestResource("Snacks", crewSize * dTime * ConfigSettings.ratesPerKerbal["Snacks"] /
			(secondsPerDayFactor / ConfigSettings.timeScale));
		//get water
		waterGot = part.RequestResource("Water", crewSize * dTime * ConfigSettings.ratesPerKerbal["Water"] /
			(secondsPerDayFactor / ConfigSettings.timeScale));
	}
}

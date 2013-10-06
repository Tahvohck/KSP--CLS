using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

class CLS_LivableArea : PartModule
{
	private readonly static BrokenPart.BreakType possibleBreaks =
		BrokenPart.BreakType.BadFilter |
		BrokenPart.BreakType.LeakOxygen |
		BrokenPart.BreakType.LeakWater;
	private BrokenPart.BreakType currentlyBroken = new BrokenPart.BreakType();


	[KSPEvent(active = false, guiActive = true, guiName = "Do repairs")]
	private void Repair() { }
	[KSPEvent(active = false, guiActive = true, guiName = "EVA repairs", externalToEVAOnly = true)]
	private void EVARepair() { Repair(); }


	private static void Awake() { CDebug.log("LivableArea Awoken."); }
	private static void Start() { CDebug.log("LivableArea started."); }


	public override void OnStart(StartState state) {
		if (HighLogic.LoadedSceneIsFlight)
			foreach (ProtoCrewMember crewMem in part.protoModuleCrew)
				if (!Backend.KerbalHealth.ContainsKey(crewMem.name))
					Backend.KerbalHealth[crewMem.name] = new KerbalBiometric();
	}


	public override void OnLoad(ConfigNode node) { CDebug.log(node.ToString()); }


	public void FixedUpdate() {
		int secondsPerDayFactor = 86400;
		double o2Want, snacksWant, waterWant;
		double o2Got, co2Made, snacksGot, waterGot;
		float dTime = TimeWarp.fixedDeltaTime;
		byte crewSize = (byte)part.protoModuleCrew.Count;

		//get oxygen
		o2Want = crewSize * dTime * ConfigSettings.ratesPerKerbal["Oxygen"] /
			(secondsPerDayFactor / ConfigSettings.timeScale);
		o2Got = part.RequestResource("Oxygen", o2Want);
		if (o2Got == 0) { //if this returned 0 it means there wasn't the amount we wanted.
			double amountLeft = Backend.getCurrentAmount("Oxygen");
			double percentOfWanted = (o2Want - amountLeft)/o2Want;
			o2Got = part.RequestResource("Oxygen", amountLeft);
			foreach (ProtoCrewMember crewMem in part.protoModuleCrew)
				if (Backend.KerbalHealth[crewMem.name].bloodstreamOxygen > 0) {
					Backend.KerbalHealth[crewMem.name].bloodstreamOxygen -= (dTime * percentOfWanted);
					CDebug.log(crewMem.name + " has " + Math.Floor(Backend.KerbalHealth[crewMem.name].bloodstreamOxygen) + " seconds to live.");
				}
				else {
					crewMem.Die();
					crewMem.seat.DespawnCrew();
				}
		}
		else
			foreach (ProtoCrewMember crewMem in part.protoModuleCrew)
				Backend.KerbalHealth[crewMem.name].resetOxygen();

		//add CO2
		co2Made = part.RequestResource("CO2", crewSize * dTime * ConfigSettings.ratesPerKerbal["CO2"] * (o2Got / o2Want) /
			(secondsPerDayFactor / ConfigSettings.timeScale));
		//(o2Got / o2Want) is to ensure that we only produce an equivalent amount of CO2 to the O2 we consumed.

		//get food
		//TODO I want this to eventually be a periodic thing instead of a constant.
		snacksWant = crewSize * dTime * ConfigSettings.ratesPerKerbal["Snacks"] /
			(secondsPerDayFactor / ConfigSettings.timeScale);
		snacksGot = part.RequestResource("Snacks", snacksWant);
		if (snacksGot == 0) { //if this returned 0 it means there wasn't the amount we wanted.
			double amountLeft = Backend.getCurrentAmount("Snacks");
			double percentOfWanted = (snacksWant - amountLeft)/snacksWant;
			snacksGot = part.RequestResource("Snacks", amountLeft);
			foreach (ProtoCrewMember crewMem in part.protoModuleCrew)
				if (Backend.KerbalHealth[crewMem.name].bloodstreamSnacks > 0)
					Backend.KerbalHealth[crewMem.name].bloodstreamSnacks -= (dTime * percentOfWanted);
				else
					crewMem.Die();
		}
		else
			foreach (ProtoCrewMember crewMem in part.protoModuleCrew)
				Backend.KerbalHealth[crewMem.name].resetSnacks();

		//get water
		waterGot = part.RequestResource("Water", crewSize * dTime * ConfigSettings.ratesPerKerbal["Water"] /
			(secondsPerDayFactor / ConfigSettings.timeScale));

	}
}

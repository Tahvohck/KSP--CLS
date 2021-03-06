﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

class CLS_LivableArea : BreakablePart
{
	
	protected override BrokenPart.BreakType possibleBreaks {
		get {
			return BrokenPart.BreakType.BadFilter | 
				BrokenPart.BreakType.LeakOxygen |
				BrokenPart.BreakType.LeakWater;
		}
	}


	[KSPEvent(active = false, guiActive = true, guiName = "Do repairs")]
	private void Repair() { }
	[KSPEvent(active = false, guiActive = true, guiName = "EVA repairs", externalToEVAOnly = true)]
	private void EVARepair() { Repair(); }


	private static void Awake() { CDebug.log("LivableArea Awoken."); }
	private static void Start() { CDebug.log("LivableArea started."); }
	public override void OnStart(StartState state) {
		if (HighLogic.LoadedSceneIsFlight) {
			Backend.regBreakablePart(this);
			foreach (ProtoCrewMember crewMem in part.protoModuleCrew)
				if (!Backend.KerbalHealth.ContainsKey(crewMem.name))
					Backend.KerbalHealth[crewMem.name] = new KerbalBiometric();
		}
	}
	public override void OnLoad(ConfigNode node) { CDebug.log(node.ToString()); }


	/// <summary>
	/// Fixed Update:
	/// Do resource consumption/creation - O2, CO2, food, water
	/// Do break chance tick [x]
	/// </summary>
	public void FixedUpdate() {
		int secondsPerDayFactor = 86400;
		double o2Want, snacksWant, waterWant;
		double o2Got, co2Made, snacksGot, waterGot;
		float dTime = TimeWarp.fixedDeltaTime;
		byte crewSize = (byte)part.protoModuleCrew.Count;

		if (crewSize > 0) {
			//get oxygen
			o2Want = crewSize * dTime * ConfigSettings.ratesPerKerbal["Oxygen"] /
				(secondsPerDayFactor / ConfigSettings.timeScale);
			o2Got = part.RequestResource("Oxygen", o2Want);
			if (o2Got == 0) { //if this returned 0 it means there wasn't the amount we wanted.
				double amountLeft = Backend.getCurrentAmount("Oxygen");
				double percentOfWanted = (o2Want - amountLeft) / o2Want;
				o2Got = part.RequestResource("Oxygen", amountLeft);
				foreach (ProtoCrewMember crewMem in part.protoModuleCrew)
					if (Backend.KerbalHealth[crewMem.name].bloodstreamOxygen > 0) {
						Backend.KerbalHealth[crewMem.name].bloodstreamOxygen -= (dTime * percentOfWanted);
						//CDebug.log(crewMem.name + " has " + Math.Floor(Backend.KerbalHealth[crewMem.name].bloodstreamOxygen) + " seconds to live.");
					}
					else
						Backend.KillKerbal(crewMem);
			}
			else
				foreach (ProtoCrewMember crewMem in part.protoModuleCrew)
					Backend.KerbalHealth[crewMem.name].resetOxygen();

			//add CO2
			co2Made = part.RequestResource("CO2", crewSize * dTime * ConfigSettings.ratesPerKerbal["CO2"] * (o2Got / o2Want) /
				(secondsPerDayFactor / ConfigSettings.timeScale));
			//(o2Got / o2Want) is to ensure that we only produce an equivalent amount of CO2 to the O2 we consumed.
			//TODO CO2 KILL
			if (part.Resources["CO2"].amount == part.Resources["CO2"].maxAmount)
				foreach (ProtoCrewMember crewMem in part.protoModuleCrew) {
					Backend.KillKerbal(crewMem);
				}

			//get food
			//TODO I want this to eventually be a periodic thing instead of a constant.
			snacksWant = crewSize * dTime * ConfigSettings.ratesPerKerbal["Snacks"] /
				(secondsPerDayFactor / ConfigSettings.timeScale);
			snacksGot = part.RequestResource("Snacks", snacksWant);
			if (snacksGot == 0) { //if this returned 0 it means there wasn't the amount we wanted.
				double amountLeft = Backend.getCurrentAmount("Snacks");
				double percentOfWanted = (snacksWant - amountLeft) / snacksWant;
				snacksGot = part.RequestResource("Snacks", amountLeft);
				foreach (ProtoCrewMember crewMem in part.protoModuleCrew)
					if (Backend.KerbalHealth[crewMem.name].bloodstreamSnacks > 0)
						Backend.KerbalHealth[crewMem.name].bloodstreamSnacks -= (dTime * percentOfWanted);
					else
						Backend.KillKerbal(crewMem);
			}
			else
				foreach (ProtoCrewMember crewMem in part.protoModuleCrew)
					Backend.KerbalHealth[crewMem.name].resetSnacks();


			//get water
			waterWant = crewSize * dTime * ConfigSettings.ratesPerKerbal["Water"] /
				(secondsPerDayFactor / ConfigSettings.timeScale);
			waterGot = part.RequestResource("Water", crewSize * dTime * ConfigSettings.ratesPerKerbal["Water"] /
				(secondsPerDayFactor / ConfigSettings.timeScale));
			if (waterGot == 0) {
				double amountLeft = Backend.getCurrentAmount("Water");
				double percentOfWanted = (waterWant - amountLeft) / waterWant;
				waterGot = part.RequestResource("Water", amountLeft);
				foreach (ProtoCrewMember crewMem in part.protoModuleCrew)
					if (Backend.KerbalHealth[crewMem.name].bloodstreamWater > 0)
						Backend.KerbalHealth[crewMem.name].bloodstreamWater -= (dTime * percentOfWanted);
					else 
						Backend.KillKerbal(crewMem);
			}
			else
				foreach (ProtoCrewMember crewMem in part.protoModuleCrew)
					Backend.KerbalHealth[crewMem.name].resetWater();
		}

		//Roll for breaks
	}
}



/// <summary>
/// The class from which all breakable CLS parts will derive. Helps to unify backend, theoretically.
/// Also should make sure that all breakable parts share the same setup.
/// </summary>
abstract class BreakablePart : PartModule {
	protected abstract BrokenPart.BreakType possibleBreaks { get; }
	protected BrokenPart.BreakType currentlyBroken = new BrokenPart.BreakType();


	/// <summary>
	/// Breaks in a random way, based on the possible break types.
	/// </summary>
	internal void BreakRandom() {
		List<BrokenPart.BreakType> possibleBreaksList = new List<BrokenPart.BreakType>();
		foreach (BrokenPart.BreakType bt in Enum.GetValues(typeof(BrokenPart.BreakType)))
			if ((possibleBreaks & bt) > 0)
				possibleBreaksList.Add(bt);

		BreakSpecific(possibleBreaksList[UnityEngine.Random.Range(0, possibleBreaksList.Count)]);
	}


	/// <summary>
	/// Breaks the current part in a specific way, but only if the part can break that way.
	/// </summary>
	internal bool BreakSpecific(BrokenPart.BreakType brkTyp) {
		currentlyBroken |= brkTyp & possibleBreaks;		//<currentlyBroken> += the intersection of <possibleBreaks> and <brkTyp>
		
		if (Backend.BrokenParts.ContainsKey(part.GetHashCode()))			//If already broken...
			Backend.BrokenParts[part.GetHashCode()].UpdateProblems(currentlyBroken);	//Update the break
		else
			Backend.regBrokenPart(new BrokenPart(part, currentlyBroken));	//Otherwise, register the new break
		
		return (brkTyp & possibleBreaks) > 0;
	}
}

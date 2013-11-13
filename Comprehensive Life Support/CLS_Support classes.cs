using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;


/// <summary>
/// Contains methods for broken parts, as well as an internal ENUM for the possible types of breaks.
/// </summary>
internal class BrokenPart
{
	readonly int id;
	readonly string partName;
	private BreakType problem;
	private float severity, sevMult;


	/// <param name="owner">Part that owns this break</param>
	/// <param name="problem">The type of problem</param>
	/// <param name="initialSeverity">The rate at which it causes a resource to be lost, if any.</param>
	/// <param name="severityMultiplier"></param>
	internal BrokenPart(Part owner, BreakType problem, float initialSeverity = -1, float severityMultiplier = 1.01f) {
		id = owner.GetHashCode();
		partName = owner.name;
		this.problem = problem;
		severity = initialSeverity;
		sevMult = severityMultiplier;
	}


	internal float Severity() {return severity;}

	internal BreakType Problems() { return problem; }


	/// <summary>Enumerator for types of breaks.
	/// </summary>
	[Flags]
	public enum BreakType : byte {
		LeakOxygen = 0x01,
		LeakCO2 = 0x02,
		LeakWater = 0x04,
		ElectronicsFailure = 0x08,
		BadFilter = 0x10
	}
}



/// <summary>
/// Used for keeping track of an inividual kerbal's biometrics.
/// </summary>
internal class KerbalBiometric {
	internal double
		bloodstreamOxygen = 180,	//Times (in seconds) until death for any of the resources.
		bloodstreamWater = 64800,	//Using the 3-3-3 rule. Not really accurate, but it's good enough for our purposes.
		bloodstreamSnacks = 453600;	//After all, if this kicks in you're already in trouble.

	internal void resetOxygen() { bloodstreamOxygen = 180; }
	internal void resetWater() { bloodstreamWater = 64800; }
	internal void resetSnacks() { bloodstreamSnacks = 453600; }
}



/// <summary>
/// This class handles all backend work that multiple different sources should be able to access 
/// (or at least shouldn't have to track themselves).
/// </summary>
class Backend
{
	internal static Dictionary<string, List<PartResource>> Resources;
	internal static Dictionary<string, double> ResourceMaximums;
	internal static Dictionary<string, int> ETTLs;
	internal static Dictionary<string, KerbalBiometric> KerbalHealth = new Dictionary<string,KerbalBiometric>();
	internal static Dictionary<string, Dictionary<int, double>> ResourceRates = new Dictionary<string,Dictionary<int,double>>();
	
	internal static List<BrokenPart> BrokenParts = new List<BrokenPart>();
	internal static List<BreakablePart> BreakableParts = new List<BreakablePart>();

	#region Initializers
	/// <summary>For initialization, non-flight-locked.
	/// </summary>
	internal static void Initialize() { }


	/// <summary>For initialization, flight-locked. 
	/// </summary>
	internal static void InitializeFlight(Vessel ves) {
		buildResourceTankLists(ves);
		InitETTLs();
	}


	/// <summary> Default the ETTLs. This may become obsolete.
	/// </summary>
	internal static void InitETTLs() {
		ETTLs = new Dictionary<string,int>();
		foreach (string s in ConfigSettings.CLSResourceNames)
			ETTLs[s] = 0;
	}
	#endregion


	/// <summary>Get the current amount of 'resName' on the ship.
	/// </summary><param name="resName"></param>
	internal static double getCurrentAmount(string resName) {
		double amount = 0;
		foreach (PartResource res in Resources[resName])
			amount += res.amount;
		return amount;
	}


	/// <summary>Register a resource production/consumption rate
	/// </summary>
	internal static void registerRate(int partID, string resName, double rate){
		ResourceRates[resName][partID] = rate;
	}
	internal static double getRate(string resname) {
		double rate = 0.0;
		foreach (KeyValuePair<int, double> kvp in ResourceRates[resname])
			rate += kvp.Value;
		return rate;
	}


	internal static void regBreakablePart(BreakablePart p) { BreakableParts.Add(p);	}
	internal static void regBrokenPart(BrokenPart broken) { BrokenParts.Add(broken); }


	/// <summary>
	/// Kill a kerbal and erase them from the ship.
	/// </summary>
	/// <param name="unluckyBastard">The unlucky bastard.</param>
	internal static void KillKerbal(ProtoCrewMember unluckyBastard) {
		unluckyBastard.Die();
		unluckyBastard.seat.part.RemoveCrewmember(unluckyBastard); 
		//Yes, this is ugly. No, I don't know how to get it to -actually- kill a Kerbal instead of just listing it as "killed".
		//Without doing this, the kerbal can still EVA, etc.
	}


	/// <summary>Pulse through vessel, discover resource tanks. Add them to appropriate list.
	/// This walk also determines the maximum stored amount of the resource.
	/// </summary>
	internal static void buildResourceTankLists(Vessel ves) {
		ResourceMaximums = new Dictionary<string, double>();
		Resources = new Dictionary<string, List<PartResource>>();

		foreach (Part p in ves.Parts)
			foreach (PartResource res in p.Resources)
				if (Resources.ContainsKey(res.resourceName)) {
					Resources[res.resourceName].Add(res);
					ResourceMaximums[res.resourceName] += res.maxAmount;
				}
				else {
					Resources[res.resourceName] = new List<PartResource> { res };
					ResourceMaximums[res.resourceName] = res.maxAmount;
				}
	}
}



/// <summary>
/// Debugging class. Generally Pre-compile-locked methods, designed to allow for easy debugging without having
/// to remove code later to reduce filesize and execution speed.
/// 
/// </summary>
class CDebug
{
#if DEBUG
	public static void log(string s) {
		UnityEngine.Debug.Log("[DEBUG][CLS]: " + s);
	}
#endif

#if VERBOSE
	public static void verbose(string s) {
		UnityEngine.Debug.Log("[VERBOSE][CLS]: " + s);
	}
#endif
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

internal class BrokenPart
{
	readonly int id;
	private BreakType problem;
	private float severity, sevMult;



	/// <param name="partHash">Hash of the part that owns this break</param>
	/// <param name="problem">The type of problem</param>
	/// <param name="initialSeverity">The rate at which it causes a resource to be lost, if any.</param>
	/// <param name="severityMultiplier"></param>
	internal BrokenPart(int partHash, BreakType problem, float initialSeverity = -1, float severityMultiplier = 1.01f) {
		id = partHash;
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
/// This class handles all backend work that multiple different sources should be able to access 
/// (or at least shouldn't have to track themselves).
/// </summary>
class Backend
{
	internal static Dictionary<string, List<PartResource>> Resources;
	internal static Dictionary<string, double> ResourceMaximums;
	internal static Dictionary<string, int> ETTLs;

	internal static List<BrokenPart> BrokenParts = new List<BrokenPart>();

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


	/// <summary> Default the ETTLs. This may become obsolete.
	/// </summary>
	internal static void InitETTLs() {
		ETTLs = new Dictionary<string,int>();
		foreach (string s in ConfigSettings.CLSResourceNames)
			ETTLs[s] = 0;
	}


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
	internal static void registerRate(string partID, string resName, double rate){
		CDebug.log("registerRate not implemented!");
	}


	/// <summary>For initialization, non-flight-locked.
	/// </summary>
	internal static void Initialize() { }

	/// <summary>For initialization, flight-locked. 
	/// </summary>
	internal static void InitializeFlight(Vessel ves) {
		buildResourceTankLists(ves);
		InitETTLs();
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

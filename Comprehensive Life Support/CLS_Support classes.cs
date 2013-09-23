using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class CLS_Resource
{
	public string name = "";
	public double ratePerKerbal = 0.0,
		amountOnEVA = 0.0,
		maxOnEVA = 0.0;
	public bool takenOnEVA = false;

	public CLS_Resource(string name, double rate) {
		this.name = name;
		this.ratePerKerbal = rate;
	}

	/// <summary>Constructor for an EVA resource. Note that if onEVA is false or amountOnEVA or maxOnEVA are null
	/// this defaults to acting exactly as the default (name, rate) constructor.
	/// </summary>
	/// <param name="name"></param><param name="rate"></param><param name="onEVA"></param>
	/// <param name="amountOnEVA"></param><param name="maxOnEVA"></param>
	public CLS_Resource(string name, double rate, bool onEVA, double amountOnEVA, double maxOnEVA) {
		this.name = name;
		this.ratePerKerbal = rate;
		if (onEVA || amountOnEVA != 0.0 || maxOnEVA != 0.0) {
			takenOnEVA = onEVA;
			if (amountOnEVA < maxOnEVA)
				this.amountOnEVA = amountOnEVA;
			else
				this.amountOnEVA = maxOnEVA;
			this.maxOnEVA = maxOnEVA;
		}
	}
}


class Backend 
{
	internal static Dictionary<string, List<PartResource>> Resources;
	internal static Dictionary<string, double> ResourceMaximums;
	internal static Dictionary<string, int> ETTLs;

	/// <summary>Pulse through vessel, discover resource tanks. Add them to appropriate list.
	/// This same walk also determines the maximum stored amount of the resource.
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


	internal static double getCurrentAmount(string resName) {
		double amount = 0;
		foreach (PartResource res in Resources[resName])
			amount += res.amount;
		return amount;
	}
}


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

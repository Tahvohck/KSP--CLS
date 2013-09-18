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
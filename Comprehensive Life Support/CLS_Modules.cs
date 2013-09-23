using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

class CLS_LivableArea : PartModule
{
	[KSPEvent(active=true, guiActive=true,guiName="Hide CLS")]
	private void hideGUI() {
		Events["showGUI"].active = true;
		Events["hideGUI"].active = false;
		CLS_FlightGui.hideStatusWindow();
	}
	[KSPEvent(active = true, guiActive = true, guiName = "Show CLS")]
	private void showGUI() {
		Events["showGUI"].active = false;
		Events["hideGUI"].active = true;
		CLS_FlightGui.showStatusWindow();
	}

	private static void Awake() {
		CDebug.log("LivableArea Awoken.");
	}
	private static void Start() {
		CDebug.log("LivableArea started.");
	}

	public override void OnLoad(ConfigNode node) {
		CDebug.log(node.ToString());
	}
}

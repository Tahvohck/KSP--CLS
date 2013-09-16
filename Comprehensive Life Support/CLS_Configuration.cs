//Version 0.1.1
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using UnityEngine;


[KSPAddon(KSPAddon.Startup.MainMenu, false)]
class CLS_Configuration : MonoBehaviour
{
	#region ACCESSIBLE MEMBERS
	internal static Dictionary<string, double> ratesPerKerbal = new Dictionary<string,double>();
	internal static List<string> CLSResources = new List<string>();
	#endregion
	#region PRIVATE MEMBERS
	private static string APPDIR = KSPUtil.ApplicationRootPath;
	private static string defaultConfigFile =
	#region defaultfile
@"#COMPREHENSIVE LIFE SUPPORT CONFIG

[RPK]
#All values are in <units> per six-hour day.
Oxygen	= 144
CO2		= -144
Water	= 8
Snacks	= 3
[/RPK]";
	#endregion
	private static string configFilePath;
	private static string defaultResourceFile =
	#region defaultfile
@"RESOURCE_DEFINITION
{
	name = Oxygen
	density = .000001429
	flowMode = ALL_VESSEL
	transfer = PUMP
}
RESOURCE_DEFINITION
{
	name = CO2
	density = 0.000001977
	flowMode = ALL_VESSEL
	transfer = PUMP
}
RESOURCE_DEFINITION
{
	name = Water
	density = .001
	flowMode = ALL_VESSEL
	transfer = PUMP
}
RESOURCE_DEFINITION
{
	name = Snacks
	density = .006
	flowMode = ALL_VESSEL
	transfer = PUMP
}";
	#endregion
	private static string resourceFilePath;
	#endregion




	#region INIT FUNCTIONS
	private void Awake() {
		APPDIR = APPDIR.Replace('\\', '/');
		APPDIR = APPDIR.Replace("KSP_Data/../", "");
		configFilePath = APPDIR + "GameData/Tahvohck/Comprehensive Life Support.cfg";
		resourceFilePath = APPDIR + "GameData/Tahvohck/Resources/CLSResources.cfg";
		CDebug.log("CLS_Config Awakes.");
	}


	private void Start() {
		if (!System.IO.File.Exists(configFilePath)) {
			System.IO.File.WriteAllText(configFilePath, defaultConfigFile);	}
		if (!System.IO.File.Exists(resourceFilePath)) {
			System.IO.File.WriteAllText(resourceFilePath, defaultResourceFile);	}
		LoadCLSResources();
		LoadConfig();
	}
	#endregion


	#region SAVE FUNCTIONS
	internal static void SaveConfig() {
		CDebug.log("Saver not implemented.");
		string toSave = "";
		toSave += SaveRPKs();
	}


	private static string SaveRPKs() {
		CDebug.log("RPK saver not implemented.");
		return "";
	}
	#endregion


	#region LOAD FUNCTIONS
	private static async Task LoadCLSResources() {
		using (StreamReader sr = new StreamReader(configFilePath)) {
			CDebug.log("Resource loader not implemented.");
			string line = "";
			string[] parts;
			string[] delimiters = { "=", "\t", " " };

			line = await sr.ReadLineAsync();
			while ((line != null) {
			}
		}
	}


	internal static void LoadConfig() {
		using (StreamReader sr = new StreamReader(configFilePath)) {
			string line = "";
			string[] parts;
			string[] delimiters = {"=", "\t", " "};

			while ((line = sr.ReadLine()) != null) {
				if (line.Trim().StartsWith("#")) { continue; }	//Skip comments.
				else if (line.Contains("[RPK]")) {
					line = sr.ReadLine();
					while (!line.Contains("[/RPK]")) {			//Until the closing tag is found...
						if (line.StartsWith("#")) { line = sr.ReadLine(); continue; }		//Skip comments.
						parts = line.Split(delimiters, 2, StringSplitOptions.RemoveEmptyEntries);
						ratesPerKerbal.Add(parts[0], double.Parse(parts[1]));
						line = sr.ReadLine();
					}
				}
				else { } //Load other things! New things! Not implemented things!
			}
			foreach (KeyValuePair<string, double> KVP in ratesPerKerbal)
				CDebug.log(KVP.Key + ": " + KVP.Value);
		}
	}
	#endregion
}

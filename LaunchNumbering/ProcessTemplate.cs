using KSP.UI.Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Text.RegularExpressions;



namespace LaunchNumbering
{
    [KSPAddon(KSPAddon.Startup.FlightEditorAndKSC, true)]
    partial class LaunchNumbererMono : MonoBehaviour
    {
        internal static LaunchNumbererMono Instance;

        internal static string PLUGINDATA;
        internal static string DEFAULTDATA;

        void Awake()
        {
            PLUGINDATA = KSPUtil.ApplicationRootPath + "GameData/LaunchNumbering/PluginData/templates.dat";
            DEFAULTDATA = KSPUtil.ApplicationRootPath + "GameData/LaunchNumbering/PluginData/default.dat";

            _numbering = new Dictionary<string, Dictionary<int, Bloc>>();
            GameEvents.OnVesselRollout.Add(RenameVessel);
            Instance = this;
            DontDestroyOnLoad(this);
        }
        //internal const string defaultTemplate = "{[name]}{-[launchNumber]}{ (Bloc [blocNumber])}";

        public void OnDestroy()
        {
            GameEvents.OnVesselRollout.Remove(RenameVessel);
            Instance = null;
        }

        internal Dictionary<string, Dictionary<int, Bloc>> _numbering;

        public void RenameVessel(ShipConstruct sc)
        {
            //Debug.Log("RenameVessel, vessel.landedAt: " + FlightGlobals.ActiveVessel.landedAt);
            var settings = HighLogic.CurrentGame.Parameters.CustomParams<LNSettings>();
            if ((settings.activeOnLaunchpad &&
                (FlightGlobals.ActiveVessel.landedAt == "LaunchPad" ||
                 FlightGlobals.ActiveVessel.landedAt == "Woomerang_Launch_Site" ||
                 FlightGlobals.ActiveVessel.landedAt == "Desert_Launch_Site")) ||
                (settings.activeOnRunway &&
                 (FlightGlobals.ActiveVessel.landedAt == "Runway" ||
                  FlightGlobals.ActiveVessel.landedAt == "Island_Airfield" ||
                  FlightGlobals.ActiveVessel.landedAt == "Desert_Airfield")) ||
                (settings.activeOnExternalLaunchpad && FlightGlobals.ActiveVessel.landedAt == "External LaunchPad")
                )
            {
                Vessel v = FlightGlobals.ActiveVessel;
                int vesselHash;
                if (LaunchNumberer.Instance.SelectedTemplate.Contains("blocNumber"))
                    vesselHash = 1;
                else
                    vesselHash = ComputeVesselHash(v);

                var vesselNumber = 1;
                var blocNumber = 1;
                if (!settings.addAlways && Char.IsDigit(v.vesselName[v.vesselName.Length - 1])) return;
                if (!_numbering.ContainsKey(v.vesselName))
                {
                    _numbering.Add(v.vesselName, new Dictionary<int, Bloc>());
                }
                var nDict = _numbering[v.vesselName];
                Bloc b;


                if (vesselHash != 1 && nDict.Count == 1 && nDict.ContainsKey(1))
                {
                    //Upgrade from earlier version
                    b = nDict[1];
                    nDict.Remove(1);
                    nDict.Add(vesselHash, b);
                }
                if (!nDict.ContainsKey(vesselHash))
                {
                    blocNumber = nDict.Count + 1;
                    b = InitializeNewBloc(blocNumber);
                    nDict.Add(vesselHash, b);
                }
                else
                {
                    b = nDict[vesselHash];
                    vesselNumber = b.vessel + 1;
                    b.vessel = vesselNumber;
                    blocNumber = b.blocNumber;
                }

                v.vesselName = ProcessTemplate(v, b, vesselNumber, blocNumber);
            }
        }

        internal static Bloc InitializeNewBloc(int blocNumber)
        {
            var settings = HighLogic.CurrentGame.Parameters.CustomParams<LNSettings>();
            return new Bloc
            {
                vessel = 1,
                blocNumber = blocNumber,
                showBloc = settings.ShowBloc,
                addAlways = settings.addAlways,
                addBlocAlways = settings.addBlocAlways,

                vesselRoman = settings.Scheme == LNSettings.NumberScheme.Roman,
                blocRoman = settings.BlocScheme == LNSettings.NumberScheme.Roman
            };
        }

        private static int ComputeVesselHash(Vessel v)
        {
            int nameCounter = 0;
            var names = new Dictionary<string, int>();
            var treeItems = new List<int>();
            foreach (var p in v.Parts)
            {
                if (!names.ContainsKey(p.name)) names.Add(p.name, ++nameCounter);
                treeItems.Add(names[p.name]);
                if (p.children.Count > 0)
                {
                    treeItems.Add(-1);
                    foreach (var c in p.children)
                    {
                        if (!names.ContainsKey(c.name)) names.Add(c.name, ++nameCounter);
                        treeItems.Add(names[c.name]);
                    }
                    treeItems.Add(-2);
                }
            }
            return QuickHash(treeItems);
        }
        private static int QuickHash(List<int> items)
        {
            int hashCode = items.Count;
            foreach (int item in items)
            {
                hashCode = unchecked(hashCode * 314159 + item);
            }
            return hashCode;
        }

        private readonly int[] _RValues = new int[] { 1000, 900, 500, 400, 100, 90, 50, 40, 10, 9, 5, 4, 1 };
        private readonly string[] _RStrings = new string[] { "M", "CM", "D", "CD", "C", "XC", "L", "XL", "X", "IX", "V", "IV", "I" };

        public string ToRoman(int value)
        {
            var rindex = 0;
            var result = string.Empty;
            while (value > 0)
            {
                while (_RValues[rindex] > value)
                {
                    rindex++;
                }
                value = value - _RValues[rindex];
                result = result + _RStrings[rindex];
            }
            return result;
        }




        internal string ProcessTemplate(string template, string vesselName, Bloc b, int vesselNumber, int blocNumber)
        {
#if true
            Debug.Log("ProcessTemplate, template: " + template);
            Debug.Log("vesselName: " + vesselName);
            Debug.Log("Bloc: " + b);
            Debug.Log("vesselNumber: " + vesselNumber);
            Debug.Log("blocNumber: " + blocNumber);
#endif

            var settings = HighLogic.CurrentGame.Parameters.CustomParams<LNSettings>();

            string name = "";

            string outerPattern = "{.*?}";
            var sections = Regex.Split(template, "(" + outerPattern + ")");

            foreach (string section in sections)
            {
                Debug.Log("section: " + section);
                if (section != "")
                {
                    string s = section;
                    if (s.Contains("[name]") || s.Contains("[launchNumber]") || s.Contains("blocNumber]"))
                    {
                        if (s.Contains("[name]"))
                        {
                            s = s.Replace("[name]", vesselName);
                        }
                        if (vesselNumber > 1 || b.addAlways)
                        {
                            s = s.Replace("[launchNumber]", (b.vesselRoman ? ToRoman(vesselNumber) : vesselNumber.ToString("D" + (settings.launchNumberMinDigits).ToString())));
                        }

                        if (b.showBloc && (b.addBlocAlways || blocNumber > 1))
                        {
                            s = s.Replace("[blocNumber]", (b.blocRoman ? ToRoman(blocNumber) : blocNumber.ToString("D" + (settings.blocNumberMinDigits).ToString())));
                        }

                        if (s != section)
                            name += s.Substring(1, s.Length - 2);

                    }
                    else
                    {
                        name += s;
                    }
                }
            }
            Debug.Log("name: " + name);
            return name;
        }

        internal string ProcessTemplate(Vessel v, Bloc b, int vesselNumber, int blocNumber)
        {
            string s = ProcessTemplate(LaunchNumberer.Instance.SelectedTemplate, v.vesselName, b, vesselNumber, blocNumber);
            //Debug.Log("template: " + SelectTemplate.selectedTemplate);
            //Debug.Log("vesselName: " + s);

            return s;

        }
    }
}
using KSP.UI.Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Text.RegularExpressions;



namespace LaunchNumbering
{
    partial class LaunchNumberer
    {
        static string PLUGINDATA = KSPUtil.ApplicationRootPath + "GameData/LaunchNumbering/PluginData/templates.dat";

        const string defaultTemplate = "{[name]}{-[launchNumber]}{ (Bloc [blocNumber])}";

        string ProcessTemplate(string template, Vessel v, Bloc b, int vesselNumber, int blocNumber)
        {
            var settings = HighLogic.CurrentGame.Parameters.CustomParams<LNSettings>();
            string name = "";

            string outerPattern = "{.*?}";
            var sections = Regex.Split(template, "(" + outerPattern + ")");

            foreach (string section in sections)
                if (section != "")
            {
                string s = section;
                if (s.Contains("[name]") || s.Contains("[launchNumber]") || s.Contains("blocNumber]"))
                {
                    s = s.Replace("[name]", v.vesselName);
                    if (vesselNumber > 1 || settings.addAlways)
                        s = s.Replace("[launchNumber]", (b.vesselRoman ? ToRoman(vesselNumber) : vesselNumber.ToString("N" + (1 +settings.launchNumberMinDigits).ToString())));

                    if (blocNumber > 1 && b.showBloc)
                    {
                        s = s.Replace("[blocNumber]", (b.blocRoman ? ToRoman(blocNumber) : blocNumber.ToString("N" + (1 +settings.blocNumberMinDigits).ToString())));
                    }
                    if (s != section)
                        name += s.Substring(1, s.Length - 2);
                }
                else
                {
                    name += s;
                }
            }

            return name;
        }

        string ProcessTemplate(Vessel v, Bloc b, int vesselNumber, int blocNumber)
        {
            string t = defaultTemplate;

            if (System.IO.File.Exists(PLUGINDATA))
            {
                var templatedata = System.IO.File.ReadAllLines(PLUGINDATA);
                // For now, only use the first line
                if (templatedata.Count() > 0)
                    t = templatedata[0];
            }

            string s = ProcessTemplate(t, v, b, vesselNumber, blocNumber);
            Debug.Log("template: " + t);
            Debug.Log("vesselName: " + s);

            return s;

        }
    }
}
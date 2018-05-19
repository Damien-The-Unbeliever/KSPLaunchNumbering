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
        internal static string PLUGINDATA = KSPUtil.ApplicationRootPath + "GameData/LaunchNumbering/PluginData/templates.dat";
        internal static string DEFAULTDATA = KSPUtil.ApplicationRootPath + "GameData/LaunchNumbering/PluginData/default.dat";

        //internal const string defaultTemplate = "{[name]}{-[launchNumber]}{ (Bloc [blocNumber])}";

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
                        s = s.Replace("[launchNumber]", (b.vesselRoman ? ToRoman(vesselNumber) : vesselNumber.ToString("D" + (settings.launchNumberMinDigits).ToString())));

                    if (blocNumber > 1 && b.showBloc)
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

            return name;
        }

        string ProcessTemplate(Vessel v, Bloc b, int vesselNumber, int blocNumber)
        {
            string s = ProcessTemplate(SelectTemplate.selectedTemplate, v, b, vesselNumber, blocNumber);
            //Debug.Log("template: " + SelectTemplate.selectedTemplate);
            //Debug.Log("vesselName: " + s);

            return s;

        }
    }
}
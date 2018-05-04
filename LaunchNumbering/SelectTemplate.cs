using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.UI.Screens;

using ToolbarControl_NS;
using ClickThroughFix;

namespace LaunchNumbering
{
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    class SelectTemplate : MonoBehaviour
    {
        const string DEFAULT_TEMPLATE = "{[name]}{-[launchNumber]}{ (Bloc [blocNumber])}";

        internal const string MODID = "LaunchNumbering_NS";
        internal const string MODNAME = "Launch Numbering Templates";
        ToolbarControl toolbarControl;
        bool GUIEnabled = false;

        Rect WindowRect;
        Vector2 scrollVector;
        const int HEIGHT = 400;
        const int WIDTH = 400;

        void Start()
        {
            AddToolbarButton();
            WindowRect = new Rect(Screen.width - WIDTH - 70, Screen.height - HEIGHT - 40, WIDTH, HEIGHT);
        }

        void AddToolbarButton()
        {

            toolbarControl = gameObject.AddComponent<ToolbarControl>();
            toolbarControl.AddToAllToolbars(ToggleGUI, ToggleGUI,
                ApplicationLauncher.AppScenes.SPACECENTER | ApplicationLauncher.AppScenes.FLIGHT |
                ApplicationLauncher.AppScenes.MAPVIEW | ApplicationLauncher.AppScenes.SPH | ApplicationLauncher.AppScenes.VAB |
                ApplicationLauncher.AppScenes.TRACKSTATION,
                MODID,
                "launchNumberingButton",
                "LaunchNumbering/PluginData/Textures/LaunchNumbering-38",
                "LaunchNumbering/PluginData/Textures/LaunchNumbering-24",
                MODNAME
            );
        }

        void ToggleGUI()
        {
            GUIEnabled = !GUIEnabled;
        }

        void OnGUI()
        {
            if (GUIEnabled)
                WindowRect = ClickThruBlocker.GUILayoutWindow(4946386, WindowRect, DoWindow, "Launch Numbering Templates");
        }

        public class Template
        {
            public string name;
            public string template;

            public Template(string Name, string Template)
            {
                name = Name;
                template = Template;
            }
        }
        List<Template> templatelist = new List<Template>();
        internal static string selectedTemplateName = "Default";
        internal static string selectedTemplate = DEFAULT_TEMPLATE;
        bool showTemplate = false;

        void LoadData()
        {
            if (System.IO.File.Exists(LaunchNumberer.PLUGINDATA))
            {
                var t = System.IO.File.ReadAllLines(LaunchNumberer.PLUGINDATA);
                templatelist.Clear();
                foreach (var l in t)
                {
                    if (l.Length > 0 && l.Substring(0,1) != "#")
                    {                        
                        var name = l.Substring(0, l.IndexOf(','));
                        var template = l.Substring(l.IndexOf(',') + 1);
                        templatelist.Add(new Template(name, template));
                    }
                }
            }
            else
            {
                templatelist.Clear();
            }
        }
        void DoWindow(int id)
        {
            if (templatelist.Count == 0)
                LoadData();

            GUILayout.BeginHorizontal();
            showTemplate = GUILayout.Toggle(showTemplate, "Show full template");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (showTemplate)
                GUILayout.Label("Current Template: " + selectedTemplate);
            else
                GUILayout.Label("Current Template: " + selectedTemplateName);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();


            scrollVector = GUILayout.BeginScrollView(scrollVector);
            string displayText = "";
            foreach (var t in templatelist)
            {
                GUILayout.BeginHorizontal();

                if (showTemplate)
                    displayText = t.template;
                else
                    displayText = t.name;
                if (GUILayout.Button(displayText))
                {
                    selectedTemplate = t.template;
                    selectedTemplateName = t.name;
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Close"))
            {
                GUIEnabled = false;
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUI.DragWindow();
        }
    }
}

//using UnityEngine;
//using ToolbarControl_NS;

namespace LaunchNumbering
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class RegisterToolbar : MonoBehaviour
    {
        void Start()
        {
            ToolbarControl.RegisterMod(SelectTemplate.MODID, SelectTemplate.MODNAME);
        }
    }
}
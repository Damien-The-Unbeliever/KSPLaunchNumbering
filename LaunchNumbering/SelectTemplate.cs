using System.Collections.Generic;
using UnityEngine;
using KSP.UI.Screens;

using ToolbarControl_NS;
using ClickThroughFix;

namespace LaunchNumbering
{
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    class SelectTemplate : MonoBehaviour
    {

        internal const string MODID = "LaunchNumbering_NS";
        internal const string MODNAME = "Launch Numbering Templates";
        ToolbarControl toolbarControl;
        bool GUIEnabled = false;

        Rect WindowRect;
        Vector2 scrollVector;
        const int HEIGHT = 300;
        const int WIDTH = 400;

        void Start()
        {
            AddToolbarButton();
            WindowRect = new Rect(Screen.width - WIDTH - 70, Screen.height - HEIGHT - 40, WIDTH, HEIGHT);
            LoadData(true);
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
            {

                if (!HighLogic.CurrentGame.Parameters.CustomParams<LNSettings>().useAltSkin)
                    GUI.skin = HighLogic.Skin;
                WindowRect = ClickThruBlocker.GUILayoutWindow(4946386, WindowRect, DoWindow, "Launch Numbering Templates");
            }
        }

        public class Template
        {
            public string name;
            public string template;
            public string example;
            public Template(string Name, string Template)
            {
                name = Name;
                template = Template;
                MakeExample();
            }
            internal void MakeExample()
            {
                Bloc b = LaunchNumbererMono.InitializeNewBloc(1);

                example = LaunchNumbererMono.Instance.ProcessTemplate(template, "Ship Name", b, 1, 1);
            }
        }
        List<Template> templatelist = new List<Template>();
        bool showTemplate = false;
        bool showOptions = false;

        void LoadData(bool setDefault = false)
        {
            //ReadDefault();
            if (System.IO.File.Exists(LaunchNumbererMono.PLUGINDATA))
            {
                var t = System.IO.File.ReadAllLines(LaunchNumbererMono.PLUGINDATA);
                templatelist.Clear();
                foreach (var l in t)
                {
                    if (l.Length > 0 && l.Substring(0,1) != "#")
                    {                        
                        var name = l.Substring(0, l.IndexOf(','));
                        var template = l.Substring(l.IndexOf(',') + 1);
                        templatelist.Add(new Template(name, template));

                        if (setDefault && name == LaunchNumberer.Instance.selectedTemplateName)
                            LaunchNumberer.Instance.SelectedTemplate = template;
                    }
                }
            }
            else
            {
                templatelist.Clear();
            }
        }
#if false
        void ReadDefault()
        {
            if (System.IO.File.Exists(LaunchNumberer.DEFAULTDATA))
                selectedTemplateName = System.IO.File.ReadAllText(LaunchNumberer.DEFAULTDATA);
        }
        void SaveDefault()
        {
            System.IO.File.WriteAllText(LaunchNumberer.DEFAULTDATA, selectedTemplateName);
        }
#endif
        void DoWindow(int id)
        {
            if (templatelist.Count == 0)
                LoadData();

            GUILayout.BeginHorizontal();
            showTemplate = GUILayout.Toggle(showTemplate, "Show full template");
            GUILayout.FlexibleSpace();
            showOptions = GUILayout.Toggle(showOptions, "Show options", GUILayout.Width(200));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            if (showOptions)
            {
                var settings = HighLogic.CurrentGame.Parameters.CustomParams<LNSettings>();

                GUILayout.BeginHorizontal();                
                settings.ShowBloc = GUILayout.Toggle(settings.ShowBloc, "Show Bloc numbers", GUILayout.Width(175));
                //GUILayout.FlexibleSpace();
                settings.addBlocAlways = GUILayout.Toggle(settings.addBlocAlways, "Add Bloc number always", GUILayout.Width(175));
                //GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                settings.addToFirstVessel = GUILayout.Toggle(settings.addToFirstVessel, "Add to first vessel", GUILayout.Width(175));
                //GUILayout.FlexibleSpace();
                settings.addAlways = GUILayout.Toggle(settings.addAlways, "Add number always", GUILayout.Width(175));
                //GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                
            }
            GUILayout.BeginHorizontal();
            if (showTemplate)
                GUILayout.Label("Current Template: " + LaunchNumberer.Instance.SelectedTemplate);
            else
                GUILayout.Label("Current Template: " + LaunchNumberer.Instance.selectedTemplateName);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();


            scrollVector = GUILayout.BeginScrollView(scrollVector);
            string displayText = "";
            foreach (var t in templatelist)
            {
                if (showOptions)
                    t.MakeExample();
                GUILayout.BeginHorizontal();

                if (showTemplate)
                    displayText = t.template;
                else
                    displayText = t.name + "     " + t.example;
                if (GUILayout.Button(displayText))
                {
                    LaunchNumberer.Instance.SelectedTemplate = t.template;
                    LaunchNumberer.Instance.selectedTemplateName = t.name;
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Close"))
            {
                //SaveDefault();
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
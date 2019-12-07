using KSP.UI.Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LaunchNumbering
{
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, new GameScenes[] { GameScenes.SPACECENTER, GameScenes.EDITOR, GameScenes.FLIGHT })]
    public partial class LaunchNumberer : ScenarioModule
    {
        private const string TopLevelNodeLabel = "LAUNCHNUMBERS";
        private const string SeriesNodeLabel = "SERIES";
        private const string BlocNodeLabel = "BLOC";
        private const string BlocNameLabel = "name";
        private const string VesselHashLabel = "vessel-hash";
        private const string BlocNumberLabel = "bloc-number";
        private const string VesselCountLabel = "vessel-count";
        private const string ShowBlocLabel = "bloc-shown";
        private const string BlocRomanLabel = "bloc-roman";
        private const string VesselRomanLabel = "vessel-roman";
        private const float MessageDisplayLength = 5.0f;
        private const string PreferredTemplate = "preferred-template";

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            if (LaunchNumbererMono.Instance == null || LaunchNumbererMono.Instance._numbering == null)
                return;
            node.TryGetValue(PreferredTemplate, ref SelectTemplate.selectedTemplateName);
            LaunchNumbererMono.Instance._numbering = new Dictionary<string, Dictionary<int, Bloc>>();
            foreach (var serNode in node.GetNodes(SeriesNodeLabel))
            {
                var blocs = new Dictionary<int, Bloc>();
                LaunchNumbererMono.Instance._numbering.Add(serNode.GetValue(BlocNameLabel), blocs);
                foreach (var blocNode in serNode.GetNodes(BlocNodeLabel))
                {
                    blocs.Add(int.Parse(blocNode.GetValue(VesselHashLabel)), new Bloc
                    {
                        blocNumber = int.Parse(blocNode.GetValue(BlocNumberLabel)),
                        vessel = int.Parse(blocNode.GetValue(VesselCountLabel)),
                        blocRoman = bool.Parse(blocNode.GetValue(BlocRomanLabel)),
                        vesselRoman = bool.Parse(blocNode.GetValue(VesselRomanLabel)),
                        showBloc = bool.Parse(blocNode.GetValue(ShowBlocLabel))
                    });
                }
            }
        }
        public override void OnSave(ConfigNode node)
        {
            if (LaunchNumbererMono.Instance != null && LaunchNumbererMono.Instance._numbering != null)
            {
                node.ClearNodes();
                node.AddValue(PreferredTemplate, SelectTemplate.selectedTemplateName);
                foreach (var series in LaunchNumbererMono.Instance._numbering)
                {
                    var serNode = new ConfigNode(SeriesNodeLabel);
                    serNode.AddValue(BlocNameLabel, series.Key);
                    foreach (var bloc in series.Value)
                    {
                        var blocNode = new ConfigNode(BlocNodeLabel);
                        blocNode.AddValue(VesselHashLabel, bloc.Key);
                        blocNode.AddValue(VesselCountLabel, bloc.Value.vessel);
                        blocNode.AddValue(BlocNumberLabel, bloc.Value.blocNumber);
                        blocNode.AddValue(BlocRomanLabel, bloc.Value.blocRoman);
                        blocNode.AddValue(VesselRomanLabel, bloc.Value.vesselRoman);
                        blocNode.AddValue(ShowBlocLabel, bloc.Value.showBloc);
                        serNode.AddNode(blocNode);
                    }
                    node.AddNode(serNode);
                }
            }
            base.OnSave(node);
        }

    }
}

using KSP.UI.Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LaunchNumbering
{
	[KSPScenario(ScenarioCreationOptions.AddToAllGames, new GameScenes[] { GameScenes.SPACECENTER, GameScenes.EDITOR, GameScenes.FLIGHT })]
	public class LaunchNumberer : ScenarioModule
	{
		public override void OnAwake()
		{
			_numbering = new Dictionary<string, Dictionary<int, Bloc>>();
			GameEvents.OnVesselRollout.Add(RenameVessel);
		}


		public void OnDestroy()
		{
			GameEvents.OnVesselRollout.Remove(RenameVessel);
		}

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

		public override void OnLoad(ConfigNode node)
		{
			_numbering = new Dictionary<string, Dictionary<int, Bloc>>();
			foreach(var serNode in node.GetNodes(SeriesNodeLabel))
			{
				var blocs = new Dictionary<int, Bloc>();
				_numbering.Add(serNode.GetValue(BlocNameLabel), blocs);
				foreach(var blocNode in serNode.GetNodes(BlocNodeLabel))
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
			node.ClearNodes();
			foreach (var series in _numbering)
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

		private Dictionary<string, Dictionary<int, Bloc>> _numbering;

		public void RenameVessel(ShipConstruct sc)
		{
			Vessel v = FlightGlobals.ActiveVessel;
			int vesselHash = ComputeVesselHash(v);
			var vesselNumber = 1;
			var blocNumber = 1;
			if (Char.IsDigit(v.vesselName[v.vesselName.Length - 1])) return;
			if (!_numbering.ContainsKey(v.vesselName))
			{
				_numbering.Add(v.vesselName, new Dictionary<int, Bloc>());
			}
			var nDict = _numbering[v.vesselName];
			Bloc b;
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
			string addition = string.Empty;
			if (vesselNumber > 1)
			{
				addition = addition + " " + (b.vesselRoman ? ToRoman(vesselNumber) : vesselNumber.ToString());
			}
			if (blocNumber > 1&&b.showBloc)
			{
				addition = addition + " (Bloc " + (b.blocRoman ? ToRoman(blocNumber) : blocNumber.ToString()) + ")";
			}

			if (!string.IsNullOrEmpty(addition))
			{
				var oldName = v.vesselName;
				var newName = oldName + addition;
				v.vesselName = newName;
				ScreenMessages.PostScreenMessage("Launch: " + newName, MessageDisplayLength, ScreenMessageStyle.UPPER_CENTER);
			}
			else
			{
				ScreenMessages.PostScreenMessage("Initial launch", MessageDisplayLength, ScreenMessageStyle.UPPER_CENTER);
			}
		}

		private static Bloc InitializeNewBloc(int blocNumber)
		{
			var settings = HighLogic.CurrentGame.Parameters.CustomParams<LNSettings>();
			return new Bloc {
				vessel = 1,
				blocNumber = blocNumber,
				showBloc = settings.ShowBloc,
				vesselRoman = settings.Scheme== LNSettings.NumberScheme.Roman,
				blocRoman = settings.BlocScheme==LNSettings.NumberScheme.Roman
			};
		}

		private static int ComputeVesselHash(Vessel v)
		{
			return 1;
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

	}
}

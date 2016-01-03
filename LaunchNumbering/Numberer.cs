using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LaunchNumbering
{
	[KSPAddon(KSPAddon.Startup.Flight, false)]
	public class Numberer : MonoBehaviour
	{
		public void Awake()
		{
			//Called first - load settings?
			GameEvents.onGameStateSave.Add(HandleSave);
			GameEvents.onGameStateLoad.Add(HandleLoad);
		}

		private const string TopLevelNodeLabel = "LAUNCHNUMBERS";
		private const string SeriesNodeLabel = "SERIES";
		private const string BlocNodeLabel = "BLOC";
		private const string BlocNameLabel = "name";
		private const string VesselHashLabel = "vessel-hash";
		private const string BlocNumberLabel = "bloc-number";
		private const string VesselCountLabel = "vessel-count";
		private const string VesselNodeLabel = "VESSEL";
		private const string VesselIdLabel = "id";

		public void HandleLoad(ConfigNode node)
		{
			_vessels = new List<Guid>();
			_numbering = new Dictionary<string, Dictionary<int, Bloc>>();
			if (!node.HasNode(TopLevelNodeLabel)) return;
			ConfigNode LNN = node.GetNode(TopLevelNodeLabel);
			foreach(var serNode in LNN.GetNodes(SeriesNodeLabel))
			{
				var blocs = new Dictionary<int, Bloc>();
				_numbering.Add(serNode.GetValue(BlocNameLabel), blocs);
				foreach(var blocNode in serNode.GetNodes(BlocNodeLabel))
				{
					blocs.Add(int.Parse(blocNode.GetValue(VesselHashLabel)), new Bloc
					{
						blocNumber = int.Parse(blocNode.GetValue(BlocNumberLabel)),
						vessel = int.Parse(blocNode.GetValue(VesselCountLabel))
					});
				}
			}
			foreach(var vesNode in LNN.GetNodes(VesselNodeLabel))
			{
				_vessels.Add(new Guid(vesNode.GetValue(VesselIdLabel)));
			}
		}
		public void HandleSave(ConfigNode node)
		{
			ConfigNode LNN;
			if (node.HasNode(TopLevelNodeLabel))
			{
				LNN = node.GetNode(TopLevelNodeLabel);
			}
			else {
				LNN = new ConfigNode(TopLevelNodeLabel);
				node.AddNode(LNN);
			}
			LNN.ClearNodes();
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
					serNode.AddNode(blocNode);
				}
				LNN.AddNode(serNode);
			}
			foreach(var vessel in _vessels)
			{
				var vesNode = new ConfigNode(VesselNodeLabel);
				vesNode.AddValue(VesselIdLabel, vessel);
				LNN.AddNode(vesNode);
			}
		}

		private List<Guid> _vessels = new List<Guid>();
		private Dictionary<string, Dictionary<int, Bloc>> _numbering = new Dictionary<string, Dictionary<int, Bloc>>();

		public void FixedUpdate()
		{
			var v = FlightGlobals.ActiveVessel;
			if (ReferenceEquals(v, null)) return;
			if (ReferenceEquals(_vessels, null)) return;
			if (_vessels.Contains(v.id)) return;
			_vessels.Add(v.id);
			int vesselHash;
			unchecked
			{
				vesselHash = v.parts.Aggregate(32767,
					(a, p) => (a << 3 | ((a >> 29) & 0x7)) ^ p.partName.GetHashCode() ^
					p.editorLinks.Aggregate(19, (a2, p2) => (a2 << 3 | ((a2 >> 29) & 0x7)) ^ p2.partName.GetHashCode()));
			}
			var vesselNumber = 1;
			var blocNumber = 1;
			if (!_numbering.ContainsKey(v.vesselName))
			{
				_numbering.Add(v.vesselName, new Dictionary<int, Bloc>());
			}
			var nDict = _numbering[v.vesselName];
			if (!nDict.ContainsKey(vesselHash))
			{
				blocNumber = nDict.Count + 1;
				nDict.Add(vesselHash, new Bloc { vessel = 1, blocNumber = blocNumber });
			}
			else
			{
				Bloc b = nDict[vesselHash];
				vesselNumber = b.vessel + 1;
				b.vessel = vesselNumber;
				blocNumber = b.blocNumber;
			}
			string addition = string.Empty;
			if (vesselNumber > 1)
			{
				addition = addition + " " + vesselNumber.ToString();
			}
			if (blocNumber > 1)
			{
				addition = addition + " (Bloc " + ToRoman(blocNumber) + ")";
			}

			if (!string.IsNullOrEmpty(addition))
			{
				v.vesselName += addition;
			}
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

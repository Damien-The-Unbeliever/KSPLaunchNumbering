﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LaunchNumbering
{
	public class LNSettings : GameParameters.CustomParameterNode
	{
		public override GameParameters.GameMode GameMode => GameParameters.GameMode.ANY;

		public override bool HasPresets => false;

		public override string Section => "Launch Numbering";

		public override int SectionOrder => 1;

		public override string Title => "Vessel Defaults";

		[GameParameters.CustomParameterUI("Numbering Scheme")]
		public NumberScheme Scheme { get; set; }
		[GameParameters.CustomParameterUI("Show Bloc numbers")]
		public bool ShowBloc { get; set; } = true;
		[GameParameters.CustomParameterUI("Bloc Numbering Scheme")]
		public NumberScheme BlocScheme { get; set; } = NumberScheme.Roman;

		public override string DisplaySection => "Launch Numbering";

		public enum NumberScheme
		{
			Arabic,
			Roman
		}
	}
}

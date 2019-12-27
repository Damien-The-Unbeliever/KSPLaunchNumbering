using System;
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

        [GameParameters.CustomParameterUI("Add number always",
            toolTip = "If false and the last char of a vessel name is a digit, no number will be added")]
        public bool addAlways { get; set; } = false;

        [GameParameters.CustomParameterUI("Add to first vessel",
            toolTip = "Add even to the first vessel in a unique series")]
        public bool addToFirstVessel { get; set; } = false;

        [GameParameters.CustomIntParameterUI("Minimum # of digits for launch number", minValue = 1, maxValue = 3,
            toolTip = "The launch number will be padded with 0's")]
        public int launchNumberMinDigits = 1;

        [GameParameters.CustomIntParameterUI("Minimum # of digits for bloc number", minValue = 1, maxValue = 3,
            toolTip = "The bloc number will be padded with 0's")]
        public int blocNumberMinDigits = 1;

        [GameParameters.CustomParameterUI("Active on Launchpad")]
        public bool activeOnLaunchpad { get; set; } = true;

        [GameParameters.CustomParameterUI("Active on Runway")]
        public bool activeOnRunway { get; set; } = true;

        [GameParameters.CustomParameterUI("Active on external Launchpads",
            toolTip ="This option is for the Extra Planetary Launchpads mod")]
        public bool activeOnExternalLaunchpad { get; set; } = true;
    }
}

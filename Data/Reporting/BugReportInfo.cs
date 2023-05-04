using System;
using System.Collections.Generic;
using System.IO;
using CommunityTools.Data.Enums;
using UnityEngine;

namespace CommunityTools.Data.Reporting
{

    public class BugReportInfo
    {
        public BugReportInfo()
        {
            Topic = BugReportInfoHelpers.GetTopic(BugReportInfoHelpers.TopicPlaceholderText);
            StepsToReproduce = BugReportInfoHelpers.GetStepsToReproduce(BugReportInfoHelpers.StepsToReproducePlaceholderText);
            PcSpecs = BugReportInfoHelpers.GetPcSpecs();
            MapCoordinates = BugReportInfoHelpers.GetMapCoordinates(Player.Get());
            Note = BugReportInfoHelpers.GetScreenshotInfo(BugReportInfoHelpers.NotePlaceholderText);
        }

        /// <summary>
        /// The topic for the bug report post.
        /// </summary>
        public Topic Topic { get; set; }

        /// <summary>
        /// Type of bug.
        /// </summary>
        public BugReportTypes Type { get; set; } = BugReportTypes.Other;

        /// <summary>
        /// Short description of the bug
        /// </summary>
        public string Description { get; set; } = BugReportInfoHelpers.DescriptionPlaceholderText;

        /// <summary>
        /// Describe the steps to reproduce the bug
        /// </summary>
        public List<StepToReproduce> StepsToReproduce { get; set; }

        /// <summary>
        /// How many times did the bug occur?
        /// </summary>
        public ReproduceRates ReproduceRate { get; set; } = ReproduceRates.Other;

        /// <summary>
        /// Describe what should have happened, if there was no bug
        /// </summary>
        public string ExpectedBehaviour { get; set; } = BugReportInfoHelpers.ExpectedBehaviourPlaceholderText;

        /// <summary>
        /// Technical PC specifications
        /// </summary>
        public PcSpecs PcSpecs { get; set; }

        /// <summary>
        /// If its graphical bug on map,
        /// include coordinates from watch (F + mouse scroll)
        /// </summary>
        public MapCoordinates MapCoordinates { get; set; }

        /// <summary>
        /// Any extra info
        /// </summary>
        public string Note { get; set; }


    }

}

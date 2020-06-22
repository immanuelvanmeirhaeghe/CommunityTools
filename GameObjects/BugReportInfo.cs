using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CommunityTools.GameObjects
{
    class BugReportInfo : MonoBehaviour
    {
        public Topic Topic { get; set; }

        public string BugReportType { get; set; }

        /// <summary>
        /// Short description of the bug
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Describe the steps to reproduce the bug
        /// </summary>
        public List<StepToReproduce> StepsToReproduce { get; set; }

        public string ReproduceRate { get; set; }

        /// <summary>
        /// Describe what should have happened, if there was no bug
        /// </summary>
        public string ExpectedBehaviour { get; set; }

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

        public BugReportInfo()
        {
            Topic = new Topic();
            StepsToReproduce = new List<StepToReproduce>();
            PcSpecs = new PcSpecs();
            MapCoordinates = new MapCoordinates();
        }
    }
}

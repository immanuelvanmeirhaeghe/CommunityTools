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
            Topic = GetTopic(BugReportInfoHelpers.TopicPlaceholderText);
            StepsToReproduce = GetStepsToReproduce(BugReportInfoHelpers.StepsToReproducePlaceholderText);
            PcSpecs = GetPcSpecs();
            MapCoordinates = GetMapCoordinates(Player.Get());
            Note = GetScreenshotInfo(BugReportInfoHelpers.NotePlaceholderText);
        }

        /// <summary>
        /// The topic for the bug report post.
        /// </summary>
        public Topic Topic { get; set; }

        /// <summary>
        /// Type of bug.
        /// </summary>
        public BugReportTypes BugReportType { get; set; } = BugReportTypes.Other;

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

        public MapCoordinates GetMapCoordinates(Player player)
        {
            player.GetGPSCoordinates(out int gps_lat, out int gps_long);

            var MapCoordinates = new MapCoordinates
            {
                GpsLat = gps_lat,
                GpsLong = gps_long
            };

            return MapCoordinates;
        }

        public PcSpecs GetPcSpecs()
        {
            var PcSpecs = new PcSpecs
            {
                OS = $"{SystemInfo.operatingSystem} - {SystemInfo.operatingSystemFamily}",
                CPU = $"{SystemInfo.processorType} with {SystemInfo.processorCount} cores",
                GPU = $"{SystemInfo.graphicsDeviceName}, Version: {SystemInfo.graphicsDeviceVersion}, Vendor: {SystemInfo.graphicsDeviceVendor}, Memory: {SystemInfo.graphicsMemorySize} MB",
                RAM = $"{SystemInfo.systemMemorySize} MB"
            };

            return PcSpecs;
        }

        public Topic GetTopic(string description = "")
        {
            var Topic = new Topic
            {
                GameVersion = GreenHellGame.s_GameVersion.WithBuildVersionToString(),
                Description = description
            };

            return Topic;
        }

        /// <summary>
        /// Get the inputted text, which by default should be formatted as a comma-separated list using semi-colon as separator.
        /// Optionally, set different separator.
        ///Example input:
        ///This is the first step description;this is the 2nd step description...;This is the 3rd step description etc;etc;
        /// </summary>
        /// <returns></returns>
        public List<StepToReproduce> GetStepsToReproduce(string stepsToReproduceString)
        {
            var StepsToReproduce = new List<StepToReproduce>();
            string[] steps = stepsToReproduceString?.Trim()?.Split(';');

            // Add by default a step
            int rank = 1;
            foreach (string step in steps)
            {
                var stepToReproduce = new StepToReproduce
                {
                    Rank = rank,
                    Description = step
                };

                StepsToReproduce.Add(stepToReproduce);
                rank++;
            }

            return StepsToReproduce;
        }

        /// <summary>
        /// Take a screenshot.and set the link in the bug report notes.
        /// </summary>
        /// <returns></returns>
        public string GetScreenshotInfo(string note = "")
        {
            string timeStamp = DateTime.Now.ToString("yyyyMMddThhmmmsZ");
            string fileName = $"{nameof(BugReportInfo)}_{timeStamp}.png";
            string fileDataPath = $"{Application.dataPath.Replace("GH_Data", "Logs")}/Screenshots/";
            string screenshotFile = $"{fileDataPath}{fileName}";

            if (!Directory.Exists(fileDataPath))
            {
                Directory.CreateDirectory(fileDataPath);
            }

            ScreenCapture.CaptureScreenshot($"{screenshotFile}");
            note += $"<a href=\"{screenshotFile}\">Screenshot {timeStamp}</a>";

            return note;
        }

    }

}

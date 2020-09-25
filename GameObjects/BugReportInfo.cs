using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace CommunityTools.GameObjects
{
    class BugReportInfo : MonoBehaviour
    {
        private static string _BugReportType = $"| UI | Crafting | Building | Multiplayer | Save Game | Items | Inventory | Other |";
        private static string _ReproduceRate = $"At least once";
        private static string _TopicDescription = $"Short topic describing the bug.";
        private static string _Description = $"Created with {nameof(CommunityTools)}";
        private static string _ExpectedBehaviour = $"Describe what you would have expected to happen in stead.";
        private static string _StepsToReproduce = $"Use a semi-colon to separate each step description like this.; Then this is step 2.; And this will become step 3.";
        private static string _Note = $"You can add any additional info here, like links to screenshots.";

        private static BugReportInfo s_Instance;

        public enum BugReportField
        {
            Topic,
            BugReportType,
            Description,
            StepsToReproduce,
            ReproduceRate,
            ExpectedBehaviour,
            PcSpecs,
            MapCoordinates,
            Note
        }

        public BugReportInfo()
        {
            BugReportType = _BugReportType;
            Topic = GetTopic(_TopicDescription);
            Description = _Description;
            ReproduceRate = _ReproduceRate;
            ExpectedBehaviour = _ExpectedBehaviour;
            StepsToReproduce = GetStepsToReproduce(_StepsToReproduce);
            PcSpecs = GetPcSpecs();
            MapCoordinates = GetMapCoordinates(Player.Get());
            Note = GetScreenshotInfo(_Note);

            s_Instance = this;
        }

        public static BugReportInfo Get()
        {
            return s_Instance;
        }

        /// <summary>
        /// The topic for the bug report post.
        /// </summary>
        public Topic Topic { get; set; }

        /// <summary>
        /// Type of bug.
        /// Example: Save Game, UI, Multiplayer...
        /// </summary>
        public string BugReportType { get; set; }

        /// <summary>
        /// Short description of the bug
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Describe the steps to reproduce the bug
        /// </summary>
        public List<StepToReproduce> StepsToReproduce { get; set; }

        /// <summary>
        /// How many times did the bug occur?
        /// Example: Once, Always.
        /// </summary>
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

        public static MapCoordinates GetMapCoordinates(Player player)
        {
            player.GetGPSCoordinates(out int gps_lat, out int gps_long);

            var MapCoordinates = new MapCoordinates
            {
                GpsLat = gps_lat,
                GpsLong = gps_long
            };

            return MapCoordinates;
        }

        public static PcSpecs GetPcSpecs()
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

        public static Topic GetTopic(string description = "")
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
        public static List<StepToReproduce> GetStepsToReproduce(string stepsToReproduce, char stepSeparator = ';')
        {
            var StepsToReproduce = new List<StepToReproduce>();
            string[] steps = stepsToReproduce?.Split(stepSeparator);

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
        public static string GetScreenshotInfo(string note = "")
        {
            string timeStamp = DateTime.Now.ToString("yyyyMMddThhmmmsZ");
            string fileName = $"{nameof(BugReportInfo)}_{timeStamp}.jpg";
            string fileDataPath = Application.dataPath + $"/Mods/{nameof(CommunityTools)}/Screenshots/";
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

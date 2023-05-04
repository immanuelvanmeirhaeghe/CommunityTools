using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CommunityTools.Data.Enums;
using UnityEngine;

namespace CommunityTools.Data.Reporting
{
    /// <summary>
    /// Helper for texts, websites,  e-mails eetc..
    /// </summary>
    public class BugReportInfoHelpers
    {
        protected static void HandleException(Exception exc, string methodName)
        {
            string info = $"[{nameof(BugReportInfoHelpers)}:{methodName}] throws exception -  {exc.TargetSite?.Name}:\n{exc.Message}\n{exc.InnerException}\n{exc.Source}\n{exc.StackTrace}";
            ModAPI.Log.Write(info);
        }

        public static string CreepyJarRedditBugReportUrl => @"https://www.reddit.com/r/GreenHell/";
        public static string CreepyJarContactEmail => @"support@creepyjar.com";
        public static string SteamForumBugReportUrl => @"https://steamcommunity.com/app/815370/discussions/0/";
        public static string SteamForumGuideUrl => @"https://steamcommunity.com/app/815370/guides/";
        public static string CreenHellWikiFandomUrl => @"https://greenhell.fandom.com/wiki/Green_Hell_Wiki";
        public static string ModAPIDiscordUrl => @"https://discord.gg/VAMuXyd";
        public static string ModAPIHubUrl => @"https://modapi.survivetheforest.net/";

        public static string TopicPlaceholderText
            => $"Short topic describing the bug.";
        public static string ExpectedBehaviourPlaceholderText
            => $"Describe what you would have expected to happen in stead.";
        public static string StepsToReproducePlaceholderText
            => $"Use a semi-colon to separate each step description like this.; Then this is step 2.; And this will become step 3.";
        public static string NotePlaceholderText
            => $"You can add any additional info here, like links to screenshots.";
        public static string DescriptionPlaceholderText
            => $"The description of the bug.";

        public static List<string> Urls { get; set; } = new List<string>();

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
        public static List<StepToReproduce> GetStepsToReproduce(string stepsToReproduceString)
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
        public static string GetScreenshotInfo(string note = "")
        {
            CaptureScreenshot(out string timeStamp, out string screenshotFile);
            note += $"<a href=\"{screenshotFile}\">Screenshot {timeStamp}</a>";

            return note;
        }

        /// <summary>
        /// Take a screenshot
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <param name="screenshotFile"></param>
        public static void CaptureScreenshot(out string timeStamp, out string screenshotFile)
        {
            timeStamp = DateTime.Now.ToString("yyyyMMddThhmmmsZ");
            string fileName = $"{nameof(BugReportInfo)}_{timeStamp}.png";
            string fileDataPath = $"{Application.dataPath.Replace("GH_Data", "Logs")}/Screenshots/";
            screenshotFile = $"{fileDataPath}{fileName}";
            if (!Directory.Exists(fileDataPath))
            {
                Directory.CreateDirectory(fileDataPath);
            }

            ScreenCapture.CaptureScreenshot(screenshotFile);
        }

        public static string[] GetBugReportTypes()
        {
            return Enum.GetNames(typeof(BugReportTypes));
        }

        public static string[] GetReproduceRates()
        {
            return Enum.GetNames(typeof(ReproduceRates));
        }

        public static string CreateBugReportAsHtml(BugReportInfo report)
        {
            var bugReportBuilder = new StringBuilder($"\n");

            try
            {
                bugReportBuilder.AppendLine($"<!DOCTYPE html>");
                bugReportBuilder.AppendLine($"<html class=\"client\">");
                bugReportBuilder.AppendLine($"  <head>");
                bugReportBuilder.AppendLine($"      <meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\">");
                bugReportBuilder.AppendLine($"      <title>");
                bugReportBuilder.AppendLine($"         {report.Topic?.GameVersion}] - {report.Topic?.Description} :: Green Hell Bug Reports");
                bugReportBuilder.AppendLine($"      </title>");
                bugReportBuilder.AppendLine($"  </head>");
                bugReportBuilder.AppendLine($"  <body>");
                bugReportBuilder.AppendLine($"      <div class=\"topic\">");
                bugReportBuilder.AppendLine($"          [{report.Topic?.GameVersion}] - {report.Topic?.Description}");
                bugReportBuilder.AppendLine($"      </div>");
                bugReportBuilder.AppendLine($"      <div class=\"content\">");
                bugReportBuilder.AppendLine($"          <br>Type: {report.Type}");
                bugReportBuilder.AppendLine($"          <br>Description: {report.Description}");
                bugReportBuilder.AppendLine($"          <ul>Steps to Reproduce:");
                foreach (StepToReproduce step in report.StepsToReproduce)
                {
                    bugReportBuilder.AppendLine($"          <li>Step {step.Rank}: {step.Description}</li>");
                }
                bugReportBuilder.AppendLine($"             </ul>");
                bugReportBuilder.AppendLine($"               <br>Reproduce rate: {report.ReproduceRate}");
                bugReportBuilder.AppendLine($"               <br>Expected behaviour: {report.ExpectedBehaviour}");
                bugReportBuilder.AppendLine($"              <ul>My PC spec:");
                bugReportBuilder.AppendLine($"                  <li>OS: {report.PcSpecs?.OS}</li>");
                bugReportBuilder.AppendLine($"                  <li>CPU: {report.PcSpecs?.CPU}</li>");
                bugReportBuilder.AppendLine($"                  <li>GPU: {report.PcSpecs?.GPU}</li>");
                bugReportBuilder.AppendLine($"                  <li>RAM: {report.PcSpecs?.RAM}</li>");
                bugReportBuilder.AppendLine($"              </ul>");
                bugReportBuilder.AppendLine($"              <br>Note:  {report.Note}");
                bugReportBuilder.AppendLine($"          </div>");
                bugReportBuilder.AppendLine($"      </body>");
                bugReportBuilder.AppendLine($"</html>");               
                ModAPI.Log.Write(bugReportBuilder.ToString());
                return bugReportBuilder.ToString();
            }
            catch (Exception exc)
            {
                HandleException(exc, nameof(CreateBugReportAsHtml));
                return string.Empty;
            }
        }

        public static string CreateBugReportAsJSON(BugReportInfo report)
        {
            var bugReportBuilder = new StringBuilder("\n");

            try
            {
                bugReportBuilder.AppendLine($"{{");
                bugReportBuilder.AppendLine($"\"Topic\":");
                bugReportBuilder.AppendLine($"{{ ");
                bugReportBuilder.AppendLine($"\"GameVersion\": \"{report.Topic?.GameVersion}\",");
                bugReportBuilder.AppendLine($"\"Description\": \"{report.Topic?.Description}\"");
                bugReportBuilder.AppendLine($"}},");
                bugReportBuilder.AppendLine($"\"Type\": \"{report.Type}\",");
                bugReportBuilder.AppendLine($"\"Description\": \"{report.Description}\",");
                bugReportBuilder.AppendLine($"\"StepsToReproduce\": [");
                foreach (StepToReproduce step in report.StepsToReproduce)
                {
                    bugReportBuilder.AppendLine($"{{");
                    bugReportBuilder.AppendLine($"\"Rank\": {step.Rank},");
                    bugReportBuilder.AppendLine($"\"Description\": \"{step.Description}\", ");
                    bugReportBuilder.AppendLine($"}},");
                }
                bugReportBuilder.AppendLine($"],");
                bugReportBuilder.AppendLine($"\"ReproduceRate\": \"{report.ReproduceRate}\",");
                bugReportBuilder.AppendLine($"\"ExpectedBehaviour\": \"{report.ExpectedBehaviour}\",");
                bugReportBuilder.AppendLine($"\"PcSpecs\":");
                bugReportBuilder.AppendLine($"{{");
                bugReportBuilder.AppendLine($"\"OS\": \"{report.PcSpecs?.OS}\",");
                bugReportBuilder.AppendLine($"\"CPU\": \"{report.PcSpecs?.CPU}\",");
                bugReportBuilder.AppendLine($"\"GPU\": \"{report.PcSpecs?.GPU}\",");
                bugReportBuilder.AppendLine($"\"RAM\": \"{report.PcSpecs?.RAM}\"");
                bugReportBuilder.AppendLine($"}},");
                bugReportBuilder.AppendLine($"\"MapCoordinates\": \"{report.MapCoordinates?.ToString()}\",");
                bugReportBuilder.AppendLine($"\"Note\": \"{report.Note}\"");
                bugReportBuilder.AppendLine($"}}");
                ModAPI.Log.Write(bugReportBuilder.ToString());
                return bugReportBuilder.ToString();
            }
            catch (Exception exc)
            {
                HandleException(exc, nameof(CreateBugReportAsJSON));
                return string.Empty;
            }
        }

    }
}
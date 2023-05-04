using System;
using System.Collections.Generic;
using CommunityTools.Data.Enums;

namespace CommunityTools.Data.Reporting
{
    /// <summary>
    /// Helper for texts, websites,  e-mails eetc..
    /// </summary>
    public class BugReportInfoHelpers
    {
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

        public static string[] GetBugReportTypes()
        {
            return Enum.GetNames(typeof(BugReportTypes));
        }

        public static string[] GetReproduceRates()
        {
            return Enum.GetNames(typeof(ReproduceRates));
        }

    }
}
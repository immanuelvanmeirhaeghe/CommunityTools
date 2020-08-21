using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CommunityTools.GameObjects.UI
{
    class BugReportScreen : BaseScreen
    {
        private static string m_BugReportType = $"| UI | Crafting | Building | Multiplayer | Save Game | Items | Inventory | Other |";
        private static string m_ReproduceRate = $"At least once";
        private static string m_TopicDescription = $"Short topic describing the bug.";
        private static string m_Description = $"The description of the bug.";
        private static string m_ExpectedBehaviour = $"Describe what you would have expected to happen in stead.";
        private static string m_StepsToReproduce = $"Use a semi-colon to separate each step description like this.; Then this is step 2.; And this will become step 3.";
        private static string m_Note = $"You can add any additional info here, like links to screenshots.";

        public bool QuickReportsEnabled = false;
        public static BugReportInfo bugReportInfo;

        protected override void Update()
        {
            base.Update();

            if (!showUI)
            {
                InitData();

                IsActivated = true;

                InitScreen();

                EnableCursor(true);
            }
            // toggle menu
            showUI = !showUI;
            if (!showUI)
            {
                EnableCursor(false);
            }

            // To make a quick bug report, if the mod option has been activated, press Keypad6
            if (!showUI && QuickReportsEnabled)
            {
                if (Input.GetKeyDown(KeyCode.Keypad6))
                {
                    InitData();
                    CreateBugReports();
                }
            }
        }

        public override void OnGUI()
        {
            base.OnGUI();
            if (showUI)
            {
                InitData();
            }
        }

        private static void InitData()
        {
            if (bugReportInfo == null)
            {
                bugReportInfo = BugReportInfo.Get();
            }
        }

        private void InitScreen()
        {
            GUI.Box(new Rect(10f, 10f, 400f, 950f), "Community tools - bug report form", windowStyle);

            // Label Topic Description
            GUI.Label(new Rect(30f, 20f, 200f, 20f), "Topic description", labelStyle);
            //Topic Description
            m_TopicDescription = GUI.TextField(new Rect(230f, 20f, 200f, 20f), m_TopicDescription, textFieldStyle);

            // Label Bug Report Type
            GUI.Label(new Rect(30f, 40f, 200f, 20f), "Bug report type", labelStyle);
            //Bug Report Type
            m_BugReportType = GUI.TextField(new Rect(230f, 40f, 200f, 20f), m_BugReportType, textFieldStyle);

            // Label Bug Description
            GUI.Label(new Rect(30f, 60f, 200f, 200f), "Description", labelStyle);
            // Bug Description
            m_Description = GUI.TextArea(new Rect(230f, 60f, 200f, 200f), m_Description, textAreaStyle);

            // Label Steps To Reproduce
            GUI.Label(new Rect(30f, 260f, 200f, 200f), "Steps to reproduce", labelStyle);
            // Steps to reproduce
            m_StepsToReproduce = GUI.TextArea(new Rect(230f, 260f, 200f, 200f), m_StepsToReproduce, textAreaStyle);

            // Bug Reproduce Rate
            GUI.Label(new Rect(30f, 460f, 200f, 20f), "Reproduce rate", labelStyle);
            //Bug Report Type
            m_ReproduceRate = GUI.TextField(new Rect(230f, 460f, 200f, 20f), m_ReproduceRate, textFieldStyle);

            // Label Expected Behaviour
            GUI.Label(new Rect(30f, 480f, 200f, 200f), "Expected behaviour", labelStyle);
            //Expected Behaviour
            m_ExpectedBehaviour = GUI.TextArea(new Rect(230f, 480f, 200f, 200f), m_ExpectedBehaviour, textAreaStyle);

            // Label Note
            GUI.Label(new Rect(30f, 680f, 200f, 200f), "Note", labelStyle);
            //Note
            m_Note = GUI.TextArea(new Rect(230f, 680f, 200f, 200f), m_Note, textAreaStyle);

            //Enable or disable quick bug reports
            QuickReportsEnabled = GUI.Toggle(new Rect(30f, 880f, 300f, 20f), QuickReportsEnabled, "Quick reports ON/OFF", toggleStyle);
            // Label Toggle Quick Report
            GUI.Label(new Rect(30f, 900f, 300f, 20f), " When enabled, press numerical keypad 5 to make a quick bug report.", labelStyle);

            // Create Bug Report Button
            if (GUI.Button(new Rect(30f, 920f, 200f, 20f), "Create bug report", buttonStyle))
            {
                OnClickCreateBugReportButton();
                showUI = false;
                EnableCursor(false);
            }
        }

        private void OnClickCreateBugReportButton()
        {
            try
            {
                CreateBugReports();
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{nameof(CommunityTools)}.{nameof(CommunityToolsScreen)}:{nameof(OnClickCreateBugReportButton)}] throws exception: {exc.Message}");
            }
        }

        private void CreateBugReports()
        {
            string timeStamp = DateTime.Now.ToString("yyyyMMddThhmmmsZ");
            string htmlReportName = $"{nameof(BugReportInfo)}_{timeStamp}.html";
            string jsonReportName = $"{nameof(BugReportInfo)}_{timeStamp}.json";

            bugReportInfo = GetBugReport();

            if (FileWrite(htmlReportName, CreateBugReportAsHtml()))
            {
                ShowHUDInfoLog($"{htmlReportName}", "HUDConstruction_Created");
            }

            if (FileWrite(jsonReportName, GetBugReportAsJSON()))
            {
                ShowHUDInfoLog($"{jsonReportName}", "HUDConstruction_Created");
            }
        }

        protected string CreateBugReportAsHtml()
        {
            StringBuilder bugReportBuilder = new StringBuilder("<!DOCTYPE html>");
            try
            {
                bugReportBuilder.AppendLine($"<html class=\"client\">" +
                                                                            $"  <head>" +
                                                                            $"      <meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\">" +
                                                                            $"      <title>" +
                                                                            $"         {bugReportInfo.Topic?.GameVersion}] - {bugReportInfo.Topic?.Description} :: Green Hell Bug Reports" +
                                                                            $"      </title>" +
                                                                            $"  </head>" +
                                                                            $"  <body>" +
                                                                            $"      <div class=\"topic\">" +
                                                                            $"          [{bugReportInfo.Topic?.GameVersion}] - {bugReportInfo.Topic?.Description}" +
                                                                            $"      </div>" +
                                                                            $"      <div class=\"content\">" +
                                                                            $"          <br>Type: {bugReportInfo.BugReportType.ToString()}" +
                                                                            $"          <br>Description: {bugReportInfo.Description}" +
                                                                            $"          <ul>Steps to Reproduce:");
                foreach (var step in bugReportInfo.StepsToReproduce)
                {
                    bugReportBuilder.AppendLine($"          <li>Step {step.Rank.ToString()}: {step.Description}</li>");
                }
                bugReportBuilder.AppendLine($"             </ul>" +
                                                                        $"               <br>Reproduce rate: {bugReportInfo.ReproduceRate.ToString()}" +
                                                                        $"               <br>Expected behaviour: {bugReportInfo.ExpectedBehaviour}" +
                                                                        $"              <ul>My PC spec:" +
                                                                        $"                  <li>OS: {bugReportInfo.PcSpecs?.OS}</li>" +
                                                                        $"                  <li>CPU: {bugReportInfo.PcSpecs?.CPU}</li>" +
                                                                        $"                  <li>GPU: {bugReportInfo.PcSpecs?.GPU}</li>" +
                                                                        $"                  <li>RAM: {bugReportInfo.PcSpecs?.RAM}</li>" +
                                                                        $"              </ul>" +
                                                                        $"              <br>Note:  {bugReportInfo.Note}" +
                                                                        $"          </div>" +
                                                                        $"      </body>" +
                                                                        $"</html>");

                return bugReportBuilder.ToString();
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{nameof(CommunityToolsScreen)}.{nameof(CommunityToolsScreen)}:{nameof(CreateBugReportAsHtml)}] throws exception: {exc.Message}");
                return bugReportBuilder.ToString();
            }
        }

        protected string GetBugReportAsJSON()
        {
            string steps = $"";
            string reportTemplate = $"";
            try
            {
                foreach (var step in bugReportInfo.StepsToReproduce)
                {
                    steps += $"" +
                        $"{{" +
                        $"\"Rank\": {step.Rank.ToString()}," +
                        $"\"Description\": \"{step.Description}\"" +
                        $"}},";
                }
                reportTemplate = $"" +
                    $"{{" +
                    $"\"Topic\":" +
                    $"{{ " +
                    $"\"GameVersion\": \"{bugReportInfo.Topic?.GameVersion}\"," +
                    $"\"Description\": \"{bugReportInfo.Topic?.Description}\"" +
                    $"}}," +
                    $"\"Type\": \"{bugReportInfo.BugReportType.ToString()}\"," +
                    $"\"Description\": \"{bugReportInfo.Description}\"," +
                    $"\"StepsToReproduce\": [" +
                    $"{steps}" +
                    $"]," +
                    $"\"ReproduceRate\": \"{bugReportInfo.ReproduceRate.ToString()}\"," +
                    $"\"ExpectedBehaviour\": \"{bugReportInfo.ExpectedBehaviour}\"," +
                    $"\"PcSpecs\":" +
                    $"{{" +
                    $"\"OS\": \"{bugReportInfo.PcSpecs?.OS}\"," +
                    $"\"CPU\": \"{bugReportInfo.PcSpecs?.CPU}\"," +
                    $"\"GPU\": \"{bugReportInfo.PcSpecs?.GPU}\"," +
                    $"\"RAM\": \"{bugReportInfo.PcSpecs?.RAM}\"" +
                    $"}}," +
                    $"\"MapCoordinates\": \"{bugReportInfo.MapCoordinates?.ToString()}\"," +
                    $"\"Note\": \"{bugReportInfo.Note}\"" +
                    $"}}";

                return reportTemplate;
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{nameof(CommunityTools)}.{nameof(CommunityToolsScreen)}:{nameof(GetBugReportAsJSON)}] throws exception: {exc.Message}");
                return reportTemplate;
            }
        }

        public static BugReportInfo GetBugReport()
        {
            bugReportInfo = new BugReportInfo
            {
                Topic = BugReportInfo.GetTopic(m_TopicDescription),
                BugReportType = m_BugReportType,
                Description = m_Description,
                ExpectedBehaviour = m_ExpectedBehaviour,
                MapCoordinates = BugReportInfo.GetMapCoordinates(player),
                PcSpecs = BugReportInfo.GetPcSpecs(),
                StepsToReproduce = BugReportInfo.GetStepsToReproduce(m_StepsToReproduce),
                ReproduceRate = m_ReproduceRate,
                Note = BugReportInfo.GetScreenshotInfo(m_Note)
            };

            return bugReportInfo;
        }

    }
}

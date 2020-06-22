using CommunityTools.GameObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace CommunityTools
{
    /// <summary>
    /// The Mod API UI for the mod.
    /// Enabled pressing RightCtrl+NumPad5.
    /// </summary>
    class CommunityToolsModule : MonoBehaviour
    {
        public bool IsActivated = false;
        private static CommunityToolsModule s_Instance;
        private bool showUI = false;
        private bool m_quickReportsEnabled = false;

        protected static BugReportInfo bugReportInfo;
        protected static ItemsManager itemsManager;
        protected static MenuInGameManager menuInGameManager;
        protected static HUDManager hUDManager;
        protected static Player player;

        protected static GUIStyle windowStyle;
        protected static GUIStyle labelStyle;
        protected static GUIStyle textFieldStyle;
        protected static GUIStyle textAreaStyle;
        protected static GUIStyle buttonStyle;
        protected static GUIStyle toggleStyle;

        private static string m_SelectedBugReportType;
        private static string m_SelectedReproduceRate;
        private static string m_TopicDescription;
        private static string m_Description;
        private static string m_ExpectedBehaviour;
        private static string m_StepsToReproduce;
        private static string m_Note;

        public CommunityToolsModule()
        {
            IsActivated = true;
            s_Instance = this;
        }

        public static CommunityToolsModule Get()
        {
            return s_Instance;
        }

        private void Update()
        {

            // To make a quick bug report, if the mod option has been activated, press Keypad5
            if (m_quickReportsEnabled && Input.GetKeyDown(KeyCode.Keypad5))
            {
                InitData();
                CreateBugReports();
            }

            // To show the mod UI, press Pause
            if (Input.GetKeyDown(KeyCode.Pause) && Input.GetKeyDown(KeyCode.Keypad5))
            {
                if (!showUI)
                {
                    InitData();

                    IsActivated = true;

                    EnableCursor(true);
                }
                // toggle menu
                showUI = !showUI;
                if (!showUI)
                {
                    EnableCursor(false);
                }
            }
        }

        private void OnGUI()
        {
            if (showUI)
            {
                InitData();
                InitSkinUI();
                InitModUI();
            }
        }

        private static void InitSkinUI()
        {
            GUI.skin = ModAPI.Interface.Skin;

            if (windowStyle == null)
            {
                windowStyle = new GUIStyle(GUI.skin.window);
            }

            if (labelStyle == null)
            {
                labelStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 12
                };
            }

            if (textFieldStyle == null)
            {
                textFieldStyle = new GUIStyle(GUI.skin.textField);
            }

            if (textAreaStyle == null)
            {
                textAreaStyle = new GUIStyle(GUI.skin.textArea);
            }

            if (buttonStyle == null)
            {
                buttonStyle = new GUIStyle(GUI.skin.button);
            }

            if (toggleStyle == null)
            {
                toggleStyle = new GUIStyle(GUI.skin.toggle);
            }
        }

        private static void EnableCursor(bool enabled = false)
        {
            CursorManager.Get().ShowCursor(enabled, false);
            player = Player.Get();

            if (enabled)
            {
                player.BlockMoves();
                player.BlockRotation();
                player.BlockInspection();
            }
            else
            {
                player.UnblockMoves();
                player.UnblockRotation();
                player.UnblockInspection();
            }
        }

        private static void InitData()
        {
            bugReportInfo = new BugReportInfo();

            itemsManager = ItemsManager.Get();

            menuInGameManager = MenuInGameManager.Get();

            hUDManager = HUDManager.Get();

            player = Player.Get();
        }

        private void InitModUI()
        {
            GUI.Box(new Rect(10f, 10f, 300f, 1050f), "Community Tools - Bug report form", GUI.skin.window);

            // Label Topic Description
            GUI.Label(new Rect(30f, 20f, 20f, 20f), "Topic description", labelStyle);
            //Topic Description
            m_TopicDescription = GUI.TextField(new Rect(230f, 20f, 200f, 20f), m_TopicDescription, textFieldStyle);

            // Label Bug Report Type
            GUI.Label(new Rect(30f, 40f, 20f, 20f), "Bug Report Type", labelStyle);
            //Bug Report Type
            m_SelectedBugReportType = GUI.TextField(new Rect(230f, 40f, 200f, 20f), m_SelectedBugReportType, textFieldStyle);

            // Label Bug Description
            GUI.Label(new Rect(30f, 60f, 20f, 20f), "Description", labelStyle);
            // Bug Description
            m_Description = GUI.TextArea(new Rect(230f, 60f, 200f, 200f), m_Description, textAreaStyle);

            // Label Steps To Reproduce
            GUI.Label(new Rect(30f, 260f, 20f, 20f), "Steps to reproduce", labelStyle);
            // Steps to reproduce
            m_StepsToReproduce = GUI.TextArea(new Rect(230f, 260f, 200f, 200f), m_StepsToReproduce, textAreaStyle);

            // Bug Reproduce Rate
            GUI.Label(new Rect(30f, 460f, 20f, 20f), "Reproduce rate", labelStyle);
            //Bug Report Type
            m_SelectedReproduceRate = GUI.TextField(new Rect(230f, 460f, 200f, 20f), m_SelectedReproduceRate, textFieldStyle);

            // Label Expected Behaviour
            GUI.Label(new Rect(30f, 480f, 20f, 20f), "Expected behaviour", labelStyle);
            //Expected Behaviour
            m_ExpectedBehaviour = GUI.TextArea(new Rect(230f, 480f, 200f, 200f), m_ExpectedBehaviour, textAreaStyle);

            // Label Note
            GUI.Label(new Rect(30f, 680f, 20f, 20f), "Note", labelStyle);
            //Note
            m_Note = GUI.TextArea(new Rect(230f, 680f, 200f, 200f), m_Note, textAreaStyle);

            //Enable or disable quick bug reports
            m_quickReportsEnabled = GUI.Toggle(new Rect(30f, 880f, 200f, 20f), m_quickReportsEnabled, "Quick reports ON/OFF (Press Keypad 5)", toggleStyle);

            // Create Bug Report Button
            if (GUI.Button(new Rect(30f, 900f, 200f, 20f), "Create bug report", GUI.skin.button))
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
                ModAPI.Log.Write($"[{nameof(CommunityToolsModule)}.{nameof(CommunityToolsModule)}:{nameof(OnClickCreateBugReportButton)}] throws exception: {exc.Message}");
            }
        }

        private void CreateBugReports()
        {
            string timeStamp = DateTime.Now.ToString("yyyyMMddThhmmmsZ");
            string htmlReportName = $"{nameof(BugReportInfo)}.{timeStamp}.html";
            string jsonReportName = $"{nameof(BugReportInfo)}.{timeStamp}.json";

            SetBugReport();

            if (FileWrite(htmlReportName, CreateBugReportAsHtml()))
            {
                ShowHUDBigInfo($"Report name {htmlReportName}", "New bug report created", HUDInfoLogTextureType.Count.ToString());
            }

            if (FileWrite(jsonReportName, GetBugReportAsJSON()))
            {
                ShowHUDBigInfo($"Report name {jsonReportName}", "New bug report created", HUDInfoLogTextureType.Count.ToString());
            }
        }

        private static void SetBugReport()
        {
            player.GetGPSCoordinates(out int gps_lat, out int gps_long);

            bugReportInfo = new BugReportInfo
            {
                Topic = new Topic
                {
                    GameVersion = GreenHellGame.s_GameVersion.WithBuildVersionToString(),
                    Description = m_TopicDescription
                },
                BugReportType = m_SelectedBugReportType,
                Description = m_Description,
                StepsToReproduce = GetStepsToReproduce(),
                ReproduceRate = m_SelectedReproduceRate,
                ExpectedBehaviour = m_ExpectedBehaviour,
                PcSpecs = new PcSpecs
                {
                    OS = $@"{SystemInfo.operatingSystem} - {SystemInfo.operatingSystemFamily}",
                    CPU = $@"{SystemInfo.processorType} with {SystemInfo.processorCount} cores",
                    GPU = $@"{SystemInfo.graphicsDeviceName}, Version: {SystemInfo.graphicsDeviceVersion}, Vendor: {SystemInfo.graphicsDeviceVendor}, Memory: {SystemInfo.graphicsMemorySize} MB",
                    RAM = $@"{SystemInfo.systemMemorySize} MB"
                },
                MapCoordinates = new MapCoordinates
                {
                    GpsLat = gps_lat,
                    GpsLong = gps_long
                },
                Note = m_Note
            };

        }

        public static void ShowHUDBigInfo(string text, string header, string textureName)
        {
            HUDBigInfo bigInfo = (HUDBigInfo)hUDManager.GetHUD(typeof(HUDBigInfo));
            HUDBigInfoData bigInfoData = new HUDBigInfoData
            {
                m_Header = header,
                m_Text = GreenHellGame.Instance.GetLocalization().Get(text),
                m_TextureName = textureName,
                m_ShowTime = Time.time
            };
            bigInfo.AddInfo(bigInfoData);
            bigInfo.Show(true);
        }

        private static List<StepToReproduce> GetStepsToReproduce()
        {
            var list = new List<StepToReproduce>();

            if (!string.IsNullOrEmpty(m_StepsToReproduce))
            {
                // Get the inputted text, which should be formatted as a comma-separated list using semi-colon as separator!
                //Example input:
                //This is the first step description;this is the 2nd step description...;This is the 3rd step description etc;etc;
                string[] steps = m_StepsToReproduce.Split(';');
                // Add by default a step
                int rank = 1;
                foreach (string step in steps)
                {
                    var stepToReproduce = new StepToReproduce
                    {
                        Rank = rank,
                        Description = step
                    };

                    list.Add(stepToReproduce);
                    rank++;
                }
            }

            return list;
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
                ModAPI.Log.Write($"[{nameof(CommunityToolsModule)}.{nameof(CommunityToolsModule)}:{nameof(CreateBugReportAsHtml)}] throws exception: {exc.Message}");
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
                    $" \"Type\": \"{bugReportInfo.BugReportType.ToString()}\"," +
                    $" \"Description\": \"{bugReportInfo.Description}\"," +
                    $"\"StepsToReproduce\": [{steps}]," +
                    $"\"ReproduceRate\": \"{bugReportInfo.ReproduceRate.ToString()}\"," +
                    $"\"ExpectedBehaviour\": \"{bugReportInfo.ExpectedBehaviour}\"," +
                    $"\"PcSpecs\":" +
                    $" {{" +
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
                ModAPI.Log.Write($"[{nameof(CommunityToolsModule)}.{nameof(CommunityToolsModule)}:{nameof(GetBugReportAsJSON)}] throws exception: {exc.Message}");
                return reportTemplate;
            }
        }

        protected static bool FileWrite(string fileName, string fileContent)
        {
            try
            {
                byte[] fileContentData = Encoding.UTF8.GetBytes(fileContent);

                var fileDataPath = Application.dataPath + $"/Mods/{nameof(CommunityTools)}/Logs/";
                if (!Directory.Exists(fileDataPath))
                {
                    Directory.CreateDirectory(fileDataPath);
                }

                var file = fileDataPath + fileName;
                if (!File.Exists(file))
                {
                    using (FileStream fileStream = File.Create(file))
                    {
                        fileStream.Write(fileContentData, 0, fileContentData.Length);
                        fileStream.Flush();
                    }
                }
                return true;
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{nameof(CommunityToolsModule)}.{nameof(CommunityToolsModule)}:{nameof(FileWrite)}] throws exception: {exc.Message}");
                return false;
            }
        }

        protected static int FileRead(string file, byte[] data, int length)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(file);
                using (FileStream fileStream = fileInfo.OpenRead())
                {
                    fileStream.Read(data, 0, (int)fileStream.Length);
                    return (int)fileStream.Length;
                }
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{nameof(CommunityToolsModule)}.{nameof(CommunityToolsModule)}:{nameof(FileRead)}] throws exception: {exc.Message}");
                return 0;
            }
        }

    }
}

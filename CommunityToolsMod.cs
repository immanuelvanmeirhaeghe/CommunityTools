using CommunityTools.GameObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CommunityTools
{
    /// <summary>
    /// The Mod API UI for the mod.
    /// Enabled pressing RightCtrl+NumPad5.
    /// </summary>
    class CommunityToolsMod : MonoBehaviour
    {
        public bool IsCommunityToolsModActive = false;

        private static CommunityToolsMod s_Instance;

        private bool showUI = false;

        private static BugReportInfo bugReportInfo;

        private static ItemsManager itemsManager;

        private static MenuInGameManager menuInGameManager;

        private static HUDManager hUDManager;

        private static Player player;

        protected static GUIStyle windowStyle;
        protected static GUIStyle labelStyle;
        protected static GUIStyle textFieldStyle;
        protected static GUIStyle textAreaStyle;
        protected static GUIStyle buttonStyle;

        private static string m_SelectedBugReportType;
        private static string m_SelectedReproduceRate;
        private static string m_TopicDescription;
        private static string m_Description;
        private static string m_ExpectedBehaviour;
        private static string m_StepsToReproduce;
        private static string m_Note;

        public CommunityToolsMod()
        {
            IsCommunityToolsModActive = true;
            s_Instance = this;
        }

        public static CommunityToolsMod Get()
        {
            return s_Instance;
        }

        public static void ShowHUDInfoLog(string itemID, string localizedTextKey)
        {
            var localization = GreenHellGame.Instance.GetLocalization();
            HUDMessages hUDMessages = (HUDMessages)hUDManager.GetHUD(typeof(HUDMessages));
            hUDMessages.AddMessage(
                $"{localization.Get(localizedTextKey)}  {localization.Get(itemID)}"
                );
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.RightControl) && Input.GetKeyDown(KeyCode.Keypad5))
            {
                if (!showUI)
                {
                    bugReportInfo = new BugReportInfo();

                    itemsManager = ItemsManager.Get();

                    menuInGameManager = MenuInGameManager.Get();

                    hUDManager = HUDManager.Get();

                    player = Player.Get();

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
                InitModUI();
            }
        }

        private static void InitData()
        {
            itemsManager = ItemsManager.Get();

            hUDManager = HUDManager.Get();

            player = Player.Get();

            InitSkinUI();
        }

        private void InitModUI()
        {
            GUI.Box(new Rect(10f, 10f, 300f, 1050f), "Community Tools - Bug report form", GUI.skin.window);

            // Label Topic Description
            GUI.Label(new Rect(30f, 20f, 200f, 20f), "Topic description", labelStyle);
            //Topic Description
            m_TopicDescription = GUI.TextField(new Rect(230f, 20f, 200f, 20f), m_TopicDescription, textFieldStyle);

            // Label Bug Report Type
            GUI.Label(new Rect(30f, 40f, 200f, 20f), "Bug Report Type", labelStyle);
            //Bug Report Type
            m_SelectedBugReportType = GUI.TextField(new Rect(230f, 40f, 200f, 20f), m_SelectedBugReportType, textFieldStyle);

            // Label Bug Description
            GUI.Label(new Rect(30f, 60f, 200f, 20f), "Description", labelStyle);
            // Bug Description
            m_Description = GUI.TextArea(new Rect(230f, 60f, 200f, 200f), m_Description, textAreaStyle);

            // Label Steps To Reproduce
            GUI.Label(new Rect(30f, 260f, 200f, 20f), "Steps to reproduce", labelStyle);
            // Steps to reproduce
            m_StepsToReproduce = GUI.TextArea(new Rect(230f, 260f, 200f, 200f), m_StepsToReproduce, textAreaStyle);

            // Bug Reproduce Rate
            GUI.Label(new Rect(30f, 460f, 200f, 20f), "Reproduce rate", labelStyle);
            //Bug Report Type
            m_SelectedReproduceRate = GUI.TextField(new Rect(230f, 460f, 200f, 20f), m_SelectedReproduceRate, textFieldStyle);

            // Label Expected Behaviour
            GUI.Label(new Rect(30f, 480f, 200f, 20f), "Expected behaviour", labelStyle);
            //Expected Behaviour
            m_ExpectedBehaviour = GUI.TextArea(new Rect(230f, 480f, 200f, 200f), m_ExpectedBehaviour, textAreaStyle);

            // Label Note
            GUI.Label(new Rect(30f, 680f, 200f, 20f), "Note", labelStyle);
            //Note
            m_Note = GUI.TextArea(new Rect(230f, 680f, 200f, 200f), m_Note, textAreaStyle);

            // Create Bug Report Button
            if (GUI.Button(new Rect(30f, 880f, 200f, 20f), "Create bug report", GUI.skin.button))
            {
                OnClickCreateBugReportButton();
                showUI = false;
                EnableCursor(false);
            }
        }

        private void OnClickCreateBugReportButton()
        {
            string bugReportTimeStamp = DateTime.Now.ToString("yyyyMMdd");
            try
            {
                SetBugReport();
                FileWrite($"{nameof(BugReportInfo)}.html", CreateBugReportAsHtml());
                FileWrite($"{nameof(BugReportInfo)}.json", GetBugReportAsJSON());
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{nameof(CommunityToolsMod)}.{nameof(CommunityToolsMod)}:{nameof(OnClickCreateBugReportButton)}] throws exception: {exc.Message}");
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
                StepsToReproduce = new List<StepsToReproduce>
                {
                    new StepsToReproduce
                    {
                        Rank=1,
                        Description = m_StepsToReproduce
                    }
                },
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

        private string CreateBugReportAsHtml()
        {
            StringBuilder bugReportBuilder = new StringBuilder("<!DOCTYPE html>");
            try
            {
                bugReportBuilder.AppendLine($"<html class=\"client\">");
                bugReportBuilder.AppendLine($"<head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\"><title>{bugReportInfo.Topic?.GameVersion}] - {bugReportInfo.Topic?.Description} :: Green Hell Bug Reports</title></head>");
                bugReportBuilder.AppendLine($"<body>:");
                bugReportBuilder.AppendLine($"<div class=\"topic\">[{bugReportInfo.Topic?.GameVersion}] - {bugReportInfo.Topic?.Description}</div>");
                bugReportBuilder.AppendLine($"<div class=\"content\">");
                bugReportBuilder.AppendLine($"<br>Type: {bugReportInfo.BugReportType.ToString()}");
                bugReportBuilder.AppendLine($"<br>Description: {bugReportInfo.Description}");
                bugReportBuilder.AppendLine($"<ul>Steps to Reproduce:");
                foreach (var step in bugReportInfo.StepsToReproduce)
                {
                    bugReportBuilder.AppendLine($"<li>Step {step.Rank.ToString()}: {step.Description}</li>");
                }
                bugReportBuilder.AppendLine($"</ul>:");
                bugReportBuilder.AppendLine($"<br>Reproduce rate: {bugReportInfo.ReproduceRate.ToString()}");
                bugReportBuilder.AppendLine($"<br>Expected behaviour: {bugReportInfo.ExpectedBehaviour}");
                bugReportBuilder.AppendLine($"<ul>My PC spec:");
                bugReportBuilder.AppendLine($"<li>OS: {bugReportInfo.PcSpecs?.OS}</li>");
                bugReportBuilder.AppendLine($"<li>CPU: {bugReportInfo.PcSpecs?.CPU}</li>");
                bugReportBuilder.AppendLine($"<li>GPU: {bugReportInfo.PcSpecs?.GPU}</li>");
                bugReportBuilder.AppendLine($"<li>RAM: {bugReportInfo.PcSpecs?.RAM}</li>");
                bugReportBuilder.AppendLine($"</ul>");
                bugReportBuilder.AppendLine($"<br>Note:  {bugReportInfo.Note}");
                bugReportBuilder.AppendLine($"</div>");
                bugReportBuilder.AppendLine($"</body>");
                bugReportBuilder.AppendLine($"</html>");

                return bugReportBuilder.ToString();
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{nameof(CommunityToolsMod)}.{nameof(CommunityToolsMod)}:{nameof(CreateBugReportAsHtml)}] throws exception: {exc.Message}");
                return bugReportBuilder.ToString();
            }
        }

        public string GetBugReportAsJSON()
        {
            string steps = $"";
            string reportTemplate = $"";
            try
            {
                foreach (var step in bugReportInfo.StepsToReproduce)
                {
                    steps += $"{{ \"Rank\": {step.Rank.ToString()}, \"Description\": \"{step.Description}\"}},";
                }
                reportTemplate = $"{{ \"Topic\": {{ \"GameVersion\": \"{bugReportInfo.Topic?.GameVersion}\", \"Description\": \"{bugReportInfo.Topic?.Description}\" }}, \"Type\": \"{bugReportInfo.BugReportType.ToString()}\", \"Description\": \"{bugReportInfo.Description}\", \"StepsToReproduce\": [{steps}], \"ReproduceRate\": \"{bugReportInfo.ReproduceRate.ToString()}\", \"ExpectedBehaviour\": \"{bugReportInfo.ExpectedBehaviour}\", \"PcSpecs\": {{ \"OS\": \"{bugReportInfo.PcSpecs?.OS}\", \"CPU\": \"{bugReportInfo.PcSpecs?.CPU}\", \"GPU\": \"{bugReportInfo.PcSpecs?.GPU}\", \"RAM\": \"{bugReportInfo.PcSpecs?.RAM}\"}}, \"MapCoordinates\": \"{bugReportInfo.MapCoordinates?.ToString()}\", \"Note\": \"{bugReportInfo.Note}\" }}";

                return reportTemplate;
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{nameof(CommunityToolsMod)}.{nameof(CommunityToolsMod)}:{nameof(GetBugReportAsJSON)}] throws exception: {exc.Message}");
                return reportTemplate;
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

        public static bool FileWrite(string fileName, string fileContent)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(fileContent);
                using (FileStream fileStream = new FileStream(Application.persistentDataPath + "/Logs/" + fileName, FileMode.Create, FileAccess.Write))
                {
                    fileStream.Write(data, 0, data.Length);
                }
                return true;
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{nameof(CommunityToolsMod)}.{nameof(CommunityToolsMod)}:{nameof(FileWrite)}] throws exception: {exc.Message}");
                return false;
            }
        }

        public static int FileRead(string file_name, byte[] data, int length)
        {
            FileInfo fileInfo = new FileInfo(Application.persistentDataPath + "/Logs/" + file_name);
            using (FileStream fileStream = fileInfo.OpenRead())
            {
                fileStream.Read(data, 0, (int)fileStream.Length);
                return (int)fileStream.Length;
            }
        }

    }
}

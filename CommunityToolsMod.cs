using CommunityTools.GameObjects;
using System;
using System.Collections.Generic;
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
            GUI.Box(new Rect(10f, 10f, 300f, 300f), "Community Tools", GUI.skin.window);
            // Label Topic Description
            GUI.Label(new Rect(30f, 20f, 200f, 20f), "Topic description", labelStyle);
            //Topic Description
            m_TopicDescription = GUI.TextField(new Rect(30f, 40f, 200f, 20f), m_TopicDescription, textFieldStyle);

            // Label Bug Report Type
            GUI.Label(new Rect(30f, 60f, 200f, 20f), "Bug Report Type", labelStyle);
            //Bug Report Type
            m_SelectedBugReportType = GUI.TextField(new Rect(30f, 80f, 200f, 20f), m_SelectedBugReportType, textFieldStyle);

            // Label Bug Description
            GUI.Label(new Rect(30f, 100f, 200f, 20f), "Description", labelStyle);
            // Bug Description
            m_Description = GUI.TextArea(new Rect(30f, 120f, 200f, 20f), m_Description, textAreaStyle);

            // Label Steps To Reproduce
            GUI.Label(new Rect(30f, 140f, 200f, 20f), "Steps to reproduce", labelStyle);
            // Steps to reproduce
            m_StepsToReproduce = GUI.TextArea(new Rect(30f, 160f, 200f, 20f), m_StepsToReproduce, textAreaStyle);

            // Bug Reproduce Rate
            GUI.Label(new Rect(30f, 180f, 200f, 20f), "Reproduce Rate", labelStyle);
            //Bug Report Type
            m_SelectedReproduceRate = GUI.TextField(new Rect(30f, 200f, 200f, 20f), m_SelectedReproduceRate, textFieldStyle);

            // Label Expected Behaviour
            GUI.Label(new Rect(30f, 220f, 200f, 20f), "Expected behaviour", labelStyle);
            //Expected Behaviour
            m_ExpectedBehaviour = GUI.TextArea(new Rect(30f, 240f, 200f, 20f), m_ExpectedBehaviour, textAreaStyle);

            // Label Note
            GUI.Label(new Rect(30f, 260f, 200f, 20f), "Note", labelStyle);
            //Note
            m_Note = GUI.TextArea(new Rect(30f, 280f, 200f, 20f), m_Note, textAreaStyle);

            // Create Bug Report Button
            if (GUI.Button(new Rect(30f, 300f, 200f, 20f), "Create bug report", GUI.skin.button))
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
                SetBugReport();
                CreateBugReportAsText();
                ModAPI.Log.Write(GetBugReportAsJSON());
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{nameof(CommunityToolsMod)}.{nameof(CommunityToolsMod)}:{nameof(OnClickCreateBugReportButton)}] throws exception: {exc.Message}");
            }
        }

        private static void SetBugReport()
        {
            string bugReportTimeStamp = DateTime.Now.ToString("yyyyMMdd");

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
                    CPU = $@"{SystemInfo.processorType} at {SystemInfo.processorFrequency} with {SystemInfo.processorCount} cores",
                    GPU = $@"{SystemInfo.graphicsDeviceName}, Version: {SystemInfo.graphicsDeviceVersion}, Vendor: {SystemInfo.graphicsDeviceVendor}, Memory: {SystemInfo.graphicsMemorySize}",
                    RAM = $@"{SystemInfo.systemMemorySize}"
                },
                MapCoordinates = new MapCoordinates
                {
                    GpsLat = gps_lat,
                    GpsLong = gps_long
                },
                Note = m_Note
            };

        }

        private void CreateBugReportAsText()
        {
            StringBuilder bugReportBuilder = new StringBuilder();
            try
            {
                bugReportBuilder.AppendLine($"Topic: [{bugReportInfo.Topic?.GameVersion}] - {bugReportInfo.Topic?.Description}");
                bugReportBuilder.AppendLine($"Type: {bugReportInfo.BugReportType.ToString()}");
                bugReportBuilder.AppendLine($"Description: {bugReportInfo.Description}");
                bugReportBuilder.AppendLine($"Steps to Reproduce:");
                foreach (var step in bugReportInfo.StepsToReproduce)
                {
                    bugReportBuilder.AppendLine($"\tStep {step.Rank.ToString()}: {step.Description}");
                }

                bugReportBuilder.AppendLine($"Reproduce rate: {bugReportInfo.ReproduceRate.ToString()}");
                bugReportBuilder.AppendLine($"Expected behaviour: {bugReportInfo.ExpectedBehaviour}");
                bugReportBuilder.AppendLine($"PC spec:");
                bugReportBuilder.AppendLine($"\tOS: {bugReportInfo.PcSpecs?.OS}");
                bugReportBuilder.AppendLine($"\tCPU: {bugReportInfo.PcSpecs?.CPU}");
                bugReportBuilder.AppendLine($"\tGPU: {bugReportInfo.PcSpecs?.GPU}");
                bugReportBuilder.AppendLine($"\tRAM: {bugReportInfo.PcSpecs?.RAM}");
                bugReportBuilder.AppendLine($"Note:  {bugReportInfo.Note}");

                ModAPI.Log.Write(bugReportBuilder.ToString());
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{nameof(CommunityToolsMod)}.{nameof(CommunityToolsMod)}:{nameof(CreateBugReportAsText)}] throws exception: {exc.Message}");
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
                    steps += $"{{ \"Rank\": {step.Rank.ToString()}, \"Description\": {step.Description}}},";
                }
                reportTemplate = $"{{ \"Topic\": {{ \"GameVersion\": {bugReportInfo.Topic?.GameVersion}, \"Description\": {bugReportInfo.Topic?.Description} }}, \"Type\": {bugReportInfo.BugReportType.ToString()}, \"Description\": {bugReportInfo.Description}, \"StepsToReproduce\": [{steps}], \"ReproduceRate\": {bugReportInfo.ReproduceRate.ToString()}, \"ExpectedBehaviour\": {bugReportInfo.ExpectedBehaviour}, \"PcSpecs\": {{ \"OS\": {bugReportInfo.PcSpecs?.OS}, \"CPU\": {bugReportInfo.PcSpecs?.CPU}, \"GPU\": {bugReportInfo.PcSpecs?.GPU}, \"RAM\": {bugReportInfo.PcSpecs?.RAM}}}, \"MapCoordinates\": {bugReportInfo.MapCoordinates?.ToString()}, \"Note\": {bugReportInfo.Note} }}";

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


    }
}

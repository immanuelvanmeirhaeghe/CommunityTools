using CommunityTools.GameObjects;
using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace CommunityTools
{
    public enum MessageType
    {
        Info,
        Warning,
        Error
    }

    /// <summary>
    /// CommunityTools is a mod for Green Hell that aims to be a tool for the gamer community and modders.
    /// For now, it helps in getting game metadata and creating a bug report.
    /// Output can be found in the game installation data folder in subfolder Logs.
    /// Enable the mod UI by pressing Home.
    /// </summary>
    public class CommunityTools : MonoBehaviour
    {
        private static CommunityTools Instance;

        private static readonly string ModName = nameof(CommunityTools);
        private static readonly string ReportPath = $"{Application.dataPath.Replace("GH_Data", "Logs")}/{nameof(CommunityTools)}.log";
        private static readonly float ModScreenTotalWidth = 500f;
        private static readonly float ModScreenTotalHeight = 150f;
        private static readonly float ModScreenMinWidth = 450f;
        private static readonly float ModScreenMaxWidth = 550f;
        private static readonly float ModScreenMinHeight = 50f;
        private static readonly float ModScreenMaxHeight = 200f;
        private static float ModScreenStartPositionX { get; set; } = (Screen.width - ModScreenMaxWidth) % ModScreenTotalWidth;
        private static float ModScreenStartPositionY { get; set; } = (Screen.height - ModScreenMaxHeight) % ModScreenTotalHeight;
        private static bool IsMinimized { get; set; } = false;

        private bool ShowUI = false;
        private bool ShowBugUI = false;

        public static Rect CommunityToolsScreen = new Rect(ModScreenStartPositionX, ModScreenStartPositionY, ModScreenTotalWidth, ModScreenTotalHeight);
        public static Rect CommunityToolsBugReportScreen = new Rect(ModScreenStartPositionX / 2f, ModScreenStartPositionY / 2f, ModScreenTotalWidth, ModScreenTotalHeight);

        public bool IsModActiveForMultiplayer { get; private set; } = false;
        public bool IsModActiveForSingleplayer => ReplTools.AmIMaster();

        private static Player LocalPlayer;
        private static HUDManager LocalHUDManager;
        private static BugReportInfo LocalBugReportInfo;

        public static string SteamForumBugReportUrl { get; private set; }
        public static string SteamForumGuideUrl { get; private set; }
        public static string CreepyJarContactEmail { get; private set; }
        public static string ReportFile { get; set; }

        public static string BugReportType = $"| UI | Crafting | Building | Multiplayer | Save Game | Items | Inventory | Other |";
        public static string ReproduceRate = $"At least once";
        public static string TopicDescription = $"Short topic describing the bug.";
        public static string Description = $"The description of the bug.";
        public static string ExpectedBehaviour = $"Describe what you would have expected to happen in stead.";
        public static string StepsToReproduce = $"Use a semi-colon to separate each step description like this.;" +
            $" Then this is step 2.; " +
            $"And this will become step 3.";
        public static string Note = $"You can add any additional info here, like links to screenshots.";

        public static string PermissionChangedMessage(string permission) => $"Permission to use mods and cheats in multiplayer was {permission}";
        public static string HUDBigInfoMessage(string message, MessageType messageType, Color? headcolor = null)
            => $"<color=#{ (headcolor != null ? ColorUtility.ToHtmlStringRGBA(headcolor.Value) : ColorUtility.ToHtmlStringRGBA(Color.red))  }>{messageType}</color>\n{message}";

        private void HandleException(Exception exc, string methodName)
        {
            string info = $"[{ModName}:{methodName}] throws exception:\n{exc.Message}";
            ModAPI.Log.Write(info);
            ShowHUDBigInfo(HUDBigInfoMessage(info, MessageType.Error, Color.red));
        }

        public void ShowHUDBigInfo(string text)
        {
            string header = $"{ModName} Info";
            string textureName = HUDInfoLogTextureType.Count.ToString();
            HUDBigInfo hudBigInfo = (HUDBigInfo)LocalHUDManager.GetHUD(typeof(HUDBigInfo));
            HUDBigInfoData.s_Duration = 2f;
            HUDBigInfoData hudBigInfoData = new HUDBigInfoData
            {
                m_Header = header,
                m_Text = text,
                m_TextureName = textureName,
                m_ShowTime = Time.time
            };
            hudBigInfo.AddInfo(hudBigInfoData);
            hudBigInfo.Show(true);
        }

        public void ShowHUDInfoLog(string itemID, string localizedTextKey)
        {
            Localization localization = GreenHellGame.Instance.GetLocalization();
            ((HUDMessages)LocalHUDManager.GetHUD(typeof(HUDMessages))).AddMessage(localization.Get(localizedTextKey) + "  " + localization.Get(itemID));
        }

        public void Start()
        {
            ModManager.ModManager.onPermissionValueChanged += ModManager_onPermissionValueChanged;
        }

        private void ModManager_onPermissionValueChanged(bool optionValue)
        {
            IsModActiveForMultiplayer = optionValue;
            ShowHUDBigInfo(
                          (optionValue ?
                            HUDBigInfoMessage(PermissionChangedMessage($"granted"), MessageType.Info, Color.green)
                            : HUDBigInfoMessage(PermissionChangedMessage($"revoked"), MessageType.Info, Color.yellow))
                            );
        }

        public CommunityTools()
        {
            SteamForumBugReportUrl = "https://steamcommunity.com/app/815370/discussions/1/";
            SteamForumGuideUrl = "https://steamcommunity.com/sharedfiles/filedetails/?id=2160052009";
            CreepyJarContactEmail = "mailto:support@creepyjar.com";
            useGUILayout = true;
            Instance = this;
        }

        public static CommunityTools Get()
        {
            return Instance;
        }

        private void EnableCursor(bool blockPlayer = false)
        {
            CursorManager.Get().ShowCursor(blockPlayer, false);

            if (blockPlayer)
            {
                LocalPlayer.BlockMoves();
                LocalPlayer.BlockRotation();
                LocalPlayer.BlockInspection();
            }
            else
            {
                LocalPlayer.UnblockMoves();
                LocalPlayer.UnblockRotation();
                LocalPlayer.UnblockInspection();
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Home))
            {
                if (!ShowUI)
                {
                    InitData();
                    EnableCursor(true);
                }
                ToggleShowUI(0);
                if (!ShowUI)
                {
                    EnableCursor(false);
                }
            }
        }

        private void ToggleShowUI(int level = 0)
        {
            switch (level)
            {
                case 0:
                    ShowUI = !ShowUI;
                    break;
                case 1:
                    ShowBugUI = !ShowBugUI;
                    break;
                default:
                    ShowUI = !ShowUI;
                    ShowBugUI = !ShowBugUI;
                    break;
            }
        }

        private void OnGUI()
        {
            if (ShowUI || ShowBugUI)
            {
                InitData();
                InitSkinUI();
                InitWindow();
            }
        }

        private void InitData()
        {
            LocalHUDManager = HUDManager.Get();
            LocalPlayer = Player.Get();
        }

        private void InitSkinUI()
        {
            GUI.skin = ModAPI.Interface.Skin;
        }

        private void InitWindow()
        {
            if (ShowUI)
            {
                ShowCommunityToolsScreen();
            }

            if (ShowBugUI)
            {
                ShowBugReportScreen();
            }
        }

        private void ScreenMenuBox(Rect screen)
        {
            if (GUI.Button(new Rect(screen.width - 40f, 0f, 20f, 20f), "-", GUI.skin.button))
            {
                CollapseWindow(screen);
            }

            if (GUI.Button(new Rect(screen.width - 20f, 0f, 20f, 20f), "X", GUI.skin.button))
            {
                CloseWindow();
            }
        }

        private void CollapseWindow(Rect screen)
        {
            if (!IsMinimized)
            {
                screen = new Rect(ModScreenStartPositionX, ModScreenStartPositionY, ModScreenTotalWidth, ModScreenMinHeight);
                IsMinimized = true;
            }
            else
            {
                screen = new Rect(ModScreenStartPositionX, ModScreenStartPositionY, ModScreenTotalWidth, ModScreenTotalHeight);
                IsMinimized = false;
            }
            InitWindow();
        }

        private void ShowCommunityToolsScreen()
        {
            CommunityToolsScreen = GUILayout.Window(GetHashCode(), CommunityToolsScreen, InitCommunityToolsScreen, ModName,
                                                                                                          GUI.skin.window,
                                                                                                          GUILayout.ExpandWidth(true),
                                                                                                          GUILayout.MinWidth(ModScreenMinWidth),
                                                                                                          GUILayout.MaxWidth(ModScreenMaxWidth),
                                                                                                          GUILayout.ExpandHeight(true),
                                                                                                          GUILayout.MinHeight(ModScreenMinHeight),
                                                                                                          GUILayout.MaxHeight(ModScreenMaxHeight));
        }

        private void InitCommunityToolsScreen(int windowID)
        {
            ModScreenStartPositionX = CommunityToolsScreen.x;
            ModScreenStartPositionY = CommunityToolsScreen.y;

            using (var modContentScope = new GUILayout.VerticalScope(GUI.skin.box))
            {
                ScreenMenuBox(CommunityToolsScreen);
                if (!IsMinimized)
                {
                    SupportOnlineBox();
                    BugReportBox();
                }
            }
            GUI.DragWindow(new Rect(0f, 0f, 10000f, 10000f));
        }

        private void BugReportBox()
        {
            using (var bugReportScope = new GUILayout.VerticalScope(GUI.skin.box))
            {
                BugReportFormBox();
                if (GUILayout.Button("Create report", GUI.skin.button))
                {
                    OnClickCreateBugReportButton();
                }
                if (GUILayout.Button("Open report screen", GUI.skin.button))
                {
                    ShowBugUI = true;
                    InitWindow();
                }
            }
        }

        private void BugReportFormBox()
        {
            using (var horizontalScope = new GUILayout.HorizontalScope(GUI.skin.box))
            {
                GUILayout.Label("Topic description: ", GUI.skin.label);
                TopicDescription = GUILayout.TextField(TopicDescription, GUI.skin.textField);
            }

            using (var horizontalScope = new GUILayout.HorizontalScope(GUI.skin.box))
            {
                GUILayout.Label("Bug report type: ", GUI.skin.label);
                BugReportType = GUILayout.TextField(BugReportType, GUI.skin.textField);
            }

            using (var horizontalScope = new GUILayout.HorizontalScope(GUI.skin.box))
            {
                GUILayout.Label("Description: ", GUI.skin.label);
                Description = GUILayout.TextArea(Description, GUI.skin.textArea);
            }

            using (var horizontalScope = new GUILayout.HorizontalScope(GUI.skin.box))
            {
                GUILayout.Label("Steps to reproduce: ", GUI.skin.label);
                StepsToReproduce = GUILayout.TextArea(StepsToReproduce, GUI.skin.textArea);
            }

            using (var horizontalScope = new GUILayout.HorizontalScope(GUI.skin.box))
            {
                GUILayout.Label("Reproduce rate: ", GUI.skin.label);
                ReproduceRate = GUILayout.TextField(ReproduceRate, GUI.skin.textField);
            }

            using (var horizontalScope = new GUILayout.HorizontalScope(GUI.skin.box))
            {
                GUILayout.Label("Expected behaviour: ", GUI.skin.label);
                ExpectedBehaviour = GUILayout.TextArea(ExpectedBehaviour, GUI.skin.textArea);
            }

            using (var horizontalScope = new GUILayout.HorizontalScope(GUI.skin.box))
            {
                GUILayout.Label("Notes: ", GUI.skin.label);
                Note = GUILayout.TextArea(Note, GUI.skin.textArea);
            }
        }

        private void SupportOnlineBox()
        {
            using (var horizontalScope = new GUILayout.HorizontalScope(GUI.skin.box))
            {
                GUILayout.Label("Click to send your question by mail to Creepy Jar Help", GUI.skin.label);
                if (GUILayout.Button("Send mail", GUI.skin.button))
                {
                    OnClickSendMailButton();
                    CloseWindow();
                }
            }

            using (var horizontalScope = new GUILayout.HorizontalScope(GUI.skin.box))
            {
                GUILayout.Label("Try to find help online on Steam ", GUI.skin.label);
                if (GUILayout.Button("Open guide", GUI.skin.button))
                {
                    OnClickOpenSteamGuideButton();
                    CloseWindow();
                }
            }

            using (var horizontalScope = new GUILayout.HorizontalScope(GUI.skin.box))
            {
                GUILayout.Label("Look at the reported bugs on Steam ", GUI.skin.label);
                if (GUILayout.Button("Open forum", GUI.skin.button))
                {
                    OnClickOpenSteamGuideButton();
                    CloseWindow();
                }
            }
        }

        private void ShowBugReportScreen()
        {
            CommunityToolsBugReportScreen = GUILayout.Window(GetHashCode(), CommunityToolsBugReportScreen, InitCommunityToolsBugReportScreen, $"{ModName} - {nameof(CommunityToolsBugReportScreen)}", GUI.skin.window,
                                                                                                          GUILayout.ExpandWidth(true),
                                                                                                          GUILayout.MinWidth(ModScreenMinWidth),
                                                                                                          GUILayout.MaxWidth(ModScreenMaxWidth),
                                                                                                          GUILayout.ExpandHeight(true),
                                                                                                          GUILayout.MinHeight(ModScreenMinHeight),
                                                                                                          GUILayout.MaxHeight(ModScreenMaxHeight));
        }

        private void InitCommunityToolsBugReportScreen(int windowID)
        {
            ModScreenStartPositionX = CommunityToolsBugReportScreen.x;
            ModScreenStartPositionY = CommunityToolsBugReportScreen.y;

            using (var modContentScope = new GUILayout.VerticalScope(GUI.skin.box))
            {
                ScreenMenuBox(CommunityToolsBugReportScreen);
                if (!IsMinimized)
                {
                    BugReportBox();
                }

            }
            GUI.DragWindow(new Rect(0f, 0f, 10000f, 10000f));
        }

        private void CloseWindow()
        {
            ShowUI = false;
            ShowBugUI = false;
            EnableCursor(false);
        }

        private void OnClickCreateBugReportButton()
        {
            try
            {
                CreateBugReport();
            }
            catch (Exception exc)
            {
                HandleException(exc, nameof(OnClickCreateBugReportButton));
            }
        }

        public void OnClickOpenSteamGuideButton()
        {
            Application.OpenURL(SteamForumGuideUrl);
        }

        public void OnClickOpenSteamForumButton()
        {
            Application.OpenURL(SteamForumBugReportUrl);
        }

        public void OnClickSendMailButton()
        {
            Application.OpenURL(CreepyJarContactEmail);
        }

        public void CreateBugReport()
        {
            try
            {
                LocalBugReportInfo = new BugReportInfo
                {
                    BugReportType = BugReportType,
                    Topic = BugReportInfo.GetTopic(TopicDescription),
                    Description = Description,
                    ExpectedBehaviour = ExpectedBehaviour,
                    ReproduceRate = ReproduceRate,
                    StepsToReproduce = BugReportInfo.GetStepsToReproduce(StepsToReproduce),
                    MapCoordinates = BugReportInfo.GetMapCoordinates(LocalPlayer),
                    PcSpecs = BugReportInfo.GetPcSpecs(),
                    Note = BugReportInfo.GetScreenshotInfo(Note)
                };

                CreateReports();

                OnClickOpenSteamForumButton();
            }
            catch (Exception exc)
            {
                HandleException(exc, nameof(CreateBugReport));
            }
        }

        protected static bool FileWrite(string fileName, string fileContent)
        {
            try
            {
                byte[] fileContentData = Encoding.UTF8.GetBytes(fileContent);

                if (!Directory.Exists(ReportPath))
                {
                    Directory.CreateDirectory(ReportPath);
                }

                ReportFile = ReportPath + fileName;
                if (!File.Exists(ReportFile))
                {
                    using (FileStream fileStream = File.Create(ReportFile))
                    {
                        fileStream.Write(fileContentData, 0, fileContentData.Length);
                        fileStream.Flush();
                    }
                }
                return true;
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{(ModName)}.{ModName}:{nameof(FileWrite)}] throws exception: {exc.Message}");
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
                ModAPI.Log.Write($"[{ModName}.{ModName}:{nameof(FileRead)}] throws exception: {exc.Message}");
                return 0;
            }
        }

        private void CreateReports()
        {
            ReportFile = CreateBugReportAsHtml();
            if (!string.IsNullOrEmpty(ReportFile))
            {
                ShowHUDBigInfo(HUDBigInfoMessage(ReportCreatedMessage($"html report in {ReportPath}"), MessageType.Info, Color.green));
                ReportFile = string.Empty;
            }

            ReportFile = GetBugReportAsJSON();
            if (!string.IsNullOrEmpty(ReportFile))
            {
                ShowHUDBigInfo(HUDBigInfoMessage(ReportCreatedMessage($"json report in {ReportPath}"), MessageType.Info, Color.green));
                ReportFile = string.Empty;
            }
        }

        private static string ReportCreatedMessage(string htmlReportName) => $"<color=#{ColorUtility.ToHtmlStringRGBA(Color.red)}>System</color>:\n{htmlReportName} created!";

        protected string CreateBugReportAsHtml()
        {
            StringBuilder bugReportBuilder = new StringBuilder($"\n");

            try
            {
                bugReportBuilder.AppendLine($"<!DOCTYPE html>");
                bugReportBuilder.AppendLine($"<html class=\"client\">");
                bugReportBuilder.AppendLine($"  <head>");
                bugReportBuilder.AppendLine($"      <meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\">");
                bugReportBuilder.AppendLine($"      <title>");
                bugReportBuilder.AppendLine($"         {LocalBugReportInfo.Topic?.GameVersion}] - {LocalBugReportInfo.Topic?.Description} :: Green Hell Bug Reports");
                bugReportBuilder.AppendLine($"      </title>");
                bugReportBuilder.AppendLine($"  </head>");
                bugReportBuilder.AppendLine($"  <body>");
                bugReportBuilder.AppendLine($"      <div class=\"topic\">");
                bugReportBuilder.AppendLine($"          [{LocalBugReportInfo.Topic?.GameVersion}] - {LocalBugReportInfo.Topic?.Description}");
                bugReportBuilder.AppendLine($"      </div>");
                bugReportBuilder.AppendLine($"      <div class=\"content\">");
                bugReportBuilder.AppendLine($"          <br>Type: {LocalBugReportInfo.BugReportType}");
                bugReportBuilder.AppendLine($"          <br>Description: {LocalBugReportInfo.Description}");
                bugReportBuilder.AppendLine($"          <ul>Steps to Reproduce:");
                foreach (var step in LocalBugReportInfo.StepsToReproduce)
                {
                    bugReportBuilder.AppendLine($"          <li>Step {step.Rank}: {step.Description}</li>");
                }
                bugReportBuilder.AppendLine($"             </ul>");
                bugReportBuilder.AppendLine($"               <br>Reproduce rate: {LocalBugReportInfo.ReproduceRate}");
                bugReportBuilder.AppendLine($"               <br>Expected behaviour: {LocalBugReportInfo.ExpectedBehaviour}");
                bugReportBuilder.AppendLine($"              <ul>My PC spec:");
                bugReportBuilder.AppendLine($"                  <li>OS: {LocalBugReportInfo.PcSpecs?.OS}</li>");
                bugReportBuilder.AppendLine($"                  <li>CPU: {LocalBugReportInfo.PcSpecs?.CPU}</li>");
                bugReportBuilder.AppendLine($"                  <li>GPU: {LocalBugReportInfo.PcSpecs?.GPU}</li>");
                bugReportBuilder.AppendLine($"                  <li>RAM: {LocalBugReportInfo.PcSpecs?.RAM}</li>");
                bugReportBuilder.AppendLine($"              </ul>");
                bugReportBuilder.AppendLine($"              <br>Note:  {LocalBugReportInfo.Note}");
                bugReportBuilder.AppendLine($"          </div>");
                bugReportBuilder.AppendLine($"      </body>");
                bugReportBuilder.AppendLine($"</html>");

                ModAPI.Log.Write(bugReportBuilder.ToString());

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
            StringBuilder bugReportBuilder = new StringBuilder("");

            try
            {
                bugReportBuilder.AppendLine($"{{");
                bugReportBuilder.AppendLine($"\"Topic\":");
                bugReportBuilder.AppendLine($"{{ ");
                bugReportBuilder.AppendLine($"\"GameVersion\": \"{LocalBugReportInfo.Topic?.GameVersion}\",");
                bugReportBuilder.AppendLine($"\"Description\": \"{LocalBugReportInfo.Topic?.Description}\"");
                bugReportBuilder.AppendLine($"}},");
                bugReportBuilder.AppendLine($"\"Type\": \"{LocalBugReportInfo.BugReportType}\",");
                bugReportBuilder.AppendLine($"\"Description\": \"{LocalBugReportInfo.Description}\",");
                bugReportBuilder.AppendLine($"\"StepsToReproduce\": [");
                foreach (var step in LocalBugReportInfo.StepsToReproduce)
                {
                    bugReportBuilder.AppendLine($"{{");
                    bugReportBuilder.AppendLine($"\"Rank\": {step.Rank},");
                    bugReportBuilder.AppendLine($"\"Description\": \"{step.Description}\", ");
                    bugReportBuilder.AppendLine($"}},");
                }
                bugReportBuilder.AppendLine($"],");
                bugReportBuilder.AppendLine($"\"ReproduceRate\": \"{LocalBugReportInfo.ReproduceRate}\",");
                bugReportBuilder.AppendLine($"\"ExpectedBehaviour\": \"{LocalBugReportInfo.ExpectedBehaviour}\",");
                bugReportBuilder.AppendLine($"\"PcSpecs\":");
                bugReportBuilder.AppendLine($"{{");
                bugReportBuilder.AppendLine($"\"OS\": \"{LocalBugReportInfo.PcSpecs?.OS}\",");
                bugReportBuilder.AppendLine($"\"CPU\": \"{LocalBugReportInfo.PcSpecs?.CPU}\",");
                bugReportBuilder.AppendLine($"\"GPU\": \"{LocalBugReportInfo.PcSpecs?.GPU}\",");
                bugReportBuilder.AppendLine($"\"RAM\": \"{LocalBugReportInfo.PcSpecs?.RAM}\"");
                bugReportBuilder.AppendLine($"}},");
                bugReportBuilder.AppendLine($"\"MapCoordinates\": \"{LocalBugReportInfo.MapCoordinates?.ToString()}\",");
                bugReportBuilder.AppendLine($"\"Note\": \"{LocalBugReportInfo.Note}\"");
                bugReportBuilder.AppendLine($"}}");

                ModAPI.Log.Write(bugReportBuilder.ToString());

                return bugReportBuilder.ToString();
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{(ModName)}.{nameof(CommunityToolsScreen)}:{nameof(GetBugReportAsJSON)}] throws exception: {exc.Message}");
                return bugReportBuilder.ToString();
            }
        }
    }
}

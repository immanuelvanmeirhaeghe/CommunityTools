using CommunityTools.GameObjects;
using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace CommunityTools
{
    class CommunityTools : MonoBehaviour
    {
        private static CommunityTools s_Instance;

        private static readonly string ModName = nameof(CommunityTools);

        private static readonly string ReportPath = $"{Application.dataPath}/Mods/{ModName}/Logs/";
        public static string ReportFile { get; set; }

        private bool ShowUI = false;

        private bool ShowBugUI = false;

        public static Rect CommunityToolsScreen = new Rect(1000f, 500f, 450f, 150f);

        public static Rect CommunityToolsBugReportScreen = new Rect(1000f, 500f, 450f, 150f);

        private static ItemsManager itemsManager;

        private static Player player;

        private static HUDManager hUDManager;

        private static BugReportInfo BugReportInfo;

        private bool _isActiveForMultiplayer;
        public bool IsModActiveForMultiplayer
        {
            get => _isActiveForMultiplayer;
            set => _isActiveForMultiplayer = FindObjectOfType(typeof(ModManager.ModManager)) != null && ModManager.ModManager.AllowModsForMultiplayer;
        }

        private bool _isActiveForSingleplayer;
        public bool IsModActiveForSingleplayer
        {
            get => _isActiveForSingleplayer;
            set => _isActiveForSingleplayer = ReplTools.AmIMaster();
        }

        public string SteamForumBugReportUrl { get; private set; }
        public string SteamForumGuideUrl { get; private set; }
        public string CreepyJarContactEmail { get; private set; }

        private static string BugReportType = $"| UI | Crafting | Building | Multiplayer | Save Game | Items | Inventory | Other |";
        private static string ReproduceRate = $"At least once";
        private static string TopicDescription = $"Short topic describing the bug.";
        private static string Description = $"The description of the bug.";
        private static string ExpectedBehaviour = $"Describe what you would have expected to happen in stead.";
        private static string StepsToReproduce = $"Use a semi-colon to separate each step description like this.; Then this is step 2.; And this will become step 3.";
        private static string Note = $"You can add any additional info here, like links to screenshots.";
        private static bool QuickReportsEnabled = false;

        public CommunityTools()
        {
            SteamForumBugReportUrl = "https://steamcommunity.com/app/815370/discussions/1/";
            SteamForumGuideUrl = "https://steamcommunity.com/sharedfiles/filedetails/?id=2160052009";
            CreepyJarContactEmail = "mailto:support@creepyjar.com";
            useGUILayout = true;
            s_Instance = this;
        }

        public static CommunityTools Get()
        {
            return s_Instance;
        }

        public void ShowHUDInfoLog(string itemID, string localizedTextKey)
        {
            Localization localization = GreenHellGame.Instance.GetLocalization();
            ((HUDMessages)hUDManager.GetHUD(typeof(HUDMessages))).AddMessage(localization.Get(localizedTextKey) + "  " + localization.Get(itemID));
        }

        public void ShowHUDBigInfo(string text, string header, string textureName)
        {
            HUDManager hUDManager = HUDManager.Get();

            HUDBigInfo hudBigInfo = (HUDBigInfo)hUDManager.GetHUD(typeof(HUDBigInfo));
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

        private void EnableCursor(bool blockPlayer = false)
        {
            CursorManager.Get().ShowCursor(blockPlayer, false);

            if (blockPlayer)
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

            if ((IsModActiveForSingleplayer || IsModActiveForMultiplayer) && QuickReportsEnabled && Input.GetKeyDown(KeyCode.Keypad5))
            {
                if (!ShowBugUI)
                {
                    InitData();
                    EnableCursor(true);
                }
                ToggleShowUI(1);
                if (!ShowBugUI)
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
            hUDManager = HUDManager.Get();
            itemsManager = ItemsManager.Get();
            player = Player.Get();
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

        private void ShowCommunityToolsScreen()
        {
            CommunityToolsScreen = GUILayout.Window(GetHashCode(), CommunityToolsScreen, InitCommunityToolsScreen, $"{ModName}", GUI.skin.window);
        }

        private void InitCommunityToolsScreen(int windowID)
        {
            using (var verticalScope = new GUILayout.VerticalScope(GUI.skin.box))
            {
                if (GUI.Button(new Rect(430f, 0f, 20f, 20f), "X", GUI.skin.button))
                {
                    CloseWindow();
                }

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

                using (var vertical2Scope = new GUILayout.VerticalScope(GUI.skin.box))
                {
                    GUILayout.Label(" When enabled, press numerical keypad 5 to make a quick bug report.", GUI.skin.label);
                    QuickReportsEnabled = GUILayout.Toggle(QuickReportsEnabled, $"Enable quick reports? ", GUI.skin.toggle);
                }

                using (var horizontalScope = new GUILayout.HorizontalScope(GUI.skin.box))
                {
                    GUILayout.Label("Click to create a bug report", GUI.skin.label);
                    if (GUILayout.Button("Create report", GUI.skin.button))
                    {
                        CloseWindow();
                        ShowBugReportScreen();
                    }
                }
            }
            GUI.DragWindow(new Rect(0f, 0f, 10000f, 10000f));
        }

        private void ShowBugReportScreen()
        {
            CommunityToolsBugReportScreen = GUILayout.Window(GetHashCode(), CommunityToolsBugReportScreen, InitCommunityToolsBugReportScreen, $"{ModName} - {nameof(CommunityToolsBugReportScreen)}", GUI.skin.window);
        }

        private void InitCommunityToolsBugReportScreen(int windowID)
        {
            using (var verticalScope = new GUILayout.VerticalScope(GUI.skin.box))
            {
                if (GUI.Button(new Rect(430f, 0f, 20f, 20f), "X", GUI.skin.button))
                {
                    CloseWindow();
                }

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

                CreateBugReportButton();
            }
            GUI.DragWindow(new Rect(0f, 0f, 10000f, 10000f));
        }

        private void CloseWindow()
        {
            ShowUI = false;
            ShowBugUI = false;
            EnableCursor(false);
        }

        private void CreateBugReportButton()
        {
            if (IsModActiveForSingleplayer || IsModActiveForMultiplayer)
            {
                using (var horizontalScope = new GUILayout.HorizontalScope(GUI.skin.box))
                {
                    if (GUILayout.Button("Create bug report", GUI.skin.button))
                    {
                        OnClickCreateBugReportButton();
                        CloseWindow();
                    }
                }
            }
            else
            {
                using (var verticalScope = new GUILayout.VerticalScope(GUI.skin.box))
                {
                    GUILayout.Label("Feature", GUI.skin.label);
                    GUILayout.Label("is only for single player or when host.", GUI.skin.label);
                    GUILayout.Label("Host can activate using ModManager.", GUI.skin.label);
                }
            }
        }

        private void OnClickCreateBugReportButton()
        {
            try
            {
                CreateBugReport();
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{ModName}.{ModName}:{nameof(OnClickCreateBugReportButton)}] throws exception: {exc.Message}");
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
                BugReportInfo = new BugReportInfo
                {
                    Topic = BugReportInfo.GetTopic(TopicDescription),
                    BugReportType = BugReportType,
                    Description = Description,
                    StepsToReproduce = BugReportInfo.GetStepsToReproduce(StepsToReproduce),
                    ExpectedBehaviour = ExpectedBehaviour,
                    MapCoordinates = BugReportInfo.GetMapCoordinates(player),
                    Note = BugReportInfo.GetScreenshotInfo(Note),
                    PcSpecs = BugReportInfo.GetPcSpecs()
                };

                CreateReports();

                Application.OpenURL(SteamForumBugReportUrl);

            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{ModName}.{ModName}:{nameof(CreateBugReport)}] throws exception: {exc.Message}");
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
            string timeStamp = DateTime.Now.ToString("yyyyMMddThhmmmsZ");
            string htmlReportName = $"{nameof(BugReportInfo)}_{timeStamp}.html";
            string jsonReportName = $"{nameof(BugReportInfo)}_{timeStamp}.json";

            if (FileWrite(htmlReportName, CreateBugReportAsHtml()))
            {
                ShowHUDBigInfo(
                   $"{htmlReportName} created",
                   $"{ModName} Info",
                   HUDInfoLogTextureType.Count.ToString());

               Application.OpenURL(ReportFile);
            }

            if (FileWrite(jsonReportName, GetBugReportAsJSON()))
            {
                ShowHUDBigInfo(
                   $"{jsonReportName} created",
                   $"{ModName} Info",
                   HUDInfoLogTextureType.Count.ToString());

               Application.OpenURL(ReportFile);
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
                                                                            $"         {BugReportInfo.Topic?.GameVersion}] - {BugReportInfo.Topic?.Description} :: Green Hell Bug Reports" +
                                                                            $"      </title>" +
                                                                            $"  </head>" +
                                                                            $"  <body>" +
                                                                            $"      <div class=\"topic\">" +
                                                                            $"          [{BugReportInfo.Topic?.GameVersion}] - {BugReportInfo.Topic?.Description}" +
                                                                            $"      </div>" +
                                                                            $"      <div class=\"content\">" +
                                                                            $"          <br>Type: {BugReportInfo.BugReportType}" +
                                                                            $"          <br>Description: {BugReportInfo.Description}" +
                                                                            $"          <ul>Steps to Reproduce:");
                foreach (var step in BugReportInfo.StepsToReproduce)
                {
                    bugReportBuilder.AppendLine($"          <li>Step {step.Rank}: {step.Description}</li>");
                }
                bugReportBuilder.AppendLine($"             </ul>" +
                                                                        $"               <br>Reproduce rate: {BugReportInfo.ReproduceRate}" +
                                                                        $"               <br>Expected behaviour: {BugReportInfo.ExpectedBehaviour}" +
                                                                        $"              <ul>My PC spec:" +
                                                                        $"                  <li>OS: {BugReportInfo.PcSpecs?.OS}</li>" +
                                                                        $"                  <li>CPU: {BugReportInfo.PcSpecs?.CPU}</li>" +
                                                                        $"                  <li>GPU: {BugReportInfo.PcSpecs?.GPU}</li>" +
                                                                        $"                  <li>RAM: {BugReportInfo.PcSpecs?.RAM}</li>" +
                                                                        $"              </ul>" +
                                                                        $"              <br>Note:  {BugReportInfo.Note}" +
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
                foreach (var step in BugReportInfo.StepsToReproduce)
                {
                    steps += $"" +
                        $"{{" +
                        $"\"Rank\": {step.Rank}," +
                        $"\"Description\": \"{step.Description}\"" +
                        $"}},";
                }
                reportTemplate = $"" +
                    $"{{" +
                    $"\"Topic\":" +
                    $"{{ " +
                    $"\"GameVersion\": \"{BugReportInfo.Topic?.GameVersion}\"," +
                    $"\"Description\": \"{BugReportInfo.Topic?.Description}\"" +
                    $"}}," +
                    $"\"Type\": \"{BugReportInfo.BugReportType}\"," +
                    $"\"Description\": \"{BugReportInfo.Description}\"," +
                    $"\"StepsToReproduce\": [" +
                    $"{steps}" +
                    $"]," +
                    $"\"ReproduceRate\": \"{BugReportInfo.ReproduceRate}\"," +
                    $"\"ExpectedBehaviour\": \"{BugReportInfo.ExpectedBehaviour}\"," +
                    $"\"PcSpecs\":" +
                    $"{{" +
                    $"\"OS\": \"{BugReportInfo.PcSpecs?.OS}\"," +
                    $"\"CPU\": \"{BugReportInfo.PcSpecs?.CPU}\"," +
                    $"\"GPU\": \"{BugReportInfo.PcSpecs?.GPU}\"," +
                    $"\"RAM\": \"{BugReportInfo.PcSpecs?.RAM}\"" +
                    $"}}," +
                    $"\"MapCoordinates\": \"{BugReportInfo.MapCoordinates?.ToString()}\"," +
                    $"\"Note\": \"{BugReportInfo.Note}\"" +
                    $"}}";

                return reportTemplate;
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{(ModName)}.{nameof(CommunityToolsScreen)}:{nameof(GetBugReportAsJSON)}] throws exception: {exc.Message}");
                return reportTemplate;
            }
        }
    }
}

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

        private static readonly string ReportPath = $"{Application.dataPath.Replace("GH_Data", "Logs")}";
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
        public bool IsModActiveForMultiplayer {
            get => _isActiveForMultiplayer;
            set => _isActiveForMultiplayer = FindObjectOfType(typeof(ModManager.ModManager)) != null && ModManager.ModManager.AllowModsForMultiplayer;
        }

        private bool _isActiveForSingleplayer;
        public bool IsModActiveForSingleplayer {
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
            if (IsModActiveForMultiplayer || IsModActiveForSingleplayer)
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
                BugReportInfo = new BugReportInfo();

                CreateReports();

                OnClickOpenSteamForumButton();
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
            ReportFile = CreateBugReportAsHtml();

            if (!string.IsNullOrEmpty(ReportFile))
            {
                ShowHUDBigInfo(
                   ReportCreatedMessage($"html report in {ReportPath}"),
                   $"{ModName} Info",
                   HUDInfoLogTextureType.Count.ToString());

                ReportFile = string.Empty;
            }

            ReportFile = GetBugReportAsJSON();

            if (!string.IsNullOrEmpty(ReportFile))
            {
                ShowHUDBigInfo(
                   ReportCreatedMessage($"json report in {ReportPath}"),
                   $"{ModName} Info",
                   HUDInfoLogTextureType.Count.ToString());

                ReportFile = string.Empty;
            }
        }

        private static string ReportCreatedMessage(string htmlReportName) => $"<color=#{ColorUtility.ToHtmlStringRGBA(Color.red)}>System</color>:\n{htmlReportName} created!";

        protected string CreateBugReportAsHtml()
        {
            StringBuilder bugReportBuilder = new StringBuilder("");

            try
            {
                bugReportBuilder.AppendLine($"<!DOCTYPE html>");
                bugReportBuilder.AppendLine($"<html class=\"client\">");
                bugReportBuilder.AppendLine($"  <head>");
                bugReportBuilder.AppendLine($"      <meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\">");
                bugReportBuilder.AppendLine($"      <title>");
                bugReportBuilder.AppendLine($"         {BugReportInfo.Topic?.GameVersion}] - {BugReportInfo.Topic?.Description} :: Green Hell Bug Reports");
                bugReportBuilder.AppendLine($"      </title>");
                bugReportBuilder.AppendLine($"  </head>");
                bugReportBuilder.AppendLine($"  <body>");
                bugReportBuilder.AppendLine($"      <div class=\"topic\">");
                bugReportBuilder.AppendLine($"          [{BugReportInfo.Topic?.GameVersion}] - {BugReportInfo.Topic?.Description}");
                bugReportBuilder.AppendLine($"      </div>");
                bugReportBuilder.AppendLine($"      <div class=\"content\">");
                bugReportBuilder.AppendLine($"          <br>Type: {BugReportInfo.BugReportType}");
                bugReportBuilder.AppendLine($"          <br>Description: {BugReportInfo.Description}");
                bugReportBuilder.AppendLine($"          <ul>Steps to Reproduce:");
                foreach (var step in BugReportInfo.StepsToReproduce)
                {
                    bugReportBuilder.AppendLine($"          <li>Step {step.Rank}: {step.Description}</li>");
                }
                bugReportBuilder.AppendLine($"             </ul>");
                bugReportBuilder.AppendLine($"               <br>Reproduce rate: {BugReportInfo.ReproduceRate}");
                bugReportBuilder.AppendLine($"               <br>Expected behaviour: {BugReportInfo.ExpectedBehaviour}");
                bugReportBuilder.AppendLine($"              <ul>My PC spec:");
                bugReportBuilder.AppendLine($"                  <li>OS: {BugReportInfo.PcSpecs?.OS}</li>");
                bugReportBuilder.AppendLine($"                  <li>CPU: {BugReportInfo.PcSpecs?.CPU}</li>");
                bugReportBuilder.AppendLine($"                  <li>GPU: {BugReportInfo.PcSpecs?.GPU}</li>");
                bugReportBuilder.AppendLine($"                  <li>RAM: {BugReportInfo.PcSpecs?.RAM}</li>");
                bugReportBuilder.AppendLine($"              </ul>");
                bugReportBuilder.AppendLine($"              <br>Note:  {BugReportInfo.Note}");
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
                bugReportBuilder.AppendLine($"\"GameVersion\": \"{BugReportInfo.Topic?.GameVersion}\",");
                bugReportBuilder.AppendLine($"\"Description\": \"{BugReportInfo.Topic?.Description}\"");
                bugReportBuilder.AppendLine($"}},");
                bugReportBuilder.AppendLine($"\"Type\": \"{BugReportInfo.BugReportType}\",");
                bugReportBuilder.AppendLine($"\"Description\": \"{BugReportInfo.Description}\",");
                bugReportBuilder.AppendLine($"\"StepsToReproduce\": [");
                foreach (var step in BugReportInfo.StepsToReproduce)
                {
                    bugReportBuilder.AppendLine($"{{");
                    bugReportBuilder.AppendLine($"\"Rank\": {step.Rank},");
                    bugReportBuilder.AppendLine($"\"Description\": \"{step.Description}\", ");
                    bugReportBuilder.AppendLine($"}},");
                }
                bugReportBuilder.AppendLine($"],");
                bugReportBuilder.AppendLine($"\"ReproduceRate\": \"{BugReportInfo.ReproduceRate}\",");
                bugReportBuilder.AppendLine($"\"ExpectedBehaviour\": \"{BugReportInfo.ExpectedBehaviour}\",");
                bugReportBuilder.AppendLine($"\"PcSpecs\":");
                bugReportBuilder.AppendLine($"{{");
                bugReportBuilder.AppendLine($"\"OS\": \"{BugReportInfo.PcSpecs?.OS}\",");
                bugReportBuilder.AppendLine($"\"CPU\": \"{BugReportInfo.PcSpecs?.CPU}\",");
                bugReportBuilder.AppendLine($"\"GPU\": \"{BugReportInfo.PcSpecs?.GPU}\",");
                bugReportBuilder.AppendLine($"\"RAM\": \"{BugReportInfo.PcSpecs?.RAM}\"");
                bugReportBuilder.AppendLine($"}},");
                bugReportBuilder.AppendLine($"\"MapCoordinates\": \"{BugReportInfo.MapCoordinates?.ToString()}\",");
                bugReportBuilder.AppendLine($"\"Note\": \"{BugReportInfo.Note}\"");
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

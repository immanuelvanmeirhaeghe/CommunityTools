using CommunityTools.Data;
using CommunityTools.Data.Enums;
using CommunityTools.Data.Interfaces;
using CommunityTools.Data.Modding;
using CommunityTools.Data.Reporting;
using CommunityTools.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using UnityEngine;

namespace CommunityTools
{
    /// <summary>
    /// CommunityTools is a mod that aims to be a tool for the gamer community and modders:
    /// Enable the mod UI by pressing [LeftAlt] + [B] or the key configurable in ModAPI.
    /// It helps in getting game metadata
    /// Take screenshots. These are placed inside the {game installation path}\Logs\Screenshots as timestamped PNG files.
    /// Create bug reports.  These are placed inside the {game installation path}\Logs folder as timestamped HTML and JSON files.
    /// Open official Creepy Jar and ModAPI support urls.
    /// </summary>
    public class CommunityTools : MonoBehaviour
    {
        private static CommunityTools Instance;
        private static readonly string ModName = nameof(CommunityTools);
        private static readonly string LogFilePath = $"{Application.dataPath.Replace("GH_Data", "Logs")}/{nameof(CommunityTools)}.log";
        private static readonly string RuntimeConfiguration = Path.Combine(Application.dataPath.Replace("GH_Data", "Mods"), $"{nameof(RuntimeConfiguration)}.xml");
        private static readonly string ReportPath = $"{Application.dataPath.Replace("GH_Data", "Logs")}";

        private static float CommunityToolsScreenTotalWidth { get; set; } = 700f;
        private static float CommunityToolsScreenTotalHeight { get; set; } = 350f;
        private static float CommunityToolsScreenMinWidth { get; set; } = 700f;
        private static float CommunityToolsScreenMaxWidth { get; set; } = Screen.width;
        private static float CommunityToolsScreenMinHeight { get; set; } = 50f;
        private static float CommunityToolsScreenMaxHeight { get; set; } = Screen.height;
        private static float CommunityToolsScreenStartPositionX { get; set; } = Screen.width / 2f;
        private static float CommunityToolsScreenStartPositionY { get; set; } = Screen.height / 2f;
        private static bool IsCommunityToolsScreenMinimized { get; set; } = false;
        private static int CommunityToolsScreenId { get; set; } = 0;

        private static float BugReportScreenTotalWidth { get; set; } = 100f;
        private static float BugReportScreenTotalHeight { get; set; } = 75f;
        private static float BugReportScreenMinWidth { get; set; } = 100f;
        private static float BugReportScreenMinHeight { get; set; } = 75f;
        private static float BugReportScreenMaxWidth { get; set; } = 100f;
        private static float BugReportScreenMaxHeight { get; set; } = 75f;
        private static float BugReportScreenStartPositionX { get; set; } = Screen.width / 3f;
        private static float BugReportScreenStartPositionY { get; set; } = Screen.height / 3f;
        private static bool IsBugReportScreenMinimized { get; set; } = false;
        private static int BugReportScreenId { get; set; } = 0;

        private bool ShowCommunityToolsScreen { get; set; } = false;
        private bool ShowBugReportScreen { get; set; } = false;
        private bool ShowCommunityToolsInfo { get; set; } = false;

        public static Rect CommunityToolsScreen = new Rect(CommunityToolsScreenStartPositionX, CommunityToolsScreenStartPositionY, CommunityToolsScreenTotalWidth, CommunityToolsScreenTotalHeight);
        public static Rect BugReportScreen = new Rect(BugReportScreenStartPositionX, BugReportScreenStartPositionY, BugReportScreenTotalWidth, BugReportScreenTotalHeight);

        public KeyCode ShortcutKey { get; set; } = KeyCode.B;

        public bool IsModActiveForMultiplayer { get; private set; } = false;
        public bool IsModActiveForSingleplayer => ReplTools.AmIMaster();

        public Vector2 ModInfoScrollViewPosition { get; set; } = Vector2.zero;
        public IConfigurableMod SelectedMod { get; set; } = default;

        private static Player LocalPlayer;
        private static HUDManager LocalHUDManager;  
        private static StylingManager LocalStylingManager;

        public string BugReportFile { get; set; }
        public string JsonReport { get; set; }
        public string JsonFilePath { get; set; }
        public string HtmlReport { get; set; }
        public string HtmlFilePath { get; set; }
        public BugReportInfo BugReportInfo { get; set; }
        public int SelectedBugReportTypeIndex { get; set; }
        public int SelectedReproduceRateIndex { get; set; }
        public string BugReportType { get; set; }
        public string ReproduceRate { get; set; }
        public string TopicDescription { get; set; }
        public string Description { get; set; }
        public string ExpectedBehaviour { get; set; }
        public string StepsToReproduce { get; set; }
        public string Note { get; set; }

        private string OnlyForSinglePlayerOrHostMessage()
             => "Only available for single player or when host. Host can activate using ModManager.";
        private string PermissionChangedMessage(string permission, string reason)
            => $"Permission to use mods and cheats in multiplayer was {permission} because {reason}.";
        private string HUDBigInfoMessage(string message, MessageType messageType, Color? headcolor = null)
            => $"<color=#{(headcolor != null ? ColorUtility.ToHtmlStringRGBA(headcolor.Value) : ColorUtility.ToHtmlStringRGBA(Color.red))}>{messageType}</color>\n{message}";
        protected virtual void OnlyForSingleplayerOrWhenHostBox()
        {
            using (var infoScope = new GUILayout.HorizontalScope(GUI.skin.box))
            {
                GUILayout.Label(OnlyForSinglePlayerOrHostMessage(), LocalStylingManager.ColoredCommentLabel(LocalStylingManager.DefaultAttentionColor));
            }
        }

        public virtual KeyCode GetShortcutKey(string buttonID)
        {
            var ConfigurableModList = GetModList();
            if (ConfigurableModList != null && ConfigurableModList.Count > 0)
            {
                SelectedMod = ConfigurableModList.Find(cfgMod => cfgMod.ID == ModName);
                return SelectedMod.ConfigurableModButtons.Find(cfgButton => cfgButton.ID == buttonID).ShortcutKey;
            }
            else
            {
                switch (buttonID)
                {
                    case nameof(ShortcutKey):
                        return KeyCode.B;
                    default:
                        return KeyCode.None;
                }
            }
        }

        private List<IConfigurableMod> GetModList()
        {
            List<IConfigurableMod> modList = new List<IConfigurableMod>();
            try
            {
                if (File.Exists(RuntimeConfiguration))
                {
                    using (XmlReader configFileReader = XmlReader.Create(new StreamReader(RuntimeConfiguration)))
                    {
                        while (configFileReader.Read())
                        {
                            configFileReader.ReadToFollowing("Mod");
                            do
                            {
                                string gameID = GameID.GreenHell.ToString();
                                string modID = configFileReader.GetAttribute(nameof(IConfigurableMod.ID));
                                string uniqueID = configFileReader.GetAttribute(nameof(IConfigurableMod.UniqueID));
                                string version = configFileReader.GetAttribute(nameof(IConfigurableMod.Version));

                                var configurableMod = new ConfigurableMod(gameID, modID, uniqueID, version);

                                configFileReader.ReadToDescendant("Button");
                                do
                                {
                                    string buttonID = configFileReader.GetAttribute(nameof(IConfigurableModButton.ID));
                                    string buttonKeyBinding = configFileReader.ReadElementContentAsString();

                                    configurableMod.AddConfigurableModButton(buttonID, buttonKeyBinding);

                                } while (configFileReader.ReadToNextSibling("Button"));

                                if (!modList.Contains(configurableMod))
                                {
                                    modList.Add(configurableMod);
                                }

                            } while (configFileReader.ReadToNextSibling("Mod"));
                        }
                    }
                }
                return modList;
            }
            catch (Exception exc)
            {
                HandleException(exc, nameof(GetModList));
                modList = new List<IConfigurableMod>();
                return modList;
            }
        }

        protected virtual void HandleException(Exception exc, string methodName)
        {
            string info = $"[{ModName}:{methodName}] throws exception -  {exc.TargetSite?.Name}:\n{exc.Message}\n{exc.InnerException}\n{exc.Source}\n{exc.StackTrace}";
            ModAPI.Log.Write(info);
            Debug.Log(info);
        }

        protected virtual void EnableCursor(bool blockPlayer = false)
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

        public virtual void ShowHUDBigInfo(string text, float duration = 3f)
        {
            string header = $"{ModName} Info";
            string textureName = HUDInfoLogTextureType.Count.ToString();

            HUDBigInfo bigInfo = (HUDBigInfo)LocalHUDManager.GetHUD(typeof(HUDBigInfo));
            HUDBigInfoData.s_Duration = duration;
            HUDBigInfoData bigInfoData = new HUDBigInfoData
            {
                m_Header = header,
                m_Text = text,
                m_TextureName = textureName,
                m_ShowTime = Time.time
            };
            bigInfo.AddInfo(bigInfoData);
            bigInfo.Show(true);
        }

        public virtual void ShowHUDInfoLog(string ItemInfo, string localizedTextKey)
        {
            Localization localization = GreenHellGame.Instance.GetLocalization();
            var messages = (HUDMessages)LocalHUDManager.GetHUD(typeof(HUDMessages));
            messages.AddMessage(localization.Get(localizedTextKey) + "  " + localization.Get(ItemInfo));
        }

        protected virtual void ModManager_onPermissionValueChanged(bool optionValue)
        {
            string reason = optionValue ? "the game host allowed usage" : "the game host did not allow usage";
            IsModActiveForMultiplayer = optionValue;

            ShowHUDBigInfo(
                          (optionValue ?
                            HUDBigInfoMessage(PermissionChangedMessage($"granted", $"{reason}"), MessageType.Info, Color.green)
                            : HUDBigInfoMessage(PermissionChangedMessage($"revoked", $"{reason}"), MessageType.Info, LocalStylingManager.DefaultAttentionColor))
                            );
        }

        protected virtual void Awake()
        {
            Instance = this;
        }

        protected virtual void OnDestroy()
        {
            Instance = null;
        }

        protected virtual void  Start()
        {
            ModManager.ModManager.onPermissionValueChanged += ModManager_onPermissionValueChanged;
            InitData();
            ShortcutKey = GetShortcutKey(nameof(ShortcutKey));
        }

        protected virtual void InitData()
        {
            LocalHUDManager = HUDManager.Get();
            LocalPlayer = Player.Get();
            LocalStylingManager = StylingManager.Get();
        }

        protected virtual void InitSkinUI()
        {
            GUI.skin = ModAPI.Interface.Skin;
        }

        public CommunityTools()
        {
            useGUILayout = true;
            Instance = this;
        }

        public static CommunityTools Get()
        {
            return Instance;
        }

        protected virtual void  Update()
        {
            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(ShortcutKey))
            {
                if (!ShowCommunityToolsScreen)
                {
                    InitData();
                    EnableCursor(true);
                }
                ToggleShowUI(0);
                if (!ShowCommunityToolsScreen)
                {
                    EnableCursor(false);
                }
            }
        }

        protected virtual void  ToggleShowUI(int controlId)
        {
            switch (controlId)
            {
                case 0:
                    ShowCommunityToolsScreen = !ShowCommunityToolsScreen;
                    return;
                case 1:
                    ShowBugReportScreen = !ShowBugReportScreen;
                    return;
                case 3:
                    ShowCommunityToolsInfo = !ShowCommunityToolsInfo;
                    return;
                default:
                    ShowCommunityToolsScreen = !ShowCommunityToolsScreen;
                    ShowBugReportScreen = !ShowBugReportScreen;
                    ShowCommunityToolsInfo = !ShowCommunityToolsInfo;
                    return;
            }
        }

       protected virtual void  OnGUI()
        {
            if (ShowCommunityToolsScreen)
            {
                InitData();
                InitSkinUI();
                ShowCommunityToolsWindow();
            }
            if (ShowBugReportScreen)
            {
                InitData();
                InitSkinUI();
                ShowBugReportWindow();
            }
        }

       protected virtual void  ShowCommunityToolsWindow()
        {
            if (CommunityToolsScreenId <= 0)
            {
                CommunityToolsScreenId = CommunityToolsScreen.GetHashCode();
            }
            string communityToolsScreenTitle = $"{ModName} created by [Dragon Legion] Immaanuel#4300";
            CommunityToolsScreen = GUILayout.Window(CommunityToolsScreenId, CommunityToolsScreen, InitCommunityToolsScreen, communityToolsScreenTitle,
                                                                                                         GUI.skin.window,
                                                                                                         GUILayout.ExpandWidth(true),
                                                                                                         GUILayout.MinWidth(CommunityToolsScreenMinWidth),
                                                                                                         GUILayout.MaxWidth(CommunityToolsScreenMaxWidth),
                                                                                                         GUILayout.ExpandHeight(true),
                                                                                                         GUILayout.MinHeight(CommunityToolsScreenMinHeight),
                                                                                                         GUILayout.MaxHeight(CommunityToolsScreenMaxHeight));
        }

        protected virtual void ShowBugReportWindow()
        {
            if (BugReportScreenId <= 0)
            {
                BugReportScreenId = BugReportScreen.GetHashCode();
            }
            string bugReportScreenTitle = $"{ModName} - {nameof(BugReportScreen)}";
            BugReportScreen = GUILayout.Window(BugReportScreenId, BugReportScreen, InitBugReportScreen, bugReportScreenTitle,
                                                                                                         GUI.skin.window,
                                                                                                         GUILayout.ExpandWidth(true),
                                                                                                         GUILayout.MinWidth(BugReportScreenMinWidth),
                                                                                                         GUILayout.MaxWidth(BugReportScreenMaxWidth),
                                                                                                         GUILayout.ExpandHeight(true),
                                                                                                         GUILayout.MinHeight(BugReportScreenMinHeight),
                                                                                                         GUILayout.MaxHeight(BugReportScreenMaxHeight));
        }

        protected virtual void InitBugReportScreen(int windowID)
        {
            BugReportScreenStartPositionX = BugReportScreen.x;
            BugReportScreenStartPositionY = BugReportScreen.y;
            BugReportScreenTotalWidth = BugReportScreen.width;
            using (new GUILayout.VerticalScope(GUI.skin.box))
            {
                BugReportScreenMenuBox();

                if (!IsBugReportScreenMinimized)
                {
                    BugReportBox();
                }
            }
            GUI.DragWindow(new Rect(0f, 0f, 10000f, 10000f));
        }

        protected virtual void BugReportScreenMenuBox()
        {
            string CollapseButtonText = IsBugReportScreenMinimized ? "O" : "-";
            if (GUI.Button(new Rect(BugReportScreen.width - 40f, 0f, 20f, 20f), CollapseButtonText, GUI.skin.button))
            {
                CollapseBugReportWindow();
            }

            if (GUI.Button(new Rect(BugReportScreen.width - 20f, 0f, 20f, 20f), "X", GUI.skin.button))
            {
                CloseWindow(1);
            }
        }

        protected virtual void ScreenMenuBox()
        {
            string CollapseButtonText = IsCommunityToolsScreenMinimized ? "O" : "-";
            if (GUI.Button(new Rect(CommunityToolsScreen.width - 40f, 0f, 20f, 20f), CollapseButtonText, GUI.skin.button))
            {
                CollapseWindow();
            }

            if (GUI.Button(new Rect(CommunityToolsScreen.width - 20f, 0f, 20f, 20f), "X", GUI.skin.button))
            {
                CloseWindow(0);
            }
        }

        protected virtual void CollapseWindow()
        {
            if (!IsCommunityToolsScreenMinimized)
            {
                CommunityToolsScreen = new Rect(CommunityToolsScreen.x, CommunityToolsScreen.y, CommunityToolsScreenTotalWidth, CommunityToolsScreenMinHeight);
                IsCommunityToolsScreenMinimized = true;
            }
            else
            {
                CommunityToolsScreen = new Rect(CommunityToolsScreen.x, CommunityToolsScreen.y, CommunityToolsScreenTotalWidth, CommunityToolsScreenTotalHeight);
                IsCommunityToolsScreenMinimized = false;
            }
            ShowCommunityToolsWindow();
        }

        protected virtual void CollapseBugReportWindow()
        {
            if (!IsBugReportScreenMinimized)
            {
                BugReportScreen = new Rect(BugReportScreen.x, BugReportScreen.y, BugReportScreenTotalWidth, BugReportScreenMinHeight);
                IsBugReportScreenMinimized = true;
            }
            else
            {
                BugReportScreen = new Rect(BugReportScreen.x, BugReportScreen.y, BugReportScreenTotalWidth, BugReportScreenTotalHeight);
                IsBugReportScreenMinimized = false;
            }
            ShowBugReportWindow();
        }

        protected virtual void  InitCommunityToolsScreen(int windowID)
        {
            CommunityToolsScreenStartPositionX = CommunityToolsScreen.x;
            CommunityToolsScreenStartPositionY = CommunityToolsScreen.y;
            CommunityToolsScreenTotalWidth = CommunityToolsScreen.width;

            using (new GUILayout.VerticalScope(GUI.skin.box))
            {
                ScreenMenuBox();
                if (!IsCommunityToolsScreenMinimized)
                {
                    CommunityToolsManagerBox();                    
                }
            }
            GUI.DragWindow(new Rect(0f, 0f, 10000f, 10000f));
        }

        protected virtual void CommunityToolsManagerBox()
        {
            try
            {
                if (IsModActiveForSingleplayer || IsModActiveForMultiplayer)
                {
                    using (new GUILayout.VerticalScope(GUI.skin.box))
                    {
                        GUILayout.Label($"{ModName} Manager", LocalStylingManager.ColoredHeaderLabel(LocalStylingManager.DefaultHeaderColor));
                        GUILayout.Label($"{ModName} Options", LocalStylingManager.ColoredSubHeaderLabel(LocalStylingManager.DefaultHeaderColor));

                        using (new GUILayout.VerticalScope(GUI.skin.box))
                        {
                            if (GUILayout.Button($"Mod Info", GUI.skin.button))
                            {
                                ToggleShowUI(3);
                            }
                            if (ShowCommunityToolsInfo)
                            {
                                CommunityToolsInfoBox();
                            }
                            MultiplayerOptionBox();
                            OnlineSupportOptionsBox();
                            if (GUILayout.Button($"Take a screenshot", GUI.skin.button, GUILayout.Width(150f)))
                            {
                                TakeScreenshot();
                            }
                            if (GUILayout.Button($"Log a bug", GUI.skin.button, GUILayout.Width(150f)))
                            {
                                ToggleShowUI(1);
                            }
                        }
                    }
                }
                else
                {
                    OnlyForSingleplayerOrWhenHostBox();
                }
            }
            catch (Exception exc)
            {
                HandleException(exc, nameof(CommunityToolsManagerBox));
            }
        }

        protected virtual void CommunityToolsInfoBox()
        {
            using (new GUILayout.VerticalScope(GUI.skin.box))
            {
                ModInfoScrollViewPosition = GUILayout.BeginScrollView(ModInfoScrollViewPosition, GUI.skin.scrollView, GUILayout.MinHeight(150f));

                GUILayout.Label("Mod Info", LocalStylingManager.ColoredSubHeaderLabel(LocalStylingManager.DefaultHighlightColor));

                using (new GUILayout.HorizontalScope(GUI.skin.box))
                {
                    GUILayout.Label($"{nameof(IConfigurableMod.GameID)}:", LocalStylingManager.FormFieldNameLabel);
                    GUILayout.Label($"{SelectedMod.GameID}", LocalStylingManager.FormFieldValueLabel);
                }
                using (new GUILayout.HorizontalScope(GUI.skin.box))
                {
                    GUILayout.Label($"{nameof(IConfigurableMod.ID)}:", LocalStylingManager.FormFieldNameLabel);
                    GUILayout.Label($"{SelectedMod.ID}", LocalStylingManager.FormFieldValueLabel);
                }
                using (new GUILayout.HorizontalScope(GUI.skin.box))
                {
                    GUILayout.Label($"{nameof(IConfigurableMod.UniqueID)}:", LocalStylingManager.FormFieldNameLabel);
                    GUILayout.Label($"{SelectedMod.UniqueID}", LocalStylingManager.FormFieldValueLabel);
                }
                using (new GUILayout.HorizontalScope(GUI.skin.box))
                {
                    GUILayout.Label($"{nameof(IConfigurableMod.Version)}:", LocalStylingManager.FormFieldNameLabel);
                    GUILayout.Label($"{SelectedMod.Version}", LocalStylingManager.FormFieldValueLabel);
                }

                GUILayout.Label("Buttons Info", LocalStylingManager.ColoredSubHeaderLabel(LocalStylingManager.DefaultHighlightColor));
                foreach (IConfigurableModButton configurableModButton in SelectedMod.ConfigurableModButtons)
                {
                    using (new GUILayout.HorizontalScope(GUI.skin.box))
                    {
                        GUILayout.Label($"{nameof(IConfigurableModButton.ID)}:", LocalStylingManager.FormFieldNameLabel);
                        GUILayout.Label($"{configurableModButton.ID}", LocalStylingManager.FormFieldValueLabel);
                    }
                    using (new GUILayout.HorizontalScope(GUI.skin.box))
                    {
                        GUILayout.Label($"{nameof(IConfigurableModButton.KeyBinding)}:", LocalStylingManager.FormFieldNameLabel);
                        GUILayout.Label($"{KeyCode.LeftAlt}+{configurableModButton.KeyBinding}", LocalStylingManager.FormFieldValueLabel);
                    }
                }

                GUILayout.EndScrollView();
            }
        }

        protected virtual void MultiplayerOptionBox()
        {
            try
            {
                using (new GUILayout.VerticalScope(GUI.skin.box))
                {
                    GUILayout.Label("Multiplayer Options", LocalStylingManager.ColoredSubHeaderLabel(Color.yellow));

                    string multiplayerOptionMessage = string.Empty;
                    if (IsModActiveForSingleplayer || IsModActiveForMultiplayer)
                    {
                        if (IsModActiveForSingleplayer)
                        {
                            multiplayerOptionMessage = $"you are the game host";
                        }
                        if (IsModActiveForMultiplayer)
                        {
                            multiplayerOptionMessage = $"the game host allowed usage";
                        }
                        GUILayout.Label(PermissionChangedMessage($"granted", multiplayerOptionMessage), LocalStylingManager.ColoredFieldValueLabel(Color.green));
                    }
                    else
                    {
                        if (!IsModActiveForSingleplayer)
                        {
                            multiplayerOptionMessage = $"you are not the game host";
                        }
                        if (!IsModActiveForMultiplayer)
                        {
                            multiplayerOptionMessage = $"the game host did not allow usage";
                        }
                        GUILayout.Label(PermissionChangedMessage($"revoked", $"{multiplayerOptionMessage}"), LocalStylingManager.ColoredFieldValueLabel(Color.yellow));
                    }
                }
            }
            catch (Exception exc)
            {
                HandleException(exc, nameof(MultiplayerOptionBox));
            }
        }
        
        protected virtual void  OnlineSupportOptionsBox()
        {
            using (new GUILayout.VerticalScope(GUI.skin.box))
            {
                GUILayout.Label("Online Support Options", LocalStylingManager.ColoredSubHeaderLabel(LocalStylingManager.DefaultHeaderColor));

                GUILayout.Label($"Depending what you click, this will open your external default e-mail or web browser program! The game will be paused.", LocalStylingManager.ColoredCommentLabel(LocalStylingManager.DefaultAttentionColor));
                using (new GUILayout.HorizontalScope(GUI.skin.box))
                {
                    GUILayout.Label("Click to send your question by mail to Creepy Jar Help", LocalStylingManager.TextLabel);
                    if (GUILayout.Button("Send mail", GUI.skin.button, GUILayout.Width(150f)))
                    {
                        ShowMainMenu();
                        OnClickSendMailButton();
                    }
                }
                using (new GUILayout.HorizontalScope(GUI.skin.box))
                {
                    GUILayout.Label("Try to find help or report your bug on Creepy Jar's Reddit", LocalStylingManager.TextLabel);
                    if (GUILayout.Button("Open Reddit", GUI.skin.button, GUILayout.Width(150f)))
                    {
                        ShowMainMenu();
                        OnClickOpenRedditButton();
                    }
                }
                using (new GUILayout.HorizontalScope(GUI.skin.box))
                {
                    GUILayout.Label("Try to find help in the Steam Community guides", LocalStylingManager.TextLabel);
                    if (GUILayout.Button("Open guide", GUI.skin.button, GUILayout.Width(150f)))
                    {
                        ShowMainMenu();
                        OnClickOpenSteamGuideButton();
                    }
                }
                using (new GUILayout.HorizontalScope(GUI.skin.box))
                {
                    GUILayout.Label("Look at the reported bugs or report your bug on the Steam Community discussion forum", LocalStylingManager.TextLabel);
                    if (GUILayout.Button("Open forum", GUI.skin.button, GUILayout.Width(150f)))
                    {
                        ShowMainMenu();
                        OnClickOpenSteamForumButton();
                    }
                }
                using (new GUILayout.HorizontalScope(GUI.skin.box))
                {
                    GUILayout.Label("Go to the official Green Hell game wiki", LocalStylingManager.TextLabel);
                    if (GUILayout.Button("Open wiki", GUI.skin.button, GUILayout.Width(150f)))
                    {
                        ShowMainMenu();
                        OnClickOpenWikiButton();
                    }
                }
                using (new GUILayout.HorizontalScope(GUI.skin.box))
                {
                    GUILayout.Label("Look for info on the ModAPI Hub", LocalStylingManager.TextLabel);
                    if (GUILayout.Button("Open ModAPI Hub", GUI.skin.button, GUILayout.Width(150f)))
                    {
                        ShowMainMenu();
                        OnClickOpenModAPIHubButton();
                    }
                }
                using (new GUILayout.HorizontalScope(GUI.skin.box))
                {
                    GUILayout.Label("Join the ModAPI Discord", LocalStylingManager.TextLabel);
                    if (GUILayout.Button("Join ModAPI Discord", GUI.skin.button, GUILayout.Width(150f)))
                    {
                        ShowMainMenu();
                        OnClickJoinModAPIDiscordButton();
                    }
                }
            }
        }

        protected virtual void ShowMainMenu()
        {
           MenuInGameManager.Get().ShowScreen(typeof(MainMenu));
        }

        protected virtual void ShowMenuDebugSelectMode()
        {
            MenuInGameManager.Get().ShowScreen(typeof(MenuDebugSelectMode));
        }

        protected virtual void CloseWindow(int controlId)
        {
            switch (controlId)
            {
                case 0:
                    ShowCommunityToolsScreen = false;
                    EnableCursor(false);
                    return;
                case 1:
                    ShowBugReportScreen = false;
                    EnableCursor(false);
                    return;
                default:
                    ShowCommunityToolsScreen = false;
                    ShowBugReportScreen = false;
                    EnableCursor(false);
                    return;
            }
        }

       protected virtual void  OnClickOpenSteamGuideButton()
        {
            Application.OpenURL(BugReportInfoHelpers.SteamForumGuideUrl);
        }

       protected virtual void  OnClickOpenSteamForumButton()
        {
            Application.OpenURL(BugReportInfoHelpers.SteamForumBugReportUrl);
        }

        protected virtual void OnClickOpenRedditButton()
        {
            Application.OpenURL(BugReportInfoHelpers.CreepyJarRedditBugReportUrl);
        }

        protected virtual void OnClickOpenModAPIHubButton()
        {
            Application.OpenURL(BugReportInfoHelpers.ModAPIHubUrl);
        }

        protected virtual void OnClickOpenWikiButton()
        {
            Application.OpenURL(BugReportInfoHelpers.CreenHellWikiFandomUrl);
        }

        protected virtual void OnClickJoinModAPIDiscordButton()
        {
            Application.OpenURL(BugReportInfoHelpers.ModAPIDiscordUrl);
        }

        protected virtual void  OnClickSendMailButton()
        {
            Application.OpenURL(BugReportInfoHelpers.CreepyJarContactEmail);
        }

        protected virtual void BugReportBox()
        {
            using (new GUILayout.VerticalScope(GUI.skin.box))
            {
                BugReportFormBox();

                using (new GUILayout.HorizontalScope(GUI.skin.box))
                {
                    GUILayout.Label($"To log your bug report in HTML and in JSON, click", LocalStylingManager.TextLabel);
                    if (GUILayout.Button("Create report", GUI.skin.button, GUILayout.Width(150f)))
                    {
                        OnClickCreateBugReportButton();
                    }
                }

                if (!string.IsNullOrEmpty(HtmlFilePath))
                {
                    using (new GUILayout.HorizontalScope(GUI.skin.box))
                    {
                        if (GUILayout.Button($"Open HTML report", GUI.skin.button, GUILayout.Width(150f)))
                        {
                            ShowMainMenu();
                            Application.OpenURL(HtmlFilePath);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(JsonFilePath))
                {
                    using (new GUILayout.HorizontalScope(GUI.skin.box))
                    {
                        if (GUILayout.Button($"Open JSON report", GUI.skin.button, GUILayout.Width(150f)))
                        {
                            ShowMainMenu();
                            Application.OpenURL(JsonFilePath);
                        }
                    }
                }

            }
        }

        protected virtual void OnClickCreateBugReportButton()
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

        protected virtual void BugReportFormBox()
        {
            using (new GUILayout.VerticalScope(GUI.skin.box))
            {
                if (BugReportInfo == null)
                {
                    BugReportInfo = new BugReportInfo();
                }
                string[] bugReportTypes = BugReportInfoHelpers.GetBugReportTypes();
                string[] reproduceRates = BugReportInfoHelpers.GetReproduceRates();
                int _SelectedBugReportTypeIndex = SelectedBugReportTypeIndex;
                BugReportTypes _SelectedBugReportType = EnumUtils<BugReportTypes>.GetValue(bugReportTypes[SelectedBugReportTypeIndex]);
                int _SelectedReproduceRateIndex = SelectedReproduceRateIndex;
                ReproduceRates _SelectedReproduceRate = EnumUtils<ReproduceRates>.GetValue(reproduceRates[SelectedReproduceRateIndex]);

                GUILayout.Label("Bug Report Form", LocalStylingManager.ColoredSubHeaderLabel(LocalStylingManager.DefaultHighlightColor));

                using (new GUILayout.HorizontalScope(GUI.skin.box))
                {
                    GUILayout.Label(nameof(BugReportInfo.Topic), LocalStylingManager.FormFieldNameLabel);
                    BugReportInfo.Topic.Description = GUILayout.TextField(BugReportInfo.Topic.Description, LocalStylingManager.FormInputTextField);                    
                }
                using (new GUILayout.HorizontalScope(GUI.skin.box))
                {
                    BugReportInfo.Type = EnumUtils<BugReportTypes>.GetValue(bugReportTypes[SelectedBugReportTypeIndex]);

                    GUILayout.Label(nameof(BugReportInfo.Type), LocalStylingManager.FormFieldNameLabel);
                    SelectedBugReportTypeIndex = GUILayout.SelectionGrid(SelectedBugReportTypeIndex, bugReportTypes, bugReportTypes.Length, LocalStylingManager.ColoredSelectedGridButton(_SelectedBugReportTypeIndex == SelectedBugReportTypeIndex));
                    if (_SelectedBugReportTypeIndex != SelectedBugReportTypeIndex)
                    {
                        BugReportInfo.Type =EnumUtils<BugReportTypes>.GetValue(bugReportTypes[SelectedBugReportTypeIndex]);
                    }
                }
                using (new GUILayout.HorizontalScope(GUI.skin.box))
                {
                    GUILayout.Label(nameof(BugReportInfo.Description), LocalStylingManager.FormFieldNameLabel);
                    BugReportInfo.Description = GUILayout.TextArea(BugReportInfo.Description, LocalStylingManager.FormInputTextAreaField);
                }
                using (new GUILayout.HorizontalScope(GUI.skin.box))
                {
                    GUILayout.Label(nameof(BugReportInfo.StepsToReproduce), LocalStylingManager.FormFieldNameLabel);
                    StepsToReproduce = GUILayout.TextArea(StepsToReproduce, LocalStylingManager.FormInputTextAreaField);

                    if (!string.IsNullOrEmpty(StepsToReproduce))
                    {
                        List<StepToReproduce> steps = BugReportInfoHelpers.GetStepsToReproduce(StepsToReproduce);
                        BugReportInfo.StepsToReproduce = steps;
                    }
                }
                using (new GUILayout.HorizontalScope(GUI.skin.box))
                {
                    BugReportInfo.ReproduceRate = EnumUtils<ReproduceRates>.GetValue(reproduceRates[SelectedReproduceRateIndex]);

                    GUILayout.Label(nameof(BugReportInfo.ReproduceRate), LocalStylingManager.FormFieldNameLabel);
                    SelectedReproduceRateIndex = GUILayout.SelectionGrid(SelectedReproduceRateIndex, reproduceRates, reproduceRates.Length, LocalStylingManager.ColoredSelectedGridButton(_SelectedReproduceRateIndex == SelectedReproduceRateIndex));
                    if (_SelectedReproduceRateIndex != SelectedReproduceRateIndex)
                    {
                        BugReportInfo.ReproduceRate = EnumUtils<ReproduceRates>.GetValue(reproduceRates[SelectedReproduceRateIndex]);
                    }
                }
                using (new GUILayout.HorizontalScope(GUI.skin.box))
                {
                    GUILayout.Label(nameof(BugReportInfo.ExpectedBehaviour), LocalStylingManager.FormFieldNameLabel);
                    BugReportInfo.ExpectedBehaviour = GUILayout.TextArea(BugReportInfo.ExpectedBehaviour, LocalStylingManager.FormInputTextAreaField);
                }
                using (new GUILayout.HorizontalScope(GUI.skin.box))
                {
                    GUILayout.Label(nameof(BugReportInfo.Note), LocalStylingManager.FormFieldNameLabel);
                    BugReportInfo.Note = GUILayout.TextArea(BugReportInfo.Note, LocalStylingManager.FormInputTextAreaField);
                }
            }
        }

        protected virtual void  CreateBugReport()
        {
            try
            {
                CreateReports();
            }
            catch (Exception exc)
            {
                HandleException(exc, nameof(CreateBugReport));
            }
        }

        protected virtual void TakeScreenshot()
        {
            try
            {
                BugReportInfoHelpers.CaptureScreenshot(out string ts, out string shot);
                if (!string.IsNullOrEmpty(shot))
                {
                    string taken = $"<color=#{ColorUtility.ToHtmlStringRGBA(Color.green)}>System</color>:\n{shot} taken!";
                    ShowHUDBigInfo(taken);
                }
                else
                {
                    string nottaken = $"Could not take screenshot. Something went wrong. Verify logs.";
                    ShowHUDBigInfo(nottaken);
                }
            }
            catch (Exception exc)
            {
                HandleException(exc, nameof(CreateBugReport));
            }
        }

        protected bool FileWrite(string fileName, string fileContent)
        {
            try
            {
                byte[] fileContentData = Encoding.UTF8.GetBytes(fileContent);

                if (!Directory.Exists(LogFilePath))
                {
                    Directory.CreateDirectory(LogFilePath);
                }

                BugReportFile = Path.Combine(LogFilePath, fileName);
                if (!File.Exists(BugReportFile))
                {
                    using (FileStream fileStream = File.Create(BugReportFile))
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

        protected int FileRead(string file, byte[] data, int length)
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

       protected virtual void  CreateReports()
        {
            string timeStamp = DateTime.Now.ToString("yyyyMMddThhmmmsZ");

            HtmlFilePath = Path.Combine(ReportPath, $"{nameof(BugReportFile)}_{timeStamp}.html");
            BugReportFile = CreateBugReportAsHtml();
            if (!string.IsNullOrEmpty(BugReportFile))
            {
                File.WriteAllText(HtmlFilePath, BugReportFile);
                ShowHUDBigInfo(HUDBigInfoMessage(ReportCreatedMessage($"{HtmlFilePath} created!"), MessageType.Info, Color.green));
               
                BugReportFile = string.Empty;
            }
            
            JsonFilePath = Path.Combine(ReportPath, $"{nameof(BugReportFile)}_{timeStamp}.json");
            BugReportFile = GetBugReportAsJSON();
            if (!string.IsNullOrEmpty(BugReportFile))
            {
                File.WriteAllText(JsonFilePath, BugReportFile);
                ShowHUDBigInfo(HUDBigInfoMessage(ReportCreatedMessage($"{JsonFilePath} created!"), MessageType.Info, Color.green));

                BugReportFile = string.Empty;
            }
        }

        protected virtual string ReportCreatedMessage(string htmlReportName)
        {
            return $"<color=#{ColorUtility.ToHtmlStringRGBA(Color.green)}>System</color>:\n{htmlReportName} created!";
        }

        protected string CreateBugReportAsHtml()
        {
            try
            {
                HtmlReport = BugReportInfoHelpers.CreateBugReportAsHtml(BugReportInfo);
                return HtmlReport;
            }
            catch (Exception exc)
            {
                HandleException(exc, nameof(CreateBugReportAsHtml));
                return string.Empty;
            }
        }

        protected string GetBugReportAsJSON()
        {
            try
            {
                JsonReport = BugReportInfoHelpers.CreateBugReportAsJSON(BugReportInfo);
                return JsonReport;
            }
            catch (Exception exc)
            {
                HandleException(exc, nameof(GetBugReportAsJSON));
                return string.Empty;
            }
        }
    }

}

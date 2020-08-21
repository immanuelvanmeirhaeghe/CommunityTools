using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CommunityTools.GameObjects.UI
{
    class BaseScreen : MenuDebugScreen
    {
        private static BaseScreen s_Instance;

        protected static GUIStyle windowStyle;
        protected static GUIStyle labelStyle;
        protected static GUIStyle textFieldStyle;
        protected static GUIStyle textAreaStyle;
        protected static GUIStyle buttonStyle;
        protected static GUIStyle toggleStyle;

        protected static MenuInGameManager menuInGameManager;
        protected static HUDManager hUDManager;
        protected static Player player;

        protected bool showUI = false;
        public bool IsActivated = false;

        public BaseScreen()
        {
            IsActivated = true;
            s_Instance = this;
        }

        public static BaseScreen Get()
        {
            return s_Instance;
        }

        protected override  void Update()
        {
            base.Update();
        }

        public virtual void OnGUI()
        {
            if (showUI)
            {
                InitData();
                InitSkinUI();
            }
        }

        private static void InitData()
        {
            menuInGameManager = MenuInGameManager.Get();

            hUDManager = HUDManager.Get();

            player = Player.Get();
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

        protected static void ShowHUDBigInfo(string text, string header, string textureName)
        {
            var localization = GreenHellGame.Instance.GetLocalization();
            HUDBigInfo hUDBigInfo = (HUDBigInfo)hUDManager.GetHUD(typeof(HUDBigInfo));
            HUDBigInfoData hUDBigInfoData = new HUDBigInfoData
            {
                m_Header = header,
                m_Text = localization.Get(text),
                m_TextureName = textureName,
                m_ShowTime = Time.time
            };
            hUDBigInfo.AddInfo(hUDBigInfoData);
            hUDBigInfo.Show(true);
        }

        protected static void ShowHUDInfoLog(string text, string key)
        {
            var localization = GreenHellGame.Instance.GetLocalization();
            HUDMessages hUDMessages = (HUDMessages)hUDManager.GetHUD(typeof(HUDMessages));
            hUDMessages.AddMessage(
                $"{localization.Get(key)}  {localization.Get(text)} "
                );
        }

        protected static void EnableCursor(bool enabled = false)
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
                ModAPI.Log.Write($"[{nameof(CommunityTools)}.{nameof(CommunityToolsScreen)}:{nameof(FileWrite)}] throws exception: {exc.Message}");
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
                ModAPI.Log.Write($"[{nameof(CommunityToolsScreen)}.{nameof(CommunityToolsScreen)}:{nameof(FileRead)}] throws exception: {exc.Message}");
                return 0;
            }
        }
    }
}

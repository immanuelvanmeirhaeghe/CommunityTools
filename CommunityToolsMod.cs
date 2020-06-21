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

        private static ItemsManager itemsManager;

        private static HUDManager hUDManager;

        private static Player player;

        private static GUIStyle labelStyle;

        public CommunityToolsMod()
        {
            IsCommunityToolsModActive = true;
            s_Instance = this;
        }

        public static CommunityToolsMod Get()
        {
            return s_Instance;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.RightControl) && Input.GetKeyDown(KeyCode.Keypad5))
            {
                if (!showUI)
                {
                    itemsManager = ItemsManager.Get();

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
            GUI.Box(new Rect(10f, 10f, 300f, 150f), "Community Tools", GUI.skin.window);

            if (GUI.Button(new Rect(30f, 25f, 200f, 20f), "Create bug report", GUI.skin.button))
            {
                OnClickCreateBugReportButton();
                showUI = false;
                EnableCursor(false);
            }

            GUI.Label(new Rect(30f, 50f, 200f, 20f), "Press Printscreen to post the bug report.", labelStyle);
        }

        private void OnClickCreateBugReportButton()
        {
            try
            {
                var texts = GreenHellGame.Instance.GetLocalization().GetLocalizedtexts();
                foreach (var kvp in texts)
                {
                    ModAPI.Log.Write($"\tLocalized Texts");
                    ModAPI.Log.Write($"\t\tKey = {kvp.Key} Value = {kvp.Value}");
                }
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{nameof(CommunityToolsMod)}.{nameof(CommunityToolsMod)}:{nameof(OnClickCreateBugReportButton)}] throws exception: {exc.Message}");
            }
        }

        public static void ShowHUDInfoLog(string itemID, string localizedTextKey)
        {
            var localization = GreenHellGame.Instance.GetLocalization();
            HUDMessages hUDMessages = (HUDMessages)hUDManager.GetHUD(typeof(HUDMessages));
            hUDMessages.AddMessage(
                $"{localization.Get(localizedTextKey)}  {localization.Get(itemID)}"
                );
        }

        private static void InitSkinUI()
        {
            GUI.skin = ModAPI.Interface.Skin;

            if (labelStyle == null)
            {
                labelStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 14
                };
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

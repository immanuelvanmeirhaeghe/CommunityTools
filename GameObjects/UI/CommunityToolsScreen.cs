using CommunityTools.GameObjects;
using CommunityTools.GameObjects.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace CommunityTools.GameObjects.UI
{
    /// <summary>
    /// The Mod API UI for the mod.
    /// Enabled pressing RightCtrl+NumPad5.
    /// </summary>
    class CommunityToolsScreen : BaseScreen
    {
        private static CommunityToolsScreen s_Instance;

        protected static BugReportScreen bugReportScreen;

        public CommunityToolsScreen()
        {
            IsActivated = true;
            bugReportScreen = new BugReportScreen();
            s_Instance = this;
        }

        protected override void Update()
        {
            base.Update();
            if (Input.GetKeyDown(KeyCode.Home))
            {
                if (!showUI)
                {
                    InitData();
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
            if (bugReportScreen == null)
            {
                bugReportScreen = (BugReportScreen)menuInGameManager.GetMenu(typeof(BugReportScreen));
                bugReportScreen.Show();
            }
        }

        private static void OnClickPrintTextDictionaryButton()
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
                ModAPI.Log.Write($"[{nameof(CommunityTools)}.{nameof(CommunityToolsScreen)}:{nameof(OnClickPrintTextDictionaryButton)}] throws exception: {exc.Message}");
            }
        }
    }
}

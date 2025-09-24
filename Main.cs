using Kbg.NppPluginNET.PluginInfrastructure;
using NppPluginNET.Utils;
using nppVisualXml.Storage;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Kbg.NppPluginNET
{
    class Main
    {
        public static readonly string PluginConfigDirectory = Path.Combine(Npp.notepad.GetConfigDirectory(), PluginName);
        internal const string PluginName = "nppVisualXml";
        static XmlViewer XmlViewer = null;
        static About About = null;
        static Settings MySettings = null;
        public static bool isShuttingDown = false;


        public static void OnNotification(ScNotification notification)
        {
            // This method is invoked whenever something is happening in notepad++
            // use eg. as
            // if (notification.Header.Code == (uint)NppMsg.NPPN_xxx)
            // { ... }
            // or
            //
            // if (notification.Header.Code == (uint)SciMsg.SCNxxx)
            // { ... }
            switch ((NppMsg)notification.Header.Code)
            {
                case NppMsg.NPPN_BUFFERACTIVATED:
                case NppMsg.NPPN_FILESAVED:
                    XmlViewer?.RefreshFromActiveDoc();
                    break;
            }

        }

        internal static void CommandMenuInit()
        {
            PluginBase.SetCommand(0, "Show VisualXml", myDockableDialog);
            PluginBase.SetCommand(1, "&About", AboutnppVisualXml);
        }

        internal static void SetToolBarIcons()
        {

        }

        internal static void PluginCleanUp()
        {

        }


        internal static void myDockableDialog()
        {
            MySettings = new Settings();
            MySettings.Load();

            ToggleXmlViewerUI();
        }
        /// <summary>
        /// Shows the "About" dialog window
        /// </summary>
        public static void AboutnppVisualXml()
        {
            About = new About();
            About.ShowDialog();
            About = null;

        }

        private static void ToggleXmlViewerUI()
        {
            XmlViewerUIVisible();
        }

        public static void XmlViewerUIVisible(bool? show = null)
        {
            if (XmlViewer == null || XmlViewer.IsDisposed)
            {
                XmlViewer = new XmlViewer
                {
                    settings = MySettings
                };

                XmlViewer.LoadSettings();
                XmlViewer.RefreshFromActiveDoc();

                IntPtr hwndClient = XmlViewer.Handle;

                var data = new NppTbData
                {
                    hClient = hwndClient,
                    pszName = "VisualXml",
                    dlgID = 0,
                    uMask = NppTbMsg.DWS_DF_CONT_RIGHT | NppTbMsg.DWS_ICONBAR,
                    hIconTab = (uint)IntPtr.Zero,
                    pszModuleName = PluginName
                };
                IntPtr pData = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(NppTbData)));
                try
                {
                    Marshal.StructureToPtr(data, pData, false);

                    // Register the dockable window
                    Win32.SendMessage(
                        PluginBase.nppData._nppHandle,
                        (uint)NppMsg.NPPM_DMMREGASDCKDLG,
                        0, pData);
                }
                finally
                {
                    Marshal.FreeHGlobal(pData);
                }

                // First registration doesn't auto-show: do it explicitly
                if (show ?? true)
                {
                    Win32.SendMessage(
                        PluginBase.nppData._nppHandle,
                        (uint)NppMsg.NPPM_DMMSHOW,
                        0, hwndClient);
                }

                return;
            }

            // Already registered: toggle or force
            if (show ?? !XmlViewer.Visible)
            {
                Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_DMMSHOW, 0, XmlViewer.Handle);
            }
            else
            {
                Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_DMMHIDE, 0, XmlViewer.Handle);
            }
        }
    }
}
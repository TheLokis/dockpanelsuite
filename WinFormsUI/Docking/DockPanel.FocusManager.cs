using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics.CodeAnalysis;

namespace WeifenLuo.WinFormsUI.Docking
{
    internal interface IContentFocusManager
    {
        void Activate(IDockContent content);
        void GiveUpFocus(IDockContent content);
        void AddToList(IDockContent content);
        void RemoveFromList(IDockContent content);
    }

    partial class DockPanel
    {
        private interface IFocusManager
        {
            void SuspendFocusTracking();
            void ResumeFocusTracking();
            bool IsFocusTrackingSuspended { get; }
            IDockContent ActiveContent { get; }
            DockPane ActivePane { get; }
            IDockContent ActiveDocument { get; }
            DockPane ActiveDocumentPane { get; }
        }

        private class FocusManagerImpl : Component, IContentFocusManager, IFocusManager
        {
            private class HookEventArgs : EventArgs
            {
                [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
                public int HookCode;
                [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
                public IntPtr wParam;
                public IntPtr lParam;
            }

            private class LocalWindowsHook : IDisposable
            {
                // Internal properties
                private IntPtr m_hHook = IntPtr.Zero;
                private NativeMethods.HookProc m_filterFunc = null;
                private Win32.HookType m_hookType;

                // Event delegate
                public delegate void HookEventHandler(object sender, HookEventArgs e);

                // Event: HookInvoked 
                public event HookEventHandler HookInvoked;
                protected void OnHookInvoked(HookEventArgs e)
                {
                    if (HookInvoked != null)
                        HookInvoked(this, e);
                }

                public LocalWindowsHook(Win32.HookType hook)
                {
                    this.m_hookType = hook;
                    this.m_filterFunc = new NativeMethods.HookProc(this.CoreHookProc);
                }

                // Default filter function
                public IntPtr CoreHookProc(int code, IntPtr wParam, IntPtr lParam)
                {
                    if (code < 0)
                        return NativeMethods.CallNextHookEx(this.m_hHook, code, wParam, lParam);

                    // Let clients determine what to do
                    HookEventArgs e = new HookEventArgs();
                    e.HookCode = code;
                    e.wParam = wParam;
                    e.lParam = lParam;
                    this.OnHookInvoked(e);

                    // Yield to the next hook in the chain
                    return NativeMethods.CallNextHookEx(this.m_hHook, code, wParam, lParam);
                }

                // Install the hook
                public void Install()
                {
                    if (this.m_hHook != IntPtr.Zero)
                        this.Uninstall();

                    int threadId = NativeMethods.GetCurrentThreadId();
                    this.m_hHook = NativeMethods.SetWindowsHookEx(this.m_hookType, this.m_filterFunc, IntPtr.Zero, threadId);
                }

                // Uninstall the hook
                public void Uninstall()
                {
                    if (this.m_hHook != IntPtr.Zero)
                    {
                        NativeMethods.UnhookWindowsHookEx(this.m_hHook);
                        this.m_hHook = IntPtr.Zero;
                    }
                }

                ~LocalWindowsHook()
                {
                    this.Dispose(false);
                }

                public void Dispose()
                {
                    this.Dispose(true);
                    GC.SuppressFinalize(this);
                }

                protected virtual void Dispose(bool disposing)
                {
                    this.Uninstall();
                }
            }

            // Use a static instance of the windows hook to prevent stack overflows in the windows kernel.
            [ThreadStatic]
            private static LocalWindowsHook sm_localWindowsHook;

            private readonly LocalWindowsHook.HookEventHandler m_hookEventHandler;

            public FocusManagerImpl(DockPanel dockPanel)
            {
                this.m_dockPanel = dockPanel;
                if (Win32Helper.IsRunningOnMono)
                    return;
                this.m_hookEventHandler = new LocalWindowsHook.HookEventHandler(this.HookEventHandler);

                // Ensure the windows hook has been created for this thread
                if (sm_localWindowsHook == null)
                {
                    sm_localWindowsHook = new LocalWindowsHook(Win32.HookType.WH_CALLWNDPROCRET);
                    sm_localWindowsHook.Install();
                }

                sm_localWindowsHook.HookInvoked += this.m_hookEventHandler;
            }

            private DockPanel m_dockPanel;
            public DockPanel DockPanel
            {
                get { return this.m_dockPanel; }
            }

            private bool m_disposed = false;
            protected override void Dispose(bool disposing)
            {
                if (!this.m_disposed && disposing)
                {
                    if (!Win32Helper.IsRunningOnMono)
                    {
                        sm_localWindowsHook.HookInvoked -= this.m_hookEventHandler;
                    }

                    this.m_disposed = true;
                }

                base.Dispose(disposing);
            }

            private IDockContent m_contentActivating = null;
            private IDockContent ContentActivating
            {
                get { return this.m_contentActivating; }
                set { this.m_contentActivating = value; }
            }

            public void Activate(IDockContent content)
            {
                if (this.IsFocusTrackingSuspended)
                {
                    this.ContentActivating = content;
                    return;
                }

                if (content == null)
                    return;
                DockContentHandler handler = content.DockHandler;
                if (handler.Form.IsDisposed)
                    return; // Should not reach here, but better than throwing an exception
                if (ContentContains(content, handler.ActiveWindowHandle))
                {
                    if (!Win32Helper.IsRunningOnMono)
                    {
                        NativeMethods.SetFocus(handler.ActiveWindowHandle);
                    }
                }

                if (handler.Form.ContainsFocus)
                    return;

                if (handler.Form.SelectNextControl(handler.Form.ActiveControl, true, true, true, true))
                    return;

                if (Win32Helper.IsRunningOnMono) 
                    return;

                // Since DockContent Form is not selectalbe, use Win32 SetFocus instead
                NativeMethods.SetFocus(handler.Form.Handle);
            }

            private List<IDockContent> m_listContent = new List<IDockContent>();
            private List<IDockContent> ListContent
            {
                get { return this.m_listContent; }
            }
            public void AddToList(IDockContent content)
            {
                if (this.ListContent.Contains(content) || this.IsInActiveList(content))
                    return;

                this.ListContent.Add(content);
            }

            public void RemoveFromList(IDockContent content)
            {
                if (this.IsInActiveList(content))
                    this.RemoveFromActiveList(content);
                if (this.ListContent.Contains(content))
                    this.ListContent.Remove(content);
            }

            private IDockContent m_lastActiveContent = null;
            private IDockContent LastActiveContent
            {
                get { return this.m_lastActiveContent; }
                set { this.m_lastActiveContent = value; }
            }

            private bool IsInActiveList(IDockContent content)
            {
                return !(content.DockHandler.NextActive == null && this.LastActiveContent != content);
            }

            private void AddLastToActiveList(IDockContent content)
            {
                IDockContent last = this.LastActiveContent;
                if (last == content)
                    return;

                DockContentHandler handler = content.DockHandler;

                if (this.IsInActiveList(content))
                    this.RemoveFromActiveList(content);

                handler.PreviousActive = last;
                handler.NextActive = null;
                this.LastActiveContent = content;
                if (last != null)
                    last.DockHandler.NextActive = this.LastActiveContent;
            }

            private void RemoveFromActiveList(IDockContent content)
            {
                if (this.LastActiveContent == content)
                    this.LastActiveContent = content.DockHandler.PreviousActive;

                IDockContent prev = content.DockHandler.PreviousActive;
                IDockContent next = content.DockHandler.NextActive;
                if (prev != null)
                    prev.DockHandler.NextActive = next;
                if (next != null)
                    next.DockHandler.PreviousActive = prev;

                content.DockHandler.PreviousActive = null;
                content.DockHandler.NextActive = null;
            }

            public void GiveUpFocus(IDockContent content)
            {
                DockContentHandler handler = content.DockHandler;
                if (!handler.Form.ContainsFocus)
                    return;

                if (this.IsFocusTrackingSuspended)
                    this.DockPanel.DummyControl.Focus();

                if (this.LastActiveContent == content)
                {
                    IDockContent prev = handler.PreviousActive;
                    if (prev != null)
                        this.Activate(prev);
                    else if (this.ListContent.Count > 0)
                        this.Activate(this.ListContent[this.ListContent.Count - 1]);
                }
                else if (this.LastActiveContent != null)
                    this.Activate(this.LastActiveContent);
                else if (this.ListContent.Count > 0)
                    this.Activate(this.ListContent[this.ListContent.Count - 1]);
            }

            private static bool ContentContains(IDockContent content, IntPtr hWnd)
            {
                Control control = Control.FromChildHandle(hWnd);
                for (Control parent = control; parent != null; parent = parent.Parent)
                    if (parent == content.DockHandler.Form)
                        return true;

                return false;
            }

            private uint m_countSuspendFocusTracking = 0;
            public void SuspendFocusTracking()
            {
                if (this.m_disposed)
                    return;

                if (this.m_countSuspendFocusTracking++ == 0)
                {
                    if (!Win32Helper.IsRunningOnMono)
                        sm_localWindowsHook.HookInvoked -= this.m_hookEventHandler;
                }
            }

            public void ResumeFocusTracking()
            {
                if (this.m_disposed || this.m_countSuspendFocusTracking == 0)
                    return;

                if (--this.m_countSuspendFocusTracking == 0)
                {
                    if (this.ContentActivating != null)
                    {
                        this.Activate(this.ContentActivating);
                        this.ContentActivating = null;
                    }

                    if (!Win32Helper.IsRunningOnMono)
                        sm_localWindowsHook.HookInvoked += this.m_hookEventHandler;

                    if (!this.InRefreshActiveWindow)
                        this.RefreshActiveWindow();
                }
            }

            public bool IsFocusTrackingSuspended
            {
                get { return this.m_countSuspendFocusTracking != 0; }
            }

            // Windows hook event handler
            private void HookEventHandler(object sender, HookEventArgs e)
            {
                Win32.Msgs msg = (Win32.Msgs)Marshal.ReadInt32(e.lParam, IntPtr.Size * 3);

                if (msg == Win32.Msgs.WM_KILLFOCUS)
                {
                    IntPtr wParam = Marshal.ReadIntPtr(e.lParam, IntPtr.Size * 2);
                    DockPane pane = this.GetPaneFromHandle(wParam);
                    if (pane == null)
                        this.RefreshActiveWindow();
                }
                else if (msg == Win32.Msgs.WM_SETFOCUS || msg == Win32.Msgs.WM_MDIACTIVATE)
                    this.RefreshActiveWindow();
            }

            private DockPane GetPaneFromHandle(IntPtr hWnd)
            {
                Control control = Control.FromChildHandle(hWnd);

                IDockContent content = null;
                DockPane pane = null;
                for (; control != null; control = control.Parent)
                {
                    content = control as IDockContent;
                    if (content != null)
                        content.DockHandler.ActiveWindowHandle = hWnd;

                    if (content != null && content.DockHandler.DockPanel == this.DockPanel)
                        return content.DockHandler.Pane;

                    pane = control as DockPane;
                    if (pane != null && pane.DockPanel == this.DockPanel)
                        break;
                }

                return pane;
            }

            private bool m_inRefreshActiveWindow = false;
            private bool InRefreshActiveWindow
            {
                get { return this.m_inRefreshActiveWindow; }
            }

            private void RefreshActiveWindow()
            {
                this.SuspendFocusTracking();
                this.m_inRefreshActiveWindow = true;

                DockPane oldActivePane = this.ActivePane;
                IDockContent oldActiveContent = this.ActiveContent;
                IDockContent oldActiveDocument = this.ActiveDocument;

                this.SetActivePane();
                this.SetActiveContent();
                this.SetActiveDocumentPane();
                this.SetActiveDocument();
                this.DockPanel.AutoHideWindow.RefreshActivePane();

                this.ResumeFocusTracking();
                this.m_inRefreshActiveWindow = false;

                if (oldActiveContent != this.ActiveContent)
                    this.DockPanel.OnActiveContentChanged(EventArgs.Empty);
                if (oldActiveDocument != this.ActiveDocument)
                    this.DockPanel.OnActiveDocumentChanged(EventArgs.Empty);
                if (oldActivePane != this.ActivePane)
                    this.DockPanel.OnActivePaneChanged(EventArgs.Empty);
            }

            private DockPane m_activePane = null;
            public DockPane ActivePane
            {
                get { return this.m_activePane; }
            }

            private void SetActivePane()
            {
                DockPane value = Win32Helper.IsRunningOnMono ? null : this.GetPaneFromHandle(NativeMethods.GetFocus());
                if (this.m_activePane == value)
                    return;

                if (this.m_activePane != null)
                    this.m_activePane.SetIsActivated(false);

                this.m_activePane = value;

                if (this.m_activePane != null)
                    this.m_activePane.SetIsActivated(true);
            }

            private IDockContent m_activeContent = null;
            public IDockContent ActiveContent
            {
                get { return this.m_activeContent; }
            }

            internal void SetActiveContent()
            {
                IDockContent value = this.ActivePane == null ? null : this.ActivePane.ActiveContent;

                if (this.m_activeContent == value)
                    return;

                if (this.m_activeContent != null)
                    this.m_activeContent.DockHandler.IsActivated = false;

                this.m_activeContent = value;

                if (this.m_activeContent != null)
                {
                    this.m_activeContent.DockHandler.IsActivated = true;
                    if (!DockHelper.IsDockStateAutoHide((this.m_activeContent.DockHandler.DockState)))
                        this.AddLastToActiveList(this.m_activeContent);
                }
            }

            private DockPane m_activeDocumentPane = null;
            public DockPane ActiveDocumentPane
            {
                get { return this.m_activeDocumentPane; }
            }

            private void SetActiveDocumentPane()
            {
                DockPane value = null;

                if (this.ActivePane != null && this.ActivePane.DockState == DockState.Document)
                    value = this.ActivePane;

                if (value == null && this.DockPanel.DockWindows != null)
                {
                    if (this.ActiveDocumentPane == null)
                        value = this.DockPanel.DockWindows[DockState.Document].DefaultPane;
                    else if (this.ActiveDocumentPane.DockPanel != this.DockPanel || this.ActiveDocumentPane.DockState != DockState.Document)
                        value = this.DockPanel.DockWindows[DockState.Document].DefaultPane;
                    else
                        value = this.ActiveDocumentPane;
                }

                if (this.m_activeDocumentPane == value)
                    return;

                if (this.m_activeDocumentPane != null)
                    this.m_activeDocumentPane.SetIsActiveDocumentPane(false);

                this.m_activeDocumentPane = value;

                if (this.m_activeDocumentPane != null)
                    this.m_activeDocumentPane.SetIsActiveDocumentPane(true);
            }

            private IDockContent m_activeDocument = null;
            public IDockContent ActiveDocument
            {
                get { return this.m_activeDocument; }
            }

            private void SetActiveDocument()
            {
                IDockContent value = this.ActiveDocumentPane == null ? null : this.ActiveDocumentPane.ActiveContent;

                if (this.m_activeDocument == value)
                    return;

                this.m_activeDocument = value;
            }
        }

        private IFocusManager FocusManager
        {
            get { return this.m_focusManager; }
        }

        internal IContentFocusManager ContentFocusManager
        {
            get { return this.m_focusManager; }
        }

        internal void SaveFocus()
        {
            this.DummyControl.Focus();
        }

        [Browsable(false)]
        public IDockContent ActiveContent
        {
            get { return this.FocusManager.ActiveContent; }
        }

        [Browsable(false)]
        public DockPane ActivePane
        {
            get { return this.FocusManager.ActivePane; }
        }

        [Browsable(false)]
        public IDockContent ActiveDocument
        {
            get { return this.FocusManager.ActiveDocument; }
        }

        [Browsable(false)]
        public DockPane ActiveDocumentPane
        {
            get { return this.FocusManager.ActiveDocumentPane; }
        }

        private static readonly object ActiveDocumentChangedEvent = new object();
        [LocalizedCategory("Category_PropertyChanged")]
        [LocalizedDescription("DockPanel_ActiveDocumentChanged_Description")]
        public event EventHandler ActiveDocumentChanged
        {
            add { this.Events.AddHandler(ActiveDocumentChangedEvent, value); }
            remove { this.Events.RemoveHandler(ActiveDocumentChangedEvent, value); }
        }
        protected virtual void OnActiveDocumentChanged(EventArgs e)
        {
            EventHandler handler = (EventHandler)this.Events[ActiveDocumentChangedEvent];
            if (handler != null)
                handler(this, e);
        }

        private static readonly object ActiveContentChangedEvent = new object();
        [LocalizedCategory("Category_PropertyChanged")]
        [LocalizedDescription("DockPanel_ActiveContentChanged_Description")]
        public event EventHandler ActiveContentChanged
        {
            add { this.Events.AddHandler(ActiveContentChangedEvent, value); }
            remove { this.Events.RemoveHandler(ActiveContentChangedEvent, value); }
        }
        protected void OnActiveContentChanged(EventArgs e)
        {
            EventHandler handler = (EventHandler)this.Events[ActiveContentChangedEvent];
            if (handler != null)
                handler(this, e);
        }

        private static readonly object ActivePaneChangedEvent = new object();
        [LocalizedCategory("Category_PropertyChanged")]
        [LocalizedDescription("DockPanel_ActivePaneChanged_Description")]
        public event EventHandler ActivePaneChanged
        {
            add { this.Events.AddHandler(ActivePaneChangedEvent, value); }
            remove { this.Events.RemoveHandler(ActivePaneChangedEvent, value); }
        }
        protected virtual void OnActivePaneChanged(EventArgs e)
        {
            EventHandler handler = (EventHandler)this.Events[ActivePaneChangedEvent];
            if (handler != null)
                handler(this, e);
        }
    }
}

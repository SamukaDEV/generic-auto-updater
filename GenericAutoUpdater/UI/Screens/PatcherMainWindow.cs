﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;
using GenericAutoUpdater.Engine;
using GenericAutoUpdater.ExceptionHandler;
using GenericAutoUpdater.Resources.TextResources;
using GenericAutoUpdater.UI;
using GenericAutoUpdater.UI.Wrappers;

namespace GenericAutoUpdater {
    /// <summary>
    /// The class representing the main window and all its widgets' behaviour.
    /// </summary>
    public partial class PatcherMainWindow : Form {

        /// <summary>
        /// Initializes a new instance of the main window.
        /// </summary>
        public PatcherMainWindow() {
            InitializeComponent();
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.ProgressChanged += BackgroundWorker1_ProgressChanged;
        }

        /// <summary>
        /// Deals with any ProgressChanged event trigger assuming the <c>ProgressChangedEventArgs</c>'s <c>UserState</c> is a <c>IWidgetWrapper</c>.
        /// </summary>
        private void BackgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            switch (e.UserState) {
                // The assignment to these local variables is needed.
                case ProgressBarWrapper c1:
                    ProgressBarWrapper pbw = (ProgressBarWrapper)e.UserState;
                    switch (pbw.ProgressBar) {
                        case ProgressiveWidgetsEnum.ProgressBar.WholeProgressBar:
                            wholeProgressBar.Value = pbw.Value;
                            break;
                        case ProgressiveWidgetsEnum.ProgressBar.DownloadProgressBar:
                            fileProgressBar.Value = pbw.Value;
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    break;
                case LabelWrapper c2:
                    LabelWrapper lw = (LabelWrapper)e.UserState;
                    switch (lw.Label) {
                        case ProgressiveWidgetsEnum.Label.InformativeLogger:
                            loggerDisplay.Text = lw.Value;
                            break;
                        case ProgressiveWidgetsEnum.Label.DownloadLogger:
                            downloaderDisplay.Text = lw.Value;
                            break;
                        case ProgressiveWidgetsEnum.Label.FileCountLogger:
                            filecount.Left = loggerDisplay.Location.X + loggerDisplay.Size.Width;
                            filecount.Text = lw.Value;
                            break;
                        case ProgressiveWidgetsEnum.Label.DownloadSpeedLogger:
                            speed.Text = lw.Value;
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Sets up the initial main window properties.
        /// </summary>
        private void setupWindowProperties() {
            Text = string.Format(MainWindowResources.MAIN_WINDOW_TITLE, MainWindowResources.CURRENT_VERSION);
            filecount.Text = string.Empty;
            speed.Text = string.Empty;
            loggerDisplay.Text = PatcherEngineResources.STARTING;
            downloaderDisplay.Text = PatcherEngineResources.STARTING;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
        }

        /// <summary>
        /// Loads the main window.
        /// </summary>
        private void loadWindow(object sender, EventArgs e) {
            setupWindowProperties();
            backgroundWorker1.RunWorkerAsync();
        }

        /// <summary>
        /// Opens your website whenever the respective link is clicked.
        /// </summary>
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            Process.Start(MainWindowResources.WEBSITE);
        }

        /// <summary>
        /// Starts your client's launcher and exits the Auto-Updater when clicked.
        /// </summary>
        private void starter_Click(object sender, EventArgs e) {
            Process.Start(MainWindowResources.STARTER);
            Application.Exit();
        }

        /// <summary>
        /// Opens the author's website whenever the respective link is clicked.
        /// </summary>
        private void author_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            Process.Start(MainWindowResources.AUTHOR_WEBSITE);
        }

        /// <summary>
        /// Supports all the engine's work in a non-UI Thread and assigns any un-handled exceptions to the Handler.
        /// </summary>
        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e) {
            BackgroundWorker bw = sender as BackgroundWorker;
            IPatcherEngine engine = new PatcherEngine(bw);
            try {
                engine.Patch();
            } catch (Exception ex) {
                Handler.Handle(ex);
            }
        }

        /// <summary>
        /// Enables the starter button and displays a final message in the logger display.
        /// </summary>
        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            starter.Enabled = true;
            loggerDisplay.Text = PatcherEngineResources.FINISHED;
        }

        /// <summary>
        /// Opens your website whenever the respective button is clicked.
        /// </summary>
        private void button1_Click(object sender, EventArgs e) {
            Process.Start(MainWindowResources.WEBSITE);
        }
    }
}

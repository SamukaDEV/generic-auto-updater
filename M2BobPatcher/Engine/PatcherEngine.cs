﻿using M2BobPatcher.Downloaders;
using M2BobPatcher.FileSystem;
using M2BobPatcher.Hash;
using M2BobPatcher.Resources.Configs;
using M2BobPatcher.Resources.TextResources;
using M2BobPatcher.TextResources;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace M2BobPatcher.Engine {
    class PatcherEngine : IPatcherEngine {

        private IFileSystemExplorer Explorer;
        private ConcurrentDictionary<string, FileMetadata> LocalMetadata;
        private Dictionary<string, FileMetadata> ServerMetadata;
        private Label LoggerDisplay;
        private Label FileLoggerDisplay;
        private Button Starter;
        private string PatchDirectory;
        private int LogicalProcessorsCount;

        delegate void SetTextCallback(Label output, string text);
        delegate void SetToggleCallback(Button button, bool state);

        public PatcherEngine(Label loggerDisplay, Label fileLoggerDisplay, Button starter) {
            Explorer = new FileSystemExplorer();
            LogicalProcessorsCount = Environment.ProcessorCount;
            LoggerDisplay = loggerDisplay;
            FileLoggerDisplay = fileLoggerDisplay;
            Starter = starter;
        }

        /**1. ask server for metadata file with all files' full path + name + extension and their sizes and their md5
          *2. download files not present locally (check this by name)
          *3. generateMetadata for all files locally (which also exist in server info)
          *4. compare generatedMetadata with the one obtained from 1.
          *5. download files which metadata differs
          */
        void IPatcherEngine.Patch() {
            GenerateServerMetadata(DownloadServerMetadataFile());
            DownloadMissingContent();
            GenerateLocalMetadata();
            DownloadOutdatedContent();
            Finish();
        }

        void IPatcherEngine.Repair() {
            throw new NotImplementedException();
        }

        private void DownloadOutdatedContent() {
            Log(LoggerDisplay, PatcherEngineResources.DOWNLOADING_OUTDATED_CONTENT);
            foreach (KeyValuePair<string, FileMetadata> entry in ServerMetadata) {
                if (!entry.Value.Hash.Equals(LocalMetadata[entry.Key].Hash))
                    Explorer.RequestWriteFile(entry.Key, PatchDirectory + entry.Key, true, Log, FileLoggerDisplay);
            }
        }

        private void DownloadMissingContent() {
            Log(LoggerDisplay, PatcherEngineResources.DOWNLOADING_MISSING_CONTENT);
            foreach (string file in ServerMetadata.Keys)
                Explorer.RequestWriteFile(file, PatchDirectory + file, false, Log, FileLoggerDisplay);
        }

        private string DownloadServerMetadataFile() {
            Log(LoggerDisplay, PatcherEngineResources.DOWNLOADING_SERVER_METADATA);
            return WebClientDownloader.DownloadString(EngineConfigs.M2BOB_PATCH_METADATA);
        }

        private void GenerateLocalMetadata() {
            Log(LoggerDisplay, PatcherEngineResources.GENERATING_LOCAL_METADATA);
            LocalMetadata = Explorer.GenerateLocalMetadata(ServerMetadata.Keys.ToArray(), LogicalProcessorsCount / 2);
        }

        private void GenerateServerMetadata(string serverMetadata) {
            Log(LoggerDisplay, PatcherEngineResources.PARSING_SERVER_METADATA);
            string[] metadataByLine = serverMetadata.Trim().Split(new[] { "\n" }, StringSplitOptions.None);
            PatchDirectory = metadataByLine[0];
            int numberOfRemoteFiles = (metadataByLine.Length - 1) / 2;
            ServerMetadata = new Dictionary<string, FileMetadata>(numberOfRemoteFiles);
            for (int i = 1; i < metadataByLine.Length; i += 2) {
                string filename = metadataByLine[i];
                string fileMd5 = metadataByLine[i + 1];
                ServerMetadata[filename] = new FileMetadata(filename, fileMd5);
            }
        }

        private void Finish() {
            Log(LoggerDisplay, PatcherEngineResources.FINISHED);
            Log(FileLoggerDisplay, PatcherEngineResources.ALL_FILES_ANALYZED);
            Toggle(Starter, true);
        }

        private void Log(Label output, string message) {
            SetOutputText(output, message);
        }

        public void Toggle(Button button, bool state) {
            SetToggleState(button, state);
        }

        private void SetOutputText(Label output, string text) {
            if (output.InvokeRequired) {
                SetTextCallback d = new SetTextCallback(SetOutputText);
                output.Invoke(d, new object[] { output, text });
            }
            else
                output.Text = text;
        }

        private void SetToggleState(Button button, bool state) {
            if (button.InvokeRequired) {
                SetToggleCallback d = new SetToggleCallback(SetToggleState);
                button.Invoke(d, new object[] { button, state });
            }
            else
                button.Enabled = state;
        }

        private void DebugPrintMetadata() {
            Console.WriteLine("Debug server:");
            foreach (KeyValuePair<string, FileMetadata> kvp in ServerMetadata) {
                Console.WriteLine("Key = {0}, Value = {1}", kvp.Key, kvp.Value.Hash);
            }
            Console.WriteLine("Debug local:");
            foreach (KeyValuePair<string, FileMetadata> kvp in LocalMetadata) {
                Console.WriteLine("Key = {0}, Value = {1}", kvp.Key, kvp.Value.Hash);
            }
        }
    }
}

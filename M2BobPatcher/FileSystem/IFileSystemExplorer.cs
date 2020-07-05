﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace M2BobPatcher.FileSystem {
    interface IFileSystemExplorer {
        ConcurrentDictionary<string, FileMetadata> GenerateLocalMetadata(string[] filesPaths, int concurrencyLevel);
        void RequestWriteFile(string path, string resource, bool overwrite, Action<Label, string> loggerFunction, Label log);
    }
}

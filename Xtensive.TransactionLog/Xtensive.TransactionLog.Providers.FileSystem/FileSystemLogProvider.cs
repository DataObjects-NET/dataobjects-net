// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2007.10.31

using System;
using System.Globalization;
using System.IO;
using Xtensive.TransactionLog;
using Xtensive.TransactionLog.Providers;
using Xtensive.TransactionLog.Providers.FileSystem.Resources;

namespace Xtensive.TransactionLog.Providers.FileSystem
{
  /// <summary>
  /// Implements <see cref="ILogProvider"/> for simple access to file system.
  /// </summary>
  public class FileSystemLogProvider : ILogProvider
  {
    private readonly string baseFolder;

    #region ILogProvider Members

    /// <summary>
    /// Checks if specified folder exists.
    /// </summary>
    /// <param name="name">Full name (with path) of folder.</param>
    /// <returns><see langword="True"/> if specified folder exists, otherwise gets <see langword="false"/>.</returns>
    public bool FolderExists(string name)
    {
      return Directory.Exists(Path.Combine(baseFolder, name));
    }

    /// <summary>
    /// Creates new folder.
    /// </summary>
    /// <param name="name">Full name (with path) of folder.</param>
    public void CreateFolder(string name)
    {
      Directory.CreateDirectory(Path.Combine(baseFolder, name));
    }

    /// <summary>
    /// Gets file's stream for read or write.
    /// </summary>
    /// <param name="name">Full name (with path) of file.</param>
    /// <param name="mode"><see cref="FileMode"/> for get <see cref="Stream"/>.</param>
    /// <returns><see cref="Stream"/> for specified file and <see cref="FileMode"/>.</returns>
    public Stream GetFileStream(string name, FileMode mode)
    {
      string path = Path.Combine(baseFolder, name);
      return new FileStream(path, mode);
    }

    /// <summary>
    /// Gets list of files in specified folder.
    /// </summary>
    /// <param name="name">Full name (with path) of folder.</param>
    /// <returns>Array of <see cref="string"/> with names of files in specified folder.</returns>
    public string[] GetFolderFiles(string name)
    {
      string path = Path.Combine(baseFolder, name);
      return Directory.GetFiles(path);
    }

    /// <summary>
    /// Deletes folder.
    /// </summary>
    /// <param name="name">Full name (with path) of folder.</param>
    public void DeleteFolder(string name)
    {
      string path = Path.Combine(baseFolder, name);
      Directory.Delete(path);
    }

    /// <summary>
    /// Deletes file.
    /// </summary>
    /// <param name="name">Full name (with path) of file.</param>
    public void DeleteFile(string name)
    {
      string path = Path.Combine(baseFolder, name);
      File.Delete(path);
    }

    #endregion

    /// <summary>
    /// Creates new instance of <see cref="FileSystemLogProvider"/>.
    /// </summary>
    /// <param name="baseFolder">Path to folder on local system where to create files and/or folders for <see cref="TransactionLog{TKey}"/>.</param>
    public FileSystemLogProvider(string baseFolder)
    {
      this.baseFolder = baseFolder;
      if (!Directory.Exists(baseFolder)) {
        if (File.Exists(baseFolder))
          throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Strings.ExFolderNameConflictsWithFile, baseFolder));
        Directory.CreateDirectory(baseFolder);
      }
    }
  }
}
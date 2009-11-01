// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2007.10.26

using System.IO;

namespace Xtensive.TransactionLog.Providers
{
  /// <summary>
  /// Log provider.
  /// </summary>
  public interface ILogProvider 
  {
    /// <summary>
    /// Checks if specified folder exists.
    /// </summary>
    /// <param name="name">Full name (with path) of folder.</param>
    /// <returns><see langword="True"/> if specified folder exists, otherwise gets <see langword="false"/>.</returns>
    bool FolderExists(string name);

    /// <summary>
    /// Creates new folder.
    /// </summary>
    /// <param name="name">Full name (with path) of folder.</param>
    void CreateFolder(string name);

    /// <summary>
    /// Gets list of files in specified folder.
    /// </summary>
    /// <param name="name">Full name (with path) of folder.</param>
    /// <returns>Array of <see cref="string"/> with names of files in specified folder.</returns>
    string[] GetFolderFiles(string name);

    /// <summary>
    /// Gets file's stream for read or write.
    /// </summary>
    /// <param name="name">Full name (with path) of file.</param>
    /// <param name="mode"><see cref="FileMode"/> for get <see cref="Stream"/>.</param>
    /// <returns><see cref="Stream"/> for specified file and <see cref="FileMode"/>.</returns>
    Stream GetFileStream(string name, FileMode mode);

    /// <summary>
    /// Deletes folder.
    /// </summary>
    /// <param name="name">Full name (with path) of folder.</param>
    void DeleteFolder(string name);

    /// <summary>
    /// Deletes file.
    /// </summary>
    /// <param name="name">Full name (with path) of file.</param>
    void DeleteFile(string name);
  }
}
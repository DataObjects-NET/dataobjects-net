// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.10.10

using System;
using System.IO;
using Xtensive.Core;
using Xtensive.Distributed.Test.Resources;

namespace Xtensive.Distributed.Test
{
  /// <summary>
  /// Provides the information about remote folder 
  /// and allows to manage the folder over the network.
  /// </summary>
  [Serializable]
  public class Folder: FileSystemItem
  {
    #region Properties

    /// <summary>
    /// Gets list of sub folders.
    /// </summary>
    public Folder[] GetFolders()
    {
      return RemoteFileManager.GetFolders(Path);
    }

    /// <summary>
    /// Gets list of <see cref="File"/>s of this folder.
    /// </summary>
    public File[] GetFiles()
    {
      return RemoteFileManager.GetFiles(Path);
    }

    #endregion

    #region Methods

    /// <summary>
    /// Deletes <see cref="Folder"/>.
    /// </summary>
    public override void Delete()
    {
      RemoteFileManager.DeleteFolder(Path);
    }

    /// <summary>
    /// Downloads <see cref="Folder"/> and all it's content to local drive.
    /// </summary>
    /// <param name="localPath">Local path there download to.</param>
    public override void Download(string localPath)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(localPath, "localPath");

      if (!Directory.Exists(localPath))
        throw new FileNotFoundException(string.Format(
          Strings.ExFolderNotFound, localPath));
      foreach (Folder folder in GetFolders()) {
        string folderFullName = System.IO.Path.Combine(localPath, folder.Name);
        Directory.CreateDirectory(folderFullName);
        folder.Download(folderFullName);
      }
      foreach (File file in GetFiles()) {
        file.Download(localPath);
      }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Creates new instance of <see cref="Folder"/>.
    /// </summary>
    /// <param name="path">Folder path.</param>
    /// <param name="directoryInfo"><see cref="DirectoryInfo"/> of the folder.</param>
    /// <param name="remoteFileManager"><see cref="RemoteFileManager"/> to communicate to <see cref="Agent"/> other the network.</param>
    internal Folder(string path, DirectoryInfo directoryInfo, RemoteFileManager remoteFileManager)
      : base(path, directoryInfo.CreationTime, directoryInfo.LastWriteTime, remoteFileManager)
    {
    }

    #endregion
  }
}
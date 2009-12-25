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
  /// Provides the information about remote file 
  /// and allows to manage the file over the network.
  /// </summary>
  [Serializable]
  public class File : FileSystemItem
  {
    #region Private fields

    private readonly long size;

    #endregion

    #region Properties

    /// <summary>
    /// Gets file size.
    /// </summary>
    public long Size
    {
      get { return size; }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Deletes file.
    /// </summary>
    public override void Delete()
    {
      RemoteFileManager.DeleteFile(Path);
    }

    /// <summary>
    /// Downloads file to local computer.
    /// </summary>
    /// <param name="localPath">Path on local drive to store file.</param>
    /// <exception cref="ArgumentNullException">Throws if <paramref name="localPath"/> is <see langword="null"/>.</exception>
    /// <exception cref="FileNotFoundException">Throws if file not found on remote side.</exception>
    public override void Download(string localPath)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(localPath, "localPath");

      if (!Directory.Exists(localPath))
        throw new FileNotFoundException(string.Format(
          Strings.ExFolderNotFound, localPath));
      using (var fs = new FileStream(
        System.IO.Path.Combine(localPath, System.IO.Path.GetFileName(Path)),
        FileMode.Create)) {
        long position = 0;
        byte[] buffer = RemoteFileManager.DownloadFilePart(Path, position, FileManager.BufferSize);
        while (buffer.Length > 0) {
          fs.Write(buffer, 0, buffer.Length);
          position += buffer.Length;
          buffer = RemoteFileManager.DownloadFilePart(Path, position, FileManager.BufferSize);
        }
        fs.Flush();
      }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Creates new instance of <see cref="File"/>.
    /// </summary>
    /// <param name="path">File path.</param>
    /// <param name="fileInfo"><see cref="FileInfo"/> of the file.</param>
    /// <param name="remoteFileManager"><see cref="RemoteFileManager"/> to communicate to <see cref="Agent"/> other the network.</param>
    internal File(string path, FileInfo fileInfo, RemoteFileManager remoteFileManager)
      : base(path, fileInfo.CreationTime, fileInfo.LastWriteTime, remoteFileManager)
    {
      size = fileInfo.Length;
    }

    #endregion
  }
}
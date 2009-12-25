// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.09.17

using System;
using System.Globalization;
using System.IO;
using Xtensive.Core;
using Xtensive.Distributed.Test.Resources;

namespace Xtensive.Distributed.Test
{
  /// <summary>
  /// Proxy class for <see cref="RemoteFileManager"/>. 
  /// Provides methods to manage folders and files on the remote side.
  /// </summary>
  public class FileManager
  {
    #region Constants

    /// <summary>
    /// Size of buffer to upload and download files.
    /// </summary>
    public const int BufferSize = 1024 * 16;

    #endregion

    #region Private fields

    private readonly RemoteFileManager remoteFileManager;

    #endregion

    #region Methods

    /// <summary>
    /// Gets <see cref="FileSystemInfo"/> of specified file or folder.
    /// </summary>
    /// <param name="remotePath">Remote path of file or folder.</param>
    /// <returns>Returns <see cref="File"/> if requested path corresponds to file, <see cref="Folder"/> if requested path corresponds to folder. Otherwise returns <see langword="null"/>.</returns>
    public FileSystemItem GetFileSystemItem(string remotePath)
    {
      return remoteFileManager.GetFileSystemItem(remotePath);
    }

    /// <summary>
    /// Creates new folder.
    /// </summary>
    /// <param name="remotePath">Remote folder path.</param>
    public void CreateFolder(string remotePath)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(remotePath, "remotePath");
      remoteFileManager.CreateFolder(remotePath);
    }

    /// <summary>
    /// Uploads file or folder to the remote side. Folder uploads with all sub folder and files. 
    /// </summary>
    /// <param name="localPath">Path on local drive to take file or folder from.</param>
    /// <param name="remotePath">Remote path to store file or folder to.</param>
    /// <exception cref="InvalidOperationException">No files to upload.</exception>
    public void Upload(string localPath, string remotePath)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(localPath, "localPath");
      ArgumentValidator.EnsureArgumentNotNull(remotePath, "remotePath");

      try {
        string path = Path.GetFullPath(localPath);
        int count = 0;
        if (Directory.Exists(path))
          count = UploadFolder(path, remotePath);
        else if (System.IO.File.Exists(path)) {
          UploadFile(path, Path.GetFileName(path));
          count++;
        }
        if (count==0)
          throw new InvalidOperationException(Strings.ExNoFilesToUpload);
      }
      catch (Exception e) {
        Log.Error(Strings.ExInvalidSourceFileOrDirectory);
        throw new InvalidOperationException(Strings.ExInvalidSourceFileOrDirectory, e);
      }
    }

    /// <summary>
    /// Downloads file from remote side to local drive.
    /// </summary>
    /// <param name="localPath">Path on local drive to store file to.</param>
    /// <param name="remotePath">Remote path to take file from.</param>
    /// <exception cref="DirectoryNotFoundException">Local directory not found.</exception>
    /// <exception cref="FileNotFoundException">Remote file not found.</exception>
    public void Download(string localPath, string remotePath)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(localPath, "localPath");
      ArgumentValidator.EnsureArgumentNotNull(remotePath, "remotePath");

      if (!Directory.Exists(localPath))
        throw new DirectoryNotFoundException(string.Format(CultureInfo.CurrentCulture, Strings.ExFolderNotFound, localPath));
      FileSystemItem fsItem = remoteFileManager.GetFileSystemItem(remotePath);
      if (fsItem==null)
        throw new FileNotFoundException(string.Format(CultureInfo.CurrentCulture, Strings.ExFileNotFound, remotePath));
      fsItem.Download(localPath);
    }

    #endregion

    #region Private methods

    /// <summary>
    /// Uploads file to the remote side.
    /// </summary>
    /// <param name="localPath">Path on local drive to take file from.</param>
    /// <param name="remotePath">Remote path to store file to.</param>
    private void UploadFile(string localPath, string remotePath)
    {
      var buffer = new byte[BufferSize];
      long position = 0;
      using (var fs = new FileStream(localPath, FileMode.Open, FileAccess.Read)) {
        int readCount = fs.Read(buffer, 0, BufferSize);
        while (readCount > 0) {
          if (readCount < BufferSize) {
            var newBuffer = new byte[readCount];
            Array.Copy(buffer, newBuffer, readCount);
            buffer = newBuffer;
          }
          try {
            remoteFileManager.UploadFilePart(buffer, remotePath, position);
          }
          catch (Exception) {
            if (position!=0) {
              try {
                remoteFileManager.DeleteFile(remotePath);
              }
              catch (Exception ex) {
                Log.Info(ex, Strings.LogRemoteFileManagerUnableToDeleteFileXxx, remotePath);
              }
            }
            throw;
          }
          position += buffer.Length;
          readCount = fs.Read(buffer, 0, buffer.Length);
        }
        fs.Close();
      }
    }

    /// <summary>
    /// Uploads folder with all sub folders and files to the remote side.
    /// </summary>
    /// <param name="localPath">Path on local drive to take folder from.</param>
    /// <param name="remotePath">Remote path to store folder to.</param>
    private int UploadFolder(string localPath, string remotePath)
    {
      int count = 0;
      if (remotePath.Length!=0) {
        remoteFileManager.CreateFolder(remotePath);
        count++;
      }
      foreach (string directory in Directory.GetDirectories(localPath))
        count += UploadFolder(Path.Combine(localPath, directory), Path.Combine(remotePath, Path.GetFileName(directory)));
      foreach (string file in Directory.GetFiles(localPath)) {
        UploadFile(Path.Combine(localPath, file), Path.Combine(remotePath, Path.GetFileName(file)));
        count++;
      }
      return count;
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Creates new instance of <see cref="FileManager"/>.
    /// </summary>
    /// <param name="remoteFileManager"><see cref="RemoteFileManager"/> to communicate with remote side.</param>
    internal FileManager(RemoteFileManager remoteFileManager)
    {
      ArgumentValidator.EnsureArgumentNotNull(remoteFileManager, "remoteFileManager");
      this.remoteFileManager = remoteFileManager;
    }

    #endregion
  }
}
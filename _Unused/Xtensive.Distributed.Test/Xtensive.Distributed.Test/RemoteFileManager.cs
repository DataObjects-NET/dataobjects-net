// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.09.26

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security.Permissions;
using Xtensive.Core;
using Xtensive.Distributed.Test.Resources;

namespace Xtensive.Distributed.Test
{
  /// <summary>
  /// Provides file operations other the Remoting
  /// </summary>
  public class RemoteFileManager : MarshalByRefObject
  {
    #region Private fields

    private readonly string rootPath;

    #endregion

    #region Properties

    /// <summary>
    /// Gets root folder where to store files.
    /// </summary>
    public string RootPath
    {
      get { return rootPath; }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Uploads file to remote side part-by-part. Appends new data to the existing file.
    /// </summary>
    /// <param name="buffer">Buffer with file's data.</param>
    /// <param name="fileName">File name.</param>
    /// <param name="position">Position in file.</param>
    public void UploadFilePart(byte[] buffer, string fileName, long position)
    {
      ArgumentValidator.EnsureArgumentNotNull(buffer, "buffer");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(fileName, "fileName");

      string fullPath = Path.Combine(rootPath, fileName);
      if (position==0 && (Directory.Exists(fullPath) || System.IO.File.Exists(fullPath))) {
        Log.Error(Strings.ExFileOrFolderAlreadyExists, fileName);
        throw new IOException(string.Format(CultureInfo.CurrentCulture, Strings.ExFileOrFolderAlreadyExists, fileName));
      }
      using (var fs = new FileStream(fullPath, FileMode.Append)) {
        if (fs.Position!=position) {
          Log.Error(Strings.ExInvalidFilePosition, fileName);
          throw new ArgumentOutOfRangeException("position", position, string.Format(CultureInfo.CurrentCulture, Strings.ExInvalidFilePosition, fileName));
        }
        fs.Write(buffer, 0, buffer.Length);
        fs.Flush();
        fs.Close();
      }
    }

    /// <summary>
    /// Creates new folder.
    /// </summary>
    /// <param name="path">Folder path.</param>
    public void CreateFolder(string path)
    {
      string fullPath = Path.Combine(rootPath, path);
      if (Directory.Exists(fullPath)) {
        Log.Error(Strings.ExFileOrFolderAlreadyExists, fullPath);
        throw new IOException(string.Format(
          Strings.ExFileOrFolderAlreadyExists, fullPath));
      }
      Directory.CreateDirectory(fullPath);
    }

    /// <summary>
    /// Deletes all files in root folder and all sub folders.
    /// </summary>
    public void DeleteAllFiles()
    {
      if (Directory.Exists(rootPath))
        Directory.Delete(rootPath, true);
    }

    /// <summary>
    /// Deletes specified file.
    /// </summary>
    /// <param name="path">File path.</param>
    public void DeleteFile(string path)
    {
      string fileName = Path.Combine(rootPath, path);
      if (System.IO.File.Exists(fileName))
        System.IO.File.Delete(fileName);
      else
        throw new FileNotFoundException(string.Format(
          Strings.ExFileNotFound, path));
    }

    /// <summary>
    /// Deletes specified folder.
    /// </summary>
    /// <param name="path">Folder path.</param>
    public void DeleteFolder(string path)
    {
      string folderName = Path.Combine(rootPath, path);
      if (Directory.Exists(folderName))
        Directory.Delete(folderName);
      else
        throw new FileNotFoundException(string.Format(
          Strings.ExFolderNotFound, path));
    }

    /// <summary>
    /// Gets <see cref="FileSystemInfo"/> of specified file or folder.
    /// </summary>
    /// <param name="path">Path of file or folder.</param>
    /// <returns>Returns <see cref="File"/> if requested path corresponds to file, <see cref="Folder"/> if requested path corresponds to folder. Otherwise returns <see langword="null"/>.</returns>
    public FileSystemItem GetFileSystemItem(string path)
    {
      string fullName = Path.Combine(rootPath, path);
      if (System.IO.File.Exists(fullName)) {
        var fileInfo = new FileInfo(fullName);
        return new File(path, fileInfo, this);
      }
      if (Directory.Exists(fullName)) {
        var directoryInfo = new DirectoryInfo(fullName);
        return new Folder(path, directoryInfo, this);
      }
      return null;
    }

    /// <summary>
    /// Gets list of sub folders in specified path.
    /// </summary>
    /// <param name="path">Path of folder where to look for sub folders.</param>
    /// <returns>Array of <see cref="Folder"/>.</returns>
    /// <exception cref="FileNotFoundException">Throws if no folder is found in specified <paramref name="path"/>.</exception>
    public Folder[] GetFolders(string path)
    {
      string folderName = Path.Combine(rootPath, path);
      if (Directory.Exists(folderName)) {
        DirectoryInfo[] directories = new DirectoryInfo(folderName).GetDirectories();
        var result = new List<Folder>(directories.Length);
        foreach (DirectoryInfo directoryInfo in directories)
          result.Add(new Folder(Path.Combine(path, directoryInfo.Name), directoryInfo, this));
        return result.ToArray();
      }
      throw new FileNotFoundException(string.Format(Strings.ExFolderNotFound, path));
    }

    /// <summary>
    /// Gets list of <see cref="File"/>s in specified folder.
    /// </summary>
    /// <param name="path">Path of folder where to look for files.</param>
    /// <returns>Array of <see cref="File"/>.</returns>
    /// <exception cref="FileNotFoundException">Throws if no folder is found in specified <paramref name="path"/>.</exception>
    public File[] GetFiles(string path)
    {
      string folderName = Path.Combine(rootPath, path);
      if (Directory.Exists(folderName)) {
        FileInfo[] files = new DirectoryInfo(folderName).GetFiles();
        var result = new List<File>(files.Length);
        foreach (FileInfo fileInfo in files)
          result.Add(new File(Path.Combine(path, fileInfo.Name), fileInfo, this));
        return result.ToArray();
      }
      throw new FileNotFoundException(string.Format(Strings.ExFolderNotFound, path));
    }

    /// <summary>
    /// Downloads part of file.
    /// </summary>
    /// <param name="path">File path.</param>
    /// <param name="position">Position in file to start read from.</param>
    /// <param name="bufferSize">Size of buffer of returned data.</param>
    /// <returns>Buffer with data. If file was readed to the end, buffer's size will be reduced to the actual
    /// size of data. If no data was readed from file because of <paramref name="position"/> points to the
    /// end of file, empty array of <see cref="byte"/> (byte[0]) will be returned.</returns>
    /// <exception cref="FileNotFoundException">Throws if no file is found in specified <paramref name="path"/>.</exception>
    public byte[] DownloadFilePart(string path, long position, int bufferSize)
    {
      string fileName = Path.Combine(rootPath, path);
      if (!System.IO.File.Exists(fileName))
        throw new FileNotFoundException(string.Format(
          Strings.ExFolderNotFound, path));
      using (var fs = new FileStream(fileName, FileMode.Open)) {
        long length = fs.Length;
        if (length <= position)
          return new byte[0];
        fs.Seek(position, SeekOrigin.Begin);
        var buffer = new byte[Math.Min(length - position, bufferSize)];
        int bytesReaded = fs.Read(buffer, 0, buffer.Length);
        Debug.Assert(bytesReaded==buffer.Length);
        return buffer;
      }
    }

    /// <summary>
    /// Obtains a lifetime service object to control the lifetime policy for this instance.
    /// </summary>
    /// <returns>
    /// An object of type <see cref="T:System.Runtime.Remoting.Lifetime.ILease"></see> used to control the lifetime policy for this instance. This is the current lifetime service object for this instance if one exists; otherwise, a new lifetime service object initialized to the value of the <see cref="P:System.Runtime.Remoting.Lifetime.LifetimeServices.LeaseManagerPollTime"></see> property.
    /// </returns>
    /// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception><filterpriority>2</filterpriority><PermissionSet><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="RemotingConfiguration, Infrastructure" /></PermissionSet>
    [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
    public override object InitializeLifetimeService()
    {
      return null;
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Creates new instance of <see cref="RemoteFileManager"/>.
    /// </summary>
    /// <param name="path">Root path.</param>
    internal RemoteFileManager(string path)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(path, "path");
      rootPath = Path.GetFullPath(path);
      if (!Directory.Exists(rootPath))
        Directory.CreateDirectory(rootPath);
    }

    #endregion
  }
}

// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.10.11

using System;

namespace Xtensive.Distributed.Test
{
  /// <summary>
  /// Abstract base class for remote file system objects - <see cref="File"/> and <see cref="Folder"/>. 
  /// </summary>
  [Serializable]
  public abstract class FileSystemItem
  {
    #region Private fields

    private readonly string path;
    private readonly DateTime createTime;
    private readonly DateTime modifyTime;

    [NonSerialized]
    private readonly RemoteFileManager remoteFileManager;

    #endregion

    #region Properties

    /// <summary>
    /// Gets full path and name.
    /// </summary>
    public string Path
    {
      get { return path; }
    }

    /// <summary>
    /// Gets create time.
    /// </summary>
    public DateTime CreateTime
    {
      get { return createTime; }
    }

    /// <summary>
    /// Gets time of last modification.
    /// </summary>
    public DateTime ModifyTime
    {
      get { return modifyTime; }
    }

    /// <summary>
    /// Gets name.
    /// </summary>
    public string Name
    {
      get { return System.IO.Path.GetFileName(path); }
    }

    /// <summary>
    /// Gets <see cref="RemoteFileManager"/>.
    /// </summary>
    internal RemoteFileManager RemoteFileManager
    {
      get { return remoteFileManager; }
    }

    #endregion

    #region Abstract methods

    /// <summary>
    /// Deletes <see cref="FileSystemItem"/>.
    /// </summary>
    public abstract void Delete();

    /// <summary>
    /// Downloads <see cref="FileSystemItem"/>.
    /// </summary>
    /// <param name="localPath"></param>
    public abstract void Download(string localPath);

    #endregion

    #region Constructors

    /// <summary>
    /// Creates new instance of <see cref="FileSystemItem"/>.
    /// </summary>
    /// <param name="path">Path.</param>
    /// <param name="createTime">Creation time.</param>
    /// <param name="modifyTime">Time of last modification.</param>
    /// <param name="remoteFileManager"><see cref="RemoteFileManager"/>.</param>
    internal FileSystemItem(string path, DateTime createTime, DateTime modifyTime, RemoteFileManager remoteFileManager)
    {
      this.path = path;
      this.remoteFileManager = remoteFileManager;
      this.modifyTime = modifyTime;
      this.createTime = createTime;
    }

    #endregion
  }
}
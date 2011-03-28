// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.03.04

using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Threading;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Disposing;
using Xtensive.Reflection;
using Xtensive.Threading;
using Xtensive.Helpers;
using Xtensive.Internals.DocTemplates;
using Xtensive.Resources;

namespace Xtensive.IO
{
  /// <summary>
  /// Provides read or write of streams for a single file.
  /// <see langword="Thread-safe." />
  /// </summary>
  public class StreamProvider : IDisposable
  {
    private const int DefaultPoolCapacity = 16;
    private const int DefaultBufferSize = 16384;

    private readonly string fileName;
    private readonly int bufferSize;
    private readonly ReaderWriterLockSlim rwLock;
    private readonly Action<bool, Stream> releaseStreamHandler;
    private Pool<Stream> pool;

    private Stream writeStream;

    /// <summary>
    /// Gets the name of the file to provide streams for.
    /// </summary>
    public string FileName
    {
      get { return fileName; }
    }

    /// <summary>
    /// Gets the size of the buffer used by the underlying streams.
    /// </summary>
    public int BufferSize
    {
      get { return bufferSize; }
    }

    /// <summary>
    /// Indicates whether file with <see cref="FileName"/> exists or not.
    /// </summary>
    public virtual bool FileExists
    {
      get { return File.Exists(fileName); }
    }

    /// <summary>
    /// Gets the stream from pool or creates a new one, but wrapped into <see cref="LeasedAccessor{T}"/>.
    /// So this method acts as <see cref="GetStream(LockType)"/>, but its result can be used within <see langword="using"/> language construction.
    /// </summary>
    /// <param name="lockType">Type of lock.</param>
    /// <returns>A <see cref="LeasedAccessor{T}"/> providing <see cref="Stream"/> allowing to access the file.</returns>
    public LeasedAccessor<Stream> GetStreamAccessor(LockType lockType)
    {
      return new LeasedAccessor<Stream>(GetStream(lockType), releaseStreamHandler);
    }

    /// <summary>
    /// Gets the stream from pool or creates a new one using <see cref="LockType.Shared"/> lock type.
    /// </summary>
    /// <returns>Stream allowing to access the file.</returns>
    public Stream GetStream()
    {
      return GetStream(LockType.Read);
    }

    /// <summary>
    /// Gets the stream from pool or creates a new one.
    /// </summary>
    /// <param name="lockType">Type of lock.</param>
    /// <returns>Stream allowing to access the file.</returns>
    public Stream GetStream(LockType lockType)
    {
      EnsureNotDisposed();
      if ((lockType & LockType.Write)!=0) {
        rwLock.BeginWrite();
        try {
          if (writeStream==null)
            writeStream = CreateStream();
          return writeStream;
        }
        catch (Exception) {
          rwLock.EndWrite();
          throw;
        }
      }
      else {
        rwLock.BeginRead();
        try {
          return pool.Consume(CreateStream);
        }
        catch (Exception) {
          rwLock.EndRead();
          throw;
        }
      }
    }

    /// <summary>
    /// Releases the stream previously acquired by <see cref="GetStream()"/> back to the pool.
    /// </summary>
    /// <param name="stream">Stream to release.</param>
    public void ReleaseStream(Stream stream)
    {
      ArgumentValidator.EnsureArgumentNotNull(stream, "stream");
      EnsureNotDisposed();
      if (ReferenceEquals(stream, writeStream)) {
        if (ResetStream(stream)!=null)
          writeStream = null;
        rwLock.EndWrite();
      }
      else {
        if (ResetStream(stream)!=null) {
          pool.Release(stream);
          pool.Remove(stream);
        }
        else
          pool.Release(stream);
        rwLock.EndRead();
      }
    }

    /// <summary>
    /// Ensures instance is not disposed.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown when object is already disposed.</exception>
    protected void EnsureNotDisposed()
    {
      if (pool==null) {
        Log.Error(Strings.LogAttemptToUseDisposedInstance, GetType().GetShortName());
        throw new ObjectDisposedException(GetType().GetShortName());
      }
    }

    #region Protected virtual methods (to override)

    /// <summary>
    /// Creates a new stream.
    /// </summary>
    /// <returns>Newly created stream.</returns>
    protected virtual Stream CreateStream()
    {
      return new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, bufferSize, FileOptions.RandomAccess);
    }

    /// <summary>
    /// Resets the stream state before returning it to pool.
    /// </summary>
    /// <param name="stream">Stream to reset.</param>
    /// <returns>An exception, if it was thrown during reset procedure; 
    /// otherwise, <see langword="null"/>.</returns>
    protected virtual Exception ResetStream(Stream stream)
    {
      try {
        stream.Position = 0;
        return null;
      }
      catch (Exception e) {
        return e;
      }
    }

    #endregion

    #region Private \ internal methods

    private void PoolItemRemoveHandler(object sender, ItemRemovedEventArgs<Stream> e)
    {
      e.Item.DisposeSafely(Log.Instance, GetType().GetShortName());
    }

    private void ReleaseStreamHandler(bool disposing, Stream stream)
    {
      ReleaseStream(stream);
    }

    #endregion

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="fileName">Name of the file to provide streams for.</param>
    public StreamProvider(string fileName)
      : this(fileName, DefaultBufferSize)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="fileName">Name of the file to provide streams for.</param>
    /// <param name="bufferSize">Size of the buffer of the underlying streams.</param>
    public StreamProvider(string fileName, int bufferSize)
      : this(fileName, DefaultPoolCapacity, bufferSize)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="fileName">Name of the file to provide streams for.</param>
    /// <param name="poolCapacity">Stream pool capacity.</param>
    /// <param name="bufferSize">Size of the buffer of the underlying streams.</param>
    public StreamProvider(string fileName, int poolCapacity, int bufferSize)
    {
      this.fileName = fileName;
      this.bufferSize = bufferSize;
      rwLock = new ReaderWriterLockSlim();
      releaseStreamHandler = ReleaseStreamHandler;
      pool = new Pool<Stream>(poolCapacity);
      pool.ItemRemoved += PoolItemRemoveHandler;
    }

    // Destructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Dispose" copy="true"/>
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Dtor" copy="true"/>
    /// </summary>
    ~StreamProvider()
    {
      Dispose(false);
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Dispose" copy="true"/>
    /// </summary>
    protected void Dispose(bool disposing)
    {
      if (disposing) {
        using (rwLock.WriteRegion()) {
          EnsureNotDisposed();
          Pool<Stream> poolBackup = pool;
          pool = null;
          poolBackup.ItemRemoved -= PoolItemRemoveHandler;
          Enumerable.Cast<IDisposable>(poolBackup).DisposeSafely(Log.Instance, GetType().GetShortName());
        }
      }
    }    
  }
}
// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2007.10.26

using System;
using System.Globalization;
using System.IO;
using System.Threading;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Serialization;
using Xtensive.Core.Serialization.Binary;
using Xtensive.Core.Serialization.Implementation;
using Xtensive.Core.Threading;
using Xtensive.TransactionLog.Providers;
using Xtensive.TransactionLog.Resources;

namespace Xtensive.TransactionLog
{
  /// <summary>
  /// Counter what periodically persists it's value to <see cref="ILogProvider"/>.
  /// </summary>
  /// <typeparam name="TKey">Type of value.</typeparam>
  public sealed class PersistCounter<TKey> : IDisposable, ISynchronizable where TKey : IComparable<TKey>
  {
    private const int SlotCount = 3;

    private readonly TimeSpan flushTimeout;
    private readonly ValueSerializer<Stream,TKey> serializer;
    private readonly ValueSerializer<Stream, int> hashSerializer = BinaryValueSerializerProvider.Default.GetSerializer<int>();
    private readonly Stream[] slots = new Stream[SlotCount];
    private int currentSlot = -1;
    private TKey value;
    private TKey persistedValue;
    private readonly ReaderWriterLockSlim syncRoot = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
    private Thread autoflushThread;
    private readonly string folderName;

    /// <summary>
    /// Gets counter value.
    /// </summary>
    /// <exception cref="InvalidOperationException">Counter has no values yet.</exception>
    public TKey Value {
      get {
        using (LockType.Shared.LockRegion(SyncRoot)) {
          if (!HasValue) {
            throw new InvalidOperationException(Strings.ExCounterHasNoValue);
          }
          return value;
        }
      }
      set {
        using (LockType.Exclusive.LockRegion(SyncRoot)) {
          ArgumentValidator.EnsureArgumentNotNull(value, "value");
          if (HasValue && value.CompareTo(this.value) <= 0) {
            throw new InvalidOperationException(Strings.ExInvalidCounterSequence);
          }
          this.value = value;
          if (!HasValue)
            currentSlot = 0;
        }
      }
    }
    
    /// <summary>
    /// Gets last persisted value.
    /// </summary>
    /// <exception cref="InvalidOperationException">Counter has no values yet.</exception>
    public TKey PersistedValue {
      get {
        using (LockType.Shared.LockRegion(SyncRoot)) {
          if (!HasValue) {
            throw new InvalidOperationException(Strings.ExCounterHasNoValue);
          }
          return persistedValue;
        }
      }
    }

    /// <summary>
    /// Gets <see langword="true"/> if counter has values, otherwise gets <see langword="false"/>.
    /// </summary>
    public bool HasValue {
      get {
        using (LockType.Shared.LockRegion(SyncRoot))
         return currentSlot!=-1;
      }
      set {
        using (LockType.Shared.LockRegion(SyncRoot))
          if (!value)
            currentSlot = -1;
      }
    }

    /// <summary>
    /// Gets time period of automatic persist.
    /// </summary>
    public TimeSpan FlushTimeout
    {
      get { return flushTimeout; }
    }

    /// <summary>
    /// Persists counter's value to <see cref="ILogProvider"/>.
    /// </summary>
    public void Flush()
    {
      using (LockType.Exclusive.LockRegion(SyncRoot))
        if (HasValue) {
          if (persistedValue.CompareTo(value) == 0)
            return;
          Stream stream = slots[currentSlot];
          stream.Seek(0, SeekOrigin.Begin);
          serializer.Serialize(stream, value);
          hashSerializer.Serialize(stream, value.GetHashCode());
          stream.Flush();
          persistedValue = value;
          currentSlot = (currentSlot + 1)%SlotCount;
        }
        else {
          foreach (Stream slot in slots) {
            slot.SetLength(0);
            slot.Flush();
          }
        }
    }

    #region ISynchronizable members

    /// <inheritdoc/>
    public bool IsSynchronized
    {
      get { return true; }
    }

    /// <inheritdoc/>
    public object SyncRoot
    {
      get { return syncRoot; }
    }

    #endregion

    #region Private \ protected methods

    private void AutoFlush()
    {
      while (true) {
        Thread.Sleep(flushTimeout);
        Flush();
      }
// ReSharper disable FunctionNeverReturns
    }
// ReSharper restore FunctionNeverReturns

    #endregion

    
    // Constructors
    
    /// <summary>
    /// Creates new instance of <see cref="PersistCounter{TKey}"/>.
    /// </summary>
    /// <param name="name">Name of counter (with path).</param>
    /// <param name="logProvider"><see cref="ILogProvider"/> to store or restore counter's value.</param>
    /// <param name="flushTimeout">Time period of automatic persist</param>
    /// <param name="serializer"><see cref="IValueSerializer{T}"/> to serialize/deserialize values.</param>
    public PersistCounter(string name, ILogProvider logProvider, TimeSpan flushTimeout, ValueSerializer<Stream,TKey> serializer)
    {
      ArgumentValidator.EnsureArgumentNotNull(name, "name");
      ArgumentValidator.EnsureArgumentNotNull(logProvider, "logProvider");
      ArgumentValidator.EnsureArgumentNotNull(serializer, "serializer");
      ArgumentValidator.EnsureArgumentIsInRange(flushTimeout, TimeSpan.Zero.Add(TimeSpan.FromTicks(1)),
        TimeSpan.MaxValue, "flushTimeout");
      this.serializer = serializer;
      // Restore values
      folderName = Path.GetDirectoryName(name);
      if (!logProvider.FolderExists(folderName)) {
        logProvider.CreateFolder(folderName);
      }
      for (int i = 0; i < SlotCount; i++) {
        string fileName = string.Format(CultureInfo.CurrentCulture, "{0}_{1}", name, i);
        Stream stream = logProvider.GetFileStream(fileName, FileMode.OpenOrCreate);
        slots[i] = stream;
        stream.Seek(0, SeekOrigin.Begin);
        if (stream.Length > 0) {
          try {
            TKey deserializedValue = this.serializer.Deserialize(stream);
            int hash = hashSerializer.Deserialize(stream);
            if (deserializedValue.GetHashCode()==hash && (currentSlot==-1 || deserializedValue.CompareTo(value)>0)) {
              persistedValue = value = deserializedValue;
              currentSlot = i;
            }
          }
          catch (Exception ex){
            Log.Info(ex, Strings.LogUnableToRestoreDataFromXxxSlot, fileName);
          }
        }
      }
      this.flushTimeout = flushTimeout;
      autoflushThread = new Thread(AutoFlush);
      autoflushThread.Start();
    }

    // Disposable pattern

    /// <see cref="DisposableDocTemplate.Dispose(bool)" copy="true"/>
    private void Dispose(bool disposing)
    {
      using (LockType.Exclusive.LockRegion(SyncRoot)) {
        if (autoflushThread != null && autoflushThread.IsAlive)
          autoflushThread.Abort();
        autoflushThread = null;
        if (disposing) {
          for (int i = 0; i < SlotCount; i++) {
            try {
              slots[i].Dispose();
            }
            catch (Exception ex) {
              Log.Info(ex, Strings.LogUnableToDisposeSlotXxx, i);
            }
          }
        }
      }
    }

    /// <see cref="DisposableDocTemplate.Dispose()" copy="true"/>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <see cref="DisposableDocTemplate.Dtor" copy="true"/>
    ~PersistCounter()
    {
      Dispose(false);
    }
  }
}
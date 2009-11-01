// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2007.10.30

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Serialization.Binary;
using Xtensive.Integrity.Transactions;
using Xtensive.TransactionLog.Providers;
using Xtensive.TransactionLog.Resources;

namespace Xtensive.TransactionLog
{
  /// <summary>
  /// Transaction log. 
  /// Stores and reads <see cref="ITransactionInfo{TKey}"/> records.
  /// </summary>
  /// <typeparam name="TKey">Type of the key of <see cref="ITransactionInfo{TKey}"/> records.</typeparam>
  public sealed class TransactionLog<TKey> : IDisposable
    where TKey : struct, IComparable<TKey>
  {
    private const int LogSegmentSize = 1024 * 1024;

    private readonly ILogProvider provider;
    private readonly string name;
    private readonly IFileNameFormatter<TKey> keyFormatter;
    private readonly long maxSegmentLength;
    private Stream segment;
    private TKey segmentKey;
    private readonly IFormatter transactionFormatter;
    private PersistCounter<TKey> firstUncommited;
    private TKey maxKey;
    private bool maxKeyHasValue;
    private readonly SortedCollection<TKey> uncommitedItems = new SortedCollection<TKey>();
    private readonly SortedCollection<TKey> segments = new SortedCollection<TKey>();
    private const string FirstUncommitedCounterNameFormat = @"{0}\Counters\FirstUncommittedCounter";

    #region Properties: LogProvider, Name, FirstUncommitted

    /// <summary>
    /// Gets <see cref="ILogProvider"/> used by log to store data to media.
    /// </summary>
    public ILogProvider Provider
    {
      get { return provider; }
    }

    /// <summary>
    /// Gets log name
    /// </summary>
    public string Name
    {
      get { return name; }
    }

    /// <summary>
    /// Gets key of first uncommitted record. 
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if no uncommitted records are available.</exception>
    public TKey? FirstUncommitted
    {
      get
      {
        if (firstUncommited.HasValue)
          return firstUncommited.Value;
        return null;
      }
    }

    #endregion

    #region Append, Truncate, Read methods

    /// <summary>
    /// Appends <see cref="ITransactionInfo{TKey}"/> to the log.
    /// </summary>
    /// <param name="record">Serializable <see cref="ITransactionInfo{TKey}"/> object to store to log.</param>
    public void Append(ITransactionInfo<TKey> record)
    {
      ArgumentValidator.EnsureArgumentNotNull(record, "record");
      if (record.State==TransactionState.Invalid)
        throw new InvalidOperationException(string.Format(Strings.ExRecordXxxStateIsInvalid, record.Identifier));
      TKey key = record.Identifier;
      if (firstUncommited.HasValue && firstUncommited.Value.CompareTo(key) > 0)
        throw new InvalidOperationException(Strings.ExInvalidKeySequance);
      if (record.State==TransactionState.Active) {
        SaveRecord(record);
        if (!firstUncommited.HasValue)
          firstUncommited.Value = key;
        if (!uncommitedItems.Contains(key))
          uncommitedItems.Add(key);
      }
      else {
        if (!uncommitedItems.Contains(key))
          throw new InvalidOperationException(Strings.ExTransactionIsNotActive);
        SaveRecord(record);
        uncommitedItems.Remove(key);
        if (uncommitedItems.Count > 0) {
          firstUncommited.Value = uncommitedItems[0];
        }
        else {
          firstUncommited.HasValue = false;
        }
      }
    }

    /// <summary>
    /// Truncates log due to specified record. Deletes all records with key less than specified.
    /// </summary>
    /// <param name="recordKey">Key of record to truncate from.</param>
    public void Truncate(TKey recordKey)
    {
      if (firstUncommited.HasValue && firstUncommited.Value.CompareTo(recordKey) < 0)
        recordKey = firstUncommited.Value;
      int index = GetSegmentIndex(recordKey);
      for (int i = 0; i <= index - 1; i++) {
        provider.DeleteFile(GetSegmentName(segments[i]));
      }
      segments.RemoveRange(0, index);
    }

    /// <summary>
    /// Gets <see cref="IEnumerator{T}"/> enumerating (reading) all the records starting
    /// from the transaction with specified <paramref name="startFromKey"/> and all
    /// successive records.
    /// </summary>
    /// <param name="startFromKey">Transaction key to enumerate the records from.</param>
    /// <returns>Described enumerator.</returns>
    public IEnumerable<ITransactionInfo<TKey>> Read(TKey startFromKey)
    {
      int index = GetSegmentIndex(startFromKey);
      if (index!=-1) {
        bool read = false;
        for (int i = 0; i < segments.Count; i++) {
          // Read through segments
          Stream segmentStream;
          bool disposeSegment = false;
          if (segment!=null && segmentKey.CompareTo(segments[i])==0) {
            segmentStream = segment;
            segmentStream.Position = 0;
          }
          else {
            segmentStream = provider.GetFileStream(GetSegmentName(segments[i]), FileMode.Open);
            disposeSegment = true;
          }
          try {
            while (segmentStream.Position < segmentStream.Length) {
              var transaction = (ITransactionInfo<TKey>) transactionFormatter.Deserialize(segmentStream);
              if (read || transaction.Identifier.CompareTo(startFromKey)==0) {
                read = true;
                yield return transaction;
              }
            }
          }
          finally {
            if (disposeSegment) {
              segmentStream.Close();
              segmentStream.Dispose();
            }
            else {
              segmentStream.Position = segmentStream.Length;
            }
          }
        }
      }
    }

    #endregion

    #region Private \ internal methods

    private void InitLog(IFileNameFormatter<TKey> keyFileNameFormatter, TimeSpan autoFlushTimeout,
      ValueSerializer<TKey> keySerializer)
    {
      if (!provider.FolderExists(Name)) {
        provider.CreateFolder(Name);
      }
      string uncommitedCounterName = string.Format(CultureInfo.CurrentCulture, FirstUncommitedCounterNameFormat, Name);
      firstUncommited = new PersistCounter<TKey>(uncommitedCounterName, provider, autoFlushTimeout, keySerializer);

      string[] files = provider.GetFolderFiles(Name);
      foreach (string file in files) {
        try {
          TKey key = keyFileNameFormatter.RestoreFromString(file);
          segments.Add(key);
        }
        catch (Exception ex){
          Log.Info(ex, Strings.LogUnableToRestoreFileXxx, file);
        }
      }
      // Restore states
      if (segments.Count > 0) {
        int index = GetSegmentIndex(firstUncommited.Value);
        if (index!=-1) {
          for (int i = index; i < segments.Count; i++) {
            using (Stream checkStream = provider.GetFileStream(GetSegmentName(segments[i]), FileMode.Open)) {
              long lastPosition = checkStream.Position;
              while (lastPosition < checkStream.Length) {
                try {
                  var transaction = (ITransactionInfo<TKey>) transactionFormatter.Deserialize(checkStream);
                  if (!maxKeyHasValue || transaction.Identifier.CompareTo(maxKey) > 0) {
                    maxKey = transaction.Identifier;
                    maxKeyHasValue = true;
                  }
                  if (transaction.State==TransactionState.Active &&
                    !uncommitedItems.Contains(transaction.Identifier)) {
                    uncommitedItems.Add(transaction.Identifier);
                    if (!firstUncommited.HasValue)
                      firstUncommited.Value = transaction.Identifier;
                  }
                  if (transaction.State==TransactionState.Completed &&
                    uncommitedItems.Contains(transaction.Identifier)) {
                    uncommitedItems.Remove(transaction.Identifier);
                  }
                }
                catch {
                  checkStream.SetLength(lastPosition);
                  break;
                }
              }
            }
          }
        }
      }
    }

    private void SaveRecord(ITransactionInfo<TKey> record)
    {
      if (segment!=null && segment.Length >= maxSegmentLength * LogSegmentSize && maxKeyHasValue &&
        maxKey.CompareTo(record.Identifier) < 0) {
        segment.Flush();
        segment.Close();
        segment.Dispose();
        segment = null;
      }
      if (segment==null) {
        string segmentName = GetSegmentName(record.Identifier);
        segment = provider.GetFileStream(segmentName, FileMode.Create);
        segmentKey = record.Identifier;
        if (!segments.Contains(record.Identifier))
          segments.Add(record.Identifier);
      }
      transactionFormatter.Serialize(segment, record);
      if (record.State!=TransactionState.Completed)
        segment.Flush();
      if (!maxKeyHasValue || maxKey.CompareTo(record.Identifier) < 0) {
        maxKey = record.Identifier;
        maxKeyHasValue = true;
      }
    }

    private string GetSegmentName(TKey key)
    {
      return string.Format(CultureInfo.CurrentCulture, @"{0}\{1}", name, keyFormatter.SaveToString(key));
    }

    private int GetSegmentIndex(TKey key)
    {
      if (segments.Count==0)
        return -1;
      int index = segments.BinarySearch(key);
      if (index < 0)
        index = (index ^ -1) - 1;
      if (index < 0)
        index = 0;
      return index;
    }

    #endregion

    // Constructors

    /// <summary>
    /// Creates new instance of <see cref="TransactionLog{TKey}"/>.
    /// </summary>
    /// <param name="provider">Log provider.</param>
    /// <param name="logName">Log name.</param>
    /// <param name="keyFileNameFormatter"><see cref="IFileNameFormatter{TKey}"/> that formats <typeparamref name="TKey"/> to filename and visa versa.</param>
    /// <param name="autoFlushTimeout">Period of time than counters flushes their state to media.</param>
    /// <param name="maxSegmentLength">Max length of segment in megabytes.</param>
    /// <param name="transactionFormatter"><see cref="IFormatter"/> that serializes and deserializes <see cref="ITransactionInfo{TKey}"/>.</param>
    /// <param name="keySerializer"><see cref="IValueSerializer{T}"/> that serializes specified <typeparamref name="TKey"/>.</param>
    public TransactionLog(ILogProvider provider, string logName, IFileNameFormatter<TKey> keyFileNameFormatter,
      TimeSpan autoFlushTimeout, long maxSegmentLength, IFormatter transactionFormatter, ValueSerializer<TKey> keySerializer)
    {
      ArgumentValidator.EnsureArgumentNotNull(provider, "provider");
      ArgumentValidator.EnsureArgumentNotNull(logName, "name");
      ArgumentValidator.EnsureArgumentNotNull(keyFileNameFormatter, "keyFileNameFormatter");
      ArgumentValidator.EnsureArgumentIsInRange(autoFlushTimeout, TimeSpan.Zero.Add(TimeSpan.FromTicks(1)),
        TimeSpan.MaxValue, "autoFlushTimeout");
      this.provider = provider;
      name = logName;
      keyFormatter = keyFileNameFormatter;
      this.maxSegmentLength = maxSegmentLength;
      this.transactionFormatter = transactionFormatter ?? new BinaryFormatter();

      InitLog(keyFileNameFormatter, autoFlushTimeout, keySerializer);
    }

    // Disposable pattern

    /// <see cref="DisposableDocTemplate.Dispose(bool)" copy="true"/>
    private void Dispose(bool disposing)
    {
      if (disposing) {
        if (segment!=null) {
          segment.Flush();
          segment.Close();
          segment.Dispose();
        }
        firstUncommited.Flush();
        firstUncommited.Dispose();
      }
    }

    /// <see cref="DisposableDocTemplate.Dispose()" copy="true"/>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <see cref="DisposableDocTemplate.Dtor()" copy="true"/>
    ~TransactionLog()
    {
      Dispose(false);
    }
  }
}
// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2010.01.21

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.Operations;

namespace Xtensive.Orm.ObjectMapping
{
  /// <summary>
  /// Result of comparison the original graph of target objects with the modified one.
  /// </summary>
  [Serializable]
  public sealed class GraphComparisonResult : Xtensive.ObjectMapping.GraphComparisonResult,
    IDisposable
  {
    private readonly Dictionary<object, object> original;
    private readonly Dictionary<object, object> modified;
    [NonSerialized]
    private BinaryFormatter formatter;
    [NonSerialized]
    private MemoryStream stream;
    [NonSerialized]
    private Dictionary<Key, VersionInfo> deserializedVersions;
    [NonSerialized]
    private Func<Key, VersionInfo> versionInfoProvider;

    /// <summary>
    /// Gets the operation sequence describing the changes.
    /// </summary>
    public new IOperationSequence Operations { 
      get { return (IOperationSequence) base.Operations; }
    }

    /// <summary>
    /// Gets the delegate that should be used to resolve object versions.
    /// </summary>
    public Func<Key, VersionInfo> VersionInfoProvider
    {
      get { return versionInfoProvider; }
    }

    private VersionInfo GetVersionInfo(Key key)
    {
      if (stream==null) {
        stream = new MemoryStream(1024);
        formatter = new BinaryFormatter();
        deserializedVersions = new Dictionary<Key, VersionInfo>();
      }
      VersionInfo result;
        if (!deserializedVersions.TryGetValue(key, out result)) {
          var keyString = key.Format();
          object foundObject;
          if (modified.TryGetValue(keyString, out foundObject)
            || original.TryGetValue(keyString, out foundObject)) {
            var version = ((IHasBinaryVersion) foundObject).Version;
            stream.SetLength(version.Length);
            stream.Seek(0, SeekOrigin.Begin);
            stream.Write(version, 0, version.Length);
            stream.Seek(0, SeekOrigin.Begin);
            result = (VersionInfo) formatter.Deserialize(stream);
            deserializedVersions.Add(key, result);
          }
          else
            return default (VersionInfo);
        }
        return result;
    }


    // Constructors

    internal GraphComparisonResult(Dictionary<object, object> original, Dictionary<object, object> modified,
      IOperationSequence operations, ReadOnlyDictionary<object, object> keyMapping)
      : base(operations, keyMapping)
    {
      this.original = original;
      this.modified = modified;
      versionInfoProvider = GetVersionInfo;
    }

    /// <summary>
    /// Called when the instance is deserialized.
    /// </summary>
    /// <param name="context">The serialization context.</param>
    [OnDeserialized]
    protected void OnDeserialized(StreamingContext context)
    {
      versionInfoProvider = GetVersionInfo;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
      stream.DisposeSafely();
      deserializedVersions = null;
    }
  }
}
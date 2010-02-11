// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.22

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using IObjectMappingOperationSet=Xtensive.Core.ObjectMapping.IOperationSet;

namespace Xtensive.Storage.Operations
{
  /// <summary>
  /// Built-in implementation of <see cref="IOperationSet"/>.
  /// </summary>
  [Serializable]
  public sealed class OperationSet : IOperationSet, 
    IObjectMappingOperationSet
  {
    private readonly List<IOperation> log = new List<IOperation>();
    private readonly List<SerializableKey> serializableKeys = new List<SerializableKey>();
    [NonSerialized]
    private HashSet<Key> keys;
    [NonSerialized]
    private ReadOnlyHashSet<Key> readOnlyKeys;

    /// <inheritdoc/>
    public long Count {
      get { return log.Count; }
    }

    /// <inheritdoc/>
    public bool IsEmpty {
      get { return log.Count==0; }
    }

    /// <inheritdoc/>
    public ReadOnlyHashSet<Key> NewKeys {
      get { return readOnlyKeys; }
    }

    /// <inheritdoc/>
    public void RegisterNewKey(Key key)
    {
      if (keys.Add(key))
        serializableKeys.Add(key);
    }

    /// <inheritdoc/>
    public void Append(IOperation operation)
    {
      log.Add(operation);
    }

    /// <inheritdoc/>
    public void Append(IOperationSet source)
    {
      log.AddRange(source);
      foreach (var key in source.NewKeys)
        RegisterNewKey(key);
    }

    /// <inheritdoc/>
    void IObjectMappingOperationSet.Apply()
    {
      Apply();
    }

    /// <inheritdoc/>
    public KeyMapping Apply()
    {
      return Apply(Session.Demand());
    }

    /// <inheritdoc/>
    public KeyMapping Apply(Session session)
    {
      var operationContext = new OperationExecutionContext(session, this);

      using (session.Activate())
      using (var ts = Transaction.Open(TransactionOpenMode.New)) { 
        foreach (var operation in log)
          operation.Prepare(operationContext);

        operationContext.KeysToPrefetch
          .Prefetch<Entity,Key>(key => key)
          .Execute();

        foreach (var operation in log)
          operation.Execute(operationContext);

        ts.Complete();
      }

      return new KeyMapping(operationContext.KeyMapping);
    }

    /// <inheritdoc/>
    public void Clear()
    {
      log.Clear();
      serializableKeys.Clear();
      keys.Clear();
    }

    #region IEnumerable<...> implementation

    /// <inheritdoc/>
    public IEnumerator<IOperation> GetEnumerator()
    {
      return log.GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public OperationSet()
    {
      keys = new HashSet<Key>();
      readOnlyKeys = new ReadOnlyHashSet<Key>(keys);
    }

    /// <summary>
    /// Called when operation set is deserialized.
    /// </summary>
    /// <param name="context">The serialization context.</param>
    [OnDeserialized]
    protected void OnDeserialized(StreamingContext context)
    {
      keys = new HashSet<Key>();
      readOnlyKeys = new ReadOnlyHashSet<Key>(keys);
      foreach (var serializedKey in serializableKeys)
        keys.Add(serializedKey.Key);
    }
  }
}
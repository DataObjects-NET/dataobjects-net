// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.22

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Operations
{
  /// <summary>
  /// Built-in implementation of <see cref="IOperationSet"/>.
  /// </summary>
  [Serializable]
  public sealed class OperationSet : IOperationSet
  {
    private readonly List<IOperation> log;
    private readonly List<SerializableKey> serializableKeys;

    /// <inheritdoc/>
    public HashSet<Key> GetKeysToRemap()
    {
      return new HashSet<Key>(serializableKeys.Select(sk => sk.Key));
    }

    /// <inheritdoc/>
    public void RegisterKeyToRemap(Key key)
    {
      serializableKeys.Add(key);
    }

    /// <inheritdoc/>
    public void Register(IOperation operation)
    {
      log.Add(operation);
    }

    /// <inheritdoc/>
    public void Register(IOperationSet source)
    {
      log.AddRange(source);
      serializableKeys.AddRange(source.GetKeysToRemap().Select(k => (SerializableKey)k));
    }

    /// <inheritdoc/>
    public KeyMapping Apply(Session session)
    {
      var operationContext = new OperationExecutionContext(session, this);
      foreach (var operation in log)
        operation.Prepare(operationContext);
      operationContext
        .Prefetch<Entity,Key>(key => key)
        .Execute();

      foreach (var operation in log)
        operation.Execute(operationContext);

      log.Clear();
      return new KeyMapping(operationContext.KeyMapping);
    }

    #region IEnumerable implementation

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
      log = new List<IOperation>();
      serializableKeys = new List<SerializableKey>();
    }
  }
}
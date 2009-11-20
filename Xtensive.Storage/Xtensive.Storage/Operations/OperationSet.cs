// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.22

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Xtensive.Storage.Operations
{
  [Serializable]
  public sealed class OperationSet : IOperationSet
  {
    private readonly List<IOperation> log;
    private readonly List<SerializableKey> serializableKeys;

    public HashSet<Key> GetKeysForRemap()
    {
      return new HashSet<Key>(serializableKeys.Select(sk => sk.Key));
    }

    public void RegisterKeyForRemap(Key key)
    {
      serializableKeys.Add(key);
    }

    public void Register(IOperation operation)
    {
      log.Add(operation);
    }

    public void Register(IOperationSet source)
    {
      log.AddRange(source);
      serializableKeys.AddRange(source.GetKeysForRemap().Select(k => (SerializableKey)k));
    }

    public KeyMapping Apply(Session session)
    {
      var operationContext = new OperationContext(session, this);
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

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public IEnumerator<IOperation> GetEnumerator()
    {
      return log.GetEnumerator();
    }

    #endregion


    // Constructors

    public OperationSet()
    {
      log = new List<IOperation>();
      serializableKeys = new List<SerializableKey>();
    }
  }
}
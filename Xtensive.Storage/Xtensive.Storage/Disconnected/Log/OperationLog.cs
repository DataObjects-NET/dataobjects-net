// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.22

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Xtensive.Storage.Disconnected.Log
{
  [Serializable]
  public class OperationLog : IOperationLog
  {
    private readonly List<IOperation> log;

    public void Register(IOperation operation)
    {
      log.Add(operation);
    }

    public void Append(IEnumerable<IOperation> operations)
    {
      log.AddRange(operations);
    }

    public void Apply(Session session)
    {
      var prefetchContext = new PrefetchContext();
      foreach (var operation in log)
        operation.Prepare(prefetchContext);
      prefetchContext.Prefetch<Entity,Key>(key => key);

      foreach (var operation in log)
        operation.Execute(session);

      log.Clear();
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

    public OperationLog()
    {
      log = new List<IOperation>();
    }
  }
}
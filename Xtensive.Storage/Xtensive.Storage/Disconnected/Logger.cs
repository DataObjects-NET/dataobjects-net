// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.22

using System;
using Xtensive.Storage.Operations;

namespace Xtensive.Storage.Disconnected
{
  public class Logger : IDisposable
  {
    private readonly Session session;
    private readonly IOperationSet set;

    public void Dispose()
    {
      DetachEventHandlers();
    }

    private void AttachEventHandlers()
    {
      session.LocalKeyCreated += LocalKeyCreated;
      session.OperationRegister += OperationRegister;
    }

    private void DetachEventHandlers()
    {
      session.LocalKeyCreated -= LocalKeyCreated;
      session.OperationRegister -= OperationRegister;
    }

    #region Session event handlers

    void LocalKeyCreated(object sender, KeyEventArgs e)
    {
      set.RegisterKeyForRemap(e.Key);
    }

    private void OperationRegister(object sender, OperationEventArgs e)
    {
      set.Register(e.Operation);
    }

    #endregion


    // Constructors

    public Logger(Session session, IOperationSet set)
    {
      this.session = session;
      this.set = set;

      AttachEventHandlers();
    }
  }
}
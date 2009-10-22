// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.22

using System;
using System.Diagnostics;

namespace Xtensive.Storage.Disconnected.Log
{
  public class Logger : IDisposable
  {
    private readonly Session session;

    private readonly IOperationLog log;

    public void Dispose()
    {
      DetachEventHandlers();
    }

    private void AttachEventHandlers()
    {
      throw new NotImplementedException();
    }

    private void DetachEventHandlers()
    {
      throw new NotImplementedException();
    }


    // Constructors

    public Logger(Session session, IOperationLog log)
    {
      this.session = session;
      this.log = log;

      AttachEventHandlers();
    }
  }
}
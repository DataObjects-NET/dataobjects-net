// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.06

using Xtensive.Integrity.Atomicity;

namespace Xtensive.Integrity.Tests
{
  public class AtomicityContext: AtomicityContextBase, 
    ISessionBound
  {
    private Session session;

    public Session Session
    {
      get { return session; }
    }

    // Constructors

    public AtomicityContext(Session session, AtomicityContextOptions options, IOperationLog operationLog) 
      : base(options, operationLog)
    {
      this.session = session;
    }
  }
}
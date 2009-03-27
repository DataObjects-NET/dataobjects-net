// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.06

using System;
using Xtensive.Integrity.Atomicity;
using Xtensive.Integrity.Atomicity.OperationLogs;

namespace Xtensive.Integrity.Tests
{
  public class Session
  {
    private readonly AtomicityContext atomicityContext;
    private readonly ValidationContext validationContext;

    public AtomicityContext AtomicityContext
    {
      get { return atomicityContext; }
    }

    public ValidationContext ValidationContext
    {
      get { return validationContext; }
    }

    public IDisposable Activate()
    {
      return atomicityContext.Activate();
    }

    
    // Constructors

    public Session()
      : this(AtomicityContextOptions.Default)
    {      
    }

    public Session(AtomicityContextOptions options)
    {
      if ((options & AtomicityContextOptions.Redoable)!=0)
        atomicityContext = new AtomicityContext(this, options, new LoggingMemoryOperationLog());
      else
        atomicityContext = new AtomicityContext(this, options, new DummyOperationLog());
      validationContext = new ValidationContext(this);
    }
  }
}
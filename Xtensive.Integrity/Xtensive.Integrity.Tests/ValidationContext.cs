// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.12.31

using System;
using Xtensive.Integrity.Validation;

namespace Xtensive.Integrity.Tests
{
  public class ValidationContext: ValidationContextBase,
    ISessionBound
  {
    private Session session;

    public Session Session
    {
      get { return session; }
    }

    public void CompleteRegion()
    {
      CompleteInconsistentRegion();
    }


    // Constructors

    public ValidationContext(Session session) 
    {
      this.session = session;
    }
  }
}
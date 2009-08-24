// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.08.20

using System;
using System.Diagnostics;

namespace Xtensive.Integrity.Validation
{
  public class InconsistentRegionBase : IDisposable
  {
    private readonly ValidationContextBase context;

    protected internal void CompleteRegion()
    {
      context.CompleteInconsistentRegion();
    }

    public void Dispose()
    {
      context.LeaveInconsistentRegion();
    }

    public InconsistentRegionBase(ValidationContextBase context)
    {
      this.context = context;
    }
  }
}
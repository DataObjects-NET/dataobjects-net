// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.08.20

using System;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Integrity.Validation
{
  /// <summary>
  /// Inconsistent region implementation.
  /// Returned by <see cref="ValidationContextBase.OpenInconsistentRegion"/> method.
  /// </summary>
  public sealed class InconsistentRegion : IDisposable
  {
    private readonly ValidationContextBase context;
    private bool isCompleted;

    /// <summary>
    /// Gets the <see cref="ValidationContextBase"/> instance this region belongs to.
    /// </summary>
    public ValidationContextBase Context {
      get { return context; }
    }

    /// <summary>
    /// Gets a value indicating whether this inconsistency is 
    /// <see cref="InconsistentRegionExtensions.Complete"/>d.
    /// </summary>
    public bool IsCompleted {
      get { return isCompleted; }
      internal set { isCompleted = value; }
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="context">The validation context this region belongs to.</param>
    public InconsistentRegion(ValidationContextBase context)
    {
      this.context = context;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
      context.LeaveInconsistentRegion(this);
    }
  }
}
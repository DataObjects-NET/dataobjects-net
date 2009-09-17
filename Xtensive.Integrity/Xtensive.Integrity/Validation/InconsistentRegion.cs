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

    private static readonly InconsistentRegion nestedRegionInstance = new InconsistentRegion();

    /// <summary>
    /// <see cref="InconsistentRegion"/> instance that is used for all <see cref="IsNested">nested</see> regions.
    /// </summary>
    public static InconsistentRegion NestedRegionInstance
    {
      get { return nestedRegionInstance; }
    }

    /// <summary>
    /// Gets a value indicating whether this instance is nested, i.e. is included into another <see cref="InconsistentRegion"/>.
    /// </summary>
    public bool IsNested
    {
      get { return this==NestedRegionInstance; }
    }

    /// <summary>
    /// Completes the region.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method must be called before disposal of
    /// any <see cref="InconsistentRegion"/>. Is invocation
    /// indicates <see cref="ValidationContextBase"/> must
    /// perform validation of region disposal. 
    /// </para>
    /// <para>
    /// If this method isn't called before region disposal, 
    /// validation context will receive <see cref="ValidationContextBase.IsValid"/>
    /// status, and any further attempts to validate there will fail.
    /// </para>
    /// </remarks>
    public void Complete()
    {
      if (!IsNested)
        IsCompleted = true;
    }

    internal bool IsCompleted { get; set; }


    // Constructors

    private InconsistentRegion()
    {
    }

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
      if (!IsNested)
        context.LeaveInconsistentRegion(this);
    }
  }
}
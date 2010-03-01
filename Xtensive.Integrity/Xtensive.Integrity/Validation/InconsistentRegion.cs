// Copyright (C) 2003-2010 Xtensive LLC.
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

    private static readonly InconsistentRegion voidRegionInstance = new InconsistentRegion();

    /// <summary>
    /// <see cref="InconsistentRegion"/> instance that is used for all <see cref="IsVoid">nested</see> regions.
    /// </summary>
    public static InconsistentRegion VoidRegionInstance
    {
      get { return voidRegionInstance; }
    }

    /// <summary>
    /// Gets a value indicating whether this instance is void region, 
    /// i.e. is included into another <see cref="InconsistentRegion"/> 
    /// and therefore does nothing on opening or disposing.
    /// </summary>
    public bool IsVoid
    {
      get { return this==VoidRegionInstance; }
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
    /// If this method isn't called before region disposal, validation will be performed on transaction commit.
    /// </para>
    /// </remarks>
    public void Complete()
    {
      if (!IsVoid)
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
      if (!IsVoid)
        context.LeaveInconsistentRegion(this);
    }
  }
}
// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.07.14

using System;
using Xtensive.Core;
using Xtensive.Integrity.Validation;

namespace Xtensive.Storage
{
  /// <summary>
  /// Helper class for specific validation regions (inconsistent regions) opening.
  /// </summary>
  public sealed class InconsistentRegion : InconsistentRegionBase
  {
    /// <summary>
    /// Opens the "inconsistent region" - the code region, in which Validate method
    /// should just queue the validation rather then perform it immediately.
    /// </summary>
    /// <returns>
    /// <see cref="IDisposable"/> object, which disposal will identify the end of the region.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The beginning of the region is the place where this method is called.
    /// </para>
    /// <para>
    /// The end of the region is the place where returned <see cref="IDisposable"/> object is disposed.
    /// The validation of all queued to-validate objects will be performed during disposal.
    /// </para>
    /// </remarks>
    public static InconsistentRegion Open()
    {
      var session = Session.Demand();
      return Open(session);
    }

    /// <summary>
    /// Opens the "inconsistent region" - the code region, in which Validate method
    /// should just queue the validation rather then perform it immediately.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <returns>
    /// <see cref="IDisposable"/> object, which disposal will identify the end of the region.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The beginning of the region is the place where this method is called.
    /// </para>
    /// <para>
    /// The end of the region is the place where returned <see cref="IDisposable"/> object is disposed.
    /// The validation of all queued to-validate objects will be performed during disposal.
    /// </para>
    /// </remarks>
    public static InconsistentRegion Open(Session session)
    {
      ArgumentValidator.EnsureArgumentNotNull(session, "session");
      var region = session.ValidationContext.OpenInconsistentRegion();
      if (region==null)
        return null;
      return new InconsistentRegion(session.ValidationContext);
    }

    internal new void CompleteRegion()
    {
      base.CompleteRegion();
    }

    // Constructors

    internal InconsistentRegion(ValidationContext context) 
      : base(context)
    {
    }
  }
}
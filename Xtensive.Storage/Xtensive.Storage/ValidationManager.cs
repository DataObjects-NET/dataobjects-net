// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.07.14

using System;
using Xtensive.Core;
using InconsistentRegion = Xtensive.Storage.Validation.InconsistentRegion;

namespace Xtensive.Storage
{
  /// <summary>
  /// Validation related helper class.
  /// </summary>
  public static class ValidationManager
  {
    /// <summary>
    /// Opens the "inconsistent region" - the code region, in which validation is
    /// just queued for delayed execution rather then performed immediately.
    /// Actual validation will happen on disposal of <see cref="InconsistentRegion"/>.
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
    /// The validation of all queued to validate objects will be performed during disposal, if
    /// <see cref="InconsistentRegion.Complete"/> method was called on
    /// <see cref="InconsistentRegion"/> object before disposal.
    /// </para>
    /// </remarks>
    public static InconsistentRegion Disable()
    {
      var session = Session.Demand();
      return Disable(session);
    }

    /// <summary>
    /// Opens the "inconsistent region" - the code region, in which validation is
    /// just queued for delayed execution rather then performed immediately.
    /// Actual validation will happen on disposal of <see cref="InconsistentRegion"/>.
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
    /// The validation of all queued to validate objects will be performed during disposal, if
    /// <see cref="InconsistentRegion.Complete"/> method was called on
    /// <see cref="InconsistentRegion"/> object before disposal.
    /// </para>
    /// </remarks>
    public static InconsistentRegion Disable(Session session)
    {
      ArgumentValidator.EnsureArgumentNotNull(session, "session");
      return session.ValidationContext.OpenInconsistentRegion();
    }

    /// <summary>
    /// Validates all instances registered in <see cref="ValidationContext"/>
    /// of current <see cref="Session"/> regardless if inconsistency
    /// regions are open or not.
    /// </summary>
    public static void Enforce()
    {
      var session = Session.Demand();
      Enforce(session);
    }

    /// <summary>
    /// Validates all instances registered in <see cref="ValidationContext"/>
    /// of specified <paramref name="session"/> regardless if inconsistency
    /// regions are open or not.
    /// </summary>
    /// <param name="session">The session.</param>
    public static void Enforce(Session session)
    {
      ArgumentValidator.EnsureArgumentNotNull(session, "session");
      session.ValidationContext.Validate();
    }
  }
}
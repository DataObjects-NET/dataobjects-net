// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.08.25

using System;
using System.Diagnostics;

namespace Xtensive.Integrity.Validation
{
  /// <summary>
  /// <see cref="InconsistentRegion"/> related extension methods.
  /// </summary>
  public static class InconsistentRegionExtensions
  {
    /// <summary>
    /// Completes the specified region.
    /// </summary>
    /// <param name="region">The region to complete.</param>
    /// <remarks>
    /// <para>
    /// This method must be called before disposal of
    /// any <see cref="InconsistentRegion"/>. Is invocation
    /// indicates <see cref="ValidationContextBase"/> must
    /// perform validation of region disposal. 
    /// </para>
    /// <para>
    /// If this method isn't called before region disposal, 
    /// validation context will receive <see cref="ValidationContextBase.IsConsistent"/>
    /// status, and any further attempts to validate there will fail.
    /// </para>
    /// </remarks>
    public static void Complete(this InconsistentRegion region)
    {
      if (region!=null)
        region.IsCompleted = true;
    }
  }
}
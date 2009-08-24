// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.08.20

namespace Xtensive.Storage
{
  public static class InconsistentRegionExtensions
  {
    /// <summary>
    /// Completes the <see cref="InconsistentRegion"/>.
    /// </summary>
    public static void Complete(this InconsistentRegion inconsistentRegion)
    {
      if (inconsistentRegion != null)
        inconsistentRegion.CompleteRegion();
    }
  }
}
// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.06.13

using System;

namespace Xtensive.Core
{
  /// <summary>
  /// Describes a class that automatically collects the garbage inside it.
  /// </summary>
  public interface IHasGarbageCollector: IHasGarbage
  {
    /// <summary>
    /// Gets the period of time between automatic garbage collections.
    /// If multiple periods are used (e.g. for generational GC), this should be the shortest one.
    /// </summary>
    TimeSpan GarbageCollectionPeriod { get; }
  }
}
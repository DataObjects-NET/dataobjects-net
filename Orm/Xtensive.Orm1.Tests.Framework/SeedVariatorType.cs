// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.09

using System;

namespace Xtensive.Orm.Tests
{
  /// <summary>
  /// Describes seed variation strategy for random sequence.
  /// </summary>
  [Flags]
  public enum SeedVariatorType
  {
    /// <summary>
    /// Default strategy.
    /// The same as <see cref="None"/>.
    /// </summary>
    Default = 0x0,
    /// <summary>
    /// No seed variation.
    /// </summary>
    None = 0x0,
    /// <summary>
    /// Seed determined by the calling type.
    /// </summary>
    CallingType = 0x1,
    /// <summary>
    /// Seed determined by the calling method.
    /// </summary>
    CallingMethod = 0x2,
    /// <summary>
    /// Seed determined by the calling assembly.
    /// </summary>
    CallingAssembly = 0x4,
    /// <summary>
    /// Seed determined by the current day (ie. truncated part of <see cref="DateTime.Now"/>).
    /// </summary>
    Day = 0x8,
  }
}
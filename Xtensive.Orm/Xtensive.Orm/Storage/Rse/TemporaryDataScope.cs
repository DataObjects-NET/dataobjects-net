// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.29

using System;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Rse
{
  /// <summary>
  /// Temporary data lifetime scope.
  /// </summary>
  [Serializable]
  public enum TemporaryDataScope
  {
    /// <summary>
    /// <see cref="EnumerationContext"/>-level scope.
    /// Temporary data "lives" for the duration of enumeration it is created in.
    /// </summary>
    Enumeration = 0,
    /// <summary>
    /// <see cref="Transaction"/>-level scope.
    /// Temporary data "lives" for the duration of transaction it is created in.
    /// </summary>
    Transaction = 1,
    /// <summary>
    /// Global (<see cref="Global"/>-level) scope.
    /// Temporary data "lives" infinitely.
    /// </summary>
    Global = 2,
  }
}
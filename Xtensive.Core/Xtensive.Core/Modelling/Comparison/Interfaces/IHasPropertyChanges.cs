// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.04.15

using System.Collections.Generic;

namespace Xtensive.Modelling.Comparison
{
  /// <summary>
  /// A contract of describing property change set.
  /// </summary>
  public interface IHasPropertyChanges : IDifference
  {
    /// <summary>
    /// Gets list of property changes.
    /// </summary>
    Dictionary<string, Difference> PropertyChanges { get; }
  }
}
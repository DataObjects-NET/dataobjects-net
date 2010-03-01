// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.26

using System;
using System.Collections.Generic;

namespace Xtensive.Modelling.Comparison.Hints
{
  /// <summary>
  /// An abstract base for comparison hint implementation.
  /// </summary>
  [Serializable]
  public abstract class Hint : IHint
  {
    /// <inheritdoc/>
    public abstract IEnumerable<HintTarget> GetTargets();
  }
}
// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.28

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Modelling.Actions;

namespace Xtensive.Modelling.Comparison
{
  /// <summary>
  /// Simple value comparison result.
  /// </summary>
  [Serializable]
  public class ValueDifference : Difference
  {
    /// <inheritdoc/>
    protected override string ParametersToString()
    {
      return "values differ";
    }


    // Constructors

    /// <inheritdoc/>
    public ValueDifference(object source, object target)
      : base(source, target)
    {
    }
  }
}
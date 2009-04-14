// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.14

using System;
using System.Diagnostics;

namespace Xtensive.Modelling.Comparison
{
  public interface IDifferentiable
  {
    Difference GetDifferenceWith(object source, string propertyName, bool swap);
  }
}
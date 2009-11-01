// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.03.08

namespace Xtensive.Core.SizeCalculators
{
  internal interface IHasSizeCalculator
  {
    ISizeCalculatorBase SizeCalculator { get; }
  }
}
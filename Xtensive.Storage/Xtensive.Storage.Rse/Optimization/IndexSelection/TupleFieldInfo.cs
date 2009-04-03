// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.03.20

using Xtensive.Core;
using Xtensive.Core.Linq;

namespace Xtensive.Storage.Rse.Optimization.IndexSelection
{
  /// <summary>
  /// Information about expression containing an access to a field of tuple.
  /// </summary>
  internal sealed class TupleFieldInfo
  {
    public readonly int FieldIndex;
    public readonly ComparisonInfo Comparison;

    // Constructors

    public TupleFieldInfo(int fieldIndex, ComparisonInfo comparison)
    {
      ArgumentValidator.EnsureArgumentIsInRange(fieldIndex, 0, int.MaxValue, "fieldIndex");
      FieldIndex = fieldIndex;
      Comparison = comparison;
    }
  }
}
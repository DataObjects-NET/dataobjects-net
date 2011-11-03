// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.03.20

using System;
using Xtensive.Core;
using Xtensive.Linq;
using IndexInfo = Xtensive.Orm.Model.IndexInfo;

namespace Xtensive.Storage.Rse.PreCompilation.Optimization.IndexSelection
{
  /// <summary>
  /// Information about expression containing an access to a field of tuple.
  /// </summary>
  [Serializable]
  internal sealed class TupleExpressionInfo
  {
    public readonly int FieldIndex;
    public readonly ComparisonInfo Comparison;


    // Constructors

    public TupleExpressionInfo(int fieldIndex, ComparisonInfo comparison)
    {
      ArgumentValidator.EnsureArgumentIsGreaterThan(fieldIndex, -1, "fieldIndex");
      FieldIndex = fieldIndex;
      Comparison = comparison;
    }
  }
}
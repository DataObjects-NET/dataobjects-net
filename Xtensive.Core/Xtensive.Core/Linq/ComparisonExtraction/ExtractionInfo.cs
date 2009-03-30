// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.03.30

using System;
using System.Linq.Expressions;

namespace Xtensive.Core.Linq.ComparisonExtraction
{
  [Serializable]
  internal class ExtractionInfo
  {
    public Expression Key { get; set; }

    public Expression Value { get; set; }

    public ComparisonMethodInfo MethodInfo { get; set; }

    public Expression ComparisonMethod { get; set; }

    public bool ReverseRequired { get; set; }

    public ExpressionType? ComparisonType { get; set; }

    public bool CanNormalize()
    {
      return Key!=null && Value!=null && ComparisonType!=null;
    }

    public void Normalize()
    {
      if (ReverseRequired)
        ComparisonType = ComparisonExtractor.ReverseOperation(ComparisonType.Value);
    }
  }
}
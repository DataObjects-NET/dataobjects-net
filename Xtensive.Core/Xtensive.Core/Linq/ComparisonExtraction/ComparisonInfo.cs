// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.03.27

using System;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Xtensive.Core.Linq.ComparisonExtraction
{
  [Serializable]
  [DebuggerDisplay("{Key} {Operation} {Value}")]
  public sealed class ComparisonInfo
  {
    public readonly Expression Key;

    public readonly Expression Value;

    public readonly ComparisonType ComparisonType;

    public readonly Expression ComplexMethod;

    public bool IsComplex { get { return ComplexMethod != null; } }

    private static ComparisonType ConvertToComparisonType(ExpressionType expressionType)
    {
      switch (expressionType) {
        case ExpressionType.Equal:
          return ComparisonType.Equal;
        case ExpressionType.NotEqual:
          return ComparisonType.NotEqual;
        case ExpressionType.LessThan:
          return ComparisonType.LessThan;
        case ExpressionType.LessThanOrEqual:
          return ComparisonType.LessThanOrEqual;
        case ExpressionType.GreaterThan:
          return ComparisonType.GreaterThan;
        case ExpressionType.GreaterThanOrEqual:
          return ComparisonType.GreaterThanOrEqual;
        default:
          throw Exceptions.InvalidArgument(expressionType, "expressionType");
      }
    }

    //Constructors

    internal ComparisonInfo(ExtractionInfo extractionInfo)
    {
      ArgumentValidator.EnsureArgumentNotNull(extractionInfo, "extractionInfo");
      ArgumentValidator.EnsureArgumentNotNull(extractionInfo.Key, "extractionInfo.Key");
      ArgumentValidator.EnsureArgumentNotNull(extractionInfo.Value, "extractionInfo.Value");
      ArgumentValidator.EnsureArgumentNotNull(extractionInfo.ComparisonType, "extractionInfo.ComparisonType");
      extractionInfo.Normalize();
      ComparisonType = ConvertToComparisonType(extractionInfo.ComparisonType.Value);
      Key = extractionInfo.Key;
      Value = extractionInfo.Value;
      if (extractionInfo.MethodInfo != null && extractionInfo.MethodInfo.IsComplex)
        ComplexMethod = extractionInfo.ComparisonMethod;
      else
        ComplexMethod = null;
    }
  }
}
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
  /// <summary>
  /// Information about a comparison operation.
  /// </summary>
  [Serializable]
  [DebuggerDisplay("{Key} {Operation} {Value}")]
  public sealed class ComparisonInfo
  {
    public readonly Expression Key;

    public readonly Expression Value;

    public readonly ComparisonType Operation;

    public readonly Expression ComplexMethod;

    /// <summary>
    /// Gets a value indicating whether a comparison operation is complex.
    /// </summary>
    /// <value>
    /// 	<see langword="true"/> if <see cref="ComplexMethod"/> is not 
    /// 	<see langword="null" />; otherwise, <see langword="false"/>.
    /// </value>
    public bool IsComplex { get { return ComplexMethod != null; } }

    internal static ComparisonInfo TryCreate(ExtractionInfo extractionInfo)
    {
      ArgumentValidator.EnsureArgumentNotNull(extractionInfo, "extractionInfo");
      ArgumentValidator.EnsureArgumentNotNull(extractionInfo.Key, "extractionInfo.Key");
      ArgumentValidator.EnsureArgumentNotNull(extractionInfo.Value, "extractionInfo.Value");

      if (!CanNormalize(extractionInfo))
        return null;
      var operation = NormalizeOperation(extractionInfo);
      Expression complexMethod = null;
      if (extractionInfo.MethodInfo != null && extractionInfo.MethodInfo.IsComplex) {
        ArgumentValidator.EnsureArgumentNotNull(extractionInfo.ComparisonMethod,
          "extractionInfo.ComparisonMethod");
        complexMethod = extractionInfo.ComparisonMethod;
      }
      return new ComparisonInfo(extractionInfo.Key, extractionInfo.Value, operation, complexMethod);
    }

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

    private static ComparisonType InvertOperation(ComparisonType comparisonType)
    {
      switch (comparisonType) {
      case ComparisonType.Equal:
        return ComparisonType.NotEqual;
      case ComparisonType.NotEqual:
        return ComparisonType.Equal;
      case ComparisonType.GreaterThan:
        return ComparisonType.LessThanOrEqual;
      case ComparisonType.GreaterThanOrEqual:
        return ComparisonType.LessThan;
      case ComparisonType.LessThan:
        return ComparisonType.GreaterThanOrEqual;
      case ComparisonType.LessThanOrEqual:
        return ComparisonType.GreaterThan;
      default:
        throw Exceptions.InvalidArgument(comparisonType, "comparisonType");
      }
    }

    private static ComparisonType ReverseOperation(ComparisonType comparisonType)
    {
      switch (comparisonType) {
      case ComparisonType.GreaterThan:
        return ComparisonType.LessThan;
      case ComparisonType.GreaterThanOrEqual:
        return ComparisonType.LessThanOrEqual;
      case ComparisonType.LessThan:
        return ComparisonType.GreaterThan;
      case ComparisonType.LessThanOrEqual:
        return ComparisonType.GreaterThanOrEqual;
      default:
        return comparisonType;
      }
    }

    private static bool CanNormalize(ExtractionInfo extractionInfo)
    {
      return !extractionInfo.ReversingRequired || extractionInfo.MethodInfo == null
        || extractionInfo.MethodInfo.CanBeReversed;
    }

    private static ComparisonType NormalizeOperation(ExtractionInfo extractionInfo)
    {
      ComparisonType result;
      if (extractionInfo.MethodInfo != null && extractionInfo.MethodInfo.CorrespondsToLikeOperation)
        result = ComparisonType.Like;
      else {
        ArgumentValidator.EnsureArgumentNotNull(extractionInfo.ComparisonOperation,
          "extractionInfo.ComparisonOperation");
        result = ConvertToComparisonType(extractionInfo.ComparisonOperation.Value);
      }
      if (extractionInfo.ReversingRequired)
        result = ReverseOperation(result);
      if (extractionInfo.InversingRequired)
        result = InvertOperation(result);
      return result;
    }

    // Constructors

    private ComparisonInfo(Expression key, Expression value, ComparisonType operation,
      Expression complexMethod)
    {
      ArgumentValidator.EnsureArgumentNotNull(key, "key");
      ArgumentValidator.EnsureArgumentNotNull(value, "value");
      Key = key;
      Value = value;
      Operation = operation;
      ComplexMethod = complexMethod;
    }
  }
}
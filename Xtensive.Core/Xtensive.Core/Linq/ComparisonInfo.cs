// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.03.27

using System;
using System.Linq.Expressions;
using Xtensive.Core;

namespace Xtensive.Linq
{
  /// <summary>
  /// Information about a comparison operation.
  /// </summary>
  [Serializable]
  public sealed class ComparisonInfo
  {
    /// <summary>
    /// The key part of the comparison expression.
    /// </summary>
    public readonly Expression Key;

    /// <summary>
    /// The value part of the comparison expression.
    /// </summary>
    public readonly Expression Value;

    /// <summary>
    /// The comparison operation.
    /// </summary>
    public readonly ComparisonOperation Operation;

    /// <summary>
    /// Contains the comparison method call expression, in case it is complex;
    /// otherwise, contains <see langword="null" />.
    /// </summary>
    public readonly Expression ComplexMethod;

    /// <summary>
    /// Gets a value indicating whether a comparison operation is complex.
    /// </summary>
    /// <value>
    ///   <see langword="true"/> if <see cref="ComplexMethod"/> is not 
    ///   <see langword="null" />; otherwise, <see langword="false"/>.
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

    private static ComparisonOperation ConvertToComparisonType(ExpressionType expressionType)
    {
      switch (expressionType) {
        case ExpressionType.Equal:
          return ComparisonOperation.Equal;
        case ExpressionType.NotEqual:
          return ComparisonOperation.NotEqual;
        case ExpressionType.LessThan:
          return ComparisonOperation.LessThan;
        case ExpressionType.LessThanOrEqual:
          return ComparisonOperation.LessThanOrEqual;
        case ExpressionType.GreaterThan:
          return ComparisonOperation.GreaterThan;
        case ExpressionType.GreaterThanOrEqual:
          return ComparisonOperation.GreaterThanOrEqual;
        default:
          throw Exceptions.InvalidArgument(expressionType, "expressionType");
      }
    }

    private static ComparisonOperation InvertOperation(ComparisonOperation comparisonOperation)
    {
      switch (comparisonOperation) {
        case ComparisonOperation.Equal:
          return ComparisonOperation.NotEqual;
        case ComparisonOperation.NotEqual:
          return ComparisonOperation.Equal;
        case ComparisonOperation.GreaterThan:
          return ComparisonOperation.LessThanOrEqual;
        case ComparisonOperation.GreaterThanOrEqual:
          return ComparisonOperation.LessThan;
        case ComparisonOperation.LessThan:
          return ComparisonOperation.GreaterThanOrEqual;
        case ComparisonOperation.LessThanOrEqual:
          return ComparisonOperation.GreaterThan;
        case ComparisonOperation.LikeStartsWith:
          return ComparisonOperation.NotLikeStartsWith;
        case ComparisonOperation.LikeEndsWith:
          return ComparisonOperation.NotLikeEndsWith;
        case ComparisonOperation.NotLikeStartsWith:
          return ComparisonOperation.LikeStartsWith;
        case ComparisonOperation.NotLikeEndsWith:
          return ComparisonOperation.LikeEndsWith;
        default:
          throw Exceptions.InvalidArgument(comparisonOperation, "comparisonOperation");
      }
    }

    private static ComparisonOperation ReverseOperation(ComparisonOperation comparisonOperation)
    {
      switch (comparisonOperation) {
        case ComparisonOperation.GreaterThan:
          return ComparisonOperation.LessThan;
        case ComparisonOperation.GreaterThanOrEqual:
          return ComparisonOperation.LessThanOrEqual;
        case ComparisonOperation.LessThan:
          return ComparisonOperation.GreaterThan;
        case ComparisonOperation.LessThanOrEqual:
          return ComparisonOperation.GreaterThanOrEqual;
        default:
          return comparisonOperation;
      }
    }

    private static bool CanNormalize(ExtractionInfo extractionInfo)
    {
      return !extractionInfo.ReversingRequired || extractionInfo.MethodInfo == null
        || extractionInfo.MethodInfo.CanBeReversed;
    }

    private static ComparisonOperation NormalizeOperation(ExtractionInfo extractionInfo)
    {
      ComparisonOperation? result = null;
      if (extractionInfo.MethodInfo != null)
        if(extractionInfo.MethodInfo.ComparisonKind == ComparisonKind.LikeStartsWith)
          result = ComparisonOperation.LikeStartsWith;
        else if (extractionInfo.MethodInfo.ComparisonKind == ComparisonKind.LikeEndsWith)
          result = ComparisonOperation.LikeEndsWith;

      if (result == null) {
        ArgumentValidator.EnsureArgumentNotNull(extractionInfo.ComparisonOperation,
          "extractionInfo.ComparisonOperation");
        result = ConvertToComparisonType(extractionInfo.ComparisonOperation.Value);
      }

      if (extractionInfo.ReversingRequired)
        result = ReverseOperation((ComparisonOperation)result);
      if (extractionInfo.InversingRequired)
        result = InvertOperation((ComparisonOperation)result);
      return (ComparisonOperation)result;
    }


    // Constructors

    private ComparisonInfo(Expression key, Expression value, ComparisonOperation operation,
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
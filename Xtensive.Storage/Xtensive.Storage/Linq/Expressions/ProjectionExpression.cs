// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.11

using System;

namespace Xtensive.Storage.Linq.Expressions
{
  internal class ProjectionExpression : ExtendedExpression
  {
    public ItemProjectorExpression ItemProjector { get; private set;}
    public ResultType ResultType { get; private set; }

    public bool IsScalar
    {
      get {  return ResultType != ResultType.All; }
    }

    public override string ToString()
    {
      return string.Format("Projection:  {0}, IsScalar = {1}", ItemProjector, IsScalar);
    }

    // Constructors

    public ProjectionExpression(
      Type type,
      ItemProjectorExpression itemProjectorExpression)
      : this(type, itemProjectorExpression, ResultType.All)
    {}

    public ProjectionExpression(
      Type type, 
      ItemProjectorExpression itemProjectorExpression, 
      ResultType resultType)
      : base(ExtendedExpressionType.Projection, type)
    {
      ItemProjector = itemProjectorExpression;
      ResultType = resultType;
    }
  }
}
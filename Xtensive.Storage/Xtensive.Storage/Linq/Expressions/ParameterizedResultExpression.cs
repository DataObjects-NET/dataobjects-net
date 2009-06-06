// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.04.27

using System;
using Xtensive.Core.Parameters;

namespace Xtensive.Storage.Linq.Expressions
{
  [Serializable]
  internal class ParameterizedResultExpression : ProjectionExpression
  {
    public Parameter QueryParameter { get; private set; }


    // Constructors

    public ParameterizedResultExpression(ProjectionExpression projectionExpression, Parameter parameter)
      : base(projectionExpression.Type, projectionExpression.ItemProjector, projectionExpression.ResultType)
    {
      QueryParameter = parameter;
    }
  }
}
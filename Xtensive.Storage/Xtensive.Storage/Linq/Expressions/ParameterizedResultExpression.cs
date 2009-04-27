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
  internal class ParameterizedResultExpression : ResultExpression
  {
    public Parameter QueryParameter { get; set; }

    // Constructors

    public ParameterizedResultExpression(ResultExpression resultExpression, Parameter parameter)
      : base(resultExpression.Type, resultExpression.RecordSet, resultExpression.Mapping, resultExpression.ItemProjector, resultExpression.ResultType)
    {
      QueryParameter = parameter;
    }
  }
}
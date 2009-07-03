// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.06.30

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using Xtensive.Core.Collections;
using Xtensive.Core.Disposing;
using Xtensive.Storage.Linq.Expressions;
using System.Linq;

namespace Xtensive.Storage.Linq
{
  [Serializable]
  internal class LinqBindingCollection : BindingCollection<ParameterExpression, ProjectionExpression>
  {
    private readonly Dictionary<ParameterExpression, IEnumerable<ParameterExpression>> linkedParameters = new Dictionary<ParameterExpression, IEnumerable<ParameterExpression>>();

    public override Disposable Add(ParameterExpression key, ProjectionExpression value)
    {
      if (key.Type != value.ItemProjector.Type)
        throw new ArgumentException("ParameterExpression must have same type as ProjectionExpression.ItemProjector");
      return base.Add(key, value);
    }

    public override Disposable PermanentAdd(ParameterExpression key, ProjectionExpression value)
    {
      if (key.Type != value.ItemProjector.Type)
        throw new ArgumentException("ParameterExpression must have same type as ProjectionExpression.ItemProjector");
      return base.PermanentAdd(key, value);
    }

    public override void ReplaceBound(ParameterExpression key, ProjectionExpression value)
    {
      base.ReplaceBound(key, value);
      IEnumerable<ParameterExpression> parameters;
      if (linkedParameters.TryGetValue(key, out parameters)) {
        foreach (var parameter in parameters) {
          if (parameter!=key) {
            var projection = this[parameter];
            var newItemProjector = projection.ItemProjector.Remap(value.ItemProjector.DataSource, 0);
            var newProjection = new ProjectionExpression(projection.Type, newItemProjector, projection.TupleParameterBindings, projection.ResultType);
            base.ReplaceBound(parameter, newProjection);
          }
        }
      }
    }

    public Disposable LinkParameters(IEnumerable<ParameterExpression> parameters)
    {
      foreach (var parameter in parameters)
        linkedParameters.Add(parameter, parameters);
      return new Disposable(isDisposing => parameters.Select(parameter => linkedParameters.Remove(parameter)));
    }
  }
}
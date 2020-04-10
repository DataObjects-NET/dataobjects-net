// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.06.30

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.Linq.Expressions;

namespace Xtensive.Orm.Linq
{
  [Serializable]
  internal class LinqBindingCollection : BindingCollection<ParameterExpression, ProjectionExpression>
  {
    private readonly Dictionary<ParameterExpression, IEnumerable<ParameterExpression>> linkedParameters
      = new Dictionary<ParameterExpression, IEnumerable<ParameterExpression>>();

    public override BindingScope Add(ParameterExpression key, ProjectionExpression value)
    {
      if (!key.Type.IsAssignableFrom(value.ItemProjector.Type)) {
        throw new ArgumentException(
          Strings.ExParameterExpressionMustHaveSameTypeAsProjectionExpressionItemProjector, nameof(key));
      }

      return base.Add(key, value);
    }

    public override void PermanentAdd(ParameterExpression key, ProjectionExpression value)
    {
      if (!key.Type.IsAssignableFrom(value.ItemProjector.Type))
        throw new ArgumentException(Strings.ExParameterExpressionMustHaveSameTypeAsProjectionExpressionItemProjector, "key");
      base.PermanentAdd(key, value);
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
            var newProjection = new ProjectionExpression(
              projection.Type, 
              newItemProjector, 
              projection.TupleParameterBindings, 
              projection.ResultType);
            base.ReplaceBound(parameter, newProjection);
          }
        }
      }
    }
    
    public Disposable LinkParameters(IEnumerable<ParameterExpression> parameters)
    {
      foreach (var parameter in parameters)
        linkedParameters.Add(parameter, parameters);
      return new Disposable(isDisposing => {
        foreach (var parameter in parameters) {
          linkedParameters.Remove(parameter);
        }
      });
    }
  }
}
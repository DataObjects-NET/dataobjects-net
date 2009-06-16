// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.05.05

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Linq.Expressions
{
  internal sealed class EntitySetExpression : FieldExpression,
    ISubQueryExpression
  {
    private TypeInfo elementType;

    public TypeInfo ElementType
    {
      get
      {
        if (elementType==null)
          elementType = Owner.PersistentType.Model.Types[Field.ItemType];
        return elementType;
      }
    }

    public override IPersistentExpression Owner
    {
      get
      {
        var value = base.Owner;
        if (value==null)
          throw new InvalidOperationException("Unable to handle EntitySetExpression without specified Owner.");
        return value;
      }
      internal set { base.Owner = value; }
    }

    public override Segment<int> Mapping
    {
      get { throw new NotSupportedException("EntitySetExpression does not have a segment."); }
    }

    public override FieldExpression RemoveOwner()
    {
      throw new NotSupportedException("Unable to remove Owner from EntitySetExpression.");
    }

    public override Expression Remap(int offset, Dictionary<Expression, Expression> processedExpressions)
    {
      if (!CanRemap)
        return this;

      Expression result;
      if (processedExpressions.TryGetValue(this, out result))
        return result;
      result = new EntitySetExpression(Field, null, DefaultIfEmpty, ProjectionExpression, ApplyParameter);
      if (base.Owner==null)
        return result;
      processedExpressions.Add(this, result);
      Owner.Remap(offset, processedExpressions);
      return result;
    }

    public override Expression Remap(int[] map, Dictionary<Expression, Expression> processedExpressions)
    {
      if (!CanRemap)
        return this;

      Expression result;
      if (processedExpressions.TryGetValue(this, out result))
        return result;
      result = new EntitySetExpression(Field, null, DefaultIfEmpty, ProjectionExpression, ApplyParameter);
      if (base.Owner==null)
        return result;
      processedExpressions.Add(this, result);
      Owner.Remap(map, processedExpressions);
      return result;
    }

    public override Expression BindParameter(ParameterExpression parameter, Dictionary<Expression, Expression> processedExpressions)
    {
      Expression result;
      if (processedExpressions.TryGetValue(this, out result))
        return result;
      result = new EntitySetExpression(Field, parameter, DefaultIfEmpty, ProjectionExpression, ApplyParameter);
      if (base.Owner==null)
        return result;
      processedExpressions.Add(this, result);
      Owner.BindParameter(parameter, processedExpressions);
      return result;
    }

    public override Expression RemoveOuterParameter(Dictionary<Expression, Expression> processedExpressions)
    {
      Expression result;
      if (processedExpressions.TryGetValue(this, out result))
        return result;
      result = new EntitySetExpression(Field, null, DefaultIfEmpty, ProjectionExpression, ApplyParameter);
      if (base.Owner==null)
        return result;
      processedExpressions.Add(this, result);
      Owner.RemoveOuterParameter(processedExpressions);
      return result;
    }

    public static EntitySetExpression CreateEntitySet(FieldInfo field, IPersistentExpression ownerExpression)
    {
      var entitySetExpression = QueryHelper.CreateEntitySetQuery((Expression) ownerExpression, field);
      // var projectionExpression = translator.Visit()
      // TODO: AG : Rewrite
      return new EntitySetExpression(field, null, false, null, null);
    }

    public override string ToString()
    {
      return string.Format("{0} {1}", base.ToString(), Name);
    }

    public ProjectionExpression ProjectionExpression { get; private set; }

    public ApplyParameter ApplyParameter { get; private set; }

    public Expression ReplaceApplyParameter(ApplyParameter newApplyParameter)
    {
      throw new NotImplementedException();
    }

    // Constructors

    private EntitySetExpression(
      FieldInfo field,
      ParameterExpression parameterExpression,
      bool defaultIfEmpty,
      ProjectionExpression projectionExpression,
      ApplyParameter applyParameter)
      : base(ExtendedExpressionType.EntitySet, field, default(Segment<int>), parameterExpression, defaultIfEmpty)
    {
      ProjectionExpression = projectionExpression;
      ApplyParameter = applyParameter;
    }
  }
}
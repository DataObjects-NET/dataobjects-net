// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.05.05

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Orm.Model;
using Xtensive.Orm.Rse;

namespace Xtensive.Orm.Linq.Expressions
{
  internal sealed class EntitySetExpression : FieldExpression
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
          throw Exceptions.InternalError(Strings.ExUnableToHandleEntitySetExpressionWithoutSpecifiedOwner, OrmLog.Instance);
        return value;
      }
      internal set { base.Owner = value; }
    }

    public override FieldExpression RemoveOwner()
    {
      throw Exceptions.InternalError(Strings.ExUnableToRemoveOwnerFromEntitySetExpression, OrmLog.Instance);
    }

    public override Expression Remap(int offset, Dictionary<Expression, Expression> processedExpressions)
    {
      if (!CanRemap)
        return this;

      Expression result;
      if (processedExpressions.TryGetValue(this, out result))
        return result;
      result = new EntitySetExpression(Field, null, DefaultIfEmpty);
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
      result = new EntitySetExpression(Field, null, DefaultIfEmpty);
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
      result = new EntitySetExpression(Field, parameter, DefaultIfEmpty);
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
      result = new EntitySetExpression(Field, null, DefaultIfEmpty);
      if (base.Owner==null)
        return result;
      processedExpressions.Add(this, result);
      Owner.RemoveOuterParameter(processedExpressions);
      return result;
    }

    public static EntitySetExpression CreateEntitySet(FieldInfo field)
    {
      return new EntitySetExpression(field, null, false);
    }

    public override string ToString()
    {
      return $"{base.ToString()} {Name}";
    }


    // Constructors

    private EntitySetExpression(
      FieldInfo field,
      ParameterExpression parameterExpression,
      bool defaultIfEmpty)
      : base(ExtendedExpressionType.EntitySet, field, default(Segment<int>), parameterExpression, defaultIfEmpty)
    {
    }
  }
}
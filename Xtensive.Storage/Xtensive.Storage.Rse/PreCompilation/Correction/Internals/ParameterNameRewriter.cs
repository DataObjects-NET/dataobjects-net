// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.05.15

using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Core.Linq;

namespace Xtensive.Storage.Rse.PreCompilation.Correction
{
  internal sealed class ParameterNameRewriter : ExpressionVisitor
  {
    private string oldName;
    private string newName;

    public LambdaExpression Rename(LambdaExpression sourceExpression, string oldName, string newName)
    {
      ArgumentValidator.EnsureArgumentNotNull(sourceExpression, "sourceExpression");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(oldName, "oldName");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(newName, "newName");
      if(sourceExpression.Parameters.Count == 0)
        throw Exceptions.CollectionIsEmpty("sourceExpression.Parameters");
      this.oldName = oldName;
      this.newName = newName;
      return (LambdaExpression)Visit(sourceExpression);
    }

    protected override Expression VisitParameter(ParameterExpression p)
    {
      return p.Name == oldName ? Expression.Parameter(p.Type, newName) : base.VisitParameter(p);
    }
  }
}
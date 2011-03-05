// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.04.15

using System;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Linq;

namespace Xtensive.Orm.Tests.Issues.Issue0631_DisconnectedStateBugs
{
  [CompilerContainer(typeof(Expression))]
  internal static class LinqExtensions
  {
    [Compiler(typeof(OwnedEntity), "OwnerText", TargetKind.PropertyGet)]
    public static Expression Current(Expression assignmentExpression)
    {
      return OwnedEntity.ownerTextExpression.BindParameters(assignmentExpression);
    }
  }
}
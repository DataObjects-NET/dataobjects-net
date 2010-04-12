// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.04.12

using System;
using System.Linq.Expressions;
using Xtensive.Core.Linq;

namespace Xtensive.Storage.Tests.Issues.Issue0631_DisconnectedStateBugs
{
  [HierarchyRoot]
  public class AnotherEntity : EntityBase
  {
    private static Expression<Func<AnotherEntity, string>> _ownerTextExpression = a => a.Owner.Text;

    public AnotherEntity(Guid id) : base(id)
    {
    }

    public static Expression<Func<AnotherEntity, string>> OwnerTextExpression
    {
      get { return _ownerTextExpression; }
    }

    [Field]
    public SampleEntity Owner { get; set; }

    public string OwnerText
    {
      get { return _ownerTextExpression.Compile()(this); }
    }

    [Field]
    public string Name { get; set; }

    #region Virtual fields helper class

    [CompilerContainer(typeof(Expression))]
    public static class CustomLinqCompilerContainer
    {
      [Compiler(typeof(AnotherEntity), "OwnerText", TargetKind.PropertyGet)]
      public static Expression Current(Expression assignmentExpression)
      {
        return _ownerTextExpression.BindParameters(assignmentExpression);
      }
    }

    #endregion
  }
}
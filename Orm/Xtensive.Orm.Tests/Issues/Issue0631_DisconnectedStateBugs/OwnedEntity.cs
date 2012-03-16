// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.04.12

using System;
using System.Linq.Expressions;
using Xtensive.Core;

namespace Xtensive.Orm.Tests.Issues.Issue0631_DisconnectedStateBugs
{
  [HierarchyRoot]
  public class OwnedEntity : EntityBase
  {
    internal readonly static Expression<Func<OwnedEntity, string>> ownerTextExpression = a => a.Owner.Text;

    [Field]
    public string Name { get; set; }

    [Field]
    public OwnerEntity Owner { get; set; }

    public string OwnerText
    {
      get { return ownerTextExpression.CachingCompile() (this); }
    }


    // Constructors
    
    public OwnedEntity(Guid id) : base(id)
    {
    }
  }
}
// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.13

using System;

namespace Xtensive.Sql.Dml
{
  [Serializable]
  public abstract class SqlArray : SqlExpression
  {
    public abstract Type ItemType { get; }
    public abstract int Length { get; }
    public abstract object[] GetValues();
    
    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    // Constructors

    internal SqlArray()
      : base(SqlNodeType.Array)
    {
    }
  }
}
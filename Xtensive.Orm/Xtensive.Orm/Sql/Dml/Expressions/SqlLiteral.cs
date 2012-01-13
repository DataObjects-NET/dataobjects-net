// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.13

using System;

namespace Xtensive.Sql.Dml
{
  [Serializable]
  public abstract class SqlLiteral : SqlExpression
  {
    public abstract Type LiteralType { get; }
    public abstract object GetValue();

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }
    
    // Constructors

    internal SqlLiteral()
      : base(SqlNodeType.Literal)
    {
    }
  }
}
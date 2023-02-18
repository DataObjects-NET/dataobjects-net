// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Core;

namespace Xtensive.Sql.Dml
{
  /// <summary>
  /// Represents a Sql parameter.
  /// </summary>
  [Serializable]
  public class SqlParameterRef : SqlExpression, ISqlCursorFetchTarget
  {
    public object Parameter { get; private set; }
    public string Name { get; private set; }
    
    public override void ReplaceWith(SqlExpression expression)
    {
      var replacingExpression = ArgumentValidator.EnsureArgumentIs<SqlParameterRef>(expression);
      Name = replacingExpression.Name;
      Parameter = replacingExpression.Parameter;
    }

    internal override object Clone(SqlNodeCloneContext context) =>
      context.NodeMapping.TryGetValue(this, out var clone)
        ? clone
        : context.NodeMapping[this] = Name != null
            ? new SqlParameterRef(Name)
            : new SqlParameterRef(Parameter);

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    // Constructor

    internal SqlParameterRef(object parameter) : base(SqlNodeType.Parameter)
    {
      Parameter = parameter;
    }

    internal SqlParameterRef(string name) : base(SqlNodeType.Parameter)
    {
      Name = name;
    }
  }
}

// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Sql.Dom.DatabaseModel;

namespace Xtensive.Sql.Dom.Ddl
{
  public class SqlAlter : SqlStatement, ISqlCompileUnit
  {
    private SqlAction action;
    private IModifiable m_object;

    public SqlAction Action {
      get {
        return action;
      }
    }

    public IModifiable Object {
      get {
        return m_object;
      }
    }


    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];

      SqlAlter clone = new SqlAlter(m_object, (SqlAction)action.Clone(context));
      context.NodeMapping[this] = clone;

      return clone;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      throw new NotImplementedException();
    }

    internal SqlAlter(IModifiable m_object, SqlAction action)
      : base(SqlNodeType.Alter)
    {
      this.action = action;
      this.m_object = m_object;
    }
  }
}

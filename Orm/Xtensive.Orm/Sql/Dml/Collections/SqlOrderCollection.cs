// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections.ObjectModel;

namespace Xtensive.Sql.Dml
{
  /// <summary>
  /// Represents collection of <see cref="SqlOrder"/>s.
  /// </summary>
  [Serializable]
  public class SqlOrderCollection : Collection<SqlOrder>
  {
    public void Add(SqlExpression expression)
    {
      Add(SqlDml.Order(expression));
    }

    public void Add(SqlExpression expression, bool ascending)
    {
      Add(SqlDml.Order(expression, ascending));
    }
    
    public void Add(int position)
    {
      Add(SqlDml.Order(position));
    }

    public void Add(int position, bool ascending)
    {
      Add(SqlDml.Order(position, ascending));
    }
  }
}

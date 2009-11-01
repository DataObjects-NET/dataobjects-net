// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections.ObjectModel;

namespace Xtensive.Sql.Dom.Dml
{
  /// <summary>
  /// Represents collection of <see cref="SqlOrder"/>s.
  /// </summary>
  [Serializable]
  public class SqlOrderCollection : Collection<SqlOrder>
  {
    public void Add(SqlExpression expression)
    {
      Add(Sql.Order(expression));
    }

    public void Add(SqlExpression expression, bool ascending)
    {
      Add(Sql.Order(expression, ascending));
    }
    
    public void Add(int position)
    {
      Add(Sql.Order(position));
    }

    public void Add(int position, bool ascending)
    {
      Add(Sql.Order(position, ascending));
    }
  }
}

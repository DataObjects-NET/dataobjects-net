// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.05.18

using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql.Compiler
{
  internal sealed class JoinSequence
  {
    public SqlTable Pivot { get; private set; }

    public IList<SqlTable> Tables { get; private set; }

    public IList<SqlJoinType> JoinTypes { get; private set; }

    public IList<SqlExpression> Conditions { get; private set; }

    public static JoinSequence Build(SqlJoinedTable root)
    {
      var joins = new List<SqlJoinExpression>();
      Traverse(root, joins);

      var result = new JoinSequence();

      foreach (var item in joins) {
        if (!(item.Left is SqlJoinedTable))
          result.Tables.Add(item.Left);
        if (!(item.Right is SqlJoinedTable))
          result.Tables.Add(item.Right);
        result.JoinTypes.Add(item.JoinType);
        result.Conditions.Add(item.Expression);
      }

      var pivot = result.Tables[0];
      result.Pivot = pivot;
      result.Tables.RemoveAt(0);

      result.Tables = new ReadOnlyCollection<SqlTable>(result.Tables);
      result.JoinTypes = new ReadOnlyCollection<SqlJoinType>(result.JoinTypes);
      result.Conditions = new ReadOnlyCollection<SqlExpression>(result.Conditions);

      return result;
    }

    private static void Traverse(SqlJoinedTable root, ICollection<SqlJoinExpression> output)
    {
      var left = root.JoinExpression.Left;
      var joinedLeft = left as SqlJoinedTable;
      if (joinedLeft!=null)
        Traverse(joinedLeft, output);

      output.Add(root.JoinExpression);

      var right = root.JoinExpression.Right;
      var joinedRight = right as SqlJoinedTable;
      if (joinedRight!=null)
        Traverse(joinedRight, output);
    }


    // Constructors

    private JoinSequence()
    {
      Tables = new List<SqlTable>();
      JoinTypes = new List<SqlJoinType>();
      Conditions = new List<SqlExpression>();
    }
  }
}
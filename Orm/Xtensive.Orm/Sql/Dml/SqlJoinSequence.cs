// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.05.18

using System.Collections.Generic;

namespace Xtensive.Sql.Dml
{
  public sealed class SqlJoinSequence
  {
    public SqlTable Pivot { get; set; }

    public IList<SqlTable> Tables { get; set; }

    public IList<SqlJoinType> JoinTypes { get; set; }

    public IList<SqlExpression> Conditions { get; set; }

    public static SqlJoinSequence Build(SqlJoinedTable root)
    {
      var joins = new List<SqlJoinExpression>();
      Traverse(root, joins);

      var result = new SqlJoinSequence();

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

    public SqlJoinSequence()
    {
      Tables = new List<SqlTable>();
      JoinTypes = new List<SqlJoinType>();
      Conditions = new List<SqlExpression>();
    }
  }
}
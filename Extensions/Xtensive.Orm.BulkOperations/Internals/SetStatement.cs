using System;
using System.Collections.Generic;
using Xtensive.Sql;
using Xtensive.Sql.Dml;

namespace Xtensive.Orm.BulkOperations
{
  internal abstract class SetStatement
  {
    #region Nested type: Insert

    private class Insert : SetStatement
    {
      private SqlInsert insert
      {
        get { return (SqlInsert) statement; }
      }

      public override SqlTable Table
      {
        get { return insert.Into; }
      }

      public override void AddValues(Dictionary<SqlColumn, SqlExpression> values)
      {
        insert.ValueRows.Add(values);
      }
    }

    #endregion

    #region Nested type: Update

    private class Update : SetStatement
    {
      private SqlUpdate update
      {
        get { return (SqlUpdate) statement; }
      }

      public override SqlTable Table
      {
        get { return update.Update; }
      }

      public override void AddValues(Dictionary<SqlColumn, SqlExpression> values)
      {
        if (update.Values.Count!=0) {
          throw new InvalidOperationException("Update values have already been initialized");
        }
        foreach (var keyValue in values) {
          update.Values.Add((SqlTableColumn)keyValue.Key, keyValue.Value);
        }
      }
    }

    #endregion

    private SqlQueryStatement statement;

    public static SetStatement Create(SqlUpdate updateStatement)
    {
      return new Update() { statement = updateStatement };
    }

    public static SetStatement Create(SqlInsert insertStatement)
    {
      return new Insert() { statement = insertStatement };
    }

    public abstract SqlTable Table { get; }

    public abstract void AddValues(Dictionary<SqlColumn, SqlExpression> values);
  }
}
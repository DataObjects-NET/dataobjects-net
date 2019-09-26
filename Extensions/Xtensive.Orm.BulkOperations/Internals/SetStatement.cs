using System;
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

      public override void AddValue(SqlTableColumn column, SqlExpression value)
      {
        insert.Values.Add(column, value);
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

      public override void AddValue(SqlTableColumn column, SqlExpression value)
      {
        update.Values.Add(column, value);
      }
    }

    #endregion

    private SqlQueryStatement statement;

    public static SetStatement Create(SqlQueryStatement statement)
    {
      SetStatement result;
      if (statement is SqlUpdate)
        result = new Update();
      else if (statement is SqlInsert)
        result = new Insert();
      else
        throw new InvalidOperationException("Statement must be SqlUpdate or SqlInsert");
      result.statement = statement;
      return result;
    }

    public abstract SqlTable Table { get; }
    public abstract void AddValue(SqlTableColumn column, SqlExpression value);
  }
}
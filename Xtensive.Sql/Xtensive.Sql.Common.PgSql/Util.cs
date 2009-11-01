using System;
using System.Text;
using Npgsql;

namespace Xtensive.Sql.Common.PgSql
{
  public delegate R Func<R>();

  public delegate R Func<A1, R>(A1 a1);

  public delegate R Func<A1, A2, R>(A1 a1, A2 a2);

  public delegate R Func<A1, A2, A3, R>(A1 a1, A2 a2, A3 a3);

  public delegate R Func<A1, A2, A3, A4, R>(A1 a1, A2 a2, A3 a3, A4 a4);

  public delegate void Proc();

  public delegate void Proc<A1>(A1 a1);

  public delegate void Proc<A1, A2>(A1 a1, A2 a2);

  public delegate void Proc<A1, A2, A3>(A1 a1, A2 a2, A3 a3);

  public delegate void Proc<A1, A2, A3, A4>(A1 a1, A2 a2, A3 a3, A4 a4);

  public static class Util
  {
    /// <summary>
    /// Return true, if the SQL type specified can have a size parameter.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static bool IsSizableType(SqlDataType type)
    {
      return type==SqlDataType.AnsiChar ||
        type==SqlDataType.AnsiVarChar ||
          type==SqlDataType.Char ||
            type==SqlDataType.VarChar;
    }

    /// <summary>
    /// Extracts all information from the provided NpgsqlException object.
    /// </summary>
    /// <param name="ex">The exception object from which the information is extracted.</param>
    /// <returns>The extracted information.</returns>
    public static string ExtractExeptionInfo(NpgsqlException ex)
    {
      bool moreThanOne = ex.Errors.Count > 1;
      int index = 1;
      StringBuilder sb = new StringBuilder(200);
      foreach (NpgsqlError err in ex.Errors) {
        if (moreThanOne)
          sb.AppendFormat("E R R O R   {0}:\n", index);
        sb.AppendFormat(" SEVERITY: {0}\n", err.Severity);
        sb.AppendFormat(" MESSAGE:  {0}\n", err.Message);
        sb.AppendFormat(" DETAIL:   {0}\n", err.Detail);
        sb.AppendFormat(" HINT:     {0}\n", err.Hint);
        sb.AppendFormat(" SQLSTATE: {0}\n", err.Code);
        sb.AppendFormat(" POSITION: {0}\n", err.Position);
        sb.AppendFormat(" WHERE:    {0}\n", err.Where);
        sb.AppendFormat(" FILE:     {0}\n", err.File);
        sb.AppendFormat(" ROUTINE:  {0}\n", err.Routine);
        sb.AppendFormat(" LINE:     {0}\n", err.Line);
        sb.AppendFormat(" SQL:      {0}\n", err.ErrorSql);
        index++;
      }
      return sb.ToString();
    }

    public static void DisposeIfNotNull(IDisposable o)
    {
      if (o!=null)
        o.Dispose();
    }
  }
}
using System;
using Npgsql;
using Xtensive.Sql.Info;

namespace Xtensive.Sql.PostgreSql
{
  internal abstract class Driver : SqlDriver
  {
    protected override ValueTypeMapping.TypeMapper CreateTypeMapper()
    {
      return new TypeMapper(this);
    }

    public override SqlConnection CreateConnection()
    {
      return new Connection(this);
    }

    public override SqlExceptionType GetExceptionType(Exception exception)
    {
      var nativeException = exception as NpgsqlException;
      if (nativeException==null)
        return SqlExceptionType.Unknown;
      
      var errorCode = nativeException.Code.ToUpperInvariant();
      var errorCodeClass = errorCode.Substring(0, 2);

      // Error codes have been taken from
      // http://www.postgresql.org/docs/8.4/static/errcodes-appendix.html

      switch (errorCodeClass) {
      case "08": // connection_exception
        return SqlExceptionType.ConnectionError;
      case "42": // syntax_error_or_access_rule_violation
        return SqlExceptionType.SyntaxError;
      }

      switch (errorCode) {
      case "23502": // not_null_violation
      case "23514": // check_violation
        return SqlExceptionType.CheckConstraintViolation;
      case "23001": // restrict_violation
      case "23503": // foreign_key_violation
        return SqlExceptionType.ReferentialContraintViolation;
      case "23505": // unique_violation
        return SqlExceptionType.UniqueConstraintViolation;
      case "40P01": // deadlock_detected
        return SqlExceptionType.Deadlock;
      case "40001": // serialization_failure
        return SqlExceptionType.SerializationFailure;
      }

      return SqlExceptionType.Unknown;
    }

    // Constructors

    protected Driver(CoreServerInfo coreServerInfo)
      : base(coreServerInfo)
    {
    }
  }
}
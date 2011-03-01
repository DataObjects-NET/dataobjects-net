using System;
using MySql.Data.MySqlClient;
using Xtensive.Sql.Drivers.MySql;
using Xtensive.Sql.Info;

namespace Xtensive.Sql.MySql
{
    internal abstract class Driver : SqlDriver
    {
        protected override SqlConnection CreateConnection(string connectionString)
        {
            return new Connection(this, connectionString);
        }

        public override SqlExceptionType GetExceptionType(Exception exception)
        {
            var nativeException = exception as MySqlException;
            if (nativeException == null)
                return SqlExceptionType.Unknown;
            int errorCode = nativeException.Number;
            string errorMessage = nativeException.Message;

            //TODO: intepret some of the error codes (Malisa)
            //http://dev.mysql.com/doc/refman/5.0/en/error-handling.html

            return SqlExceptionType.Unknown;
        }

        // Constructors

        protected Driver(CoreServerInfo coreServerInfo)
            : base(coreServerInfo)
        {
        }
    }
}

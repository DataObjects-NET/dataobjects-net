// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Csaba Beer
// Created:    2011.01.08

using Xtensive.Sql.Info;
using FirebirdSql.Data.FirebirdClient;

namespace Xtensive.Sql.Firebird
{
    internal abstract class Driver : SqlDriver
    {
        /// <inheritdoc/>
        protected override SqlConnection CreateConnection(string connectionString)
        {
            return new Connection(this, connectionString);
        }

        /// <inheritdoc/>
        public override SqlExceptionType GetExceptionType(System.Exception exception)
        {
            var nativeException = exception as FbException;
            if (nativeException == null)
                return base.GetExceptionType(exception);
            switch (nativeException.Errors[nativeException.Errors.Count - 1].Number)
            {
                case 335544569:
                    return SqlExceptionType.SyntaxError;
                case 335544347: //  exactly: validation error for column
                case 335544558: //  exactly: operation violates check constraint
                    return SqlExceptionType.CheckConstraintViolation;
                case 335544665:
                    return SqlExceptionType.UniqueConstraintViolation;
                case 335544466:
                    return SqlExceptionType.ReferentialConstraintViolation;
                case 335544336: //  exactly: deadlock 
                case 335544345: //  exactly: lock conflict on no wait transaction
                    return SqlExceptionType.Deadlock;
                case 335544510: //  exactly: lock_timeout on wait transaction
                    return SqlExceptionType.OperationTimeout;
                default:
                    return SqlExceptionType.Unknown;
            }
        }

        // Constructors

        protected Driver(CoreServerInfo coreServerInfo)
            : base(coreServerInfo)
        {
        }
    }
}
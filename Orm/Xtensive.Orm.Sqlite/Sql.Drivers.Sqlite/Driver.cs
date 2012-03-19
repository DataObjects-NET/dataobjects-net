// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Malisa Ncube
// Created:    2011.04.29

using System;
using System.Data.SQLite;
using System.Security;
using Xtensive.Sql.Info;

namespace Xtensive.Sql.Drivers.SQLite
{
	internal abstract class Driver : SqlDriver
	{
		/// <inheritdoc/>
#if NET40
		[SecuritySafeCritical]
#endif
		protected override SqlConnection CreateConnection(string connectionString)
		{
			return new Connection(this, connectionString);
		}

		/// <inheritdoc/>
#if NET40
		[SecuritySafeCritical]
#endif
		public override SqlExceptionType GetExceptionType(Exception exception)
		{
			var nativeException = exception as SQLiteException;
			return SqlExceptionType.Unknown;
		}

		// Constructors

		/// <inheritdoc/>
		protected Driver(CoreServerInfo coreServerInfo)
			: base(coreServerInfo)
		{
		}
	}
}
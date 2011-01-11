// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Csaba Beer
// Created:    2011.01.08

using System.Data;
using System.Data.Common;
using FirebirdSql.Data.FirebirdClient;

namespace Xtensive.Sql.Firebird
{
    internal class Connection : SqlConnection
    {

        private FbConnection underlyingConnection;
        private FbTransaction activeTransaction;

        /// <inheritdoc/>
        public override DbConnection UnderlyingConnection
        {
            get { return underlyingConnection; }
        }

        /// <inheritdoc/>
        public override DbTransaction ActiveTransaction
        {
            get { return activeTransaction; }
        }

        /// <inheritdoc/>
        public override DbParameter CreateParameter()
        {
            return new FbParameter();
        }

        /// <inheritdoc/>
        public override void BeginTransaction()
        {
            EnsureTrasactionIsNotActive();
            activeTransaction = underlyingConnection.BeginTransaction();
        }

        /// <inheritdoc/>
        public override void BeginTransaction(IsolationLevel isolationLevel)
        {
            EnsureTrasactionIsNotActive();
            activeTransaction = underlyingConnection.BeginTransaction(SqlHelper.ReduceIsolationLevel(isolationLevel));
        }

        /// <inheritdoc/>
        protected override void ClearActiveTransaction()
        {
            activeTransaction = null;
        }

        /// <inheritdoc/>
        public override void MakeSavepoint(string name)
        {
            EnsureTransactionIsActive();
            activeTransaction.Save(name);
        }

        /// <inheritdoc/>
        public override void RollbackToSavepoint(string name)
        {
            EnsureTransactionIsActive();
            activeTransaction.Rollback(name);
        }

        // Constructors
        public Connection(SqlDriver driver, string connectionString)
            : base(driver, connectionString)
        {
            underlyingConnection = new FbConnection(connectionString);
        }
    }
}
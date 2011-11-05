// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Csaba Beer
// Created:    2011.01.08

using System.Data;
using System.Data.Common;
using FirebirdSql.Data.FirebirdClient;

namespace Xtensive.Sql.Drivers.Firebird
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
            BeginTransaction(IsolationLevel.Serializable);
        }

        /// <inheritdoc/>
        public override void BeginTransaction(IsolationLevel isolationLevel)
        {
            EnsureTrasactionIsNotActive();
//            activeTransaction = underlyingConnection.BeginTransaction(SqlHelper.ReduceIsolationLevel(isolationLevel));
            FbTransactionOptions transactionOptions = new FbTransactionOptions();
            transactionOptions.WaitTimeout = 10;
            IsolationLevel innerIsolationLevel = SqlHelper.ReduceIsolationLevel(isolationLevel);
            switch (innerIsolationLevel)
            {
                case IsolationLevel.ReadCommitted: transactionOptions.TransactionBehavior = FbTransactionBehavior.ReadCommitted | FbTransactionBehavior.NoRecVersion | FbTransactionBehavior.Write | FbTransactionBehavior.NoWait;
                    break;
                case IsolationLevel.Serializable: transactionOptions.TransactionBehavior = FbTransactionBehavior.Concurrency | FbTransactionBehavior.Write | FbTransactionBehavior.Wait;
                    break;
                default:
                    throw new System.NotImplementedException("Isolation level " + innerIsolationLevel + " is not supported!");
            }
            activeTransaction = underlyingConnection.BeginTransaction(transactionOptions);
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
};
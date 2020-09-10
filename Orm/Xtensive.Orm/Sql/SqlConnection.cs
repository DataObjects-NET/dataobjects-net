// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm;
using Xtensive.Sql.Info;

namespace Xtensive.Sql
{
  /// <summary>
  /// A connection to a database.
  /// </summary>
  public abstract class SqlConnection : SqlDriverBound,
    IDisposable, IAsyncDisposable
  {
    private int? commandTimeout;
    private ConnectionInfo connectionInfo;
    private IExtensionCollection extensions;
    private bool isDisposed;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void EnsureIsNotDisposed()
    {
      if (isDisposed) {
        throw new InvalidOperationException("Connection is disposed.");
      }
    }

    /// <summary>
    /// Gets the underlying connection.
    /// </summary>
    public abstract DbConnection UnderlyingConnection { get; }

    /// <summary>
    /// Gets the active transaction.
    /// </summary>
    public abstract DbTransaction ActiveTransaction { get; }

    /// <summary>
    /// Gets <see cref="IExtensionCollection"/> associated with this instance.
    /// </summary>
    public IExtensionCollection Extensions => extensions ??= new ExtensionCollection();

    /// <summary>
    /// Gets or sets <see cref="ConnectionInfo"/> to use.
    /// </summary>
    public ConnectionInfo ConnectionInfo
    {
      get => connectionInfo;
      set {
        ArgumentValidator.EnsureArgumentNotNull(value, nameof(value));
        EnsureIsNotDisposed();

        UnderlyingConnection.ConnectionString = Driver.GetConnectionString(value);
        connectionInfo = value;
      }
    }

    /// <summary>
    /// Gets or sets the command timeout.
    /// </summary>
    public int? CommandTimeout
    {
      get => commandTimeout;
      set {
        if (value != null) {
          ArgumentValidator.EnsureArgumentIsInRange(value.Value, 0, 65535, nameof(value));
        }

        EnsureIsNotDisposed();

        commandTimeout = value;
      }
    }

    /// <summary>
    /// Gets the state of the connection.
    /// </summary>
    public ConnectionState State => isDisposed ? ConnectionState.Closed : UnderlyingConnection.State;

    /// <summary>
    /// Creates and returns a <see cref="DbCommand"/> object associated with the current connection.
    /// </summary>
    /// <returns>Created command.</returns>
    public DbCommand CreateCommand()
    {
      EnsureIsNotDisposed();
      var command = CreateNativeCommand();
      if (commandTimeout != null) {
        command.CommandTimeout = commandTimeout.Value;
      }

      command.Transaction = ActiveTransaction;
      return command;
    }
    
    /// <summary>
    /// Creates and returns a <see cref="DbCommand"/> object with specified <paramref name="statement"/>.
    /// Created command will be associated with the current connection.
    /// </summary>
    /// <returns>Created command.</returns>
    public DbCommand CreateCommand(ISqlCompileUnit statement)
    {
      ArgumentValidator.EnsureArgumentNotNull(statement, nameof(statement));
      EnsureIsNotDisposed();

      var command = CreateCommand();
      command.CommandText = Driver.Compile(statement).GetCommandText();
      return command;
    }
    
    /// <summary>
    /// Creates and returns a <see cref="DbCommand"/> object with specified <paramref name="commandText"/>.
    /// Created command will be associated with the current connection.
    /// </summary>
    /// <returns>Created command.</returns>
    public DbCommand CreateCommand(string commandText)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(commandText, nameof(commandText));
      EnsureIsNotDisposed();

      var command = CreateCommand();
      command.CommandText = commandText;
      return command;
    }

    /// <summary>
    /// Creates the parameter.
    /// </summary>
    /// <returns>Created parameter.</returns>
    public abstract DbParameter CreateParameter();
    
    /// <summary>
    /// Creates the cursor parameter.
    /// </summary>
    /// <returns>Created parameter.</returns>
    public virtual DbParameter CreateCursorParameter() => throw SqlHelper.NotSupported(ServerFeatures.CursorParameters);

    /// <summary>
    /// Creates the character large object bound to this connection.
    /// Created object initially have NULL value (<see cref="ILargeObject.IsNull"/> returns <see langword="true"/>)
    /// </summary>
    /// <returns>Created CLOB.</returns>
    public virtual ICharacterLargeObject CreateCharacterLargeObject() =>
      throw SqlHelper.NotSupported(ServerFeatures.LargeObjects);

    /// <summary>
    /// Creates the binary large object bound to this connection.
    /// Created object initially have NULL value (<see cref="ILargeObject.IsNull"/> returns <see langword="true"/>)
    /// </summary>
    /// <returns>Created BLOB.</returns>
    public virtual IBinaryLargeObject CreateBinaryLargeObject() =>
      throw SqlHelper.NotSupported(ServerFeatures.LargeObjects);

    /// <summary>
    /// Opens the connection.
    /// </summary>
    public virtual void Open()
    {
      EnsureIsNotDisposed();
      UnderlyingConnection.Open();
    }

    /// <summary>
    /// Opens the connection and initialize it with given script.
    /// </summary>
    /// <param name="initializationScript">Initialization script.</param>
    public virtual void OpenAndInitialize(string initializationScript)
    {
      UnderlyingConnection.Open();
      if (string.IsNullOrEmpty(initializationScript)) {
        return;
      }

      using var command = UnderlyingConnection.CreateCommand();
      command.CommandText = initializationScript;
      command.ExecuteNonQuery();
    }

    /// <summary>
    /// Opens the connection asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Token to control cancellation.</param>
    /// <returns>Awaitable task.</returns>
    public virtual Task OpenAsync(CancellationToken cancellationToken)
    {
      cancellationToken.ThrowIfCancellationRequested();
      return UnderlyingConnection.OpenAsync(cancellationToken);
    }

    /// <summary>
    /// Opens the connection and initialize it with given script asynchronously.
    /// </summary>
    /// <param name="initializationScript">Initialization script.</param>
    /// <param name="token">Token to control cancellation.</param>
    /// <returns>Awaitable task.</returns>
    public virtual async Task OpenAndInitializeAsync(string initializationScript, CancellationToken token = default)
    {
      token.ThrowIfCancellationRequested();
      await UnderlyingConnection.OpenAsync(token).ConfigureAwait(false);
      if (string.IsNullOrEmpty(initializationScript)) {
        return;
      }

      try {
        var command = UnderlyingConnection.CreateCommand();
        await using (command.ConfigureAwait(false)) {
          command.CommandText = initializationScript;
          await command.ExecuteNonQueryAsync(token).ConfigureAwait(false);
        }
      }
      catch (OperationCanceledException) {
        await UnderlyingConnection.CloseAsync().ConfigureAwait(false);
        throw;
      }
    }

    /// <summary>
    /// Closes the connection.
    /// </summary>
    public virtual void Close()
    {
      EnsureIsNotDisposed();
      UnderlyingConnection.Close();
    }

    /// <summary>
    /// Closes the connection.
    /// </summary>
    public virtual Task CloseAsync()
    {
      EnsureIsNotDisposed();
      await UnderlyingConnection.CloseAsync();
    }

    /// <summary>
    /// Begins the transaction.
    /// </summary>
    public abstract void BeginTransaction();

    /// <summary>
    /// Begins the transaction.
    /// </summary>
    /// <param name="token">The cancellation token to be used to cancel this operation.</param>
    public virtual ValueTask BeginTransactionAsync(CancellationToken token = default)
    {
      BeginTransaction();
      return default;
    }

    /// <summary>
    /// Begins the transaction with the specified <see cref="IsolationLevel"/>.
    /// </summary>
    /// <param name="isolationLevel">The isolation level.</param>
    public abstract void BeginTransaction(IsolationLevel isolationLevel);

    /// <summary>
    /// Begins the transaction with the specified <see cref="IsolationLevel"/>.
    /// </summary>
    /// <param name="isolationLevel">The isolation level.</param>
    /// <param name="token">The cancellation token to be used to cancel this operation.</param>
    public virtual ValueTask BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken token = default)
    {
      BeginTransaction(isolationLevel);
      return default;
    }

    /// <summary>
    /// Commits the current transaction.
    /// </summary>
    public virtual void Commit()
    {
      EnsureIsNotDisposed();
      EnsureTransactionIsActive();

      try {
        ActiveTransaction.Commit();
      }
      finally {
        ActiveTransaction.Dispose();
        ClearActiveTransaction();
      }
    }

    /// <summary>
    /// Commits the current transaction.
    /// </summary>
    public virtual async Task CommitAsync(CancellationToken token = default)
    {
      EnsureIsNotDisposed();
      EnsureTransactionIsActive();
      try {
        await ActiveTransaction.CommitAsync(token).ConfigureAwait(false);
      }
      finally {
        await ActiveTransaction.DisposeAsync().ConfigureAwait(false);
        ClearActiveTransaction();
      }
    }

    /// <summary>
    /// Rollbacks the current transaction.
    /// </summary>
    public virtual void Rollback()
    {
      EnsureIsNotDisposed();
      EnsureTransactionIsActive();

      try {
        ActiveTransaction.Rollback();
      }
      finally {
        ActiveTransaction.Dispose();
        ClearActiveTransaction();
      }
    }

    /// <summary>
    /// Rollbacks the current transaction.
    /// </summary>
    public virtual async Task RollbackAsync(CancellationToken token = default)
    {
      EnsureIsNotDisposed();
      EnsureTransactionIsActive();
      try {
        await ActiveTransaction.RollbackAsync(token).ConfigureAwait(false);
      }
      finally {
        await ActiveTransaction.DisposeAsync().ConfigureAwait(false);
        ClearActiveTransaction();
      }
    }

    /// <summary>
    /// Makes the transaction savepoint.
    /// </summary>
    /// <param name="name">The name of the savepoint.</param>
    public virtual void MakeSavepoint(string name)
    {
      EnsureIsNotDisposed();
      // That's ok to make a savepoint even if they aren't supported -
      // default impl. will fail on rollback
    }

    /// <summary>
    /// Rollbacks current transaction to the specified savepoint.
    /// </summary>
    /// <param name="name">The name of the savepoint.</param>
    public virtual void RollbackToSavepoint(string name) => throw SqlHelper.NotSupported(ServerFeatures.Savepoints);

    /// <summary>
    /// Releases the savepoint with the specfied name.
    /// </summary>
    /// <param name="name">The name of the savepoint.</param>
    public virtual void ReleaseSavepoint(string name)
    {
      EnsureIsNotDisposed();
      // That's ok to release a savepoint even if they aren't supported - 
      // default impl. will fail on rollback
    }

    /// <inheritdoc/>
    public void Dispose()
    {
      if (isDisposed) {
        return;
      }

      isDisposed = true;

      try {
        if (ActiveTransaction != null) {
          ActiveTransaction.Dispose();
          ClearActiveTransaction();
        }
      }
      finally {
        UnderlyingConnection.DisposeSafely();
        ClearUnderlyingConnection();
      }
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
      if (isDisposed) {
        return;
      }

      isDisposed = true;
      try {
        if (ActiveTransaction != null) {
          await ActiveTransaction.DisposeAsync().ConfigureAwait(false);
          ClearActiveTransaction();
        }
      }
      finally {
        await UnderlyingConnection.DisposeAsync().ConfigureAwait(false);
        ClearUnderlyingConnection();
      }
    }

    /// <summary>
    /// Clears the active transaction (i.e. sets <see cref="ActiveTransaction"/> to <see langword="null"/>.
    /// </summary>
    protected abstract void ClearActiveTransaction();

    /// <summary>
    /// Clears underlying connection (i.e. sets <see cref="UnderlyingConnection"/> to <see langword="null"/>.
    /// </summary>
    protected abstract void ClearUnderlyingConnection();

    /// <summary>
    /// Creates the native command.
    /// </summary>
    /// <returns>Created command.</returns>
    protected virtual DbCommand CreateNativeCommand() => UnderlyingConnection.CreateCommand();

    /// <summary>
    /// Ensures the transaction is active (i.e. <see cref="ActiveTransaction"/> is not <see langword="null"/>).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void EnsureTransactionIsActive()
    {
      if (ActiveTransaction == null) {
        throw new InvalidOperationException(Strings.ExTransactionShouldBeActive);
      }
    }

    /// <summary>
    /// Ensures the transaction is not active (i.e. <see cref="ActiveTransaction"/> is <see langword="null"/>).
    /// </summary>
    protected void EnsureTransactionIsNotActive()
    {
      if (ActiveTransaction != null) {
        throw new InvalidOperationException(Strings.ExTransactionShouldNotBeActive);
      }
    }

    // Constructors

    protected SqlConnection(SqlDriver driver)
      : base(driver)
    {
    }
  }
}
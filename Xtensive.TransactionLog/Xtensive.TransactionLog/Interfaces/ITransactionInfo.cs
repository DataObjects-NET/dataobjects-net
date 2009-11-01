using System;
using Xtensive.Core;
using Xtensive.Integrity.Transactions;

namespace Xtensive.TransactionLog
{
  /// <summary>
  /// Describes entities for <see cref="TransactionLog{TKey}"/>.
  /// </summary>
  /// <typeparam name="TKey">Type of key for entities.</typeparam>
  public interface ITransactionInfo<TKey> : IIdentified<TKey>
    where TKey : IComparable<TKey>
  {
    /// <summary>
    /// Gets <see cref="TransactionState"/>.
    /// </summary>
    TransactionState State { get; }
    
    /// <summary>
    /// Gets transaction data.
    /// </summary>
    object Data { get; }
  }
}
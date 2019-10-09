using System;

namespace Xtensive.Orm.Reprocessing
{
  /// <summary>
  /// Tells to reprocessing engine, that needs to rollback transaction and return default value.
  /// </summary>
  public class RollbackTransactionException : Exception
  {
  }
}
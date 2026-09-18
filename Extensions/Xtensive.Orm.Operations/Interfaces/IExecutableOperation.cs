// Copyright (C) 2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

namespace Xtensive.Orm.Operations.Interfaces
{
  /// <summary>
  /// Extension for <see cref="IOperation"/> contract of operation that
  /// can be executed later.
  /// </summary>
  public interface IExecutableOperation : IOperation
  {
    /// <summary>
    /// Prepares the operation using specified execution context.
    /// </summary>
    /// <param name="context">The operation execution context.</param>
    void Prepare(OperationExecutionContext context);

    /// <summary>
    /// Executes the operation using specified execution context.
    /// </summary>
    /// <param name="context">The operation execution context.</param>
    void Execute(OperationExecutionContext context);
  }
}
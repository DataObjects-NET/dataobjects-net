// Copyright (C) 2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

namespace Xtensive.Orm.Operations
{
  /// <summary>
  /// Various extension.
  /// </summary>
  public static class OperationExtensions
  {
    /// <summary>
    /// Easy access to operation factory.
    /// </summary>
    public static IOperationFactory GetOperationFactory(this IOperationRegistry operationRegistry)
    {
      return operationRegistry.Session.Domain.Services.Get<IOperationFactory>();
    }
  }
}

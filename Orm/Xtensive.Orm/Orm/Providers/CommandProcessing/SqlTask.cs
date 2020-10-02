// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.10.30

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// An abstract task for <see cref="CommandProcessor"/>.
  /// </summary>
  public abstract class SqlTask
  {
    /// <summary>
    /// Processes this command with the specified <see cref="CommandProcessor"/>.
    /// </summary>
    /// <param name="processor">The processor to use.</param>
    /// <param name="context">A contextual information to be used while processing
    /// this <see cref="SqlTask"/> instance.</param>
    public abstract void ProcessWith(ISqlTaskProcessor processor, CommandProcessorContext context);
  }
}
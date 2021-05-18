// Copyright (C) 2010-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2010.03.01

using System;
using System.Collections.Generic;

namespace Xtensive.Orm
{
  /// <summary>
  /// Operation sequence contract.
  /// </summary>
  public interface IOperationSequence : IEnumerable<IOperation>, Core.IOperationSequence
  {
    /// <summary>
    /// Applies this operation sequence to the specified session.
    /// </summary>
    /// <param name="session">The session to apply the sequence to.</param>
    /// <returns>Key mapping.</returns>
    KeyMapping Replay(Session session);
  }
}
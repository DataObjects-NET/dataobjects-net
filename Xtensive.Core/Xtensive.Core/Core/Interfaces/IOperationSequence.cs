// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.03.01

using Xtensive.Collections;

namespace Xtensive.Core
{
  /// <summary>
  /// General operation sequence contract.
  /// Allows to replay the operations contained in it later.
  /// </summary>
  public interface IOperationSequence : ICountable
  {
    /// <summary>
    /// Replays the operations contained in sequence on <paramref name="target"/> object.
    /// </summary>
    /// <param name="target">The target object to replay the sequence at.</param>
    /// <returns>The result of execution.</returns>
    object Replay(object target);
  }
}
// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2010.02.18

using System;
using Xtensive.Core;

namespace Xtensive.Storage.Operations
{
  /// <summary>
  /// Contract for an operation that will be executed only once
  /// in a given <see cref="OperationSet"/>.
  /// </summary>
  public interface IUniqueOperation : IIdentified
  {
    /// <summary>
    /// Gets a value indicating whether the <see cref="InvalidOperationException"/> will be thrown
    /// if the duplicate of this operation is found.
    /// </summary>
    bool IgnoreDuplicate { get; }
  }
}
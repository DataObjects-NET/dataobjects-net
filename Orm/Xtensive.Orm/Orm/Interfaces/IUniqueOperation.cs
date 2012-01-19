// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2010.02.18

using System;
using Xtensive.Core;

namespace Xtensive.Orm
{
  /// <summary>
  /// Contract for an operation that must be logged only once
  /// in a given <see cref="OperationLog"/>.
  /// </summary>
  public interface IUniqueOperation : IIdentified
  {
    /// <summary>
    /// Gets a value indicating whether to ignore the duplicate of this operation, 
    /// or to throw an <see cref="InvalidOperationException"/>.
    /// </summary>
    bool IgnoreIfDuplicate { get; }
  }
}
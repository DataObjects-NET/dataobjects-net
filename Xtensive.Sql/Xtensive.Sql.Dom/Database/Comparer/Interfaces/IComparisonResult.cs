// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.26

using System;
using Xtensive.Core;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  public interface IComparisonResult : ILockable
  {
    /// <summary>
    /// Gets <see langword="true"/> if result contains changes, otherwise gets <see langword="false"/>.
    /// </summary>
    bool HasChanges { get; }

    /// <summary>
    /// Gets comparison type.
    /// </summary>
    ComparisonResultType ResultType { get; }

    Type Type { get; }
  }
}
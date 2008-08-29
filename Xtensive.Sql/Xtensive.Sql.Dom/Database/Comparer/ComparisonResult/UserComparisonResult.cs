// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.29

using System;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  [Serializable]
  public class UserComparisonResult : NodeComparisonResult,
    IComparisonResult<User>
  {
    /// <inheritdoc/>
    public User NewValue
    {
      get { return (User)base.NewValue; }
    }

    /// <inheritdoc/>
    public User OriginalValue
    {
      get { return (User)base.OriginalValue; }
    }

    public UserComparisonResult(User originalValue, User newValue)
      : base(originalValue, newValue)
    {
    }
  }
}
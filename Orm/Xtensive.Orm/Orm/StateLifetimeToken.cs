// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2014.01.30

namespace Xtensive.Orm
{
  /// <summary>
  /// State lifetime token for transactional objects.
  /// </summary>
  public sealed class StateLifetimeToken
  {
    /// <summary>
    /// Gets value indicating whenever this token is active.
    /// </summary>
    public bool IsActive { get; private set; }

    internal void Expire()
    {
      IsActive = false;
    }

    internal StateLifetimeToken()
    {
      IsActive = true;
    }
  }
}
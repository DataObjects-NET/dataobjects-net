// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.12.18

namespace Xtensive.Caching
{
  /// <summary>
  /// Invalidatable object contract.
  /// </summary>
  public interface IInvalidatable
  {
    /// <summary>
    /// Invalidates the state of this object.
    /// </summary>
    void Invalidate();
  }
}
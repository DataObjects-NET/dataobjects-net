// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.08.28

using ValidationContextBase = Xtensive.Storage.Validation.ValidationContextBase;

namespace Xtensive.Storage
{
  /// <summary>
  /// Validation context used by <see cref="Session"/>.
  /// </summary>
  public sealed class ValidationContext : Validation.ValidationContextBase
  {
    internal new void Reset()
    {
      base.Reset();
    }
  }
}
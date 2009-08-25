// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.08.28

using Xtensive.Integrity.Validation;

namespace Xtensive.Storage
{
  /// <summary>
  /// Validation context used by <see cref="Session"/>.
  /// </summary>
  public sealed class ValidationContext : ValidationContextBase
  {
    internal new void Reset()
    {
      base.Reset();
    }
  }
}
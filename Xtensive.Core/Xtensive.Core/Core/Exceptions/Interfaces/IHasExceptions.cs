// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.06

using System;
using System.Collections.Generic;

namespace Xtensive.Core
{
  /// <summary>
  /// "Has exceptions" contract.
  /// </summary>
  public interface IHasExceptions
  {
    /// <summary>
    /// Gets the sequence of exceptions associated with the current object.
    /// </summary>
    IEnumerable<Exception> Exceptions { get; }
  }
}
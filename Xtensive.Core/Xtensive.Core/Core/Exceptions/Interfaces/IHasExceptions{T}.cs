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
  /// An object having <see cref="Exceptions"/> property.
  /// </summary>
  /// <typeparam name="TException">The type of the exception.</typeparam>
  public interface IHasExceptions<TException> : IHasExceptions
    where TException : Exception
  {
    /// <summary>
    /// Gets the enumerable of exceptions related to this object.
    /// </summary>
    new IEnumerable<TException> Exceptions { get; }
  }
}
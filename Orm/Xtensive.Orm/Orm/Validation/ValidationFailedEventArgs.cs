// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.02.18

using System;
using System.Collections.Generic;
using System.Linq;

namespace Xtensive.Orm.Validation
{
  /// <summary>
  /// Event data for validation failure event.
  /// </summary>
  public class ValidationFailedEventArgs : EventArgs
  {
    /// <summary>
    /// Gets or sets value indicating whether
    /// this error is handled. If this property is set to true
    /// <see cref="Session.Validate"/> will no throw exception
    /// for this validation failure.
    /// </summary>
    public bool Handled { get; set; }

    /// <summary>
    /// Gets exceptions occured during validation.
    /// </summary>
    public ICollection<Exception> Exceptions { get; private set; }

    /// <summary>
    /// Creates new instance of this class.
    /// </summary>
    /// <param name="exceptions">Validation exceptions.</param>
    public ValidationFailedEventArgs(IEnumerable<Exception> exceptions)
    {
      Exceptions = exceptions.ToList().AsReadOnly();
    }
  }
}
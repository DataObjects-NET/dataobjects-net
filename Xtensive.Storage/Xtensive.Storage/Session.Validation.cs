// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.11.07

using System;
using Xtensive.Storage;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage
{
  public partial class Session 
  {
    /// <summary>
    /// Gets the current validation context.
    /// </summary>
    /// <exception cref="InvalidOperationException">Can not get validation context: There is no active transaction.</exception>
    public ValidationContext ValidationContext {
      get {
        if (Transaction==null)
          throw new InvalidOperationException(Strings.ExCanNotGetValidationContextThereIsNoActiveTransaction);
        return Transaction.ValidationContext;
      }
    }

    /// <summary>
    /// Opens the "inconsistent region" - the code region, in which changed entities
    /// should just queue the validation rather then perform it immediately.
    /// </summary>
    /// <returns></returns>
    public IDisposable OpenInconsistentRegion()
    {
      return ValidationContext.OpenInconsistentRegion();
    }
  }
}
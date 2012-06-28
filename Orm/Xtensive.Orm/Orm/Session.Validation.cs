// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.11.07

using System;
using Xtensive.Core;
using Xtensive.Orm.Validation;

namespace Xtensive.Orm
{
  public partial class Session 
  {
    /// <summary>
    /// Opens the "inconsistent region" - the code region, in which validation is
    /// just queued for delayed execution rather then performed immediately.
    /// Actual validation will happen on disposal of <see cref="ICompletableScope"/>.
    /// </summary>
    /// <returns>
    /// <see cref="IDisposable"/> object, which disposal will identify the end of the region.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The beginning of the region is the place where this method is called.
    /// </para>
    /// <para>
    /// The end of the region is the place where returned <see cref="IDisposable"/> object is disposed.
    /// The validation of all queued to validate objects will be performed during disposal, if
    /// <see cref="ICompletableScope.Complete"/> method was called on
    /// <see cref="ICompletableScope"/> object before disposal.
    /// </para>
    /// </remarks>
    public ICompletableScope DisableValidation()
    {
      return ValidationContext.DisableValidation();
    }

    /// <summary>
    /// Validates all instances registered in <see cref="ValidationContext"/>
    /// of current <see cref="Session"/> regardless if inconsistency
    /// regions are open or not.
    /// </summary>
    public void Validate()
    {
      ValidationContext.Validate();
    }

    /// <summary>
    /// Gets the current validation context.
    /// </summary>
    /// <exception cref="InvalidOperationException">Can not get validation context: There is no active transaction.</exception>
    internal ValidationContext ValidationContext {
      get
      {
        var transaction = Transaction;
        if (transaction==null)
          throw new InvalidOperationException(Strings.ExCanNotGetValidationContextThereIsNoActiveTransaction);
        return transaction.ValidationContext;
      }
    }
  }
}
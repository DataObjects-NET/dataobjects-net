// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.11.26

using System;
using Xtensive.Core;
using Xtensive.Core.Disposing;
using Xtensive.Integrity.Transactions;
using Xtensive.Storage.Resources;
using AggregateException = Xtensive.Core.AggregateException;

namespace Xtensive.Storage
{
  partial class Transaction
  {
    private void BeginValidation()
    {
      ValidationContext.Reset();
      if (Session.Domain.Configuration.ValidationMode==ValidationMode.OnDemand)
        inconsistentRegion = ValidationContext.OpenInconsistentRegion();
    }

    private void CompleteValidation()
    {
      if (inconsistentRegion==null && !ValidationContext.IsConsistent)
        throw new InvalidOperationException(Strings.ExCanNotCommitATransactionValidationContextIsInInconsistentState);

      try {
        Validation.Enforce(Session);

        if (inconsistentRegion!=null) {
          inconsistentRegion.Complete();
          inconsistentRegion.Dispose();
          inconsistentRegion = null;
        }
      }
      catch (AggregateException exception) {
        throw new InvalidOperationException(Strings.ExCanNotCommitATransactionEntitiesValidationFailed, exception);
      }
    }

    private void AbortValidation()
    {
      inconsistentRegion.DisposeSafely();
    }
  }
}
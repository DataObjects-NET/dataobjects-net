// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.11.26

using System;
using Xtensive.Core;
using Xtensive.Core.Disposing;
using Xtensive.Storage.Resources;

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
          inconsistentRegion.DisposeSafely();
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
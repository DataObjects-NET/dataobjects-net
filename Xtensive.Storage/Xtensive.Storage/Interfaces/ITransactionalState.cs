// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.10.15

namespace Xtensive.Storage
{
  public interface ITransactionalState : ITransactionBound
  {
    void EnsureConsistency(Transaction current);

    void Reset(Transaction current);
  }
}
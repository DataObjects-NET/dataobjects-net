// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.10.15

namespace Xtensive.Storage.Internals
{
  public interface ITransactionalState
  {
    Transaction Transaction { get; }

    bool IsConsistent(Transaction current);

    void Reset(Transaction current);
  }
}
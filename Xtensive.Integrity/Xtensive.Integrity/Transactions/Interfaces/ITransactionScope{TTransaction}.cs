// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.18

namespace Xtensive.Integrity.Transactions
{
  public interface ITransactionScope<TTransaction>: ITransactionScope
    where TTransaction: class, ITransaction
  {
  }
}
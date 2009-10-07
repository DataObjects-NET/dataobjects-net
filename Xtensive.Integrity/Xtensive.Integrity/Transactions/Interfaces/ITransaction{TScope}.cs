// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.18

using System;
using Xtensive.Core.IoC;

namespace Xtensive.Integrity.Transactions
{
  public interface ITransaction<TScope>: ITransaction,
    IContext<TScope>
    where TScope: class, IDisposable
  {
  }
}
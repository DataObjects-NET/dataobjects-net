// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.18

using System;

namespace Xtensive.Transactions
{
  public interface ITransactionScope: IDisposable
  {
    void Complete();
  }
}
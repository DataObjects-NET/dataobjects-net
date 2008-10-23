// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.10.23

namespace Xtensive.Storage
{
  public interface ITransactionBound
  {
    Transaction Transaction { get; }
  }
}
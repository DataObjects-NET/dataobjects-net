// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.21

namespace Xtensive.Storage.Operations
{
  public interface IOperation
  {
    void Prepare(OperationContext operationContext);
    void Execute(OperationContext operationContext);
  }
}
// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ilyin
// Created:    2007.06.04

namespace Xtensive.Core.Links.LinkedReference.Operations
{
  internal interface ISetReferenceOperation
  {
    void Execute(object operationOwner, object dependentObject,
      SetReferenceExecuteOption setReferenceExecuteOption);
  }
}
// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ilyin
// Created:    2007.06.04

using Xtensive.Core.Links.LinkedSet.Operations;

namespace Xtensive.Core.Links.LinkedSet
{
  internal interface IRemoveItemOperation
  {
    void Execute(ILinkOwner operationOwner, object linkedObject,
      RemoveItemExecuteOption removeItemExecuteOption);
  }
}
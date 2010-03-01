// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Ilyin
// Created:    2007.06.28

using System;

namespace Xtensive.Core.Links.LinkedSet.Operations
{
  [Flags]
  internal enum RemoveItemExecuteOption
  {
    Default = 0,
    ExecSetContainerCall = 1,
    ExecRemoveItemCall = 2,
    ExecAll = 3
  }
}
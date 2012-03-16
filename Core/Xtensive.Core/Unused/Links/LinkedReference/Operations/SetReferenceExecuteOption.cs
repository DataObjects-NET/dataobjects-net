// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Ilyin
// Created:    2007.06.28

using System;

namespace Xtensive.Core.Links.LinkedReference.Operations
{
  [Flags]
  internal enum SetReferenceExecuteOption
  {
    Default = 0,
    ExecNewValueSetProperty = 1,
    ExecOldValueSetProperty = 2,
    ExecLinkedAddOperation = 4,
    ExecLinkedRemoveOperation = 8,
    ExecAll = 15
  }
}
// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.08.12

using System;

namespace Xtensive.Core.Threading
{
  internal interface ITask
  {
    void RegisterException(Exception exception);

    void Execute();
  }
}
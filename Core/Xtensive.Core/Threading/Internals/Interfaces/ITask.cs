// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.01.12

using System;

namespace Xtensive.Threading
{
  public interface ITask
  {
    void Execute();
    void Terminate(Exception exception);
  }
}
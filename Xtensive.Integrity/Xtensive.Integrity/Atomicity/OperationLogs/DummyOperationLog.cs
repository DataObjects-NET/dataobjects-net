// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.12.31

namespace Xtensive.Integrity.Atomicity.OperationLogs
{
  public class DummyOperationLog: IOperationLog
  {
    public void Append(IRedoDescriptor redoDescriptor)
    {
      return;
    }
  }
}
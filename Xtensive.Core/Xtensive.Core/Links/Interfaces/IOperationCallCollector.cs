// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ilyin
// Created:    2007.06.04

namespace Xtensive.Core.Links
{
  internal interface IOperationCallCollector
  {
    void ExecutePrologue();
    void ExecuteOperation();
    void ExecuteEpilogue();
    void OnBeforeExecute();
    void OnAfterExecute();
  }
}
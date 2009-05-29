// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.05.28

using System;
using System.Collections.Generic;
using Xtensive.Storage.Building.FixupActions;

namespace Xtensive.Storage.Building
{
  internal class ModelInspectionResult
  {
    public Queue<FixupAction> Actions { get; private set; }

    public bool HasActions
    {
      get { return Actions.Count > 0; }
    }

    public void Register(FixupAction action)
    {
      Actions.Enqueue(action);
    }

    public ModelInspectionResult()
    {
      Actions = new Queue<FixupAction>();
    }
  }
}
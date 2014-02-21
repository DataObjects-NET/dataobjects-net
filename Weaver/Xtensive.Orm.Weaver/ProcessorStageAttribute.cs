// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.12.25

using System;

namespace Xtensive.Orm.Weaver
{
  [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
  public sealed class ProcessorStageAttribute : Attribute
  {
    public Type StageType { get; private set; }
    public int Priority { get; private set; }

    public ProcessorStageAttribute(Type stageType, int priority)
    {
      StageType = stageType;
      Priority = priority;
    }
  }
}
// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.10.12

using System;

namespace Xtensive.Core.IoC
{
  [Serializable]
  internal sealed class ServiceInfo
  {
    public Type Type { get; private set;}

    public Type MapTo { get; private set; }

    public string Name { get; private set; }

    public bool IsSingleton { get; private set; }


    // Constructors

    public ServiceInfo(Type type, Type mapTo, string name, bool isSingleton)
    {
      Type = type;
      MapTo = mapTo;
      Name = name;
      IsSingleton = isSingleton;
    }
  }
}
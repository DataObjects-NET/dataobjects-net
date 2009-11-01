// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.10.20

using Xtensive.Core.Aspects;

namespace Xtensive.Core.Aspects.Tests
{
  public class ChangerSample: ChangerSampleBase
  {
    private string namePrefix;

    [Changer]
    [Trace(TraceOptions.All)]
    public string NamePrefix
    {
      get { return namePrefix; }
      set
      {
        namePrefix = value;
      }
    }

    [Changer]
    [Trace(TraceOptions.All)]
    public void SetAll(string namePrefix, string name, int age)
    {
      NamePrefix = namePrefix;
      SetAll(name, age);
    }

    
    // Constructors

    public ChangerSample(string namePrefix, string name, int age)
      : base(name, age)
    {
      this.namePrefix = namePrefix;
    }
  }
}
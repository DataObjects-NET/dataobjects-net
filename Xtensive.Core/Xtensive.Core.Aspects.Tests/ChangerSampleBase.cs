// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.10.20

using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Core.Aspects;
using Xtensive.Core.Notifications;

namespace Xtensive.Core.Aspects.Tests
{
  public class ChangerSampleBase
  {
    private string name;
    private int age;
    private int changingCount = 0;
    private int changedCount = 0;

    [Changer]
    [Trace(TraceOptions.All)]
    public string Name
    {
      get { return name; }
      set
      {
        ArgumentValidator.EnsureArgumentNotNullOrEmpty(value, "value");
        name = value;
      }
    }

    [Changer]
    [Trace(TraceOptions.All)]
    public int Age
    {
      get { return age; }
      set {
        ArgumentValidator.EnsureArgumentIsInRange(value, 0, 200, "value");
        age = value;
      }
    }

    [Trace(TraceOptions.All)]
    public int AgeWithChangingGetter
    {
      [Changer]
      get { return age; }
      [Changer]
      set {
        ArgumentValidator.EnsureArgumentIsInRange(value, 0, 200, "value");
        age = value;
      }
    }

    public bool IsChanging
    {
      get { return changingCount != changedCount; }
    }

    public int ChangeCount
    {
      get { return changedCount; }
    }
    
    [Changer]
    [Trace(TraceOptions.All)]
    public void SetAll(string name, int age)
    {
      Name = name;
      Age = age;
    }

    public void ResetChangeCounters()
    {
      changingCount = 0;
      changedCount  = 0;
    }

    public override string ToString()
    {
      return name + ", " + age.ToString();
    }

    // Event handlers

    private void OnChanging(object sender, ChangeNotifierEventArgs e)
    {
      Assert.AreEqual(sender, this);
      changingCount++;
      Assert.IsTrue(IsChanging);
      Log.Info("{0} is changing {1} time.", this, changingCount);
    }

    private void OnChanged(object sender, ChangeNotifierEventArgs e)
    {
      Assert.AreEqual(sender, this);
      changedCount++;
      Log.Info("{0} is changed {1} time.", this, changedCount);
    }


    // Constructors

    public ChangerSampleBase(string name, int age)
    {
      this.name = name;
      this.age = age;
      (this as IChangeNotifier).Changing += OnChanging;
      (this as IChangeNotifier).Changed  += OnChanged;
    }
  }
}
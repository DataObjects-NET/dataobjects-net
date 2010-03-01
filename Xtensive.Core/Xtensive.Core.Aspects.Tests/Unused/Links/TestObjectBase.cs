// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.06.10

using System;
using NUnit.Framework;
using Xtensive.Core.Aspects;
using Xtensive.Core.Links;

namespace Xtensive.Core.Aspects.Tests.Links
{
  public abstract class TestObjectBase : IHasName
  {
    private string name;

    public string Name
    {
      get { return name; }
      set
      {
        ArgumentValidator.EnsureArgumentNotNullOrEmpty(value, "value");
        OnChanging("Name", name, value);
        name = value;
        OnChanged("Name", name, value);
      }
    }

    protected void OnPropertyAction(string action, string propertyName, object oldValue, object newValue)
    {
      Log.Info("{0,-8}: {1}.{2} = {3} (was {4})", action, ReplaceNull(this), propertyName, ReplaceNull(newValue), ReplaceNull(oldValue));
    }

    protected void OnChanging(string propertyName, object oldValue, object newValue)
    {
      if (LinkTests.PerformanceTest)
        return;
      LinkTests.EventCount++;
      Assert.AreEqual(LinkTests.EventStage, LinkEventStage.Prologue);
      OnPropertyAction("Changing", propertyName, oldValue, newValue);
    }

    protected void OnSet(string propertyName, object oldValue, object newValue)
    {
      if (LinkTests.PerformanceTest)
        return;
      LinkTests.EventCount++;
      if (LinkTests.EventStage==LinkEventStage.Prologue)
        LinkTests.EventStage = LinkEventStage.Operation;
      Assert.AreEqual(LinkTests.EventStage, LinkEventStage.Operation);
      OnPropertyAction("Set", propertyName, oldValue, newValue);
    }

    protected void OnChanged(string propertyName, object oldValue, object newValue)
    {
      if (LinkTests.PerformanceTest)
        return;
      LinkTests.EventCount++;
      if (LinkTests.EventStage==LinkEventStage.Operation)
        LinkTests.EventStage = LinkEventStage.Epilogue;
      Assert.AreEqual(LinkTests.EventStage, LinkEventStage.Epilogue);
      OnPropertyAction("Changed", propertyName, oldValue, newValue);
    }

    private static object ReplaceNull(object obj)
    {
      if (obj==null)
        return "null";
      else
        return obj;
    }

    public override string ToString()
    {
      return Name;
    }


    // Constructors

    public TestObjectBase(string name)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      this.name = name;
    }
  }
}
// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.06.25

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Xtensive.Orm;
using Xtensive.Orm.Upgrade;

#if _TEST_COMPILATION
[assembly: Persistent(AttributeTargetAssemblies = "Assembly2")]
#endif

namespace UpgradeHandlersSorting.Model
{
  [Serializable]
  [HierarchyRoot]
  public class Simple2 : Entity
  {
    [Field, Key]
    public int Id
    {
      get { return GetFieldValue<int>("Id");}
    }

    protected Simple2(EntityState state)
      : base(state)
    {}
  }

#if _TEST_COMPILATION
  public class UpgradeHandler2 : UpgradeHandler
  {
    public static int Count = 2;
    
    public override bool IsEnabled {
      get { return true; }
    }

    public override bool CanUpgradeFrom(string oldVersion)
    {
      return oldVersion == null || int.Parse(oldVersion) == 2;
    }

    public override void OnStage()
    {
      if (UpgradeContext.Current.Stage == UpgradeStage.Upgrading) {
        Type handler1Type = null;
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
          if (assembly.GetName().Name=="Assembly1") {
            handler1Type = assembly.GetType("UpgradeHandlersSorting.Model.UpgradeHandler1");
            break;
          }
        }
        int handler1Count = (int) handler1Type.GetField("Count").GetValue(null);
        Count += handler1Count;
      }
      base.OnStage();
    }
  }
#endif
}
// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.07.03

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Xtensive.Orm;
using Xtensive.Orm.Building;
using Xtensive.Orm.Building.Definitions;
using Xtensive.Orm.Upgrade;

namespace Modules.Model
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
  public class UpgradeHandler2 : UpgradeHandler, IModule
  {
    public static int Count = 2;

    public static int ModuleCount = 2;

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
        Type handler1Type = GetTypeFromAssembly("ModuleAssembly1", "Modules.Model.UpgradeHandler1");
        int handler1Count = (int) handler1Type.GetField("Count").GetValue(null);
        Count += handler1Count;
      }
      base.OnStage();
    }

    public static Type GetTypeFromAssembly(string assemblyName, string typeFullName)
    {
      Type result = null;
      foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
        if (assembly.GetName().Name== assemblyName) {
          result = assembly.GetType(typeFullName);
          break;
        }
      }
      return result;
    }

    public void OnBuilt(Domain domain)
    {
      if (domain==null)
        throw new ArgumentNullException();
      Type handler1Type = GetTypeFromAssembly("ModuleAssembly1", "Modules.Model.UpgradeHandler1");
      int handler1Count = (int) handler1Type.GetField("ModuleCount").GetValue(null);
      ModuleCount += handler1Count;
    }

    public void OnDefinitionsBuilt(BuildingContext context, DomainModelDef model)
    {}
  }

  public class Module2 : IModule
  {
    public static int ModuleCount = 2;

    public void OnBuilt(Domain domain)
    {
      if (domain==null)
        throw new ArgumentNullException();
      Type module1Type = UpgradeHandler2.GetTypeFromAssembly("ModuleAssembly1", "Modules.Model.UpgradeHandler1");
      int module1Count = (int) module1Type.GetField("ModuleCount").GetValue(null);
      ModuleCount += module1Count;
    }

    public void OnDefinitionsBuilt(BuildingContext context, DomainModelDef model)
    {}
  }
#endif
}
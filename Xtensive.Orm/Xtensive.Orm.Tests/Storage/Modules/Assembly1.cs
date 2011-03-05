// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.07.03

using System;
using System.Diagnostics;
using System.Linq;
using Xtensive.Orm;
using Xtensive.Orm.Building;
using Xtensive.Orm.Building.Definitions;
using Xtensive.Orm.Upgrade;

namespace Modules.Model
{
  [Serializable]
  [HierarchyRoot]
  public class Simple1 : Entity
  {
    [Field, Key]
    public int Id
    {
      get { return GetFieldValue<int>("Id");}
    }

    protected Simple1(EntityState state)
      : base(state)
    {}
  }

#if _TEST_COMPILATION
  public class UpgradeHandler1 : UpgradeHandler, IModule
  {
    public static int Count = 0;
    public static int ModuleCount = 0;

    public override bool IsEnabled {
      get { return true; }
    }

    public override bool CanUpgradeFrom(string oldVersion)
    {
      return oldVersion == null || oldVersion == "0.0.0.0";
    }

    protected override void AddUpgradeHints()
    {
      UpgradeContext.Current.Hints
        .Add(new RenameTypeHint("Modules.Model.Simple0", typeof(Simple1)));
    }

    public override string AssemblyVersion
    {
      get{ return "0.0.0.1"; }
    }

    public override string AssemblyName
    {
      get{ return "ModuleAssembly0";}
    }

    public override void OnStage()
    {
      if (UpgradeContext.Current.Stage == UpgradeStage.Upgrading)
        Count = 1;
      base.OnStage();
    }

    public void OnBuilt(Domain domain)
    {
      if (domain == null)
        throw new ArgumentNullException();
      ModuleCount = 1;
    }

    public void OnDefinitionsBuilt(BuildingContext context, DomainModelDef model)
    {}
  }

  public class Module1 : IModule
  {
    public static int ModuleCount = 0;

    public void OnBuilt(Domain domain)
    {
      if (domain == null)
        throw new ArgumentNullException();
      ModuleCount = 1;
    }

    public void OnDefinitionsBuilt(BuildingContext context, DomainModelDef model)
    {}
  }
#endif
}
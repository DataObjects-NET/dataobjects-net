// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.04.30

using System;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Reflection;
using System.Linq;
using Xtensive.Core.Helpers;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Upgrade.Hints;

namespace Xtensive.Storage.Upgrade
{
  /// <summary>
  /// Default <see cref="IUpgradeHandler"/> implementation.
  /// </summary>
  public class UpgradeHandler : IUpgradeHandler
  {
    /// <summary>
    /// The ".Recycled" suffix.
    /// </summary>
    public static readonly string RecycledSuffix = ".Recycled";

    private Assembly assembly;
    private string assemblyName;
    private string assemblyVersion;

    /// <inheritdoc/>
    public virtual bool IsEnabled {
      get { return true; }
    }

    /// <inheritdoc/>
    public Assembly Assembly {
      get {
        if (assembly!=null)
          return assembly;
        assembly = DetectAssembly();
        return assembly;
      }
    }

    /// <inheritdoc/>
    public virtual string AssemblyName {
      get {
        if (assemblyName!=null)
          return assemblyName;
        assemblyName = DetectAssemblyName();
        return assemblyName;
      }
    }

    /// <inheritdoc/>
    public string AssemblyVersion {
      get {
        if (assemblyVersion!=null)
          return assemblyVersion;
        assemblyVersion = DetectAssemblyVersion();
        return assemblyVersion;
      }
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentOutOfRangeException"><c>context.Stage</c> is out of range.</exception>
    public virtual void OnBeforeStage()
    {
      var context = UpgradeContext.Current;
      switch (context.Stage) {
        case UpgradeStage.Validation:
          break;
        case UpgradeStage.Upgrading:
          AddUpgradeHints();
          break;
        case UpgradeStage.Final:
          break;
        default:
          throw new ArgumentOutOfRangeException("context.Stage");
      }
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentOutOfRangeException"><c>context.Stage</c> is out of range.</exception>
    public virtual void OnStage()
    {
      var context = UpgradeContext.Current;
      switch (context.Stage) {
        case UpgradeStage.Validation:
          CheckMetadata();
          break;
        case UpgradeStage.Upgrading:
          UpdateMetadata();
          OnUpgrade();
          break;
        case UpgradeStage.Final:
          break;
        default:
          throw new ArgumentOutOfRangeException("context.Stage");
      }
    }

    /// <inheritdoc/>
    public virtual bool CanUpgradeFrom(string oldVersion)
    {
      return oldVersion==null || oldVersion==AssemblyVersion;
    }

    /// <summary>
    /// Override this method to implement custom persistent data migration logic.
    /// </summary>
    public virtual void OnUpgrade()
    {
      return;
    }

    /// <exception cref="ArgumentOutOfRangeException"><c>UpgradeContext.Stage</c> is out of range.</exception>
    public virtual bool IsTypeAvailable(Type type, UpgradeStage upgradeStage)
    {
      if (type.Assembly!=Assembly)
        return false;
      switch (upgradeStage) {
      case UpgradeStage.Validation:
        return type.GetAttribute<SystemTypeAttribute>(AttributeSearchOptions.InheritNone)!=null;
      case UpgradeStage.Upgrading:
        return true;
      case UpgradeStage.Final:
        return type.GetAttribute<RecycledAttribute>(AttributeSearchOptions.InheritNone)==null;
      default:
        throw new ArgumentOutOfRangeException("UpgradeContext.Stage");
      }
    }

    /// <inheritdoc/>
    public virtual string GetTypeName(Type type)
    {
      return type.FullName;
    }

    #region Protected methods

    /// <summary>
    /// Override this method to add upgrade hints to 
    /// <see cref="Upgrade.UpgradeContext.Hints"/> collection.
    /// </summary>
    protected virtual void AddUpgradeHints()
    {
      AddAutoHints();
    }

    /// <summary>
    /// Checks the metadata (see <see cref="Xtensive.Storage.Metadata"/>).
    /// </summary>
    protected virtual void CheckMetadata()
    {
      return;
    }

    /// <summary>
    /// Upgrades the metadata (see <see cref="Xtensive.Storage.Metadata"/>).
    /// </summary>
    protected virtual void UpdateMetadata()
    {
      return;
    }

    /// <summary>
    /// Detects the assembly this handler is made for.
    /// </summary>
    /// <returns>The assembly.</returns>
    private Assembly DetectAssembly()
    {
      return GetType().Assembly;
    }

    /// <summary>
    /// Detects the name of the assembly this handler is made for.
    /// </summary>
    /// <returns>The name of the assembly.</returns>
    protected virtual string DetectAssemblyName()
    {
      var t = GetType();
      var a = Assembly;
      var ai = 
        t.GetAttributes<AssemblyInfoAttribute>(AttributeSearchOptions.InheritNone).SingleOrDefault() 
        ?? Attribute.GetCustomAttributes(a, typeof(AssemblyInfoAttribute), false).SingleOrDefault() as AssemblyInfoAttribute;
      return (ai==null || ai.Name.IsNullOrEmpty()) 
        ? a.GetName().Name 
        : ai.Name;
    }

    /// <summary>
    /// Detects the version of the assembly this handler is made for.
    /// </summary>
    /// <returns>The version of the assembly.</returns>
    protected virtual string DetectAssemblyVersion()
    {
      var t = GetType();
      var a = Assembly;
      var ai = 
        t.GetAttributes<AssemblyInfoAttribute>(AttributeSearchOptions.InheritNone).SingleOrDefault() 
        ?? Attribute.GetCustomAttributes(a, typeof(AssemblyInfoAttribute), false).SingleOrDefault() as AssemblyInfoAttribute;
      return (ai==null || ai.Version.IsNullOrEmpty()) 
        ? a.GetName().Version.ToString() 
        : ai.Version;
    }

    /// <summary>
    /// Adds the "auto" hints - e.g. hints for recycled types.
    /// </summary>
    protected virtual void AddAutoHints()
    {
      var context = UpgradeContext.Demand();
      var recycled =
        from t in Assembly.GetTypes()
        where typeof (Entity).IsAssignableFrom(t)
        let a = t.GetAttribute<RecycledAttribute>(AttributeSearchOptions.InheritNone)
        where a!=null
        select new {Type = t, Attribute = a};

      foreach (var r in recycled) {
        var oldName = r.Attribute.OriginalName;
        if (oldName.IsNullOrEmpty())
          oldName = GetOriginalName(r.Type);
        else if (!oldName.Contains(".")) {
          string ns = TryStripRecycledSuffix(r.Type.Namespace);
          oldName = ns + "." + oldName;
        }
        context.Hints.Add(new RenameTypeHint(r.Type, oldName));
        // TODO: Add table rename hint as well
      }
    }

    /// <summary>
    /// Gets the original name of the recycled type.
    /// </summary>
    /// <param name="recycledType">The recycled type to get the original name for.</param>
    /// <returns>The original name of the recycled type.</returns>
    protected virtual string GetOriginalName(Type recycledType)
    {
      return TryStripRecycledSuffix(recycledType.Namespace) + "." + recycledType.Name;
    }

    /// <summary>
    /// Tries to strip the ".Recycled" suffix from the namespace.
    /// </summary>
    /// <param name="nameSpace">The namespace to remove the suffix from.</param>
    /// <returns>
    /// The namespace without ".Recycled" suffix, if it was there;
    /// otherwise the same value.
    /// </returns>
    protected virtual string TryStripRecycledSuffix(string nameSpace)
    {
      return nameSpace.TryCutSuffix(RecycledSuffix);
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public UpgradeHandler()
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="assembly">The assembly this handler is bound to.</param>
    public UpgradeHandler(Assembly assembly)
    {
      this.assembly = assembly;
    }
  }
}
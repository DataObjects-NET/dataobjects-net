// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.04.30

using System;
using System.Reflection;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Reflection;
using System.Linq;
using Xtensive.Modelling.Comparison.Hints;

namespace Xtensive.Orm.Upgrade
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
      get {
        // Must be disabled for Xtensive.Storage,
        // since there is SystemUpgradeHandler.
        return Assembly!=typeof(UpgradeHandler).Assembly;
      }
    }

    /// <inheritdoc/>
    public virtual Assembly Assembly {
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
    public virtual string AssemblyVersion {
      get {
        if (assemblyVersion!=null)
          return assemblyVersion;
        assemblyVersion = DetectAssemblyVersion();
        return assemblyVersion;
      }
    }

    public UpgradeContext UpgradeContext { get; private set; }

    /// <inheritdoc/>
    /// <exception cref="ArgumentOutOfRangeException"><c>context.Stage</c> is out of range.</exception>
    public virtual void OnBeforeStage()
    {
      var context = UpgradeContext;
      switch (context.Stage) {
        case UpgradeStage.Initializing:
          break;
        case UpgradeStage.Upgrading:
          AddUpgradeHints(context.Hints);
          AddAutoHints(context.Hints);
          break;
        case UpgradeStage.Final:
          break;
        default:
          throw new ArgumentOutOfRangeException("context.Stage");
      }
    }

    /// <inheritdoc/>
    public virtual void OnSchemaReady()
    {
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentOutOfRangeException"><c>context.Stage</c> is out of range.</exception>
    public virtual void OnStage()
    {
      var context = UpgradeContext;
      switch (context.Stage) {
        case UpgradeStage.Initializing:
          break;
        case UpgradeStage.Upgrading:
          OnUpgrade();
          break;
        case UpgradeStage.Final:
          break;
        default:
          throw new ArgumentOutOfRangeException("context.Stage");
      }
    }

    public virtual void OnBeforeExecuteActions(UpgradeActionSequence actions)
    {
      // Doing nothing
    }

    /// <inheritdoc/>
    public virtual bool CanUpgradeFrom(string oldVersion)
    {
      return oldVersion==null || oldVersion==AssemblyVersion;
    }

    /// <exception cref="ArgumentOutOfRangeException"><c>UpgradeContext.Stage</c> is out of range.</exception>
    public virtual bool IsTypeAvailable(Type type, UpgradeStage upgradeStage)
    {
      ArgumentValidator.EnsureArgumentNotNull(type, "type");
      if (type.Assembly!=Assembly)
        throw new ArgumentOutOfRangeException("type");
      switch (upgradeStage) {
      case UpgradeStage.Initializing:
        return type.GetAttribute<SystemTypeAttribute>()!=null;
      case UpgradeStage.Upgrading:
        return true;
      case UpgradeStage.Final:
        return type.GetAttribute<RecycledAttribute>()==null;
      default:
        throw new ArgumentOutOfRangeException("UpgradeContext.Stage");
      }
    }

    /// <exception cref="ArgumentOutOfRangeException"><c>UpgradeContext.Stage</c> is out of range.</exception>
    public bool IsFieldAvailable(PropertyInfo field, UpgradeStage upgradeStage)
    {
      ArgumentValidator.EnsureArgumentNotNull(field, "field");
      var type = field.DeclaringType;
      if (type.Assembly!=Assembly)
        throw new ArgumentOutOfRangeException("field");
      if (upgradeStage==UpgradeStage.Final)
        return field.GetAttribute<RecycledAttribute>()==null;
      return true;
    }

    #region Protected methods

    /// <summary>
    /// Override this method to implement custom persistent data migration logic.
    /// </summary>
    public virtual void OnUpgrade()
    {
    }

    /// <summary>
    /// Override this method to add upgrade hints to
    /// <see cref="Upgrade.UpgradeContext.Hints"/> collection.
    /// </summary>
    /// <param name="hints">A set of hints to add new hints to
    /// (a shortcut to <see cref="Upgrade.UpgradeContext.Hints"/> collection).</param>
    protected virtual void AddUpgradeHints(ISet<UpgradeHint> hints)
    {
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
        t.GetAttributes<AssemblyInfoAttribute>().SingleOrDefault() 
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
        t.GetAttributes<AssemblyInfoAttribute>().SingleOrDefault() 
        ?? Attribute.GetCustomAttributes(a, typeof(AssemblyInfoAttribute), false).SingleOrDefault() as AssemblyInfoAttribute;
      return (ai==null || ai.Version.IsNullOrEmpty()) 
        ? a.GetName().Version.ToString() 
        : ai.Version;
    }

    /// <summary>
    /// Adds the "auto" hints - e.g. hints for recycled types.
    /// </summary>
    /// <param name="hints">A set of hints to add new hints to
    /// (a shortcut to <see cref="Upgrade.UpgradeContext.Hints"/> collection).</param>
    protected virtual void AddAutoHints(ISet<UpgradeHint> hints)
    {
      var context = UpgradeContext;
      var types = Assembly.GetTypes();
      var registeredTypes = (
        from t in types
        where context.Configuration.Types.Contains(t)
        select t).ToArray();

      var recycledTypes =
        from t in registeredTypes
        let a = t.GetAttribute<RecycledAttribute>()
        where a!=null
        select new {Type = t, Attribute = a};

      foreach (var r in recycledTypes) {
        var oldName = r.Attribute.OriginalName;
        if (oldName.IsNullOrEmpty())
          oldName = GetOriginalName(r.Type);
        else if (!oldName.Contains(".")) {
          string ns = TryStripRecycledSuffix(r.Type.Namespace);
          oldName = ns + "." + oldName;
        }
        var renameHint = new RenameTypeHint(oldName, r.Type);
        var similarHints =
          from h in hints
          let similarHint = h as RenameTypeHint
          where similarHint!=null
          where similarHint.NewType==renameHint.NewType
          select similarHint;
        if (!similarHints.Any())
          hints.Add(renameHint);
        // TODO: Add table rename hint as well
      }

      var recycledProperties =
        from t in registeredTypes
        from p in t.GetProperties(BindingFlags.DeclaredOnly 
          | BindingFlags.Instance
          | BindingFlags.Public
          | BindingFlags.NonPublic)
        let pa = p.GetAttribute<FieldAttribute>()
        let ra = p.GetAttribute<RecycledAttribute>()
        where pa!=null && ra!=null
        select new {Property = p, Attribute = ra};

      foreach (var r in recycledProperties) {
        var oldName = r.Attribute.OriginalName;
        if (!oldName.IsNullOrEmpty()) {
          // TODO: Add column rename hint here
          hints.Add(new RenameFieldHint(r.Property.DeclaringType, oldName, r.Property.Name));
        }
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
      UpgradeContext = UpgradeContext.Demand();
    }

    internal UpgradeHandler(Assembly assembly)
      : this()
    {
      this.assembly = assembly;
    }
  }
}
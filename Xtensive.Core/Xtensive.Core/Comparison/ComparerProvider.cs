// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.01.14

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Reflection;
using Xtensive.Core.Resources;
using Xtensive.Core.Collections;

namespace Xtensive.Core.Comparison
{
  /// <summary>
  /// Default <see cref="IComparer{T}"/> provider. 
  /// Provides default comparer for the specified type.
  /// </summary>
  /// <remarks>
  /// <para id="About"><see cref="HasStaticDefaultDocTemplate" copy="true" /></para>
  /// </remarks>
  [Serializable]
  public class ComparerProvider : AssociateProvider,
    IComparerProvider
  {
    private static readonly ComparerProvider defaultProvider = new ComparerProvider();
    private static readonly SystemComparerProvider systemProvider = SystemComparerProvider.Instance;

    /// <see cref="HasStaticDefaultDocTemplate.Default" copy="true" />
    [DebuggerStepThrough]
    public static ComparerProvider Default
    {
      get { return defaultProvider; }
    }

    /// <summary>
    /// Gets system comparer provider.
    /// A shortcut to <see cref="SystemComparerProvider.Instance"/>.
    /// </summary>
    [DebuggerStepThrough]
    public static SystemComparerProvider System
    {
      get { return systemProvider; }
    }

    #region IComparerProvider Members

    /// <inheritdoc/>
    public virtual AdvancedComparer<T> GetComparer<T>()
    {
      return GetAssociate<T, IAdvancedComparer<T>, AdvancedComparer<T>>();
    }

    #endregion

    #region Protected method overrides

    /// <inheritdoc/>
    protected override TAssociate CreateAssociate<TKey, TAssociate>(out Type foundFor)
    {
      TAssociate associate = base.CreateAssociate<TKey, TAssociate>(out foundFor);
      if (associate!=null)
        return associate;
      // Ok, null, but probably just because type cast has failed;
      // let's try to wrap it. TKey is type for which we're getting
      // the comparer.
      IAdvancedComparerBase comparer = base.CreateAssociate<TKey, IAdvancedComparerBase>(out foundFor);
      if (foundFor==null) {
        Log.Warning(Strings.LogCantFindAssociateFor,
          TypeSuffixes.ToDelimitedString(" \\ "),
          typeof (TAssociate).GetShortName(),
          typeof (TKey).GetShortName());
        return null;
      }
      if (foundFor==typeof (TKey))
        return (TAssociate) comparer;
      Type baseComparerWrapperType = typeof (BaseComparerWrapper<,>);
      associate =
        baseComparerWrapperType.Activate(new Type[] {typeof (TKey), foundFor}, ConstructorParams) as
          TAssociate;
      if (associate!=null) {
        Log.Warning(Strings.LogGenericAssociateIsUsedFor,
          baseComparerWrapperType.GetShortName(),
          typeof (TKey).GetShortName(),
          foundFor.GetShortName(),
          typeof (TKey).GetShortName());
        return associate;
      }
      else {
        Log.Warning(Strings.LogGenericAssociateCreationHasFailedFor,
          baseComparerWrapperType.GetShortName(),
          typeof (TKey).GetShortName(),
          foundFor.GetShortName(),
          typeof (TKey).GetShortName());
        return null;
      }
    }

    /// <inheritdoc/>
    protected override TResult ConvertAssociate<TKey, TAssociate, TResult>(TAssociate associate)
    {
      if (ReferenceEquals(associate, null))
        return default(TResult);
      else
        return (TResult) (object) new AdvancedComparer<TKey>((IAdvancedComparer<TKey>) associate);
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    protected ComparerProvider()
    {
      TypeSuffixes = new[] {"Comparer"};
      ConstructorParams = new object[] {this, ComparisonRules.Positive};
      Type t = typeof (BooleanComparer);
      AddHighPriorityLocation(t.Assembly, t.Namespace);
    }
  }
}
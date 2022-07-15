// Copyright (C) 2008-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Nick Svetlov
// Created:    2008.01.14

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using Xtensive.Core;
using Xtensive.Reflection;

namespace Xtensive.Comparison
{
  /// <summary>
  /// Default <see cref="IComparer{T}"/> provider. 
  /// Provides default comparer for the specified type.
  /// </summary>
  [Serializable]
  public class ComparerProvider : AssociateProvider,
    IComparerProvider
  {
    private static readonly Type BaseComparerWrapperType = typeof(BaseComparerWrapper<,>);
    private static readonly ComparerProvider DefaultProvider = new ComparerProvider();
    private static readonly SystemComparerProvider SystemProvider = SystemComparerProvider.Instance;

    /// <summary>
    /// Gets default instance of this type.
    /// </summary>
    public static ComparerProvider Default
    {
      [DebuggerStepThrough]
      get => DefaultProvider;
    }

    /// <summary>
    /// Gets system comparer provider.
    /// A shortcut to <see cref="SystemComparerProvider.Instance"/>.
    /// </summary>
    public static SystemComparerProvider System
    {
      [DebuggerStepThrough]
      get => SystemProvider;
    }

    #region IComparerProvider Members

    /// <inheritdoc/>
    public virtual AdvancedComparer<T> GetComparer<T>()
      => GetAssociate<T, IAdvancedComparer<T>, AdvancedComparer<T>>();

    #endregion

    #region Protected method overrides

    /// <inheritdoc/>
    protected override TAssociate CreateAssociate<TKey, TAssociate>(out Type foundFor)
    {
      var associate = base.CreateAssociate<TKey, TAssociate>(out foundFor);
      if (associate != null) {
        return associate;
      }
      // Ok, null, but probably just because type cast has failed;
      // let's try to wrap it. TKey is type for which we're getting
      // the comparer.
      var comparer = base.CreateAssociate<TKey, IAdvancedComparerBase>(out foundFor);
      if (foundFor == null) {
        CoreLog.Warning(nameof(Strings.LogCantFindAssociateFor),
          TypeSuffixes.ToDelimitedString(" \\ "),
          typeof(TAssociate).GetShortName(),
          typeof(TKey).GetShortName());
        return null;
      }
      if (foundFor == typeof(TKey)) {
        return (TAssociate) comparer;
      }
      associate = BaseComparerWrapperType.Activate(new[] { typeof(TKey), foundFor }, ConstructorParams) as TAssociate;
      if (associate != null) {
        CoreLog.Warning(nameof(Strings.LogGenericAssociateIsUsedFor),
          BaseComparerWrapperType.GetShortName(),
          typeof (TKey).GetShortName(),
          foundFor.GetShortName(),
          typeof (TKey).GetShortName());
        return associate;
      }
      else {
        CoreLog.Warning(nameof(Strings.LogGenericAssociateCreationHasFailedFor),
          BaseComparerWrapperType.GetShortName(),
          typeof (TKey).GetShortName(),
          foundFor.GetShortName(),
          typeof (TKey).GetShortName());
        return null;
      }
    }

    /// <inheritdoc/>
    protected override TResult ConvertAssociate<TKey, TAssociate, TResult>(TAssociate associate)
    {
      return ReferenceEquals(associate, null)
        ? default(TResult)
        : (TResult) (object) new AdvancedComparer<TKey>((IAdvancedComparer<TKey>) associate);
    }

    #endregion


    // Constructors

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    protected ComparerProvider()
    {
      TypeSuffixes = new[] { "Comparer" };
      ConstructorParams = new object[] { this, ComparisonRules.Positive };
      AddHighPriorityLocation(BaseComparerWrapperType.Assembly, BaseComparerWrapperType.Namespace);
    }

    protected ComparerProvider(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}

// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.01.14

using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Reflection;
using Xtensive.Internals.DocTemplates;
using Xtensive.IoC;
using Xtensive.Resources;
using Xtensive.Threading;

//using Xtensive.SizeCalculators;

namespace Xtensive.Hashing
{
  /// <summary>
  /// Default <see cref="IHasher{T}"/> provider. 
  /// Provides default hasher for the specified type.
  /// </summary>
  /// <remarks>
  /// <para id="About"><see cref="HasStaticDefaultDocTemplate" copy="true" /></para>
  /// </remarks>
  [Serializable]
  public class HasherProvider : AssociateProvider,
    IHasherProvider
  {
    private static readonly HasherProvider @default = new HasherProvider();
    private ThreadSafeDictionary<Type, IHasherBase> hashers = 
      ThreadSafeDictionary<Type, IHasherBase>.Create(new object());
    private ThreadSafeCached<IHasherBase> objectHasher = 
      ThreadSafeCached<IHasherBase>.Create(new object());

    /// <see cref="HasStaticDefaultDocTemplate.Default" copy="true" />
    public static IHasherProvider Default
    {
      [DebuggerStepThrough]
      get { return @default; }
    }

    #region IHasherProvider members

    /// <summary>
    /// Gets <see cref="IHasher{T}"/> for the specified type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Type to get the hasher for.</typeparam>
    /// <returns><see cref="IHasher{T}"/> for the specified type <typeparamref name="T"/>.</returns>
    public virtual Hasher<T> GetHasher<T>()
    {
      return GetAssociate<T, IHasher<T>, Hasher<T>>();
    }

    /// <inheritdoc/>
    public IHasherBase GetHasherByInstance(object value)
    {
      if (value == null)
        return objectHasher.GetValue(
          _this => _this.GetHasher<object>().Implementation, 
          this);
      else
        return GetHasherByType(value.GetType());
    }

    /// <inheritdoc/>
    public IHasherBase GetHasherByType(Type type)
    {
      return hashers.GetValue(type,
        (_type, _this) => _this
          .GetType()
          .GetMethod("InnerGetHasherBase", 
            BindingFlags.Instance | 
            BindingFlags.NonPublic, 
            null, ArrayUtils<Type>.EmptyArray, null)
          .GetGenericMethodDefinition()
          .MakeGenericMethod(new[] {_type})
          .Invoke(_this, null)
          as IHasherBase,
        this);
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
      // the hasher.
      IHasherBase hasher = base.CreateAssociate<TKey, IHasherBase>(out foundFor);
      if (foundFor==null) {
        Log.Warning(Strings.LogCantFindAssociateFor,
          TypeSuffixes.ToDelimitedString(" \\ "),
          typeof (TAssociate).GetShortName(),
          typeof (TKey).GetShortName());
        return null;
      }
      if (foundFor==typeof (TKey))
        return (TAssociate) hasher;
      Type baseHasherWrapperType = typeof (BaseHasherWrapper<,>);
      associate =
        baseHasherWrapperType.Activate(new[] {typeof (TKey), foundFor}, ConstructorParams) as
          TAssociate;
      if (associate!=null) {
        Log.Warning(Strings.LogGenericAssociateIsUsedFor,
          baseHasherWrapperType.GetShortName(),
          typeof (TKey).GetShortName(),
          foundFor.GetShortName(),
          typeof (TKey).GetShortName());
        return associate;
      }
      Log.Warning(Strings.LogGenericAssociateCreationHasFailedFor,
        baseHasherWrapperType.GetShortName(),
        typeof (TKey).GetShortName(),
        foundFor.GetShortName(),
        typeof (TKey).GetShortName());
      return null;
    }

    /// <inheritdoc/>
    protected override TResult ConvertAssociate<TKey, TAssociate, TResult>(TAssociate associate)
    {
      if (ReferenceEquals(associate, null))
        return default(TResult);
      return (TResult) (object) new Hasher<TKey>((IHasher<TKey>) associate);
    }

    #endregion

    #region Private \ internal methods

    protected IHasherBase InnerGetHasherBase<T>()
    {
      var a = GetAssociate<T, IHasher<T>, Hasher<T>>();
      if (a!=null)
        return a.Implementation;
      else
        return null;
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    protected HasherProvider()
    {
      TypeSuffixes = new[] {"Hasher"};
      Type t = typeof (HasherProvider);
      AddHighPriorityLocation(t.Assembly, t.Namespace);
    }
  }
}
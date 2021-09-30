// Copyright (C) 2008-2021 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.18

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security;
using System.Text;
using System.Threading;
using Xtensive.Comparison;

using Xtensive.Reflection;

namespace Xtensive.Core
{
  /// <summary>
  /// Implements base functionality for associate provider.
  /// Creates and caches associates.
  /// </summary>
  [Serializable]
  public abstract class AssociateProvider :
    IDeserializationCallback,
    ISerializable
  {
    private static readonly AsyncLocal<HashSet<(Type, Type)>> inProgressAsync = new AsyncLocal<HashSet<(Type, Type)>>();

    private static HashSet<(Type, Type)> InProgress
    {
      get => inProgressAsync.Value ??= new HashSet<(Type, Type)>();
      set => inProgressAsync.Value = value;
    }

    [NonSerialized]
    private ConcurrentDictionary<(Type, Type), Lazy<object>> cache;

    private object[] constructorParams;
    private string[] typeSuffixes;

    [NonSerialized]
    private object highPriorityLocationsLock = new object();

    private List<Pair<Assembly, string>> highPriorityLocations = new List<Pair<Assembly, string>>();

    private List<Pair<Assembly, string>> HighPriorityLocations {
      get {
        lock (highPriorityLocationsLock) {
          return highPriorityLocations;
        }
      }
    }

    /// <summary>
    /// Gets associate constructor parameters.
    /// </summary>
    protected object[] ConstructorParams
    {
      [DebuggerStepThrough]
      get => constructorParams;
      [DebuggerStepThrough]
      set => constructorParams = value;
    }

    /// <summary>
    /// Gets or sets associate type suffixes.
    /// </summary>
    protected string[] TypeSuffixes
    {
      [DebuggerStepThrough]
      get => typeSuffixes;
      [DebuggerStepThrough]
      set => typeSuffixes = value;
    }

    /// <summary>
    /// Adds high priority location for associate search.
    /// </summary>
    /// <param name="assembly">Assembly to search in.</param>
    /// <param name="nameSpace">Namespace to search in.</param>
    public void AddHighPriorityLocation(Assembly assembly, string nameSpace)
    {
      AddHighPriorityLocation(assembly, nameSpace, false);
    }

    /// <summary>
    /// Adds high priority location for associate search.
    /// </summary>
    /// <param name="assembly">Assembly to search in.</param>
    /// <param name="nameSpace">Namespace to search in.</param>
    /// <param name="overriding">Indicates whether specified location should 
    /// override all the others (i.e. be a first in the list of locations).</param>
    protected void AddHighPriorityLocation(Assembly assembly, string nameSpace, bool overriding)
    {
      lock (highPriorityLocationsLock) {
        var newHighPriorityLocations = new List<Pair<Assembly, string>>(highPriorityLocations.Count + 1);
        var newLocation = new Pair<Assembly, string>(assembly, nameSpace);

        if (overriding) {
          newHighPriorityLocations.Add(newLocation);
          newHighPriorityLocations.AddRange(highPriorityLocations);
        }
        else {
          newHighPriorityLocations.AddRange(highPriorityLocations);
          newHighPriorityLocations.Add(newLocation);
        }
        highPriorityLocations = newHighPriorityLocations;
      }
    }

    /// <summary>
    /// Gets associate instance for specified parameter and result types.
    /// All associate instances are cached and returned on the same calls further.
    /// </summary>
    /// <typeparam name="TKey">Type to provide the associate for.</typeparam>
    /// <typeparam name="TAssociate">Type of result to provide the associate for.</typeparam>
    /// <typeparam name="TResult">The type of result.</typeparam>
    /// <returns>Associate instance, if found;
    /// otherwise, <see langword="null"/>.</returns>
    protected TResult GetAssociate<TKey, TAssociate, TResult>()
      where TAssociate : class
    {
      var key = (typeof(TKey), typeof(TResult));
      return (TResult) cache.GetOrAdd(key,
        _key => new Lazy<object>(
          () => ConvertAssociate<TKey, TAssociate, TResult>(CreateAssociate<TKey, TAssociate>(out var _)))).Value;
    }

    /// <summary>
    /// Gets associate instance for specified parameters and result types.
    /// All associate created instances are cached and returned on the same calls further.
    /// </summary>
    /// <typeparam name="TKey1">First type to try to provide the associate for.</typeparam>
    /// <typeparam name="TKey2">Second type to try to provide the associate for.</typeparam>
    /// <typeparam name="TAssociate">Type of result to provide the associate for.</typeparam>
    /// <typeparam name="TResult">The type of result.</typeparam>
    /// <returns>Associate instance, if found;
    /// otherwise, <see langword="null"/>.</returns>
    /// <exception cref="InvalidOperationException"><c>InvalidOperationException</c>.</exception>
    protected TResult GetAssociate<TKey1, TKey2, TAssociate, TResult>()
      where TAssociate : class
    {
      var key = (typeof(TKey1), typeof(TResult));
      return (TResult) cache.GetOrAdd(key, _key => new Lazy<object>(() => {
        var associate1 = CreateAssociate<TKey1, TAssociate>(out var foundFor);
        var associate2 = CreateAssociate<TKey2, TAssociate>(out foundFor);
        // Preferring non-null ;)
        TAssociate associate = null;
        if (associate1 == null) {
          associate = associate2;
        }
        else if (associate2 == null) {
          associate = associate1;
        }
        else {
          // Both are non-null; preferring one of two
          associate = PreferAssociate<TKey1, TKey2, TAssociate>(associate1, associate2);
        }
        if (associate == null) {
          // Try to get complex associate (create it manually)
          associate = CreateCustomAssociate<TKey1, TKey2, TAssociate>();
          if (associate == null) {
            var stringBuilder = new StringBuilder();
            for (var i = 0; i < TypeSuffixes.Length; i++) {
              if (i != 0) {
                _ = stringBuilder.Append(", ");
              }
              _ = stringBuilder.Append(TypeSuffixes[i]);
            }
            throw new InvalidOperationException(string.Format(
              Strings.ExCantFindAssociate2, stringBuilder,
              typeof(TAssociate).GetShortName(),
              typeof(TKey1).GetShortName(),
              typeof(TKey2).GetShortName()));
          }
        }
        return ConvertAssociate<TKey1, TKey2, TAssociate, TResult>(associate);
      })).Value;
    }

    /// <summary>
    /// Chooses preferred associate instance from two associates.
    /// </summary>
    /// <param name="associate1">First choice option.</param>
    /// <param name="associate2">Second choice option.</param>
    /// <typeparam name="TKey1">First associate key type.</typeparam>
    /// <typeparam name="TKey2">Second associate key type.</typeparam>
    /// <typeparam name="TAssociate">Type of associate to choose.</typeparam>
    /// <returns>Preferred associate instance.</returns>
    protected virtual TAssociate PreferAssociate<TKey1, TKey2, TAssociate>(TAssociate associate1,
      TAssociate associate2)
    {
      var locationPosition1 = GetAssociateLocationPosition(associate1);
      var locationPosition2 = GetAssociateLocationPosition(associate2);
      return locationPosition1 <= locationPosition2 ? associate1 : associate2;
    }

    /// <summary>
    /// Gets the position of specified associate in <see cref="HighPriorityLocations"/>
    /// list.
    /// </summary>
    /// <param name="associate">Associate to get the position for.</param>
    /// <typeparam name="TAssociate">Type of associate.</typeparam>
    /// <returns>Associate position. <see cref="int.MaxValue"/>, if its
    /// location isn't listed in <see cref="HighPriorityLocations"/> list.</returns>
    protected int GetAssociateLocationPosition<TAssociate>(TAssociate associate)
    {
      var entry = new Pair<Assembly, string>(
        associate.GetType().Assembly,
        associate.GetType().Namespace);
      var hpl = HighPriorityLocations;
      for (var i = 0; i < hpl.Count; i++) {
        if (AdvancedComparerStruct<Pair<Assembly, string>>.Default.Equals(hpl[i], entry)) {
          return i;
        }
      }
      return int.MaxValue;
    }

    /// <summary>
    /// Creates associate instance for specified parameter and result types.
    /// </summary>
    /// <typeparam name="TKey">Type to create the associate for.</typeparam>
    /// <typeparam name="TAssociate">Type of result to create the associate for.</typeparam>
    /// <param name="foundFor">The type associate was found for.</param>
    /// <returns>Newly created instance of associate, if found;
    /// otherwise, <see langword="null"/>.</returns>
    /// <exception cref="InvalidOperationException">Recursive associate lookup.</exception>
    protected virtual TAssociate CreateAssociate<TKey, TAssociate>(out Type foundFor)
      where TAssociate : class
    {
      var progressionMark = (typeof(TKey), typeof(TAssociate));
      if (!InProgress.Add(progressionMark)) {
        throw new InvalidOperationException(Strings.ExRecursiveAssociateLookupDetected);
      }
      try {
        return TypeHelper.CreateAssociate<TAssociate>(
          typeof(TKey), out foundFor, TypeSuffixes, constructorParams, HighPriorityLocations);
      }
      finally {
        _ = InProgress.Remove(progressionMark);
      }
    }

    /// <summary>
    /// Creates associate by some custom way if no standard associate is found. 
    /// Override it to create the associate manually.
    /// </summary>
    /// <typeparam name="TKey1">First type to try to provide the associate for.</typeparam>
    /// <typeparam name="TKey2">Second type to try to provide the associate for.</typeparam>
    /// <typeparam name="TAssociate">Type of result to provide the associate for.</typeparam>
    /// <returns>Associate instance or <see langword="null"/>.</returns>
    protected virtual TAssociate CreateCustomAssociate<TKey1, TKey2, TAssociate>()
      where TAssociate : class
      => null;

    /// <summary>
    /// Converts <paramref name="associate"/> to <typeparamref name="TResult"/> object.
    /// </summary>
    /// <typeparam name="TKey">The type of key.</typeparam>
    /// <typeparam name="TAssociate">The type of associate.</typeparam>
    /// <typeparam name="TResult">The type of result.</typeparam>
    /// <param name="associate">Associate to convert to result.</param>
    /// <returns>Conversion result.</returns>
    protected virtual TResult ConvertAssociate<TKey, TAssociate, TResult>(TAssociate associate)
      => (TResult) (object) associate;

    /// <summary>
    /// Converts <paramref name="associate"/> to <typeparamref name="TResult"/> object.
    /// </summary>
    /// <typeparam name="TKey1">The type of key 1.</typeparam>
    /// <typeparam name="TKey2">The type of key 2.</typeparam>
    /// <typeparam name="TAssociate">The type of associate.</typeparam>
    /// <typeparam name="TResult">The type of result.</typeparam>
    /// <param name="associate">Associate to convert to result.</param>
    /// <returns>Conversion result.</returns>
    protected virtual TResult ConvertAssociate<TKey1, TKey2, TAssociate, TResult>(TAssociate associate)
      => (TResult) (object) associate;


    // Constructors

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    protected AssociateProvider()
    {
      constructorParams = new object[] { this };
      cache = new ConcurrentDictionary<(Type, Type), Lazy<object>>();
    }

    protected AssociateProvider(SerializationInfo info, StreamingContext context)
    {
      if (info == null) {
        throw new ArgumentNullException(nameof(info));
      }

      var constructorParamsExceptThis = (object[]) info.GetValue(nameof(constructorParams), typeof(object[]));
      constructorParams = new object[constructorParamsExceptThis.Length + 1];
      constructorParams[0] = this;
      Array.Copy(constructorParamsExceptThis, 0, constructorParams, 1, constructorParamsExceptThis.Length);

      typeSuffixes = (string[]) info.GetValue(nameof(typeSuffixes), typeof(string[]));

      var highPriorityLocationsSerializable = (List<Pair<string, string>>) info.GetValue(nameof(highPriorityLocations), typeof(List<Pair<string, string>>));
      highPriorityLocations = highPriorityLocationsSerializable.SelectToList(ls => new Pair<Assembly, string>(Assembly.Load(ls.First), ls.Second));
    }

    /// <summary>
    /// Performs post-deserialization actions.
    /// </summary>
    /// <param name="sender"></param>
    public virtual void OnDeserialization(object sender)
    {
      cache = new ConcurrentDictionary<(Type, Type), Lazy<object>>();
    }

    /// <inheritdoc/>
    [SecurityCritical]
    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      object[] constructorParamsExceptThis = null;
      // need to exclude this form parameters to prevent loop
      if (constructorParams.Length == 1) {
        constructorParamsExceptThis = Array.Empty<object>();
      }
      else {
        constructorParamsExceptThis = new object[constructorParams.Length - 1];
        Array.Copy(constructorParams, 1, constructorParamsExceptThis, 0, constructorParamsExceptThis.Length);
      }
      info.AddValue(nameof(constructorParams), constructorParamsExceptThis, constructorParams.GetType());
      info.AddValue(nameof(typeSuffixes), typeSuffixes, typeSuffixes.GetType());

      var highPriorityLocationsSerializable = HighPriorityLocations.SelectToList(l => new Pair<string, string>(l.First.FullName, l.Second));
      info.AddValue(nameof(highPriorityLocations), highPriorityLocationsSerializable, highPriorityLocationsSerializable.GetType());
    }
  }
}
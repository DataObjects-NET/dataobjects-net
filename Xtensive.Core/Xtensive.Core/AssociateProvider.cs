// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.18

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Xtensive.Core.Collections;
using Xtensive.Core.Comparison;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Reflection;
using Xtensive.Core.Resources;

namespace Xtensive.Core
{
  /// <summary>
  /// Implements base functionality for associate provider.
  /// Creates and caches associates.
  /// </summary>
  [Serializable]
  public abstract class AssociateProvider :
    IDeserializationCallback
  {
    [NonSerialized]
    private static readonly object _lock = new object();
    [NonSerialized]
    private static Cached<AdvancedComparer<Pair<Assembly, string>>> positionComparer;
    [NonSerialized]
    private TypeBasedDictionary cache;
    [ThreadStatic]
    private static ISet<KeyValuePair<Type, Type>> inProgress = new SetSlim<KeyValuePair<Type, Type>>();

    private object[] constructorParams;
    private string[] typeSuffixes;
    private List<Pair<Assembly, string>> highPriorityLocations = new List<Pair<Assembly, string>>();

    /// <summary>
    /// Gets associate constructor parameters.
    /// </summary>
    protected object[] ConstructorParams
    {
      get { return constructorParams; }
      set { constructorParams = value; }
    }

    /// <summary>
    /// Gets or sets associate type suffixes.
    /// </summary>
    protected string[] TypeSuffixes { 
      get { return typeSuffixes; }
      set { typeSuffixes = value; }
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
      lock (_lock) {
        List<Pair<Assembly, string>> newHighPriorityLocations = new List<Pair<Assembly, string>>(highPriorityLocations);
        if (overriding)
          newHighPriorityLocations.Insert(0, new Pair<Assembly, string>(assembly, nameSpace));
        else
          newHighPriorityLocations.Add(new Pair<Assembly, string>(assembly, nameSpace));
        highPriorityLocations = newHighPriorityLocations;
      }
    }

    /// <summary>
    /// Gets a list of high priority locations.
    /// </summary>
    protected List<Pair<Assembly, string>> HighPriorityLocations
    {
      get { return highPriorityLocations; }
    }

    /// <summary>
    /// Gets associate instance for specified parameter and result types.
    /// All associate created instances are cached and returned on the same calls further.
    /// </summary>
    /// <typeparam name="TKey">Type to provide the associate for.</typeparam>
    /// <typeparam name="TAssociate">Type of result to provide the associate for.</typeparam>
    /// <typeparam name="TResult">The type of result.</typeparam>
    /// <returns>Associate instance, if found;
    /// otherwise, <see langword="null"/>.</returns>
    protected TResult GetAssociate<TKey, TAssociate, TResult>()
      where TAssociate : class
    {
      TResult result = cache.GetValue<TKey, TResult>();
      if (ReferenceEquals(result, default(TResult)))
        lock (_lock) {
          result = cache.GetValue<TKey, TResult>();
          if (!ReferenceEquals(result, default(TResult)))
            return result;
          Type foundFor;
          result = ConvertAssociate<TKey, TAssociate, TResult>(CreateAssociate<TKey, TAssociate>(out foundFor));
          cache.SetValue<TKey, TResult>(result);
        }
      return result;
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
      TResult result = cache.GetValue<TKey1, TResult>();
      if (ReferenceEquals(result, default(TResult)))
        lock (_lock) {
          result = cache.GetValue<TKey1, TResult>();
          if (!ReferenceEquals(result, default(TResult)))
            return result;
          Type foundFor;
          TAssociate associate1 = CreateAssociate<TKey1, TAssociate>(out foundFor);
          TAssociate associate2 = CreateAssociate<TKey2, TAssociate>(out foundFor);
          // Preferring non-null ;)
          TAssociate associate;
          if (associate1==null)
            associate = associate2;
          else if (associate2==null)
            associate = associate1;
          else
            // Both are non-null; preferring one of two
            associate = PreferAssociate<TKey1, TKey2, TAssociate>(associate1, associate2);
          if (associate==null) {
            // Try to get complex associate (create it manually).
            associate = CreateCustomAssociate<TKey1, TKey2, TAssociate>();
            if (associate==null) {
              StringBuilder stringBuilder = new StringBuilder();
              for (int i = 0; i < TypeSuffixes.Length; i++) {
                if (i!=0) {
                  stringBuilder.Append(", ");
                }
                stringBuilder.Append(TypeSuffixes[i]);
              }
              throw new InvalidOperationException(String.Format(
                Strings.ExCantFindAssociate2, stringBuilder,
                typeof (TAssociate).GetShortName(),
                typeof (TKey1).GetShortName(),
                typeof (TKey2).GetShortName()));
            }
          }
          result = ConvertAssociate<TKey1, TKey2, TAssociate, TResult>(associate);
          cache.SetValue<TKey1, TResult>(result);
        }
      return result;
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
      int locationPosition1 = GetAssociateLocationPosition(associate1);
      int locationPosition2 = GetAssociateLocationPosition(associate2);
      if (locationPosition1 <= locationPosition2)
        return associate1;
      else
        return associate2;
    }

    /// <summary>
    /// Gets the position of specified associate in <see cref="HighPriorityLocations"/>
    /// list.
    /// </summary>
    /// <param name="associate">Associate to get the position for.</param>
    /// <typeparam name="TAssociate">Type of associate.</typeparam>
    /// <returns>Associate position. <see cref="Int32.MaxValue"/>, if its
    /// location isn't listed in <see cref="HighPriorityLocations"/> list.</returns>
    protected int GetAssociateLocationPosition<TAssociate>(TAssociate associate)
    {
      Pair<Assembly, string> entry = new Pair<Assembly, string>(
        associate.GetType().Assembly,
        associate.GetType().Namespace);
      List<Pair<Assembly, string>> hpl = HighPriorityLocations;
      for (int i = 0; i < hpl.Count; i++)
        if (AdvancedComparerStruct<Pair<Assembly, string>>.Default.Equals(hpl[i], entry))
          return i;
      return Int32.MaxValue;
    }

    /// <summary>
    /// Creates associate instance for specified parameter and result types.
    /// </summary>
    /// <typeparam name="TKey">Type to create the associate for.</typeparam>
    /// <typeparam name="TAssociate">Type of result to create the associate for.</typeparam>
    /// <param name="foundFor">The type associate was found for.</param>
    /// <returns>Newly created instance of associate, if found;
    /// otherwise, <see langword="null"/>.</returns>
    protected virtual TAssociate CreateAssociate<TKey, TAssociate>(out Type foundFor)
      where TAssociate : class
    {
      if (inProgress == null)
        inProgress = new SetSlim<KeyValuePair<Type, Type>>();
      KeyValuePair<Type, Type> progressionMark = new KeyValuePair<Type, Type>(typeof (TKey), typeof (TAssociate));
      if (inProgress.Contains(progressionMark))
        throw new InvalidOperationException(Strings.ExRecursiveAssociateLookupDetected);
      inProgress.Add(progressionMark);
      try {
        return TypeHelper.CreateAssociate<TAssociate>(
          typeof (TKey), out foundFor, TypeSuffixes, constructorParams, highPriorityLocations);
      }
      finally {
        inProgress.Remove(progressionMark);
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
    {
      return null;
    }

    /// <summary>
    /// Converts <paramref name="associate"/> to <typeparamref name="TResult"/> object.
    /// </summary>
    /// <typeparam name="TKey">The type of key.</typeparam>
    /// <typeparam name="TAssociate">The type of associate.</typeparam>
    /// <typeparam name="TResult">The type of result.</typeparam>
    /// <param name="associate">Associate to convert to result.</param>
    /// <returns>Conversion result.</returns>
    protected virtual TResult ConvertAssociate<TKey, TAssociate, TResult>(TAssociate associate)
    {
      return (TResult) (object) associate;
    }

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
    {
      return (TResult) (object) associate;
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    protected AssociateProvider()
    {
      cache = TypeBasedDictionary.Create();
      constructorParams = new object[] {this};
    }

    /// <see cref="SerializableDocTemplate.OnDeserialization"/>
    public virtual void OnDeserialization(object sender)
    {
      cache = TypeBasedDictionary.Create();
    }
  }
}
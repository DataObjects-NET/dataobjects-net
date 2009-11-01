// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2007.05.30

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Comparison;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Reflection;
using Xtensive.Core.Resources;
using Xtensive.Core.Tuples.Internals;
using System.Linq;

namespace Xtensive.Core.Tuples
{
  /// <summary>
  /// Tuple descriptor.
  /// Provides information about <see cref="ITuple"/> structure.
  /// </summary>
  [Serializable]
  public class TupleDescriptor : 
    IIdentified<int>,
    IEquatable<TupleDescriptor>,
    IComparable<TupleDescriptor>,
    ICountable<Type>,
    IList<Type>
  {
    private static int totalCount;
    private int identifier;
    private readonly int fieldCount;
    internal readonly Type[] fieldTypes;
    private ITupleFactory tupleFactory;
    private bool isCompiled;
    private int cachedHashCode;

    /// <summary>
    /// Gets the empty tuple descriptor.
    /// </summary>
    /// <value>The empty tuple descriptor.</value>
    public static TupleDescriptor Empty
    {
      get { return EmptyTupleDescriptor.Instance; }
    }

    /// <summary>
    /// Gets total count of compiled descriptors.
    /// </summary>
    public static int TotalCount
    {
      get { return totalCount; }
    }

    /// <inheritdoc/>
    public int Identifier
    {
      get { return identifier; }
    }

    /// <inheritdoc/>
    object IIdentified.Identifier
    {
      get { return identifier; }
    }

    /// <summary>
    /// Indicates whether class for handling underlying 
    /// <see cref="ITuple"/> is already compiled.
    /// </summary>
    public bool IsCompiled
    {
      get { return isCompiled; }
    }

    /// <summary>
    /// Gets the type of underlying <see cref="ITuple"/>
    /// implementation. <see langword="Null"/>, if
    /// <see cref="IsCompiled"/>==<see langword="false"/>.
    /// </summary>
    public Type TupleType {
      get {
        if (!isCompiled)
          return null;
        else
          return TupleFactory.GetType();
      }
    }

    #region Execute methods

    /// <summary>
    /// Executes the <see cref="ITupleActionHandler{TActionData}.Execute{TFieldType}"/> method
    /// of the <paramref name="actionHandler"/> for the field with specified <paramref name="fieldIndex"/>.
    /// </summary>
    /// <param name="actionHandler">Action handler to execute the <see cref="ITupleActionHandler{TActionData}.Execute{TFieldType}"/> method of.</param>
    /// <param name="actionData">Action data to pass.</param>
    /// <param name="fieldIndex">Field to execute the <see cref="ITupleActionHandler{TActionData}.Execute{TFieldType}"/> method for.</param>
    /// <returns>Execute method result.</returns>
    /// <typeparam name="TActionData">The type of action data to pass to the <see cref="ITupleActionHandler{TActionData}.Execute{TFieldType}"/> method.</typeparam>
    public virtual bool Execute<TActionData>(
      ITupleActionHandler<TActionData> actionHandler, ref TActionData actionData, int fieldIndex) 
      where TActionData : struct
    {
      throw Exceptions.InternalError(
        "TupleDescriptor.Execute is invoked right on TupleDescriptor!", Log.Instance);
    }

    /// <summary>
    /// Executes the <see cref="ITupleActionHandler{TActionData}.Execute{TFieldType}"/> method
    /// of the <paramref name="actionHandler"/> on all the fields of the descriptor in specified <paramref name="direction"/>.
    /// </summary>
    /// <param name="actionHandler">Action handler to execute the <see cref="ITupleActionHandler{TActionData}.Execute{TFieldType}"/> method of.</param>
    /// <param name="actionData">Action data to pass through all the execution sequence.</param>
    /// <param name="direction">Field processing direction.</param>
    /// <typeparam name="TActionData">The type of action data to pass to the <see cref="ITupleActionHandler{TActionData}.Execute{TFieldType}"/> method.</typeparam>
    public virtual void Execute<TActionData>(
      ITupleActionHandler<TActionData> actionHandler, ref TActionData actionData, Direction direction) 
      where TActionData : struct
    {
      return;
    }

    /// <summary>
    /// Executes the <see cref="ITupleActionHandler{TActionData}.Execute{TFieldType}"/> method
    /// of the <paramref name="functionHandler"/> on all the fields of the descriptor in specified <paramref name="direction"/>.
    /// </summary>
    /// <param name="functionHandler">Function handler to execute the <see cref="ITupleActionHandler{TActionData}.Execute{TFieldType}"/> method of.</param>
    /// <param name="functionData">Function data to pass through all the execution sequence.</param>
    /// <param name="direction">Field processing direction.</param>
    /// <returns>The value of <see cref="ITupleFunctionData{TResult}.Result"/> property on completion of
    /// execution sequence.</returns>
    /// <typeparam name="TFunctionData">The type of function data to pass to the <see cref="ITupleActionHandler{TActionData}.Execute{TFieldType}"/> method.</typeparam>
    /// <typeparam name="TResult">The type of result.</typeparam>
    public virtual TResult Execute<TFunctionData, TResult>(
      ITupleFunctionHandler<TFunctionData, TResult> functionHandler, ref TFunctionData functionData, Direction direction) 
      where TFunctionData : struct, ITupleFunctionData<TResult>
    {
      return functionData.Result;
    }

    #endregion

    #region Compilation related members

    /// <summary>
    /// Ensures the descriptor is compiled.
    /// </summary>
    protected internal void EnsureIsCompiled()
    {
      if (!isCompiled)
        TupleDescriptorCache.Compile();
      Debug.Assert(isCompiled, "TupleDescriptor should have IsCompiled flag set to true after the compilation!");
    }

    internal void Compiled(ITupleFactory factory)
    {
      identifier = ++totalCount;
      tupleFactory = factory;
      isCompiled = true;
    }

    #endregion

    #region ICountable, IList members

    /// <inheritdoc/>
    public Type this[int fieldIndex] {
      get {
        return fieldTypes[fieldIndex];
      }
      set {
        throw Exceptions.CollectionIsReadOnly(null);
      }
    }

    /// <inheritdoc/>
    public int Count {
      get { return fieldCount; }
    }

    /// <inheritdoc/>
    long ICountable.Count
    {
      get { return fieldCount; }
    }

    /// <inheritdoc/>
    public int IndexOf(Type item)
    {
      return fieldTypes.IndexOf(item, true);
    }

    /// <inheritdoc/>
    public void Insert(int index, Type item)
    {
      throw Exceptions.CollectionIsReadOnly(null);
    }

    /// <inheritdoc/>
    public void RemoveAt(int index)
    {
      throw Exceptions.CollectionIsReadOnly(null);
    }

    /// <inheritdoc/>
    public void Add(Type item)
    {
      throw Exceptions.CollectionIsReadOnly(null);
    }

    /// <inheritdoc/>
    public void Clear()
    {
      throw Exceptions.CollectionIsReadOnly(null);
    }

    /// <inheritdoc/>
    public bool Contains(Type item)
    {
      return fieldTypes.IndexOf(item, true)>=0;
    }

    /// <inheritdoc/>
    public void CopyTo(Type[] array, int arrayIndex)
    {
      fieldTypes.Copy(array, arrayIndex);
    }

    /// <inheritdoc/>
    public bool Remove(Type item)
    {
      throw Exceptions.CollectionIsReadOnly(null);
    }

    /// <inheritdoc/>
    public bool IsReadOnly
    {
      get { return true; }
    }

    /// <inheritdoc/>
    public IEnumerator<Type> GetEnumerator()
    {
      for (int i = 0; i<fieldCount; i++)
        yield return fieldTypes[i];
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion

    #region IEquatable members, GetHashCode 

    /// <inheritdoc/>
    public bool Equals(TupleDescriptor other)
    {
      if (other==null) return false;
      if (other==this) return true;
      if (isCompiled && other.isCompiled)
        return false;
      return fieldCount==other.fieldCount && 
        AdvancedComparer<Type[]>.Default.Equals(fieldTypes, other.fieldTypes);
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(this, obj)) return true;
      return Equals(obj as TupleDescriptor);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      if (cachedHashCode==0) {
        int hashCode = fieldCount;
        for (int i = 0; i < fieldCount; i++)
          hashCode = unchecked (fieldTypes[i].GetHashCode() + 29*hashCode);
        if (hashCode==0)
          hashCode = -1;
        cachedHashCode = hashCode;
      }
      return cachedHashCode;
    }

    #endregion

    #region IComparable members

    /// <inheritdoc/>
    public int CompareTo(TupleDescriptor other)
    {
      if (other==null) return 1;
      if (other==this) return 0;
      return AdvancedComparerStruct<Type[]>.Default.Compare(fieldTypes, other.fieldTypes);
    }

    #endregion

    #region Private \ internal methods

    internal ITupleFactory TupleFactory
    {
      get { return tupleFactory; }
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      for (int i = 0; i<fieldCount; i++) {
        if (i>0)
          sb.Append(", ");
        sb.Append(fieldTypes[i].GetShortName());
      }
      return String.Format(Strings.TupleDescriptorFormat, sb);
    }

    #region Create methods (base)

    /// <summary>
    /// Creates or returns already created descriptor
    /// for provided set of types.
    /// </summary>
    /// <param name="fieldTypes">List of tuple field types.</param>
    /// <returns>Either new or existing tuple descriptor
    /// describing the specified set of fields.</returns>
    public static TupleDescriptor Create(Type[] fieldTypes)
    {
      ArgumentValidator.EnsureArgumentNotNull(fieldTypes, "fieldTypes");
      TupleDescriptor descriptor = new TupleDescriptor(fieldTypes);
      return TupleDescriptorCache.Register(descriptor);
    }

    /// <summary>
    /// Creates or returns already created descriptor
    /// for provided set of types.
    /// </summary>
    /// <param name="fieldTypes">List of tuple field types.</param>
    /// <returns>Either new or existing tuple descriptor
    /// describing the specified set of fields.</returns>
    public static TupleDescriptor Create(IList<Type> fieldTypes)
    {
      ArgumentValidator.EnsureArgumentNotNull(fieldTypes, "fieldTypes");
      TupleDescriptor descriptor = new TupleDescriptor(fieldTypes);
      return TupleDescriptorCache.Register(descriptor);
    }

    /// <summary>
    /// Creates or returns already created descriptor
    /// for provided set of types.
    /// </summary>
    /// <param name="fieldTypes">Enumerable of tuple field types.</param>
    /// <returns>Either new or existing tuple descriptor
    /// describing the specified set of fields.</returns>
    public static TupleDescriptor Create(IEnumerable<Type> fieldTypes)
    {
      return Create(fieldTypes.ToArray());
    }

    /// <summary>
    /// Returns an instance of existing descriptor of specified
    /// <see cref="GeneratedTupleDescriptor"/> type.
    /// </summary>
    /// <returns>Instance of existing tuple descriptor
    /// of specified type.</returns>
    public static TupleDescriptor Create(Type descriptorType)
    {
      Type baseType = typeof (GeneratedTupleDescriptor);
      if (!baseType.IsAssignableFrom(descriptorType))
        throw new ArgumentException(
          Strings.ExSpecifiedTypeShouldBeGeneratedTupleDescriptorOrItsDescendant, "descriptorType");
      if (descriptorType==typeof(EmptyTupleDescriptor))
        return Create(ArrayUtils<Type>.EmptyArray);
      else {
        if (!descriptorType.IsGenericType)
          throw new ArgumentException(
            Strings.ExSpecifiedTypeShouldBeGeneratedTupleDescriptorOrItsDescendant, "descriptorType");
        Type[] descriptorGenericArgs = descriptorType.GetGenericArguments();
        return Create(descriptorGenericArgs);
      }
    }

    #endregion

    #region Create<...> methods (generic shortcuts)

    /// <summary>
    /// Creates new <see cref="TupleDescriptor"/> by its field type(s).
    /// </summary>
    /// <typeparam name="T">Type of the only tuple field.</typeparam>
    /// <returns>Newly created <see cref="TupleDescriptor"/> object.</returns>
    public static TupleDescriptor Create<T>()
    {
      return Create(new[] {
        typeof(T)
      });
    }

    /// <summary>
    /// Creates new <see cref="TupleDescriptor"/> by its field type(s).
    /// </summary>
    /// <typeparam name="T1">Type of the first tuple field.</typeparam>
    /// <typeparam name="T2">Type of the 2nd tuple field.</typeparam>
    /// <returns>Newly created <see cref="TupleDescriptor"/> object</returns>
    public static TupleDescriptor Create<T1,T2>()
    {
      return Create(new[] {
        typeof(T1),
        typeof(T2)
      });
    }

    /// <summary>
    /// Creates new <see cref="TupleDescriptor"/> by its field type(s).
    /// </summary>
    /// <typeparam name="T1">Type of the first tuple field.</typeparam>
    /// <typeparam name="T2">Type of the 2nd tuple field.</typeparam>
    /// <typeparam name="T3">Type of the 3rd tuple field.</typeparam>
    /// <returns>Newly created <see cref="TupleDescriptor"/> object</returns>
    public static TupleDescriptor Create<T1,T2,T3>()
    {
      return Create(new[] {
        typeof(T1),
        typeof(T2),
        typeof(T3)
      });
    }

    /// <summary>
    /// Creates new <see cref="TupleDescriptor"/> by its field type(s).
    /// </summary>
    /// <typeparam name="T1">Type of the first tuple field.</typeparam>
    /// <typeparam name="T2">Type of the 2nd tuple field.</typeparam>
    /// <typeparam name="T3">Type of the 3rd tuple field.</typeparam>
    /// <typeparam name="T4">Type of the 4th tuple field.</typeparam>
    /// <returns>Newly created <see cref="TupleDescriptor"/> object</returns>
    public static TupleDescriptor Create<T1,T2,T3,T4>()
    {
      return Create(new[] {
        typeof(T1),
        typeof(T2),
        typeof(T3),
        typeof(T4)
      });
    }

    /// <summary>
    /// Creates new <see cref="TupleDescriptor"/> by its field type(s).
    /// </summary>
    /// <typeparam name="T1">Type of the first tuple field.</typeparam>
    /// <typeparam name="T2">Type of the 2nd tuple field.</typeparam>
    /// <typeparam name="T3">Type of the 3rd tuple field.</typeparam>
    /// <typeparam name="T4">Type of the 4th tuple field.</typeparam>
    /// <typeparam name="T5">Type of the 5th tuple field.</typeparam>
    /// <returns>Newly created <see cref="TupleDescriptor"/> object</returns>
    public static TupleDescriptor Create<T1,T2,T3,T4,T5>()
    {
      return Create(new[] {
        typeof(T1),
        typeof(T2),
        typeof(T3),
        typeof(T4),
        typeof(T5)
      });
    }

    /// <summary>
    /// Creates new <see cref="TupleDescriptor"/> by its field type(s).
    /// </summary>
    /// <typeparam name="T1">Type of the first tuple field.</typeparam>
    /// <typeparam name="T2">Type of the 2nd tuple field.</typeparam>
    /// <typeparam name="T3">Type of the 3rd tuple field.</typeparam>
    /// <typeparam name="T4">Type of the 4th tuple field.</typeparam>
    /// <typeparam name="T5">Type of the 5th tuple field.</typeparam>
    /// <typeparam name="T6">Type of the 6th tuple field.</typeparam>
    /// <returns>Newly created <see cref="TupleDescriptor"/> object</returns>
    public static TupleDescriptor Create<T1,T2,T3,T4,T5,T6>()
    {
      return Create(new[] {
        typeof(T1),
        typeof(T2),
        typeof(T3),
        typeof(T4),
        typeof(T5),
        typeof(T6)
      });
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="fieldTypes">The types of <see cref="TupleDescriptor"/> fields.</param>
    protected TupleDescriptor(IList<Type> fieldTypes)
    {
      ArgumentValidator.EnsureArgumentNotNull(fieldTypes, "fieldTypes");
      fieldCount = fieldTypes.Count;
      Type[] copy = new Type[fieldCount];
      for (int i = 0; i<fieldCount; i++) {
        Type t = fieldTypes[i];
        if (t.IsNullable())
          t = t.GetGenericArguments()[0]; // Substituting Nullable<T> to T
        copy[i] = t;
      }
      this.fieldTypes = copy;
    }

//    private TupleDescriptor(Type[] fieldTypes)
//    {
//      ArgumentValidator.EnsureArgumentNotNull(fieldTypes, "fieldTypes");
//      Type[] copy = new Type[fieldTypes.Length];
//      for (int i = 0; i<fieldTypes.Length; i++) {
//        Type t = fieldTypes[i];
//        if (t.IsNullable())
//          t = t.GetGenericArguments()[0]; // Substituting Nullable<T> to T
//        copy[i] = t;
//      }
//      this.fieldTypes = copy;
//    }
  }
}

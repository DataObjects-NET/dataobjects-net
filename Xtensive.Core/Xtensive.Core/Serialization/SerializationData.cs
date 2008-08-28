// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.19

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using Xtensive.Core.Collections;
using Xtensive.Core.Comparison;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Resources;
using Xtensive.Core.Reflection;
using System.Linq;
using Xtensive.Core.Serialization.Implementation;

namespace Xtensive.Core.Serialization
{
  /// <summary>
  /// Provides high-level access to the serializing or deserializing data.
  /// </summary>
  [DebuggerDisplay("{ToString()}")]
  public abstract class SerializationData : IEnumerable<string>
  {
    protected const string TypePropertyName = "GetType()";
    protected const string ReferencePropertyName = "#";

    #region Properties

    /// <summary>
    /// Gets the serializer this instance is bound to.
    /// </summary>
    public static WorkingSerializerBase Serializer {
      [DebuggerStepThrough]
      get { return SerializationContext.Current.Serializer; }
    }

    /// <summary>
    /// Gets the type associated with this instance.
    /// </summary>
    public Type Type { get; protected set; }

    /// <summary>
    /// Gets the reference associated with this instance.
    /// </summary>
    public IReference Reference { get; protected set; }

    /// <summary>
    /// Gets the source object associated with this instance.
    /// </summary>
    public object Source { get; internal set; }

    /// <summary>
    /// Gets the origin object associated with this instance.
    /// </summary>
    public object Origin { get; internal set; }

    /// <summary>
    /// Gets the count of slots in this instance.
    /// </summary>
    public abstract int Count { get; }

    /// <summary>
    /// Gets the count of slots fetched by <see cref="GetValue{T}"/>-like methods.
    /// </summary>
    public abstract int ReadCount { get; }

    /// <summary>
    /// Gets the count of skipped slots (the slots that aren't 
    /// fetched by <see cref="GetValue{T}"/>-like methods).
    /// </summary>
    public int SkipCount {
      get { return Count-ReadCount; }
    }

    /// <summary>
    /// Gets or sets the serialized type of the object (<see cref="Source"/>) described by this instance.
    /// </summary>
    public virtual Type SerializedType {
      get {
        Type = GetValue<Token<Type>>(TypePropertyName).Value;
        return Type;
      }
      set {
        Type = value;
        AddValue(TypePropertyName, Token.GetOrCreate(value)); 
      }
    }

    /// <summary>
    /// Gets or sets the serialized <see cref="Reference"/> value to the object (<see cref="Source"/>) described by this instance.
    /// </summary>
    public virtual IReference SerializedReference {
      get {
        Reference = GetObject<IReference>(ReferencePropertyName);
        return Reference;
      }
      set {
        Reference = value;
        AddObject(ReferencePropertyName, value); 
      }
    }

    #endregion

    #region AddValue methods

    /// <summary>
    /// Adds the <paramref name="value"/> to this instance.
    /// </summary>
    /// <typeparam name="T">The type of the value to add.</typeparam>
    /// <param name="name">The name to associate with the value.</param>
    /// <param name="value">The value to add.</param>
    public abstract void AddValue<T>(string name, T value);

    /// <summary>
    /// Adds the <paramref name="value"/> to this instance.
    /// </summary>
    /// <typeparam name="T">The type of the value to add.</typeparam>
    /// <param name="name">The name to associate with the value.</param>
    /// <param name="value">The value to add.</param>
    /// <param name="preferAttributes">Temporary changes 
    /// <see cref="SerializationContext.PreferAttributes"/> value 
    /// for the duration of this call.</param>
    public void AddValue<T>(string name, T value, bool preferAttributes)
    {
      var context = SerializationContext.Current;
      var oldPreferAttributes = context.PreferAttributes;
      context.PreferAttributes = preferAttributes;
      try {
        AddValue(name, value);
      }
      finally {
        context.PreferAttributes = oldPreferAttributes;
      }
    }

    /// <summary>
    /// Adds the <paramref name="value"/> to this instance,
    /// if <paramref name="value"/> isn't equal to <paramref name="originValue"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value to add.</typeparam>
    /// <param name="name">The name to associate with the value.</param>
    /// <param name="value">The value to add.</param>
    /// <param name="originValue">The value of the origin.</param>
    public void AddValue<T>(string name, T value, T originValue)
    {
      if (!AdvancedComparerStruct<T>.System.Equals(value, originValue))
        AddValue(name, value);
    }

    /// <summary>
    /// Adds the <paramref name="value"/> to this instance,
    /// if <paramref name="value"/> isn't equal to <paramref name="originValue"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value to add.</typeparam>
    /// <param name="name">The name to associate with the value.</param>
    /// <param name="value">The value to add.</param>
    /// <param name="originValue">The value of the origin.</param>
    /// <param name="preferAttributes">Temporary changes 
    /// <see cref="SerializationContext.PreferAttributes"/> value 
    /// for the duration of this call.</param>
    public void AddValue<T>(string name, T value, T originValue, bool preferAttributes)
    {
      if (!AdvancedComparerStruct<T>.System.Equals(value, originValue)) {
        var context = SerializationContext.Current;
        var oldPreferAttributes = context.PreferAttributes;
        context.PreferAttributes = preferAttributes;
        try {
          AddValue(name, value);
        }
        finally {
          context.PreferAttributes = oldPreferAttributes;
        }
      }
    }

    /// <summary>
    /// Adds the value read by <paramref name="getter"/> 
    /// from the <see cref="Source"/> to this instance,
    /// if it isn't equal to the same read from the <see cref="Origin"/>.
    /// </summary>
    /// <typeparam name="TOwner">The type of the value owner.</typeparam>
    /// <typeparam name="T">The type of the value to add.</typeparam>
    /// <param name="name">The name to associate with the value.</param>
    /// <param name="getter">The value getter.</param>
    public void AddValue<TOwner, T>(string name, Func<TOwner, T> getter)
    {
      AddValue(name, 
        getter.Invoke((TOwner) Source), 
        getter.Invoke((TOwner) Origin));
    }

    /// <summary>
    /// Adds the value read by <paramref name="getter"/> 
    /// from the <see cref="Source"/> to this instance,
    /// if it isn't equal to the same read from the <see cref="Origin"/>.
    /// </summary>
    /// <typeparam name="TOwner">The type of the value owner.</typeparam>
    /// <typeparam name="T">The type of the value to add.</typeparam>
    /// <param name="name">The name to associate with the value.</param>
    /// <param name="getter">The value getter.</param>
    /// <param name="preferAttributes">Temporary changes 
    /// <see cref="SerializationContext.PreferAttributes"/> value 
    /// for the duration of this call.</param>
    public void AddValue<TOwner, T>(string name, Func<TOwner, T> getter, bool preferAttributes)
    {
      AddValue(name, 
        getter.Invoke((TOwner) Source), 
        getter.Invoke((TOwner) Origin),
        preferAttributes);
    }

    #endregion

    #region AddObject methods

    /// <summary>
    /// Adds the object to this instance.
    /// </summary>
    /// <param name="name">The name to associate with the object.</param>
    /// <param name="value">The object to add.</param>
    /// <returns>
    /// A <see cref="SerializationData"/> describing added object
    /// or a reference to it.
    /// </returns>
    public SerializationData AddObject(string name, object value)
    {
      return AddObject(name, value, null, SerializationContext.Current.PreferNesting);
    }

    /// <summary>
    /// Adds the object to this instance.
    /// </summary>
    /// <param name="name">The name to associate with the object.</param>
    /// <param name="value">The object to add.</param>
    /// <param name="preferNesting"><see langword="true"/> if its preferable to nest it;
    /// otherwise, <see langword="false"/>.</param>
    /// <returns>
    /// A <see cref="SerializationData"/> describing added object
    /// or a reference to it.
    /// </returns>
    public SerializationData AddObject(string name, object value, bool preferNesting)
    {
      return AddObject(name, value, null, preferNesting);
    }

    /// <summary>
    /// Adds the object to this instance.
    /// </summary>
    /// <param name="name">The name to associate with the object.</param>
    /// <param name="value">The object to add.</param>
    /// <param name="originValue">The origin object value.</param>
    /// <returns>
    /// A <see cref="SerializationData"/> describing added object
    /// or a reference to it.
    /// </returns>
    public SerializationData AddObject(string name, object value, object originValue)
    {
      return AddObject(name, value, originValue, SerializationContext.Current.PreferNesting);
    }

    /// <summary>
    /// Adds the object to this instance.
    /// </summary>
    /// <param name="name">The name to associate with the object.</param>
    /// <param name="value">The object to add.</param>
    /// <param name="originValue">The origin object value.</param>
    /// <param name="preferNesting"><see langword="true"/> if its preferable to nest it;
    /// otherwise, <see langword="false"/>.</param>
    /// <returns>
    /// A <see cref="SerializationData"/> describing added object
    /// or a reference to it.
    /// </returns>
    public virtual SerializationData AddObject(string name, object value, object originValue, bool preferNesting)
    {
      var data = Serializer.GetObjectData(value, originValue, preferNesting);
      AddValue(name, data);
      return data;
    }

    /// <summary>
    /// Adds the object read by <paramref name="getter"/>
    /// from the <see cref="Source"/> to this instance,
    /// providing its origin read from the <see cref="Origin"/> by the same way.
    /// </summary>
    /// <typeparam name="TOwner">The type of the value owner.</typeparam>
    /// <param name="name">The name to associate with the value.</param>
    /// <param name="getter">The value getter.</param>
    /// <returns>
    /// A <see cref="SerializationData"/> describing added object
    /// or a reference to it.
    /// </returns>
    public SerializationData AddObject<TOwner>(string name, Func<TOwner, object> getter)
    {
      return AddObject(name, getter, SerializationContext.Current.PreferNesting);
    }

    /// <summary>
    /// Adds the object read by <paramref name="getter"/>
    /// from the <see cref="Source"/> to this instance,
    /// providing its origin read from the <see cref="Origin"/> by the same way.
    /// </summary>
    /// <typeparam name="TOwner">The type of the value owner.</typeparam>
    /// <param name="name">The name to associate with the value.</param>
    /// <param name="getter">The value getter.</param>
    /// <param name="preferNesting"><see langword="true"/> if its preferable to nest it;
    /// otherwise, <see langword="false"/>.</param>
    /// <returns>
    /// A <see cref="SerializationData"/> describing added object
    /// or a reference to it.
    /// </returns>
    public SerializationData AddObject<TOwner>(string name, Func<TOwner, object> getter, bool preferNesting)
    {
      return AddObject(name,
        getter.Invoke((TOwner) Source),
        getter.Invoke((TOwner) Origin),
        preferNesting);
    }

    #endregion

    #region GetValue methods

    /// <summary>
    /// Gets a value from this instance.
    /// </summary>
    /// <typeparam name="T">The type of the value to get.</typeparam>
    /// <param name="name">The name associated with the value.</param>
    /// <returns>
    /// The value associated with the <paramref name="name"/>.
    /// </returns>
    public abstract T GetValue<T>(string name);

    #endregion

    #region GetObject methods

    /// <summary>
    /// Gets an object from this instance.
    /// </summary>
    /// <param name="name">The name associated with the object.</param>
    /// <returns>
    /// The object associated with the <paramref name="name"/>.
    /// </returns>
    public T GetObject<T>(string name)
    {
      return GetObject<T>(name, null);
    }

    /// <summary>
    /// Gets an object from this instance.
    /// </summary>
    /// <param name="name">The name associated with the object.</param>
    /// <param name="originValue">The origin object value.</param>
    /// <returns>
    /// The object associated with the <paramref name="name"/>.
    /// </returns>
    public virtual T GetObject<T>(string name, object originValue)
    {
      var data = GetValue<SerializationData>(name);
      return (T) Serializer.SetObjectData(originValue, data);
    }

    #endregion

    #region GetReference methods

    /// <summary>
    /// Gets reference to an object from this instance.
    /// </summary>
    /// <param name="name">The name associated with the object.</param>
    /// <returns>
    /// The reference to the object associated with the <paramref name="name"/>.
    /// </returns>
    public IReference GetReference(string name)
    {
      return GetReference(name, null);
    }

    /// <summary>
    /// Gets reference to an object from this instance.
    /// </summary>
    /// <param name="name">The name associated with the object.</param>
    /// <param name="originValue">The origin object value.</param>
    /// <returns>
    /// The reference to the object associated with the <paramref name="name"/>.
    /// </returns>
    public virtual IReference GetReference(string name, object originValue)
    {
      var data = GetValue<SerializationData>(name);
      var source = Serializer.SetObjectData(originValue, data);
      if (data.Reference!=null)
        // We've just deserialized an object with reference
        return data.Reference;
      else
        // We've just deserialized a reference or something else, which isn't referable
        // (the last case will lead to type cast failure)
        return (IReference) source;
    }

    #endregion

    #region HasValue, SkipValue, RemoveValue

    /// <summary>
    /// Determines whether the value with specified name exists.
    /// </summary>
    /// <param name="name">The name associated with the value.</param>
    /// <returns>
    /// <see langword="true"/> if the value with specified name exists; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public abstract bool HasValue(string name);

    /// <summary>
    /// "Officially" skips the value by marking it as read.
    /// </summary>
    /// <param name="name">The name associated with the value.</param>
    public abstract void SkipValue(string name);

    /// <summary>
    /// Removes the value with the specified name.
    /// </summary>
    /// <param name="name">The name associated with the value.</param>
    public abstract void RemoveValue(string name);

    #endregion

    #region AddFixup methods

    /// <summary>
    /// Adds a fixup action to an object 
    /// associated with the specified <paramref name="name"/>.
    /// </summary>
    /// <param name="name">The name associated with the object.</param>
    /// <param name="target">Object to execute the <paramref name="action"/> on.</param>
    /// <param name="action">The action to be executed when the referred object will be deserialized.</param>
    /// <typeparam name="T">Type of the <paramref name="target"/>.</typeparam>
    public void AddFixup<T>(string name, T target, Action<T, IReference> action) 
    {
      IReference reference = GetReference(name, null);
      if (reference.IsNull())
        return;
      object tmp;
      if (reference.TryResolve(out tmp))
        action.Invoke(target, reference);
      else
        SerializationContext.Current.FixupManager.Add(target, reference, action);
    }

    /// <summary>
    /// Adds a fixup action to an object 
    /// associated with the specified <paramref name="name"/>.
    /// </summary>
    /// <param name="name">The name associated with the object.</param>
    /// <param name="action">The action to be executed when the referred object will be deserialized.</param>
    /// <typeparam name="T">Type of the target object.</typeparam>
    public void AddFixup<T>(string name, Action<T, IReference> action) 
    {
      IReference reference = GetReference(name, null);
      if (reference.IsNull())
        return;
      object tmp;
      if (reference.TryResolve(out tmp))
        action.Invoke((T) Source, reference);
      else
        SerializationContext.Current.FixupManager.Add(this, reference, 
          (d, r) => action.Invoke((T) d.Source, r));
    }

    #endregion

    #region UpdateSource method

    /// <summary>
    /// Updates the <see cref="Source"/> property value.
    /// Note that if <see cref="Reference"/> is set, it
    /// can be updated just once. Successive update with
    /// different <paramref name="source"/> value will fail,
    /// since <see cref="Reference"/> can be 
    /// <see cref="ReferenceManager.Define"/>d just once.
    /// </summary>
    /// <param name="source">The new source.</param>
    public void UpdateSource(object source)
    {
      if (Reference!=null)
        SerializationContext.Current.ReferenceManager.Define(Reference, source);
      Source = source;
    }

    #endregion

    #region EnsureXxx methods

    /// <summary>
    /// Ensures there are no skipped slots during reading the data 
    /// (<see cref="SkipCount"/> is <see langword="0" />).
    /// </summary>
    /// <exception cref="SerializationException">Some slots were skipped.</exception>
    public void EnsureNoSkips()
    {
      if (SkipCount!=0)
        throw new SerializationException(
          Strings.ExDeserializationErrorUnrecognizedSlotsAreFound);
    }

    #endregion

    #region IEnumerable<...> methods

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    /// <inheritdoc/>
    public abstract IEnumerator<string> GetEnumerator();

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format(Strings.SerializationDataFormat,
        Type!=null ? Type.GetShortName() : "null",
        Reference!=null ? Reference.ToString() : "null",
        this.Select(s => string.Format("'{0}'", s)).ToCommaDelimitedString());
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>.
    /// </summary>
    /// <param name="reference">The <see cref="Reference"/> property value.</param>
    /// <param name="source">The <see cref="Source"/> property value.</param>
    /// <param name="origin">The <see cref="Origin"/> property value.</param>
    protected SerializationData(IReference reference, object source, object origin)
      : this()
    {
      Reference = reference;
      Source = source;
      Origin = origin;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>.
    /// </summary>
    protected SerializationData()
    {
    }
  }
}
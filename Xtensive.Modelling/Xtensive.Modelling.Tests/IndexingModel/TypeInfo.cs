// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.24

using System;
using System.Globalization;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core;
using System.Text;
using Xtensive.Core.Reflection;
using Xtensive.Modelling.Tests.IndexingModel.Resources;
using Xtensive.Modelling.Validation;

namespace Xtensive.Modelling.Tests.IndexingModel
{
  /// <summary>
  /// Type of table column.
  /// </summary>
  [Serializable]
  public sealed class TypeInfo : IEquatable<TypeInfo>,
    IValidatable
  {
    /// <summary>
    /// Gets the type of the data.
    /// </summary>
    public Type Type { get;  private set; }

    /// <summary>
    /// Indicates whether <see cref="Type"/> is nullable.
    /// </summary>
    public bool IsNullable { 
      get {
        return Type.IsNullable();
      }
    }

    /// <summary>
    /// Gets the length.
    /// </summary>
    public int Length { get;  private set; }

    /// <summary>
    /// Gets the culture.
    /// </summary>
    public CultureInfo Culture { get;  private set; }

    /// <summary>
    /// Gets the scale.
    /// </summary>
    public int Scale { get;  private set; }

    /// <summary>
    /// Gets the precision.
    /// </summary>
    public int Precision { get;  private set; }

    /// <inheritdoc/>
    public void Validate()
    {
      // TODO: Implement
    }

    #region Equality members

    public bool Equals(TypeInfo other)
    {
      if (ReferenceEquals(null, other))
        return false;
      if (ReferenceEquals(this, other))
        return true;
      return 
        Type==Type && 
        other.Length==Length && 
        other.Scale==Scale && 
        other.Precision==Precision;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      if (obj.GetType()!=typeof (TypeInfo))
        return false;
      return Equals((TypeInfo) obj);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      unchecked {
        int result = (Type!=null ? Type.GetHashCode() : 0);
        result = (result * 397) ^ Length;
        result = (result * 397) ^ Scale;
        result = (result * 397) ^ Precision;
        if (Culture!=null)
          result = (result * 397) ^ Culture.GetHashCode();
        return result;
      }
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      var sb = new StringBuilder();
      sb.Append(string.Format(Strings.PropertyPairFormat, Strings.Type, Type.GetShortName()));
      if (Length > 0)
        sb.Append(Strings.Comma).Append(string.Format(
          Strings.PropertyPairFormat, Strings.Length, Length));
      if (Culture!=null)
        sb.Append(Strings.Comma).Append(string.Format(
          Strings.PropertyPairFormat, Strings.Culture, Culture));
      if (Scale > 0)
        sb.Append(Strings.Comma).Append(string.Format(
          Strings.PropertyPairFormat, Strings.Scale, Scale));
      if (Precision > 0)
        sb.Append(Strings.Comma).Append(string.Format(
          Strings.PropertyPairFormat, Strings.Precision, Precision));
      return sb.ToString();
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="dataType">Type of the data.</param>
    public TypeInfo(Type dataType)
    {
      ArgumentValidator.EnsureArgumentNotNull(dataType, "dataType");
      Type = dataType;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="dataType">Type of the data.</param>
    /// <param name="length">The length.</param>
    public TypeInfo(Type dataType, int length)
      : this(dataType)
    {
      ArgumentValidator.EnsureArgumentIsInRange(length, 0, int.MaxValue, "length");
      Length = length;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="dataType">Type of the data.</param>
    /// <param name="length">The length.</param>
    /// <param name="culture">The culture.</param>
    public TypeInfo(Type dataType, int length, CultureInfo culture)
      : this(dataType, length)
    {
      ArgumentValidator.EnsureArgumentNotNull(culture, "culture");
      Culture = culture;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="dataType">Type of the data.</param>
    /// <param name="length">The length.</param>
    /// <param name="scale">The scale.</param>
    /// <param name="precision">The precision.</param>
    public TypeInfo(Type dataType, int length, int scale, int precision)
      : this(dataType, length)
    {
      Scale = scale;
      Precision = precision;
    }
  }
}
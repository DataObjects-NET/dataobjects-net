// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Core;

namespace Xtensive.Sql.Dml
{
  [Serializable]
  public class SqlNative : SqlExpression, IConvertible
  {
    private string value;

    /// <summary>
    /// Gets the value.
    /// </summary>
    public string Value
    {
      get { return value; }
    }

    public override void ReplaceWith(SqlExpression expression)
    {
      ArgumentValidator.EnsureArgumentNotNull(expression, "expression");
      ArgumentValidator.EnsureArgumentIs<SqlNative>(expression, "expression");
      SqlNative replacingExpression = expression as SqlNative;
      value = replacingExpression.Value;
    }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];

      SqlNative clone = new SqlNative(value);
      context.NodeMapping[this] = clone;
      return clone;
    }

    // Constructor

    internal SqlNative(string value) : base(SqlNodeType.Constant)
    {
      this.value = value;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    #region IConvertible Members

    ///<summary>
    ///Returns the <see cref="T:System.TypeCode"></see> for this instance.
    ///</summary>
    ///
    ///<returns>
    ///The enumerated constant that is the <see cref="T:System.TypeCode"></see> of the class or value type that implements this interface.
    ///</returns>
    ///<filterpriority>2</filterpriority>
    TypeCode IConvertible.GetTypeCode()
    {
      return value.GetTypeCode();
    }

    ///<summary>
    ///Converts the value of this instance to an equivalent Boolean value using the specified culture-specific formatting information.
    ///</summary>
    ///
    ///<returns>
    ///A Boolean value equivalent to the value of this instance.
    ///</returns>
    ///
    ///<param name="provider">An <see cref="T:System.IFormatProvider"></see> interface implementation that supplies culture-specific formatting information. </param><filterpriority>2</filterpriority>
    bool IConvertible.ToBoolean(IFormatProvider provider)
    {
      return ((IConvertible)value).ToBoolean(provider);
    }

    ///<summary>
    ///Converts the value of this instance to an equivalent Unicode character using the specified culture-specific formatting information.
    ///</summary>
    ///
    ///<returns>
    ///A Unicode character equivalent to the value of this instance.
    ///</returns>
    ///
    ///<param name="provider">An <see cref="T:System.IFormatProvider"></see> interface implementation that supplies culture-specific formatting information. </param><filterpriority>2</filterpriority>
    char IConvertible.ToChar(IFormatProvider provider)
    {
      return ((IConvertible)value).ToChar(provider);
    }

    ///<summary>
    ///Converts the value of this instance to an equivalent 8-bit signed integer using the specified culture-specific formatting information.
    ///</summary>
    ///
    ///<returns>
    ///An 8-bit signed integer equivalent to the value of this instance.
    ///</returns>
    ///
    ///<param name="provider">An <see cref="T:System.IFormatProvider"></see> interface implementation that supplies culture-specific formatting information. </param><filterpriority>2</filterpriority>
    sbyte IConvertible.ToSByte(IFormatProvider provider)
    {
      return ((IConvertible)value).ToSByte(provider);
    }

    ///<summary>
    ///Converts the value of this instance to an equivalent 8-bit unsigned integer using the specified culture-specific formatting information.
    ///</summary>
    ///
    ///<returns>
    ///An 8-bit unsigned integer equivalent to the value of this instance.
    ///</returns>
    ///
    ///<param name="provider">An <see cref="T:System.IFormatProvider"></see> interface implementation that supplies culture-specific formatting information. </param><filterpriority>2</filterpriority>
    byte IConvertible.ToByte(IFormatProvider provider)
    {
      return ((IConvertible)value).ToByte(provider);
    }

    ///<summary>
    ///Converts the value of this instance to an equivalent 16-bit signed integer using the specified culture-specific formatting information.
    ///</summary>
    ///
    ///<returns>
    ///An 16-bit signed integer equivalent to the value of this instance.
    ///</returns>
    ///
    ///<param name="provider">An <see cref="T:System.IFormatProvider"></see> interface implementation that supplies culture-specific formatting information. </param><filterpriority>2</filterpriority>
    short IConvertible.ToInt16(IFormatProvider provider)
    {
      return ((IConvertible)value).ToInt16(provider);
    }

    ///<summary>
    ///Converts the value of this instance to an equivalent 16-bit unsigned integer using the specified culture-specific formatting information.
    ///</summary>
    ///
    ///<returns>
    ///An 16-bit unsigned integer equivalent to the value of this instance.
    ///</returns>
    ///
    ///<param name="provider">An <see cref="T:System.IFormatProvider"></see> interface implementation that supplies culture-specific formatting information. </param><filterpriority>2</filterpriority>
    ushort IConvertible.ToUInt16(IFormatProvider provider)
    {
      return ((IConvertible)value).ToUInt16(provider);
    }

    ///<summary>
    ///Converts the value of this instance to an equivalent 32-bit signed integer using the specified culture-specific formatting information.
    ///</summary>
    ///
    ///<returns>
    ///An 32-bit signed integer equivalent to the value of this instance.
    ///</returns>
    ///
    ///<param name="provider">An <see cref="T:System.IFormatProvider"></see> interface implementation that supplies culture-specific formatting information. </param><filterpriority>2</filterpriority>
    int IConvertible.ToInt32(IFormatProvider provider)
    {
      return ((IConvertible)value).ToInt32(provider);
    }

    ///<summary>
    ///Converts the value of this instance to an equivalent 32-bit unsigned integer using the specified culture-specific formatting information.
    ///</summary>
    ///
    ///<returns>
    ///An 32-bit unsigned integer equivalent to the value of this instance.
    ///</returns>
    ///
    ///<param name="provider">An <see cref="T:System.IFormatProvider"></see> interface implementation that supplies culture-specific formatting information. </param><filterpriority>2</filterpriority>
    uint IConvertible.ToUInt32(IFormatProvider provider)
    {
      return ((IConvertible)value).ToUInt32(provider);
    }

    ///<summary>
    ///Converts the value of this instance to an equivalent 64-bit signed integer using the specified culture-specific formatting information.
    ///</summary>
    ///
    ///<returns>
    ///An 64-bit signed integer equivalent to the value of this instance.
    ///</returns>
    ///
    ///<param name="provider">An <see cref="T:System.IFormatProvider"></see> interface implementation that supplies culture-specific formatting information. </param><filterpriority>2</filterpriority>
    long IConvertible.ToInt64(IFormatProvider provider)
    {
      return ((IConvertible)value).ToInt64(provider);
    }

    ///<summary>
    ///Converts the value of this instance to an equivalent 64-bit unsigned integer using the specified culture-specific formatting information.
    ///</summary>
    ///
    ///<returns>
    ///An 64-bit unsigned integer equivalent to the value of this instance.
    ///</returns>
    ///
    ///<param name="provider">An <see cref="T:System.IFormatProvider"></see> interface implementation that supplies culture-specific formatting information. </param><filterpriority>2</filterpriority>
    ulong IConvertible.ToUInt64(IFormatProvider provider)
    {
      return ((IConvertible)value).ToUInt64(provider);
    }

    ///<summary>
    ///Converts the value of this instance to an equivalent single-precision floating-point number using the specified culture-specific formatting information.
    ///</summary>
    ///
    ///<returns>
    ///A single-precision floating-point number equivalent to the value of this instance.
    ///</returns>
    ///
    ///<param name="provider">An <see cref="T:System.IFormatProvider"></see> interface implementation that supplies culture-specific formatting information. </param><filterpriority>2</filterpriority>
    float IConvertible.ToSingle(IFormatProvider provider)
    {
      return ((IConvertible)value).ToSingle(provider);
    }

    ///<summary>
    ///Converts the value of this instance to an equivalent double-precision floating-point number using the specified culture-specific formatting information.
    ///</summary>
    ///
    ///<returns>
    ///A double-precision floating-point number equivalent to the value of this instance.
    ///</returns>
    ///
    ///<param name="provider">An <see cref="T:System.IFormatProvider"></see> interface implementation that supplies culture-specific formatting information. </param><filterpriority>2</filterpriority>
    double IConvertible.ToDouble(IFormatProvider provider)
    {
      return ((IConvertible)value).ToDouble(provider);
    }

    ///<summary>
    ///Converts the value of this instance to an equivalent <see cref="T:System.Decimal"></see> number using the specified culture-specific formatting information.
    ///</summary>
    ///
    ///<returns>
    ///A <see cref="T:System.Decimal"></see> number equivalent to the value of this instance.
    ///</returns>
    ///
    ///<param name="provider">An <see cref="T:System.IFormatProvider"></see> interface implementation that supplies culture-specific formatting information. </param><filterpriority>2</filterpriority>
    decimal IConvertible.ToDecimal(IFormatProvider provider)
    {
      return ((IConvertible)value).ToDecimal(provider);
    }

    ///<summary>
    ///Converts the value of this instance to an equivalent <see cref="T:System.DateTime"></see> using the specified culture-specific formatting information.
    ///</summary>
    ///
    ///<returns>
    ///A <see cref="T:System.DateTime"></see> instance equivalent to the value of this instance.
    ///</returns>
    ///
    ///<param name="provider">An <see cref="T:System.IFormatProvider"></see> interface implementation that supplies culture-specific formatting information. </param><filterpriority>2</filterpriority>
    DateTime IConvertible.ToDateTime(IFormatProvider provider)
    {
      return ((IConvertible)value).ToDateTime(provider);
    }

    ///<summary>
    ///Converts the value of this instance to an equivalent <see cref="T:System.String"></see> using the specified culture-specific formatting information.
    ///</summary>
    ///
    ///<returns>
    ///A <see cref="T:System.String"></see> instance equivalent to the value of this instance.
    ///</returns>
    ///
    ///<param name="provider">An <see cref="T:System.IFormatProvider"></see> interface implementation that supplies culture-specific formatting information. </param><filterpriority>2</filterpriority>
    string IConvertible.ToString(IFormatProvider provider)
    {
      return value.ToString(provider);
    }

    ///<summary>
    ///Converts the value of this instance to an <see cref="T:System.Object"></see> of the specified <see cref="T:System.Type"></see> that has an equivalent value, using the specified culture-specific formatting information.
    ///</summary>
    ///
    ///<returns>
    ///An <see cref="T:System.Object"></see> instance of type conversionType whose value is equivalent to the value of this instance.
    ///</returns>
    ///
    ///<param name="provider">An <see cref="T:System.IFormatProvider"></see> interface implementation that supplies culture-specific formatting information. </param>
    ///<param name="conversionType">The <see cref="T:System.Type"></see> to which the value of this instance is converted. </param><filterpriority>2</filterpriority>
    object IConvertible.ToType(Type conversionType, IFormatProvider provider)
    {
      return ((IConvertible)value).ToType(conversionType, provider);
    }

    #endregion
  }
}
// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Core;

namespace Xtensive.Sql.Dml
{
  [Serializable]
  public class SqlNative : SqlExpression, IConvertible
  {
    /// <summary>
    /// Gets the value.
    /// </summary>
    public string Value { get; private set; }

    public override void ReplaceWith(SqlExpression expression)
    {
      var replacingExpression = ArgumentValidator.EnsureArgumentIs<SqlNative>(expression);
      Value = replacingExpression.Value;
    }

    internal override SqlNative Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlNative(t.Value));

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
      return Value.GetTypeCode();
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
      return ((IConvertible) Value).ToBoolean(provider);
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
      return ((IConvertible) Value).ToChar(provider);
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
      return ((IConvertible) Value).ToSByte(provider);
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
      return ((IConvertible) Value).ToByte(provider);
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
      return ((IConvertible) Value).ToInt16(provider);
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
      return ((IConvertible) Value).ToUInt16(provider);
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
      return ((IConvertible) Value).ToInt32(provider);
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
      return ((IConvertible) Value).ToUInt32(provider);
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
      return ((IConvertible) Value).ToInt64(provider);
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
      return ((IConvertible) Value).ToUInt64(provider);
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
      return ((IConvertible) Value).ToSingle(provider);
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
      return ((IConvertible) Value).ToDouble(provider);
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
      return ((IConvertible) Value).ToDecimal(provider);
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
      return ((IConvertible) Value).ToDateTime(provider);
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
      return Value.ToString(provider);
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
      return ((IConvertible) Value).ToType(conversionType, provider);
    }

    #endregion


    // Constructors

    internal SqlNative(string value) : base(SqlNodeType.Native)
    {
      Value = value;
    }
  }
}
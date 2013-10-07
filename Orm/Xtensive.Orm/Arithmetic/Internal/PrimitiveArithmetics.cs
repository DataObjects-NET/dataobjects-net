// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.22

using System;
using Xtensive.Arithmetic;


namespace Xtensive.Arithmetic
{
  [Serializable]
  internal sealed class ByteArithmetic
    : ArithmeticBase<Byte>
  {
    private const Byte zero     = (Byte) 0;
    private const Byte one      = (Byte) 1;
    private const Byte minusOne = unchecked ((Byte) (-1));

    /// <inheritdoc/>
    public override Byte Zero
    {
      get { return zero; }
    }

    /// <inheritdoc/>
    public override Byte One
    {
      get { return one; }
    }

	/// <inheritdoc/>
    public override Byte MaxValue
    {
      get { return Byte.MaxValue; }
    }
    
    /// <inheritdoc/>
    public override Byte MinValue
    {
      get { return Byte.MinValue; }
    }
    
    /// <inheritdoc/>
    public override bool IsSigned
    {
      get { return Byte.MinValue != zero; }
    }

    /// <inheritdoc/>
    public override Byte Add(Byte value1, Byte value2)
    {
      if (OverflowAllowed) {
        unchecked{
          return (Byte)(value1 + value2);
        }
      }
      else {
        checked{
          return (Byte)(value1 + value2);
        }
      }
    }

    /// <inheritdoc/>
    public override Byte Negation(Byte value)
    {
      if (OverflowAllowed) {
        unchecked{
          return (Byte)(minusOne*value);
        }
      }
      else {
        checked{
          return (Byte)(minusOne*value);
        }
      }
    }

    /// <inheritdoc/>
    public override Byte Subtract(Byte value1, Byte value2)
    {
      if (OverflowAllowed) {
        unchecked{
          return (Byte)(value1 - value2);
        }
      }
      else {
        checked{
          return (Byte)(value1 - value2);
        }
      }
    }

    /// <inheritdoc/>
    public override Byte Multiply(Byte value, double factor)
    {
      if (OverflowAllowed) {
        unchecked{
          return (Byte)((double)value*factor);
        }
      }
      else {
        checked{
          return (Byte)((double)value*factor);
        }
      }
    }

    /// <inheritdoc/>
    public override Byte Divide(Byte value, double factor)
    {
      if (OverflowAllowed) {
        unchecked{
          return (Byte)((double)value/factor);
        }
      }
      else {
        checked{
          return (Byte)((double)value/factor);
        }
      }
    }

    /// <inheritdoc/>
    protected override IArithmetic<Byte> CreateNew(ArithmeticRules rules)
    {
      return new ByteArithmetic(Provider, rules);
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    public ByteArithmetic(IArithmeticProvider provider, ArithmeticRules rule)
      : base(provider, rule)
    {
    }
  }
  
  [Serializable]
  internal sealed class SByteArithmetic
    : ArithmeticBase<SByte>
  {
    private const SByte zero     = (SByte) 0;
    private const SByte one      = (SByte) 1;
    private const SByte minusOne = unchecked ((SByte) (-1));

    /// <inheritdoc/>
    public override SByte Zero
    {
      get { return zero; }
    }

    /// <inheritdoc/>
    public override SByte One
    {
      get { return one; }
    }

	/// <inheritdoc/>
    public override SByte MaxValue
    {
      get { return SByte.MaxValue; }
    }
    
    /// <inheritdoc/>
    public override SByte MinValue
    {
      get { return SByte.MinValue; }
    }
    
    /// <inheritdoc/>
    public override bool IsSigned
    {
      get { return SByte.MinValue != zero; }
    }

    /// <inheritdoc/>
    public override SByte Add(SByte value1, SByte value2)
    {
      if (OverflowAllowed) {
        unchecked{
          return (SByte)(value1 + value2);
        }
      }
      else {
        checked{
          return (SByte)(value1 + value2);
        }
      }
    }

    /// <inheritdoc/>
    public override SByte Negation(SByte value)
    {
      if (OverflowAllowed) {
        unchecked{
          return (SByte)(minusOne*value);
        }
      }
      else {
        checked{
          return (SByte)(minusOne*value);
        }
      }
    }

    /// <inheritdoc/>
    public override SByte Subtract(SByte value1, SByte value2)
    {
      if (OverflowAllowed) {
        unchecked{
          return (SByte)(value1 - value2);
        }
      }
      else {
        checked{
          return (SByte)(value1 - value2);
        }
      }
    }

    /// <inheritdoc/>
    public override SByte Multiply(SByte value, double factor)
    {
      if (OverflowAllowed) {
        unchecked{
          return (SByte)((double)value*factor);
        }
      }
      else {
        checked{
          return (SByte)((double)value*factor);
        }
      }
    }

    /// <inheritdoc/>
    public override SByte Divide(SByte value, double factor)
    {
      if (OverflowAllowed) {
        unchecked{
          return (SByte)((double)value/factor);
        }
      }
      else {
        checked{
          return (SByte)((double)value/factor);
        }
      }
    }

    /// <inheritdoc/>
    protected override IArithmetic<SByte> CreateNew(ArithmeticRules rules)
    {
      return new SByteArithmetic(Provider, rules);
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    public SByteArithmetic(IArithmeticProvider provider, ArithmeticRules rule)
      : base(provider, rule)
    {
    }
  }
  
  [Serializable]
  internal sealed class CharArithmetic
    : ArithmeticBase<Char>
  {
    private const Char zero     = (Char) 0;
    private const Char one      = (Char) 1;
    private const Char minusOne = unchecked ((Char) (-1));

    /// <inheritdoc/>
    public override Char Zero
    {
      get { return zero; }
    }

    /// <inheritdoc/>
    public override Char One
    {
      get { return one; }
    }

	/// <inheritdoc/>
    public override Char MaxValue
    {
      get { return Char.MaxValue; }
    }
    
    /// <inheritdoc/>
    public override Char MinValue
    {
      get { return Char.MinValue; }
    }
    
    /// <inheritdoc/>
    public override bool IsSigned
    {
      get { return Char.MinValue != zero; }
    }

    /// <inheritdoc/>
    public override Char Add(Char value1, Char value2)
    {
      if (OverflowAllowed) {
        unchecked{
          return (Char)(value1 + value2);
        }
      }
      else {
        checked{
          return (Char)(value1 + value2);
        }
      }
    }

    /// <inheritdoc/>
    public override Char Negation(Char value)
    {
      if (OverflowAllowed) {
        unchecked{
          return (Char)(minusOne*value);
        }
      }
      else {
        checked{
          return (Char)(minusOne*value);
        }
      }
    }

    /// <inheritdoc/>
    public override Char Subtract(Char value1, Char value2)
    {
      if (OverflowAllowed) {
        unchecked{
          return (Char)(value1 - value2);
        }
      }
      else {
        checked{
          return (Char)(value1 - value2);
        }
      }
    }

    /// <inheritdoc/>
    public override Char Multiply(Char value, double factor)
    {
      if (OverflowAllowed) {
        unchecked{
          return (Char)((double)value*factor);
        }
      }
      else {
        checked{
          return (Char)((double)value*factor);
        }
      }
    }

    /// <inheritdoc/>
    public override Char Divide(Char value, double factor)
    {
      if (OverflowAllowed) {
        unchecked{
          return (Char)((double)value/factor);
        }
      }
      else {
        checked{
          return (Char)((double)value/factor);
        }
      }
    }

    /// <inheritdoc/>
    protected override IArithmetic<Char> CreateNew(ArithmeticRules rules)
    {
      return new CharArithmetic(Provider, rules);
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    public CharArithmetic(IArithmeticProvider provider, ArithmeticRules rule)
      : base(provider, rule)
    {
    }
  }
  
  [Serializable]
  internal sealed class Int16Arithmetic
    : ArithmeticBase<Int16>
  {
    private const Int16 zero     = (Int16) 0;
    private const Int16 one      = (Int16) 1;
    private const Int16 minusOne = unchecked ((Int16) (-1));

    /// <inheritdoc/>
    public override Int16 Zero
    {
      get { return zero; }
    }

    /// <inheritdoc/>
    public override Int16 One
    {
      get { return one; }
    }

	/// <inheritdoc/>
    public override Int16 MaxValue
    {
      get { return Int16.MaxValue; }
    }
    
    /// <inheritdoc/>
    public override Int16 MinValue
    {
      get { return Int16.MinValue; }
    }
    
    /// <inheritdoc/>
    public override bool IsSigned
    {
      get { return Int16.MinValue != zero; }
    }

    /// <inheritdoc/>
    public override Int16 Add(Int16 value1, Int16 value2)
    {
      if (OverflowAllowed) {
        unchecked{
          return (Int16)(value1 + value2);
        }
      }
      else {
        checked{
          return (Int16)(value1 + value2);
        }
      }
    }

    /// <inheritdoc/>
    public override Int16 Negation(Int16 value)
    {
      if (OverflowAllowed) {
        unchecked{
          return (Int16)(minusOne*value);
        }
      }
      else {
        checked{
          return (Int16)(minusOne*value);
        }
      }
    }

    /// <inheritdoc/>
    public override Int16 Subtract(Int16 value1, Int16 value2)
    {
      if (OverflowAllowed) {
        unchecked{
          return (Int16)(value1 - value2);
        }
      }
      else {
        checked{
          return (Int16)(value1 - value2);
        }
      }
    }

    /// <inheritdoc/>
    public override Int16 Multiply(Int16 value, double factor)
    {
      if (OverflowAllowed) {
        unchecked{
          return (Int16)((double)value*factor);
        }
      }
      else {
        checked{
          return (Int16)((double)value*factor);
        }
      }
    }

    /// <inheritdoc/>
    public override Int16 Divide(Int16 value, double factor)
    {
      if (OverflowAllowed) {
        unchecked{
          return (Int16)((double)value/factor);
        }
      }
      else {
        checked{
          return (Int16)((double)value/factor);
        }
      }
    }

    /// <inheritdoc/>
    protected override IArithmetic<Int16> CreateNew(ArithmeticRules rules)
    {
      return new Int16Arithmetic(Provider, rules);
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    public Int16Arithmetic(IArithmeticProvider provider, ArithmeticRules rule)
      : base(provider, rule)
    {
    }
  }
  
  [Serializable]
  internal sealed class UInt16Arithmetic
    : ArithmeticBase<UInt16>
  {
    private const UInt16 zero     = (UInt16) 0;
    private const UInt16 one      = (UInt16) 1;
    private const UInt16 minusOne = unchecked ((UInt16) (-1));

    /// <inheritdoc/>
    public override UInt16 Zero
    {
      get { return zero; }
    }

    /// <inheritdoc/>
    public override UInt16 One
    {
      get { return one; }
    }

	/// <inheritdoc/>
    public override UInt16 MaxValue
    {
      get { return UInt16.MaxValue; }
    }
    
    /// <inheritdoc/>
    public override UInt16 MinValue
    {
      get { return UInt16.MinValue; }
    }
    
    /// <inheritdoc/>
    public override bool IsSigned
    {
      get { return UInt16.MinValue != zero; }
    }

    /// <inheritdoc/>
    public override UInt16 Add(UInt16 value1, UInt16 value2)
    {
      if (OverflowAllowed) {
        unchecked{
          return (UInt16)(value1 + value2);
        }
      }
      else {
        checked{
          return (UInt16)(value1 + value2);
        }
      }
    }

    /// <inheritdoc/>
    public override UInt16 Negation(UInt16 value)
    {
      if (OverflowAllowed) {
        unchecked{
          return (UInt16)(minusOne*value);
        }
      }
      else {
        checked{
          return (UInt16)(minusOne*value);
        }
      }
    }

    /// <inheritdoc/>
    public override UInt16 Subtract(UInt16 value1, UInt16 value2)
    {
      if (OverflowAllowed) {
        unchecked{
          return (UInt16)(value1 - value2);
        }
      }
      else {
        checked{
          return (UInt16)(value1 - value2);
        }
      }
    }

    /// <inheritdoc/>
    public override UInt16 Multiply(UInt16 value, double factor)
    {
      if (OverflowAllowed) {
        unchecked{
          return (UInt16)((double)value*factor);
        }
      }
      else {
        checked{
          return (UInt16)((double)value*factor);
        }
      }
    }

    /// <inheritdoc/>
    public override UInt16 Divide(UInt16 value, double factor)
    {
      if (OverflowAllowed) {
        unchecked{
          return (UInt16)((double)value/factor);
        }
      }
      else {
        checked{
          return (UInt16)((double)value/factor);
        }
      }
    }

    /// <inheritdoc/>
    protected override IArithmetic<UInt16> CreateNew(ArithmeticRules rules)
    {
      return new UInt16Arithmetic(Provider, rules);
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    public UInt16Arithmetic(IArithmeticProvider provider, ArithmeticRules rule)
      : base(provider, rule)
    {
    }
  }
  
  [Serializable]
  internal sealed class Int32Arithmetic
    : ArithmeticBase<Int32>
  {
    private const Int32 zero     = (Int32) 0;
    private const Int32 one      = (Int32) 1;
    private const Int32 minusOne = unchecked ((Int32) (-1));

    /// <inheritdoc/>
    public override Int32 Zero
    {
      get { return zero; }
    }

    /// <inheritdoc/>
    public override Int32 One
    {
      get { return one; }
    }

	/// <inheritdoc/>
    public override Int32 MaxValue
    {
      get { return Int32.MaxValue; }
    }
    
    /// <inheritdoc/>
    public override Int32 MinValue
    {
      get { return Int32.MinValue; }
    }
    
    /// <inheritdoc/>
    public override bool IsSigned
    {
      get { return Int32.MinValue != zero; }
    }

    /// <inheritdoc/>
    public override Int32 Add(Int32 value1, Int32 value2)
    {
      if (OverflowAllowed) {
        unchecked{
          return (Int32)(value1 + value2);
        }
      }
      else {
        checked{
          return (Int32)(value1 + value2);
        }
      }
    }

    /// <inheritdoc/>
    public override Int32 Negation(Int32 value)
    {
      if (OverflowAllowed) {
        unchecked{
          return (Int32)(minusOne*value);
        }
      }
      else {
        checked{
          return (Int32)(minusOne*value);
        }
      }
    }

    /// <inheritdoc/>
    public override Int32 Subtract(Int32 value1, Int32 value2)
    {
      if (OverflowAllowed) {
        unchecked{
          return (Int32)(value1 - value2);
        }
      }
      else {
        checked{
          return (Int32)(value1 - value2);
        }
      }
    }

    /// <inheritdoc/>
    public override Int32 Multiply(Int32 value, double factor)
    {
      if (OverflowAllowed) {
        unchecked{
          return (Int32)((double)value*factor);
        }
      }
      else {
        checked{
          return (Int32)((double)value*factor);
        }
      }
    }

    /// <inheritdoc/>
    public override Int32 Divide(Int32 value, double factor)
    {
      if (OverflowAllowed) {
        unchecked{
          return (Int32)((double)value/factor);
        }
      }
      else {
        checked{
          return (Int32)((double)value/factor);
        }
      }
    }

    /// <inheritdoc/>
    protected override IArithmetic<Int32> CreateNew(ArithmeticRules rules)
    {
      return new Int32Arithmetic(Provider, rules);
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    public Int32Arithmetic(IArithmeticProvider provider, ArithmeticRules rule)
      : base(provider, rule)
    {
    }
  }
  
  [Serializable]
  internal sealed class UInt32Arithmetic
    : ArithmeticBase<UInt32>
  {
    private const UInt32 zero     = (UInt32) 0;
    private const UInt32 one      = (UInt32) 1;
    private const UInt32 minusOne = unchecked ((UInt32) (-1));

    /// <inheritdoc/>
    public override UInt32 Zero
    {
      get { return zero; }
    }

    /// <inheritdoc/>
    public override UInt32 One
    {
      get { return one; }
    }

	/// <inheritdoc/>
    public override UInt32 MaxValue
    {
      get { return UInt32.MaxValue; }
    }
    
    /// <inheritdoc/>
    public override UInt32 MinValue
    {
      get { return UInt32.MinValue; }
    }
    
    /// <inheritdoc/>
    public override bool IsSigned
    {
      get { return UInt32.MinValue != zero; }
    }

    /// <inheritdoc/>
    public override UInt32 Add(UInt32 value1, UInt32 value2)
    {
      if (OverflowAllowed) {
        unchecked{
          return (UInt32)(value1 + value2);
        }
      }
      else {
        checked{
          return (UInt32)(value1 + value2);
        }
      }
    }

    /// <inheritdoc/>
    public override UInt32 Negation(UInt32 value)
    {
      if (OverflowAllowed) {
        unchecked{
          return (UInt32)(minusOne*value);
        }
      }
      else {
        checked{
          return (UInt32)(minusOne*value);
        }
      }
    }

    /// <inheritdoc/>
    public override UInt32 Subtract(UInt32 value1, UInt32 value2)
    {
      if (OverflowAllowed) {
        unchecked{
          return (UInt32)(value1 - value2);
        }
      }
      else {
        checked{
          return (UInt32)(value1 - value2);
        }
      }
    }

    /// <inheritdoc/>
    public override UInt32 Multiply(UInt32 value, double factor)
    {
      if (OverflowAllowed) {
        unchecked{
          return (UInt32)((double)value*factor);
        }
      }
      else {
        checked{
          return (UInt32)((double)value*factor);
        }
      }
    }

    /// <inheritdoc/>
    public override UInt32 Divide(UInt32 value, double factor)
    {
      if (OverflowAllowed) {
        unchecked{
          return (UInt32)((double)value/factor);
        }
      }
      else {
        checked{
          return (UInt32)((double)value/factor);
        }
      }
    }

    /// <inheritdoc/>
    protected override IArithmetic<UInt32> CreateNew(ArithmeticRules rules)
    {
      return new UInt32Arithmetic(Provider, rules);
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    public UInt32Arithmetic(IArithmeticProvider provider, ArithmeticRules rule)
      : base(provider, rule)
    {
    }
  }
  
  [Serializable]
  internal sealed class Int64Arithmetic
    : ArithmeticBase<Int64>
  {
    private const Int64 zero     = (Int64) 0;
    private const Int64 one      = (Int64) 1;
    private const Int64 minusOne = unchecked ((Int64) (-1));

    /// <inheritdoc/>
    public override Int64 Zero
    {
      get { return zero; }
    }

    /// <inheritdoc/>
    public override Int64 One
    {
      get { return one; }
    }

	/// <inheritdoc/>
    public override Int64 MaxValue
    {
      get { return Int64.MaxValue; }
    }
    
    /// <inheritdoc/>
    public override Int64 MinValue
    {
      get { return Int64.MinValue; }
    }
    
    /// <inheritdoc/>
    public override bool IsSigned
    {
      get { return Int64.MinValue != zero; }
    }

    /// <inheritdoc/>
    public override Int64 Add(Int64 value1, Int64 value2)
    {
      if (OverflowAllowed) {
        unchecked{
          return (Int64)(value1 + value2);
        }
      }
      else {
        checked{
          return (Int64)(value1 + value2);
        }
      }
    }

    /// <inheritdoc/>
    public override Int64 Negation(Int64 value)
    {
      if (OverflowAllowed) {
        unchecked{
          return (Int64)(minusOne*value);
        }
      }
      else {
        checked{
          return (Int64)(minusOne*value);
        }
      }
    }

    /// <inheritdoc/>
    public override Int64 Subtract(Int64 value1, Int64 value2)
    {
      if (OverflowAllowed) {
        unchecked{
          return (Int64)(value1 - value2);
        }
      }
      else {
        checked{
          return (Int64)(value1 - value2);
        }
      }
    }

    /// <inheritdoc/>
    public override Int64 Multiply(Int64 value, double factor)
    {
      if (OverflowAllowed) {
        unchecked{
          return (Int64)((double)value*factor);
        }
      }
      else {
        checked{
          return (Int64)((double)value*factor);
        }
      }
    }

    /// <inheritdoc/>
    public override Int64 Divide(Int64 value, double factor)
    {
      if (OverflowAllowed) {
        unchecked{
          return (Int64)((double)value/factor);
        }
      }
      else {
        checked{
          return (Int64)((double)value/factor);
        }
      }
    }

    /// <inheritdoc/>
    protected override IArithmetic<Int64> CreateNew(ArithmeticRules rules)
    {
      return new Int64Arithmetic(Provider, rules);
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    public Int64Arithmetic(IArithmeticProvider provider, ArithmeticRules rule)
      : base(provider, rule)
    {
    }
  }
  
  [Serializable]
  internal sealed class UInt64Arithmetic
    : ArithmeticBase<UInt64>
  {
    private const UInt64 zero     = (UInt64) 0;
    private const UInt64 one      = (UInt64) 1;
    private const UInt64 minusOne = unchecked ((UInt64) (-1));

    /// <inheritdoc/>
    public override UInt64 Zero
    {
      get { return zero; }
    }

    /// <inheritdoc/>
    public override UInt64 One
    {
      get { return one; }
    }

	/// <inheritdoc/>
    public override UInt64 MaxValue
    {
      get { return UInt64.MaxValue; }
    }
    
    /// <inheritdoc/>
    public override UInt64 MinValue
    {
      get { return UInt64.MinValue; }
    }
    
    /// <inheritdoc/>
    public override bool IsSigned
    {
      get { return UInt64.MinValue != zero; }
    }

    /// <inheritdoc/>
    public override UInt64 Add(UInt64 value1, UInt64 value2)
    {
      if (OverflowAllowed) {
        unchecked{
          return (UInt64)(value1 + value2);
        }
      }
      else {
        checked{
          return (UInt64)(value1 + value2);
        }
      }
    }

    /// <inheritdoc/>
    public override UInt64 Negation(UInt64 value)
    {
      if (OverflowAllowed) {
        unchecked{
          return (UInt64)(minusOne*value);
        }
      }
      else {
        checked{
          return (UInt64)(minusOne*value);
        }
      }
    }

    /// <inheritdoc/>
    public override UInt64 Subtract(UInt64 value1, UInt64 value2)
    {
      if (OverflowAllowed) {
        unchecked{
          return (UInt64)(value1 - value2);
        }
      }
      else {
        checked{
          return (UInt64)(value1 - value2);
        }
      }
    }

    /// <inheritdoc/>
    public override UInt64 Multiply(UInt64 value, double factor)
    {
      if (OverflowAllowed) {
        unchecked{
          return (UInt64)((double)value*factor);
        }
      }
      else {
        checked{
          return (UInt64)((double)value*factor);
        }
      }
    }

    /// <inheritdoc/>
    public override UInt64 Divide(UInt64 value, double factor)
    {
      if (OverflowAllowed) {
        unchecked{
          return (UInt64)((double)value/factor);
        }
      }
      else {
        checked{
          return (UInt64)((double)value/factor);
        }
      }
    }

    /// <inheritdoc/>
    protected override IArithmetic<UInt64> CreateNew(ArithmeticRules rules)
    {
      return new UInt64Arithmetic(Provider, rules);
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    public UInt64Arithmetic(IArithmeticProvider provider, ArithmeticRules rule)
      : base(provider, rule)
    {
    }
  }
  
  [Serializable]
  internal sealed class DecimalArithmetic
    : ArithmeticBase<Decimal>
  {
    private const Decimal zero     = (Decimal) 0;
    private const Decimal one      = (Decimal) 1;
    private const Decimal minusOne = unchecked ((Decimal) (-1));

    /// <inheritdoc/>
    public override Decimal Zero
    {
      get { return zero; }
    }

    /// <inheritdoc/>
    public override Decimal One
    {
      get { return one; }
    }

	/// <inheritdoc/>
    public override Decimal MaxValue
    {
      get { return Decimal.MaxValue; }
    }
    
    /// <inheritdoc/>
    public override Decimal MinValue
    {
      get { return Decimal.MinValue; }
    }
    
    /// <inheritdoc/>
    public override bool IsSigned
    {
      get { return Decimal.MinValue != zero; }
    }

    /// <inheritdoc/>
    public override Decimal Add(Decimal value1, Decimal value2)
    {
      if (OverflowAllowed) {
        unchecked{
          return (Decimal)(value1 + value2);
        }
      }
      else {
        checked{
          return (Decimal)(value1 + value2);
        }
      }
    }

    /// <inheritdoc/>
    public override Decimal Negation(Decimal value)
    {
      if (OverflowAllowed) {
        unchecked{
          return (Decimal)(minusOne*value);
        }
      }
      else {
        checked{
          return (Decimal)(minusOne*value);
        }
      }
    }

    /// <inheritdoc/>
    public override Decimal Subtract(Decimal value1, Decimal value2)
    {
      if (OverflowAllowed) {
        unchecked{
          return (Decimal)(value1 - value2);
        }
      }
      else {
        checked{
          return (Decimal)(value1 - value2);
        }
      }
    }

    /// <inheritdoc/>
    public override Decimal Multiply(Decimal value, double factor)
    {
      if (OverflowAllowed) {
        unchecked{
          return (Decimal)((double)value*factor);
        }
      }
      else {
        checked{
          return (Decimal)((double)value*factor);
        }
      }
    }

    /// <inheritdoc/>
    public override Decimal Divide(Decimal value, double factor)
    {
      if (OverflowAllowed) {
        unchecked{
          return (Decimal)((double)value/factor);
        }
      }
      else {
        checked{
          return (Decimal)((double)value/factor);
        }
      }
    }

    /// <inheritdoc/>
    protected override IArithmetic<Decimal> CreateNew(ArithmeticRules rules)
    {
      return new DecimalArithmetic(Provider, rules);
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    public DecimalArithmetic(IArithmeticProvider provider, ArithmeticRules rule)
      : base(provider, rule)
    {
    }
  }
  
  [Serializable]
  internal sealed class SingleArithmetic
    : ArithmeticBase<Single>
  {
    private const Single zero     = (Single) 0;
    private const Single one      = (Single) 1;
    private const Single minusOne = unchecked ((Single) (-1));

    /// <inheritdoc/>
    public override Single Zero
    {
      get { return zero; }
    }

    /// <inheritdoc/>
    public override Single One
    {
      get { return one; }
    }

	/// <inheritdoc/>
    public override Single MaxValue
    {
      get { return Single.MaxValue; }
    }
    
    /// <inheritdoc/>
    public override Single MinValue
    {
      get { return Single.MinValue; }
    }
    
    /// <inheritdoc/>
    public override bool IsSigned
    {
      get { return Single.MinValue != zero; }
    }

    /// <inheritdoc/>
    public override Single Add(Single value1, Single value2)
    {
      if (OverflowAllowed) {
        unchecked{
          return (Single)(value1 + value2);
        }
      }
      else {
        checked{
          return (Single)(value1 + value2);
        }
      }
    }

    /// <inheritdoc/>
    public override Single Negation(Single value)
    {
      if (OverflowAllowed) {
        unchecked{
          return (Single)(minusOne*value);
        }
      }
      else {
        checked{
          return (Single)(minusOne*value);
        }
      }
    }

    /// <inheritdoc/>
    public override Single Subtract(Single value1, Single value2)
    {
      if (OverflowAllowed) {
        unchecked{
          return (Single)(value1 - value2);
        }
      }
      else {
        checked{
          return (Single)(value1 - value2);
        }
      }
    }

    /// <inheritdoc/>
    public override Single Multiply(Single value, double factor)
    {
      if (OverflowAllowed) {
        unchecked{
          return (Single)((double)value*factor);
        }
      }
      else {
        checked{
          return (Single)((double)value*factor);
        }
      }
    }

    /// <inheritdoc/>
    public override Single Divide(Single value, double factor)
    {
      if (OverflowAllowed) {
        unchecked{
          return (Single)((double)value/factor);
        }
      }
      else {
        checked{
          return (Single)((double)value/factor);
        }
      }
    }

    /// <inheritdoc/>
    protected override IArithmetic<Single> CreateNew(ArithmeticRules rules)
    {
      return new SingleArithmetic(Provider, rules);
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    public SingleArithmetic(IArithmeticProvider provider, ArithmeticRules rule)
      : base(provider, rule)
    {
    }
  }
  
  [Serializable]
  internal sealed class DoubleArithmetic
    : ArithmeticBase<Double>
  {
    private const Double zero     = (Double) 0;
    private const Double one      = (Double) 1;
    private const Double minusOne = unchecked ((Double) (-1));

    /// <inheritdoc/>
    public override Double Zero
    {
      get { return zero; }
    }

    /// <inheritdoc/>
    public override Double One
    {
      get { return one; }
    }

	/// <inheritdoc/>
    public override Double MaxValue
    {
      get { return Double.MaxValue; }
    }
    
    /// <inheritdoc/>
    public override Double MinValue
    {
      get { return Double.MinValue; }
    }
    
    /// <inheritdoc/>
    public override bool IsSigned
    {
      get { return Double.MinValue != zero; }
    }

    /// <inheritdoc/>
    public override Double Add(Double value1, Double value2)
    {
      if (OverflowAllowed) {
        unchecked{
          return (Double)(value1 + value2);
        }
      }
      else {
        checked{
          return (Double)(value1 + value2);
        }
      }
    }

    /// <inheritdoc/>
    public override Double Negation(Double value)
    {
      if (OverflowAllowed) {
        unchecked{
          return (Double)(minusOne*value);
        }
      }
      else {
        checked{
          return (Double)(minusOne*value);
        }
      }
    }

    /// <inheritdoc/>
    public override Double Subtract(Double value1, Double value2)
    {
      if (OverflowAllowed) {
        unchecked{
          return (Double)(value1 - value2);
        }
      }
      else {
        checked{
          return (Double)(value1 - value2);
        }
      }
    }

    /// <inheritdoc/>
    public override Double Multiply(Double value, double factor)
    {
      if (OverflowAllowed) {
        unchecked{
          return (Double)((double)value*factor);
        }
      }
      else {
        checked{
          return (Double)((double)value*factor);
        }
      }
    }

    /// <inheritdoc/>
    public override Double Divide(Double value, double factor)
    {
      if (OverflowAllowed) {
        unchecked{
          return (Double)((double)value/factor);
        }
      }
      else {
        checked{
          return (Double)((double)value/factor);
        }
      }
    }

    /// <inheritdoc/>
    protected override IArithmetic<Double> CreateNew(ArithmeticRules rules)
    {
      return new DoubleArithmetic(Provider, rules);
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    public DoubleArithmetic(IArithmeticProvider provider, ArithmeticRules rule)
      : base(provider, rule)
    {
    }
  }
  
}
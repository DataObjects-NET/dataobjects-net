// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.22

using System;

namespace Xtensive.SizeCalculators
{
  [Serializable]
  internal class BooleanSizeCalculator : SizeCalculatorBase<Boolean>
  {
    public override int GetDefaultSize()
    {
      return sizeof (Boolean);
    }

    public override int GetValueSize(Boolean value)
    {
      return sizeof (Boolean);
    }


    // Constructors

    public BooleanSizeCalculator(ISizeCalculatorProvider provider)
      : base(provider)
    {
    }
  }
  
  [Serializable]
  internal class ByteSizeCalculator : SizeCalculatorBase<Byte>
  {
    public override int GetDefaultSize()
    {
      return sizeof (Byte);
    }

    public override int GetValueSize(Byte value)
    {
      return sizeof (Byte);
    }


    // Constructors

    public ByteSizeCalculator(ISizeCalculatorProvider provider)
      : base(provider)
    {
    }
  }
  
  [Serializable]
  internal class SByteSizeCalculator : SizeCalculatorBase<SByte>
  {
    public override int GetDefaultSize()
    {
      return sizeof (SByte);
    }

    public override int GetValueSize(SByte value)
    {
      return sizeof (SByte);
    }


    // Constructors

    public SByteSizeCalculator(ISizeCalculatorProvider provider)
      : base(provider)
    {
    }
  }
  
  [Serializable]
  internal class CharSizeCalculator : SizeCalculatorBase<Char>
  {
    public override int GetDefaultSize()
    {
      return sizeof (Char);
    }

    public override int GetValueSize(Char value)
    {
      return sizeof (Char);
    }


    // Constructors

    public CharSizeCalculator(ISizeCalculatorProvider provider)
      : base(provider)
    {
    }
  }
  
  [Serializable]
  internal class Int16SizeCalculator : SizeCalculatorBase<Int16>
  {
    public override int GetDefaultSize()
    {
      return sizeof (Int16);
    }

    public override int GetValueSize(Int16 value)
    {
      return sizeof (Int16);
    }


    // Constructors

    public Int16SizeCalculator(ISizeCalculatorProvider provider)
      : base(provider)
    {
    }
  }
  
  [Serializable]
  internal class UInt16SizeCalculator : SizeCalculatorBase<UInt16>
  {
    public override int GetDefaultSize()
    {
      return sizeof (UInt16);
    }

    public override int GetValueSize(UInt16 value)
    {
      return sizeof (UInt16);
    }


    // Constructors

    public UInt16SizeCalculator(ISizeCalculatorProvider provider)
      : base(provider)
    {
    }
  }
  
  [Serializable]
  internal class Int32SizeCalculator : SizeCalculatorBase<Int32>
  {
    public override int GetDefaultSize()
    {
      return sizeof (Int32);
    }

    public override int GetValueSize(Int32 value)
    {
      return sizeof (Int32);
    }


    // Constructors

    public Int32SizeCalculator(ISizeCalculatorProvider provider)
      : base(provider)
    {
    }
  }
  
  [Serializable]
  internal class UInt32SizeCalculator : SizeCalculatorBase<UInt32>
  {
    public override int GetDefaultSize()
    {
      return sizeof (UInt32);
    }

    public override int GetValueSize(UInt32 value)
    {
      return sizeof (UInt32);
    }


    // Constructors

    public UInt32SizeCalculator(ISizeCalculatorProvider provider)
      : base(provider)
    {
    }
  }
  
  [Serializable]
  internal class Int64SizeCalculator : SizeCalculatorBase<Int64>
  {
    public override int GetDefaultSize()
    {
      return sizeof (Int64);
    }

    public override int GetValueSize(Int64 value)
    {
      return sizeof (Int64);
    }


    // Constructors

    public Int64SizeCalculator(ISizeCalculatorProvider provider)
      : base(provider)
    {
    }
  }
  
  [Serializable]
  internal class UInt64SizeCalculator : SizeCalculatorBase<UInt64>
  {
    public override int GetDefaultSize()
    {
      return sizeof (UInt64);
    }

    public override int GetValueSize(UInt64 value)
    {
      return sizeof (UInt64);
    }


    // Constructors

    public UInt64SizeCalculator(ISizeCalculatorProvider provider)
      : base(provider)
    {
    }
  }
  
  [Serializable]
  internal class SingleSizeCalculator : SizeCalculatorBase<Single>
  {
    public override int GetDefaultSize()
    {
      return sizeof (Single);
    }

    public override int GetValueSize(Single value)
    {
      return sizeof (Single);
    }


    // Constructors

    public SingleSizeCalculator(ISizeCalculatorProvider provider)
      : base(provider)
    {
    }
  }
  
  [Serializable]
  internal class DoubleSizeCalculator : SizeCalculatorBase<Double>
  {
    public override int GetDefaultSize()
    {
      return sizeof (Double);
    }

    public override int GetValueSize(Double value)
    {
      return sizeof (Double);
    }


    // Constructors

    public DoubleSizeCalculator(ISizeCalculatorProvider provider)
      : base(provider)
    {
    }
  }
  
}
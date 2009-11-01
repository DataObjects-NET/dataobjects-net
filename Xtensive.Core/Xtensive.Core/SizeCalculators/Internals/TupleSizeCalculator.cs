// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.05.13

using System;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Reflection;
using Xtensive.Core.Tuples;

namespace Xtensive.Core.SizeCalculators
{
  [Serializable]
  internal class TupleSizeCalculator : SizeCalculatorBase<Tuple>,
    IFinalAssociate,
    ITupleFunctionHandler<TupleSizeCalculator.TupleSizeCalculatorData, int>
  {
    private readonly object _lock = new object();
    private ThreadSafeDictionary<TupleDescriptor, ISizeCalculatorBase[]> cache = ThreadSafeDictionary<TupleDescriptor, ISizeCalculatorBase[]>.Create();

    #region Nested type: TupleSizeCalculatorData 

    private struct TupleSizeCalculatorData : ITupleFunctionData<int>
    {
      public int Result;
      public readonly Tuple Tuple;
      public readonly ISizeCalculatorBase[] Calculators;

      int ITupleFunctionData<int>.Result
      {
        get { return Result; }
      }

      public TupleSizeCalculatorData(Tuple tuple, ISizeCalculatorBase[] calculators)
      {
        Result = 0;
        Tuple = tuple;
        Calculators = calculators;
      }
    }

    #endregion

    /// <inheritdoc/>
    public override int GetValueSize(Tuple value)
    {
      if (value==null) 
        return SizeCalculatorProvider.PointerFieldSize;

      TupleSizeCalculatorData data = new TupleSizeCalculatorData(value, GetCalculators(value));
      int result = value.Descriptor.Execute(this, ref data, Direction.Positive);

      return result + SizeCalculatorProvider.PointerFieldSize + SizeCalculatorProvider.HeapObjectHeaderSize;
    }

    #region Private \ internal methods

    bool ITupleActionHandler<TupleSizeCalculatorData>.Execute<TFieldType>(ref TupleSizeCalculatorData actionData, int fieldIndex)
    {
      ISizeCalculatorBase sizeCalculator = actionData.Calculators[fieldIndex];
      ISizeCalculator<TFieldType> calculator = sizeCalculator as ISizeCalculator<TFieldType>;
      
      if (!actionData.Tuple.HasValue(fieldIndex)) {
        if (calculator == null) // Unknown means there is reference field
          actionData.Result += SizeCalculatorProvider.PointerFieldSize; 
        else
          actionData.Result += calculator.GetDefaultSize();
      }
      else {
        TFieldType value = actionData.Tuple.GetValue<TFieldType>(fieldIndex);
        if (calculator == null)
          actionData.Result += Provider.GetSizeCalculatorByInstance(value).GetInstanceSize(value); 
        else
          actionData.Result += calculator.GetValueSize(value);
      }
      return false;
    }

    private ISizeCalculatorBase[] GetCalculators(Tuple tuple)
    {
      TupleDescriptor descriptor = tuple.Descriptor;
      ISizeCalculatorBase[] calculators = cache.GetValue(descriptor);
      if (calculators!=null)
        return calculators;
      lock (_lock) {
        calculators = cache.GetValue(descriptor);
        if (calculators!=null)
          return calculators;
        int count = descriptor.Count;
        calculators = new ISizeCalculatorBase[count];
        for (int i = 0; i < count; i++) {
          Type fieldType = tuple.Descriptor[i];
          ISizeCalculatorBase calculator = Provider.GetSizeCalculatorByType(fieldType);
          if (!fieldType.IsFinal() && !(calculator is IFinalAssociate))
            calculator = null;
          calculators[i] = calculator;
        }
        cache.SetValue(descriptor, calculators);
      }
      return calculators;
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    public TupleSizeCalculator(ISizeCalculatorProvider provider)
      : base(provider)
    {
    }
  }
}
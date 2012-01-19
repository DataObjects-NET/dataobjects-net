// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.05.13

using System;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.IoC;
using Xtensive.Reflection;
using Xtensive.Threading;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.SizeCalculators
{
  [Serializable]
  internal class TupleSizeCalculator : SizeCalculatorBase<Tuples.Tuple>,
    IFinalAssociate
  {
    private readonly object _lock = new object();

    private ThreadSafeDictionary<TupleDescriptor, ISizeCalculatorBase[]> calculators =
      ThreadSafeDictionary<TupleDescriptor, ISizeCalculatorBase[]>.Create(new object());

    #region Nested type: TupleSizeCalculatorData 

    private struct TupleSizeCalculatorData
    {
      public int Result;
      public readonly Tuples.Tuple Tuple;
      public readonly ISizeCalculatorBase[] Calculators;

      public TupleSizeCalculatorData(Tuples.Tuple tuple, ISizeCalculatorBase[] calculators)
      {
        Result = 0;
        Tuple = tuple;
        Calculators = calculators;
      }
    }

    private class SizeCulculatorHandler
    {
      public ExecutionSequenceHandler<TupleSizeCalculatorData>[] Handlers;

      public SizeCulculatorHandler(TupleDescriptor descriptor)
      {
        Handlers = new ExecutionSequenceHandler<TupleSizeCalculatorData>[descriptor.Count];
      }
    }

    #endregion

    [NonSerialized]
    private ThreadSafeList<SizeCulculatorHandler> sizeCulculatorHandlers;

    /// <inheritdoc/>
    public override int GetValueSize(Tuples.Tuple value)
    {
      if (value==null)
        return SizeCalculatorProvider.PointerFieldSize;

      var data = new TupleSizeCalculatorData(value, GetCalculators(value));
      SizeCulculatorHandler h = GetSizeCulculatorHandler(value.Descriptor);
      DelegateHelper.ExecuteDelegates(h.Handlers, ref data, Direction.Positive);
      return data.Result
        + SizeCalculatorProvider.PointerFieldSize
          + SizeCalculatorProvider.HeapObjectHeaderSize;
    }

    #region Private \ internal methods

    private SizeCulculatorHandler GetSizeCulculatorHandler(TupleDescriptor descriptor)
    {
      return sizeCulculatorHandlers.GetValue(descriptor.Identifier,
        (indentifier, _this, _descriptor) => {
          var box = new Box<SizeCulculatorHandler>(new SizeCulculatorHandler(descriptor));
          ExecutionSequenceHandler<Box<SizeCulculatorHandler>>[] initializers =
            DelegateHelper.CreateDelegates<ExecutionSequenceHandler<Box<SizeCulculatorHandler>>>(
              _this, _this.GetType(), "InitializeStep", _descriptor);
          DelegateHelper.ExecuteDelegates(initializers, ref box, Direction.Positive);
          return box.Value;
        },
        this, descriptor);
    }

    private bool InitializeStep<TFieldType>(ref Box<SizeCulculatorHandler> data, int fieldIndex)
    {
      data.Value.Handlers[fieldIndex] = Execute<TFieldType>;
      return false;
    }

    private bool Execute<TFieldType>(ref TupleSizeCalculatorData actionData, int fieldIndex)
    {
      var sizeCalculator = actionData.Calculators[fieldIndex];
      var calculator = sizeCalculator as ISizeCalculator<TFieldType>;

      if (!actionData.Tuple.GetFieldState(fieldIndex).HasValue()) {
        if (calculator==null) // Unknown means there is reference field
          actionData.Result += SizeCalculatorProvider.PointerFieldSize;
        else
          actionData.Result += calculator.GetDefaultSize();
      }
      else {
        var value = actionData.Tuple.GetValue<TFieldType>(fieldIndex);
        if (calculator==null)
          actionData.Result += Provider.GetSizeCalculatorByInstance(value).GetInstanceSize(value);
        else
          actionData.Result += calculator.GetValueSize(value);
      }
      return false;
    }

    private ISizeCalculatorBase[] GetCalculators(Tuples.Tuple tuple)
    {
      return calculators.GetValue(tuple.Descriptor,
        (descriptor, _this) => {
          int count = descriptor.Count;
          var result = new ISizeCalculatorBase[count];
          for (int i = 0; i < count; i++) {
            Type fieldType = descriptor[i];
            ISizeCalculatorBase calculator = _this.Provider.GetSizeCalculatorByType(fieldType);
            if (!fieldType.IsFinal() && !(calculator is IFinalAssociate))
              calculator = null;
            result[i] = calculator;
          }
          return result;
        },
        this);
    }

    private void Initialize()
    {
      sizeCulculatorHandlers.Initialize(new object());
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    public TupleSizeCalculator(ISizeCalculatorProvider provider)
      : base(provider)
    {
      Initialize();
    }
  }
}
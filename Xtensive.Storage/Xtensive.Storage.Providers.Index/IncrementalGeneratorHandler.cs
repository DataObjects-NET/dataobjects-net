// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.24

using System;
using Xtensive.Core.Arithmetic;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Providers.Index
{
  [Serializable]
  public class IncrementalGeneratorHandler : Providers.IncrementalGeneratorHandler
  {
    private object counter;
    private readonly ActionHandler handler = new ActionHandler();
    private ActionData data;

    #region Nested members

    private class ActionHandler : ITupleActionHandler<ActionData>
    {
      public bool Execute<TFieldType>(ref ActionData actionData, int fieldIndex)
      {
        Arithmetic<TFieldType> arithmetic = Arithmetic<TFieldType>.Default;
        TFieldType value = actionData.Value == null ? arithmetic.Zero : (TFieldType) actionData.Value;
        actionData.Tuple.SetValue(fieldIndex, arithmetic.Add(value, arithmetic.One));
        return true;
      }
    }

    private struct ActionData
    {
      public object Value;
      public Tuple Tuple;
    }

    #endregion

    /// <inheritdoc/>
    public override void Fill(Tuple tuple)
    {
      data.Value = counter;
      data.Tuple = tuple;
      tuple.Descriptor.Execute(handler, ref data, 0);
      counter = tuple.GetValue(0);
    }
  }
}
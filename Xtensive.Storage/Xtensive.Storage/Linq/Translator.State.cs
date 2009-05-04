// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.05.04

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core.Collections;
using Xtensive.Core.Disposing;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Linq.Expressions.Mappings;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Linq
{
  internal sealed partial class Translator
  {
    private State state;

    private sealed class State
    {
      private readonly Translator translator;
      private List<CalculatedColumnDescriptor> calculatedColumns;
      private ParameterExpression[] parameters;
      private ParameterExpression[] outerParameters;
      private MappingReference mappingRef;
      private ParameterExpression tuple;
      private ParameterExpression record;
      private bool entityAsKey;
      private bool calculateExpressions;
      private bool recordIsUsed;

      public List<CalculatedColumnDescriptor> CalculatedColumns
      {
        get { return calculatedColumns; }
        set { calculatedColumns = value; }
      }

      public MappingReference MappingRef
      {
        get { return mappingRef; }
        set { mappingRef = value; }
      }

      public ParameterExpression[] Parameters
      {
        get { return parameters; }
        set { parameters = value; }
      }

      public ParameterExpression[] OuterParameters
      {
        get { return outerParameters; }
        set { outerParameters = value; }
      }

      public ParameterExpression Tuple
      {
        get { return tuple; }
      }

      public ParameterExpression Record
      {
        get { return record; }
      }

      public bool EntityAsKey
      {
        get { return entityAsKey; }
        set { entityAsKey = value; }
      }

      public bool CalculateExpressions
      {
        get { return calculateExpressions; }
        set { calculateExpressions = value; }
      }

      public bool RecordIsUsed
      {
        get { return recordIsUsed; }
        set
        {
          if (value) {
            if (!entityAsKey)
              recordIsUsed = true;
          }
          else
            recordIsUsed = false;
        }
      }

      public IDisposable CreateScope()
      {
        var currentState = translator.state;
        var newState = new State(currentState);
        translator.state = newState;
        return new Disposable((b) => {
          currentState.RecordIsUsed = currentState.RecordIsUsed | newState.RecordIsUsed;
          translator.state = currentState;
        });
      }

      public IDisposable CreateLambdaScope(LambdaExpression le)
      {
        var currentState = translator.state;
        var newState = new State(currentState);
        newState.RecordIsUsed = false;
        newState.tuple = Expression.Parameter(typeof(Tuple), "t");
        newState.record = Expression.Parameter(typeof(Record), "r");
        newState.outerParameters = newState.outerParameters.Concat(newState.parameters).ToArray();
        newState.parameters = le.Parameters.ToArray();
        newState.calculatedColumns = new List<CalculatedColumnDescriptor>();
        translator.state = newState;
        return new Disposable((b) => translator.state = currentState);
      }


      // Constructors

      public State(Translator translator)
      {
        this.translator = translator;
        entityAsKey = true;
        outerParameters = parameters = ArrayUtils<ParameterExpression>.EmptyArray;
        calculatedColumns = new List<CalculatedColumnDescriptor>();
        mappingRef = new MappingReference();
      }

      private State(State currentState)
      {
        translator = currentState.translator;
        calculatedColumns = currentState.calculatedColumns;
        parameters = currentState.parameters;
        outerParameters = currentState.outerParameters;
        mappingRef = currentState.mappingRef;
        tuple = currentState.tuple;
        record = currentState.record;
        calculateExpressions = currentState.calculateExpressions;
        entityAsKey = currentState.entityAsKey;
        recordIsUsed = currentState.recordIsUsed;
      }
    }
  }
}
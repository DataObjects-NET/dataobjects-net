// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2010.01.21

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core.Collections;
using Xtensive.Core.Disposing;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Linq
{
  internal sealed class TranslatorState
  {
      private readonly Translator translator;
      private List<CalculatedColumnDescriptor> calculatedColumns;
      private ParameterExpression[] parameters;
      private ParameterExpression[] outerParameters;
      private bool buildingProjection;
      private bool calculateExpressions;
      private LambdaExpression currentLambda;
      private bool joinLocalCollectionEntity;
      private IncludeAlgorithm includeAlgorithm = IncludeAlgorithm.Auto;

      public IncludeAlgorithm IncludeAlgorithm
      {
        get { return includeAlgorithm; }
        set { includeAlgorithm = value; }
      }

      public bool JoinLocalCollectionEntity
      {
        get { return joinLocalCollectionEntity; }
        set { joinLocalCollectionEntity = value; }
      }

      public List<CalculatedColumnDescriptor> CalculatedColumns
      {
        get { return calculatedColumns; }
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

      public bool BuildingProjection
      {
        get { return buildingProjection; }
        set { buildingProjection = value; }
      }

      public bool CalculateExpressions
      {
        get { return calculateExpressions; }
        set { calculateExpressions = value; }
      }

      public LambdaExpression CurrentLambda
      {
        get { return currentLambda; }
        set { currentLambda = value; }
      }

      public IDisposable CreateScope()
      {
        var currentState = translator.state;
        var newState = new TranslatorState(currentState);
        translator.state = newState;
        return new Disposable(_ => translator.state = currentState);
      }

      public IDisposable CreateLambdaScope(LambdaExpression le)
      {
        var currentState = translator.state;
        var newState = new TranslatorState(currentState);
        newState.outerParameters = newState.outerParameters.Concat(newState.parameters).ToArray();
        newState.parameters = Enumerable.ToArray(le.Parameters);
        newState.calculatedColumns = new List<CalculatedColumnDescriptor>();
        newState.currentLambda = le;
        newState.includeAlgorithm = includeAlgorithm;
        translator.state = newState;
        return new Disposable(_ => translator.state = currentState);
      }


      // Constructors

      public TranslatorState(Translator translator)
      {
        this.translator = translator;
        buildingProjection = true;
        outerParameters = parameters = ArrayUtils<ParameterExpression>.EmptyArray;
        calculatedColumns = new List<CalculatedColumnDescriptor>();
      }

      private TranslatorState(TranslatorState currentState)
      {
        translator = currentState.translator;
        calculatedColumns = currentState.calculatedColumns;
        parameters = currentState.parameters;
        outerParameters = currentState.outerParameters;
        calculateExpressions = currentState.calculateExpressions;
        buildingProjection = currentState.buildingProjection;
        currentLambda = currentState.currentLambda;
        joinLocalCollectionEntity = currentState.joinLocalCollectionEntity;
        includeAlgorithm = currentState.includeAlgorithm;
      }
  }
}
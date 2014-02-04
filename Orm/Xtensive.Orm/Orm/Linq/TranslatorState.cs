// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2010.01.21

using System;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Collections;
using Xtensive.Core;

namespace Xtensive.Orm.Linq
{
  internal sealed class TranslatorState
  {
    private readonly Translator translator;

    public ParameterExpression[] Parameters { get; set; }

    public ParameterExpression[] OuterParameters { get; set; }

    public LambdaExpression CurrentLambda { get; set; }

    public IncludeAlgorithm IncludeAlgorithm { get; set; }

    public Type TypeOfEntityStoredInKey { get; set; }

    public bool JoinLocalCollectionEntity { get; set; }

    public bool AllowCalculableColumnCombine { get; set; }

    public bool BuildingProjection { get; set; }

    public bool CalculateExpressions { get; set; }

    public bool GroupingKey { get; set; }

    public bool IsTailMethod { get; set; }

    public bool RequestCalculateExpressions { get; set; }

    public bool RequestCalculateExpressionsOnce { get; set; }

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
      newState.OuterParameters = newState.OuterParameters.Concat(newState.Parameters).ToArray();
      newState.Parameters = Enumerable.ToArray(le.Parameters);
      newState.CurrentLambda = le;
      newState.IncludeAlgorithm = IncludeAlgorithm;
      newState.IsTailMethod = IsTailMethod;
      translator.state = newState;
      return new Disposable(_ => translator.state = currentState);
    }


    // Constructors

    public TranslatorState(Translator translator)
    {
      this.translator = translator;
      IncludeAlgorithm = IncludeAlgorithm.Auto;
      BuildingProjection = true;
      IsTailMethod = true;
      TypeOfEntityStoredInKey = null;
      OuterParameters = Parameters = ArrayUtils<ParameterExpression>.EmptyArray;
    }

    private TranslatorState(TranslatorState currentState)
    {
      translator = currentState.translator;
      Parameters = currentState.Parameters;
      OuterParameters = currentState.OuterParameters;
      CalculateExpressions = currentState.CalculateExpressions;
      BuildingProjection = currentState.BuildingProjection;
      CurrentLambda = currentState.CurrentLambda;
      JoinLocalCollectionEntity = currentState.JoinLocalCollectionEntity;
      AllowCalculableColumnCombine = currentState.AllowCalculableColumnCombine;
      IncludeAlgorithm = currentState.IncludeAlgorithm;
      IsTailMethod = currentState.IsTailMethod;
      GroupingKey = currentState.GroupingKey;
      RequestCalculateExpressionsOnce = currentState.RequestCalculateExpressionsOnce;
      RequestCalculateExpressions = currentState.RequestCalculateExpressions;
      TypeOfEntityStoredInKey = currentState.TypeOfEntityStoredInKey;
    }
  }
}
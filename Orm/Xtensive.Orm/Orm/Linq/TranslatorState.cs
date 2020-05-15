// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2010.01.21

using System;
using System.Linq.Expressions;
using Xtensive.Core;

namespace Xtensive.Orm.Linq
{
  internal sealed class TranslatorState
  {
    [Flags]
    private enum TranslatorStateFlags
    {
      JoinLocalCollectionEntity = 1,
      AllowCalculableColumnCombine = 1 << 1,
      IsBuildingProjection = 1 << 2,
      CalculateExpressions = 1 << 3,
      IsGroupingKey = 1 << 4,
      IsTailMethod = 1 << 5,
      ShouldOmitConvertToObject = 1 << 6,
      RequestCalculateExpressions = 1 << 7,
      RequestCalculateExpressionsOnce = 1 << 8
    }

    internal readonly ref struct TranslatorScope
    {
      private readonly TranslatorState previousState;
      private readonly Translator translator;

      public void Dispose() => translator.state = previousState;

      public TranslatorScope(Translator translator, TranslatorState previousState)
      {
        this.translator = translator;
        this.previousState = previousState;
      }
    }

    private readonly Translator translator;

    private TranslatorStateFlags flags;

    public ParameterExpression[] Parameters { get; set; }

    public ParameterExpression[] OuterParameters { get; set; }

    public LambdaExpression CurrentLambda { get; set; }

    public IncludeAlgorithm IncludeAlgorithm { get; set; }

    public Type TypeOfEntityStoredInKey { get; set; }

    public bool JoinLocalCollectionEntity
    {
      get => (flags & TranslatorStateFlags.JoinLocalCollectionEntity) != 0;
      set => flags = value
        ? flags | TranslatorStateFlags.JoinLocalCollectionEntity
        : flags & ~TranslatorStateFlags.JoinLocalCollectionEntity;
    }

    public bool AllowCalculableColumnCombine    {
      get => (flags & TranslatorStateFlags.AllowCalculableColumnCombine) != 0;
      set => flags = value
        ? flags | TranslatorStateFlags.AllowCalculableColumnCombine
        : flags & ~TranslatorStateFlags.AllowCalculableColumnCombine;
    }

    public bool BuildingProjection
    {
      get => (flags & TranslatorStateFlags.IsBuildingProjection) != 0;
      set => flags = value
        ? flags | TranslatorStateFlags.IsBuildingProjection
        : flags & ~TranslatorStateFlags.IsBuildingProjection;
    }

    public bool CalculateExpressions
    {
      get => (flags & TranslatorStateFlags.CalculateExpressions) != 0;
      set => flags = value
        ? flags | TranslatorStateFlags.CalculateExpressions
        : flags & ~TranslatorStateFlags.CalculateExpressions;
    }

    public bool GroupingKey
    {
      get => (flags & TranslatorStateFlags.IsGroupingKey) != 0;
      set => flags = value
        ? flags | TranslatorStateFlags.IsGroupingKey
        : flags & ~TranslatorStateFlags.IsGroupingKey;
    }

    public bool IsTailMethod
    {
      get => (flags & TranslatorStateFlags.IsTailMethod) != 0;
      set => flags = value
        ? flags | TranslatorStateFlags.IsTailMethod
        : flags & ~TranslatorStateFlags.IsTailMethod;
    }

    public bool ShouldOmitConvertToObject
    {
      get => (flags & TranslatorStateFlags.ShouldOmitConvertToObject) != 0;
      set => flags = value
        ? flags | TranslatorStateFlags.ShouldOmitConvertToObject
        : flags & ~TranslatorStateFlags.ShouldOmitConvertToObject;
    }

    public bool RequestCalculateExpressions
    {
      get => (flags & TranslatorStateFlags.RequestCalculateExpressions) != 0;
      set => flags = value
        ? flags | TranslatorStateFlags.RequestCalculateExpressions
        : flags & ~TranslatorStateFlags.RequestCalculateExpressions;
    }

    public bool RequestCalculateExpressionsOnce
    {
      get => (flags & TranslatorStateFlags.RequestCalculateExpressionsOnce) != 0;
      set => flags = value
        ? flags | TranslatorStateFlags.RequestCalculateExpressionsOnce
        : flags & ~TranslatorStateFlags.RequestCalculateExpressionsOnce;
    }

    public TranslatorScope CreateScope()
    {
      var currentState = translator.state;
      translator.state = new TranslatorState(currentState);
      return new TranslatorScope(translator, currentState);
    }

    public TranslatorScope CreateLambdaScope(LambdaExpression le)
    {
      var currentState = translator.state;
      var newState = new TranslatorState(currentState);
      var newOuterParameters = new ParameterExpression[newState.OuterParameters.Length + newState.Parameters.Length];
      newState.OuterParameters.CopyTo(newOuterParameters, 0);
      newState.Parameters.CopyTo(newOuterParameters, newState.OuterParameters.Length);
      newState.OuterParameters = newOuterParameters;
      newState.Parameters = le.Parameters.ToArray(le.Parameters.Count);
      newState.CurrentLambda = le;
      translator.state = newState;
      return new TranslatorScope(translator, currentState);
    }


    // Constructors

    public TranslatorState(Translator translator)
    {
      this.translator = translator;
      flags = TranslatorStateFlags.IsBuildingProjection | TranslatorStateFlags.IsTailMethod;
      OuterParameters = Parameters = Array.Empty<ParameterExpression>();
      IncludeAlgorithm = IncludeAlgorithm.Auto;
      TypeOfEntityStoredInKey = null;
    }

    private TranslatorState(TranslatorState currentState)
    {
      translator = currentState.translator;
      flags = currentState.flags;
      Parameters = currentState.Parameters;
      OuterParameters = currentState.OuterParameters;
      CurrentLambda = currentState.CurrentLambda;
      IncludeAlgorithm = currentState.IncludeAlgorithm;
      TypeOfEntityStoredInKey = currentState.TypeOfEntityStoredInKey;
    }
  }
}
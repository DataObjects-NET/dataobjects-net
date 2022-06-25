// Copyright (C) 2010-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Gamzov
// Created:    2010.01.21

using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Xtensive.Orm.Linq
{
  internal readonly struct TranslatorState
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
      RequestCalculateExpressionsOnce = 1 << 8,
      SkipNullableColumnsDetectionInGroupBy = 1 << 9
    }

    internal readonly struct TranslatorScope : IDisposable
    {
      private readonly TranslatorState previousState;
      private readonly Translator translator;

      public void Dispose() => translator.RestoreState(previousState);

      public TranslatorScope(Translator translator)
      {
        this.translator = translator;
        previousState = translator.State;
      }
    }

    public static readonly TranslatorState InitState = new TranslatorState {
      BuildingProjection = true,
      IsTailMethod = true,
      OuterParameters = Array.Empty<ParameterExpression>(),
      Parameters = Array.Empty<ParameterExpression>(),
      CurrentLambda = null,
      IncludeAlgorithm = IncludeAlgorithm.Auto,
      TypeOfEntityStoredInKey = null,
    };

    private readonly TranslatorStateFlags flags;

    public ParameterExpression[] Parameters { get; init; }

    public ParameterExpression[] OuterParameters { get; init; }

    public LambdaExpression CurrentLambda { get; init; }

    public IncludeAlgorithm IncludeAlgorithm { get; init; }

    public Type TypeOfEntityStoredInKey { get; init; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool GetFlag(TranslatorStateFlags f) => (flags & f) != 0;

    public bool JoinLocalCollectionEntity
    {
      get => GetFlag(TranslatorStateFlags.JoinLocalCollectionEntity);
      init => ModifyFlag(ref flags, TranslatorStateFlags.JoinLocalCollectionEntity, value);
    }

    public bool AllowCalculableColumnCombine
    {
      get => GetFlag(TranslatorStateFlags.AllowCalculableColumnCombine);
      init => ModifyFlag(ref flags, TranslatorStateFlags.AllowCalculableColumnCombine, value);
    }

    public bool BuildingProjection
    {
      get => GetFlag(TranslatorStateFlags.IsBuildingProjection);
      init => ModifyFlag(ref flags, TranslatorStateFlags.IsBuildingProjection, value);
    }

    public bool CalculateExpressions
    {
      get => GetFlag(TranslatorStateFlags.CalculateExpressions);
      init => ModifyFlag(ref flags, TranslatorStateFlags.CalculateExpressions, value);
    }

    public bool GroupingKey
    {
      get => GetFlag(TranslatorStateFlags.IsGroupingKey);
      init => ModifyFlag(ref flags, TranslatorStateFlags.IsGroupingKey, value);
    }

    public bool IsTailMethod
    {
      get => GetFlag(TranslatorStateFlags.IsTailMethod);
      init => ModifyFlag(ref flags, TranslatorStateFlags.IsTailMethod, value);
    }

    public bool ShouldOmitConvertToObject
    {
      get => GetFlag(TranslatorStateFlags.ShouldOmitConvertToObject);
      init => ModifyFlag(ref flags, TranslatorStateFlags.ShouldOmitConvertToObject, value);
    }

    public bool RequestCalculateExpressions
    {
      get => GetFlag(TranslatorStateFlags.RequestCalculateExpressions);
      init => ModifyFlag(ref flags, TranslatorStateFlags.RequestCalculateExpressions, value);
    }

    public bool RequestCalculateExpressionsOnce
    {
      get => GetFlag(TranslatorStateFlags.RequestCalculateExpressionsOnce);
      init => ModifyFlag(ref flags, TranslatorStateFlags.RequestCalculateExpressionsOnce, value);
    }

    public bool SkipNullableColumnsDetectionInGroupBy
    {
      get => GetFlag(TranslatorStateFlags.SkipNullableColumnsDetectionInGroupBy);
      init => ModifyFlag(ref flags, TranslatorStateFlags.SkipNullableColumnsDetectionInGroupBy, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ModifyFlag(ref TranslatorStateFlags flags, TranslatorStateFlags f, bool value) =>
      flags = value ? flags | f : flags & ~f;

    public TranslatorState(in TranslatorState currentState)
    {
      this = currentState;
    }
  }
}

#if !NET5_0_OR_GREATER
// Workaround of error CS0518: Predefined type 'System.Runtime.CompilerServices.IsExternalInit' is not defined or imported
namespace System.Runtime.CompilerServices
{
  internal static class IsExternalInit { }
}
#endif

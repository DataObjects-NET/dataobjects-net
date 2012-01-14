// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.29

using System;
using Xtensive.Core;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Internals
{
  internal static class TupleExtensions
  {
    public static bool ContainsEmptyValues(this Tuple target)
    {
      for (int i = 0; i < target.Count; i++) {
        var state = target.GetFieldState(i);
        if (!state.HasValue())
          return true;
      }
      return false;
    }

    public static bool ContainsNonEmptyValues(this Tuple target)
    {
      for (int i = 0; i < target.Count; i++) {
        var state = target.GetFieldState(i);
        if (state.HasValue())
          return true;
      }
      return false;
    }

    public static bool ContainsEmptyValues(this Tuple target, Segment<int> segment)
    {
      for (int i = segment.Offset; i < segment.EndOffset; i++) {
        var state = target.GetFieldState(i);
        if (!state.HasValue())
          return true;
      }
      return false;
    }

    public static bool ContainsNonEmptyValues(this Tuple target, Segment<int> segment)
    {
      for (int i = segment.Offset; i < segment.EndOffset; i++) {
        var state = target.GetFieldState(i);
        if (state.HasValue())
          return true;
      }
      return false;
    }

    public static bool AreAllColumnsAvalilable(this Tuple target, Segment<int> segment)
    {
      for (int i = segment.Offset; i < segment.EndOffset; i++) {
        var state = target.GetFieldState(i);
        if (!state.IsAvailable())
          return false;
      }
      return true;
    }

    public static bool IsAtLeastOneColumAvailable(this Tuple target, Segment<int> segment)
    {
      for (int i = segment.Offset; i < segment.EndOffset; i++) {
        var state = target.GetFieldState(i);
        if (state.IsAvailable())
          return true;
      }
      return false;
    }
  }
}
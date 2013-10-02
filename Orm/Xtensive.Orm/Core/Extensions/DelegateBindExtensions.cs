// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.06

using System;

namespace Xtensive.Core
{
  /// <summary>
  /// Extension methods for binding delegates to parameters.
  /// </summary>
  public static class DelegateBindExtensions
  {
    /// <summary>Binds first argument to specified delegate.</summary>
    /// <returns> A delegate that takes the rest of arguments of original delegate.</returns>
    public static Func<TResult> Bind<T1, TResult>(this Func<T1, TResult> d, T1 arg1)
    {
      if (d==null) return null; // someone's dirty hack
      return () => d.Invoke(arg1);
    }

    /// <summary>Binds first argument to specified delegate.</summary>
    /// <returns> A delegate that takes the rest of arguments of original delegate.</returns>
    public static Func<T2, TResult> Bind<T1, T2, TResult>(this Func<T1, T2, TResult> d, T1 arg1)
    {
      if (d==null) return null; // someone's dirty hack
      return (arg2) => d.Invoke(arg1, arg2);
    }

    /// <summary>Binds first argument to specified delegate.</summary>
    /// <returns> A delegate that takes the rest of arguments of original delegate.</returns>
    public static Func<T2, T3, TResult> Bind<T1, T2, T3, TResult>(this Func<T1, T2, T3, TResult> d, T1 arg1)
    {
      if (d==null) return null; // someone's dirty hack
      return (arg2, arg3) => d.Invoke(arg1, arg2, arg3);
    }

    /// <summary>Binds first argument to specified delegate.</summary>
    /// <returns> A delegate that takes the rest of arguments of original delegate.</returns>
    public static Func<T2, T3, T4, TResult> Bind<T1, T2, T3, T4, TResult>(this Func<T1, T2, T3, T4, TResult> d, T1 arg1)
    {
      if (d==null) return null; // someone's dirty hack
      return (arg2, arg3, arg4) => d.Invoke(arg1, arg2, arg3, arg4);
    }

    /// <summary>Binds first argument to specified delegate.</summary>
    /// <returns> A delegate that takes the rest of arguments of original delegate.</returns>
    public static Func<T2, T3, T4, T5, TResult> Bind<T1, T2, T3, T4, T5, TResult>(this Func<T1, T2, T3, T4, T5, TResult> d, T1 arg1)
    {
      if (d==null) return null; // someone's dirty hack
      return (arg2, arg3, arg4, arg5) => d.Invoke(arg1, arg2, arg3, arg4, arg5);
    }

    /// <summary>Binds first argument to specified delegate.</summary>
    /// <returns> A delegate that takes the rest of arguments of original delegate.</returns>
    public static Func<T2, T3, T4, T5, T6, TResult> Bind<T1, T2, T3, T4, T5, T6, TResult>(this Func<T1, T2, T3, T4, T5, T6, TResult> d, T1 arg1)
    {
      if (d==null) return null; // someone's dirty hack
      return (arg2, arg3, arg4, arg5, arg6) => d.Invoke(arg1, arg2, arg3, arg4, arg5, arg6);
    }

    /// <summary>Binds first argument to specified delegate.</summary>
    /// <returns> A delegate that takes the rest of arguments of original delegate.</returns>
    public static Func<T2, T3, T4, T5, T6, T7, TResult> Bind<T1, T2, T3, T4, T5, T6, T7, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, TResult> d, T1 arg1)
    {
      if (d==null) return null; // someone's dirty hack
      return (arg2, arg3, arg4, arg5, arg6, arg7) => d.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
    }

    /// <summary>Binds first argument to specified delegate.</summary>
    /// <returns> A delegate that takes the rest of arguments of original delegate.</returns>
    public static Func<T2, T3, T4, T5, T6, T7, T8, TResult> Bind<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> d, T1 arg1)
    {
      if (d==null) return null; // someone's dirty hack
      return (arg2, arg3, arg4, arg5, arg6, arg7, arg8) => d.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
    }

    /// <summary>Binds first argument to specified delegate.</summary>
    /// <returns> A delegate that takes the rest of arguments of original delegate.</returns>
    public static Func<T2, T3, T4, T5, T6, T7, T8, T9, TResult> Bind<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> d, T1 arg1)
    {
      if (d==null) return null; // someone's dirty hack
      return (arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9) => d.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
    }

    /// <summary>Binds first argument to specified delegate.</summary>
    /// <returns> A delegate that takes the rest of arguments of original delegate.</returns>
    public static Func<T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> Bind<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> d, T1 arg1)
    {
      if (d==null) return null; // someone's dirty hack
      return (arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10) => d.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
    }

    /// <summary>Binds first argument to specified delegate.</summary>
    /// <returns> A delegate that takes the rest of arguments of original delegate.</returns>
    public static Func<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> Bind<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> d, T1 arg1)
    {
      if (d==null) return null; // someone's dirty hack
      return (arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11) => d.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
    }

    /// <summary>Binds first argument to specified delegate.</summary>
    /// <returns> A delegate that takes the rest of arguments of original delegate.</returns>
    public static Func<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> Bind<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> d, T1 arg1)
    {
      if (d==null) return null; // someone's dirty hack
      return (arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12) => d.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
    }

    /// <summary>Binds first argument to specified delegate.</summary>
    /// <returns> A delegate that takes the rest of arguments of original delegate.</returns>
    public static Func<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> Bind<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> d, T1 arg1)
    {
      if (d==null) return null; // someone's dirty hack
      return (arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13) => d.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);
    }

    /// <summary>Binds first argument to specified delegate.</summary>
    /// <returns> A delegate that takes the rest of arguments of original delegate.</returns>
    public static Func<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> Bind<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> d, T1 arg1)
    {
      if (d==null) return null; // someone's dirty hack
      return (arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14) => d.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14);
    }

    /// <summary>Binds first argument to specified delegate.</summary>
    /// <returns> A delegate that takes the rest of arguments of original delegate.</returns>
    public static Func<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> Bind<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> d, T1 arg1)
    {
      if (d==null) return null; // someone's dirty hack
      return (arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15) => d.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15);
    }

    /// <summary>Binds first argument to specified delegate.</summary>
    /// <returns> A delegate that takes the rest of arguments of original delegate.</returns>
    public static Func<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> Bind<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> d, T1 arg1)
    {
      if (d==null) return null; // someone's dirty hack
      return (arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16) => d.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16);
    }

    /// <summary>Binds first argument to specified delegate.</summary>
    /// <returns> A delegate that takes the rest of arguments of original delegate.</returns>
    public static Action Bind<T1>(this Action<T1> d, T1 arg1)
    {
      if (d==null) return null; // someone's dirty hack
      return () => d.Invoke(arg1);
    }

    /// <summary>Binds first argument to specified delegate.</summary>
    /// <returns> A delegate that takes the rest of arguments of original delegate.</returns>
    public static Action<T2> Bind<T1, T2>(this Action<T1, T2> d, T1 arg1)
    {
      if (d==null) return null; // someone's dirty hack
      return (arg2) => d.Invoke(arg1, arg2);
    }

    /// <summary>Binds first argument to specified delegate.</summary>
    /// <returns> A delegate that takes the rest of arguments of original delegate.</returns>
    public static Action<T2, T3> Bind<T1, T2, T3>(this Action<T1, T2, T3> d, T1 arg1)
    {
      if (d==null) return null; // someone's dirty hack
      return (arg2, arg3) => d.Invoke(arg1, arg2, arg3);
    }

    /// <summary>Binds first argument to specified delegate.</summary>
    /// <returns> A delegate that takes the rest of arguments of original delegate.</returns>
    public static Action<T2, T3, T4> Bind<T1, T2, T3, T4>(this Action<T1, T2, T3, T4> d, T1 arg1)
    {
      if (d==null) return null; // someone's dirty hack
      return (arg2, arg3, arg4) => d.Invoke(arg1, arg2, arg3, arg4);
    }

    /// <summary>Binds first argument to specified delegate.</summary>
    /// <returns> A delegate that takes the rest of arguments of original delegate.</returns>
    public static Action<T2, T3, T4, T5> Bind<T1, T2, T3, T4, T5>(this Action<T1, T2, T3, T4, T5> d, T1 arg1)
    {
      if (d==null) return null; // someone's dirty hack
      return (arg2, arg3, arg4, arg5) => d.Invoke(arg1, arg2, arg3, arg4, arg5);
    }

    /// <summary>Binds first argument to specified delegate.</summary>
    /// <returns> A delegate that takes the rest of arguments of original delegate.</returns>
    public static Action<T2, T3, T4, T5, T6> Bind<T1, T2, T3, T4, T5, T6>(this Action<T1, T2, T3, T4, T5, T6> d, T1 arg1)
    {
      if (d==null) return null; // someone's dirty hack
      return (arg2, arg3, arg4, arg5, arg6) => d.Invoke(arg1, arg2, arg3, arg4, arg5, arg6);
    }

    /// <summary>Binds first argument to specified delegate.</summary>
    /// <returns> A delegate that takes the rest of arguments of original delegate.</returns>
    public static Action<T2, T3, T4, T5, T6, T7> Bind<T1, T2, T3, T4, T5, T6, T7>(this Action<T1, T2, T3, T4, T5, T6, T7> d, T1 arg1)
    {
      if (d==null) return null; // someone's dirty hack
      return (arg2, arg3, arg4, arg5, arg6, arg7) => d.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
    }

    /// <summary>Binds first argument to specified delegate.</summary>
    /// <returns> A delegate that takes the rest of arguments of original delegate.</returns>
    public static Action<T2, T3, T4, T5, T6, T7, T8> Bind<T1, T2, T3, T4, T5, T6, T7, T8>(this Action<T1, T2, T3, T4, T5, T6, T7, T8> d, T1 arg1)
    {
      if (d==null) return null; // someone's dirty hack
      return (arg2, arg3, arg4, arg5, arg6, arg7, arg8) => d.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
    }

    /// <summary>Binds first argument to specified delegate.</summary>
    /// <returns> A delegate that takes the rest of arguments of original delegate.</returns>
    public static Action<T2, T3, T4, T5, T6, T7, T8, T9> Bind<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> d, T1 arg1)
    {
      if (d==null) return null; // someone's dirty hack
      return (arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9) => d.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
    }

    /// <summary>Binds first argument to specified delegate.</summary>
    /// <returns> A delegate that takes the rest of arguments of original delegate.</returns>
    public static Action<T2, T3, T4, T5, T6, T7, T8, T9, T10> Bind<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> d, T1 arg1)
    {
      if (d==null) return null; // someone's dirty hack
      return (arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10) => d.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
    }

    /// <summary>Binds first argument to specified delegate.</summary>
    /// <returns> A delegate that takes the rest of arguments of original delegate.</returns>
    public static Action<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Bind<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> d, T1 arg1)
    {
      if (d==null) return null; // someone's dirty hack
      return (arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11) => d.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
    }

    /// <summary>Binds first argument to specified delegate.</summary>
    /// <returns> A delegate that takes the rest of arguments of original delegate.</returns>
    public static Action<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Bind<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> d, T1 arg1)
    {
      if (d==null) return null; // someone's dirty hack
      return (arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12) => d.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
    }

    /// <summary>Binds first argument to specified delegate.</summary>
    /// <returns> A delegate that takes the rest of arguments of original delegate.</returns>
    public static Action<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Bind<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> d, T1 arg1)
    {
      if (d==null) return null; // someone's dirty hack
      return (arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13) => d.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);
    }

    /// <summary>Binds first argument to specified delegate.</summary>
    /// <returns> A delegate that takes the rest of arguments of original delegate.</returns>
    public static Action<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Bind<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> d, T1 arg1)
    {
      if (d==null) return null; // someone's dirty hack
      return (arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14) => d.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14);
    }

    /// <summary>Binds first argument to specified delegate.</summary>
    /// <returns> A delegate that takes the rest of arguments of original delegate.</returns>
    public static Action<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Bind<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> d, T1 arg1)
    {
      if (d==null) return null; // someone's dirty hack
      return (arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15) => d.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15);
    }

    /// <summary>Binds first argument to specified delegate.</summary>
    /// <returns> A delegate that takes the rest of arguments of original delegate.</returns>
    public static Action<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Bind<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> d, T1 arg1)
    {
      if (d==null) return null; // someone's dirty hack
      return (arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16) => d.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16);
    }

  }
}

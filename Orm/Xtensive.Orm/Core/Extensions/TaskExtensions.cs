using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Xtensive.Core
{
  public static class TaskExtensions
  {
#if DO_CONFIGURE_AWAIT_FALSE
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static ConfiguredTaskAwaitable ConfigureAwaitFalse(this Task task) => task.ConfigureAwait(false);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static ConfiguredTaskAwaitable<T> ConfigureAwaitFalse<T>(this Task<T> task) => task.ConfigureAwait(false);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static ConfiguredValueTaskAwaitable ConfigureAwaitFalse(this ValueTask task) => task.ConfigureAwait(false);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static ConfiguredValueTaskAwaitable<T> ConfigureAwaitFalse<T>(this ValueTask<T> task) => task.ConfigureAwait(false);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static ConfiguredAsyncDisposable ConfigureAwaitFalse(this IAsyncDisposable source) => source.ConfigureAwait(false);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static ConfiguredCancelableAsyncEnumerable<T> ConfigureAwaitFalse<T>(this IAsyncEnumerable<T> source) => source.ConfigureAwait(false);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static ConfiguredCancelableAsyncEnumerable<T> ConfigureAwaitFalse<T>(this ConfiguredCancelableAsyncEnumerable<T> source) => source.ConfigureAwait(false);
#else
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Task ConfigureAwaitFalse(this Task task) => task;
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Task<T> ConfigureAwaitFalse<T>(this Task<T> task) => task;
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static ValueTask ConfigureAwaitFalse(this ValueTask task) => task;
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static ValueTask<T> ConfigureAwaitFalse<T>(this ValueTask<T> task) => task;
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static IAsyncDisposable ConfigureAwaitFalse(this IAsyncDisposable source) => source;
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static IAsyncEnumerable<T> ConfigureAwaitFalse<T>(this IAsyncEnumerable<T> source) => source;
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static ConfiguredCancelableAsyncEnumerable<T> ConfigureAwaitFalse<T>(this ConfiguredCancelableAsyncEnumerable<T> source) => source;
#endif
  }
}

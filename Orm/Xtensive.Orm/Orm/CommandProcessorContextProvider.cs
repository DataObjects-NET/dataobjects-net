// Copyright (C) 2019 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2019.07.12

using System;
using System.Collections.Concurrent;
using Xtensive.Orm.Providers;

namespace Xtensive.Orm
{
  public sealed class CommandProcessorContextProvider : SessionBound, IDisposable
  {
    private ConcurrentDictionary<CommandProcessorContext, CommandProcessorContext> providedContexts
      = new ConcurrentDictionary<CommandProcessorContext, CommandProcessorContext>();

    /// <inheritdoc/>
    public int AliveContextCount { get { return providedContexts.Count; } }

    /// <inheritdoc/>
    public CommandProcessorContext ProvideContext()
    {
      return ProvideContext(false);
    }

    /// <inheritdoc/>
    public CommandProcessorContext ProvideContext(bool allowPartialExecution)
    {
      var context = new CommandProcessorContext(allowPartialExecution);
      providedContexts.AddOrUpdate(context, (CommandProcessorContext) null, (processorContext, commandProcessorContext) => { return null; });
      context.Disposed += RemoveDesposedContext;
      return context;
    }

    public void Dispose()
    {
      providedContexts.Clear();
    }

    private void RemoveDesposedContext(object sender, EventArgs args)
    {
      CommandProcessorContext outResult;
      providedContexts.TryRemove((CommandProcessorContext) sender, out outResult);
    }
    
    internal CommandProcessorContextProvider(Session session)
      : base(session)
    {
    }
  }
}

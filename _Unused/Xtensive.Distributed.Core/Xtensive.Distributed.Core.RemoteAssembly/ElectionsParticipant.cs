using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Xtensive.Messaging.Diagnostics;

namespace Xtensive.Distributed.Core.RemoteAssembly
{
  public class ElectionsParticipant: MarshalByRefObject 
  {
    private ElectionContext context;
    private SimpleElectionAlgorithm algorithm;

    public ElectionResult CurrentResult
    {
      get
      {
        if (null == context.Result)
          return null;
        else if (context.Result.IsActual)
          return context.Result;
        else
          return null;
      }
    }

    public void StartElections(ElectionGroup group, NetworkEntity me)
    {
      // Inject falures
      DebugInfo.IsOperable = true;
      DebugInfo.Reset(true, true);
      DebugInfo.SkipSendProbability = 0.20;
      DebugInfo.StatisticsLogPeriod = TimeSpan.FromSeconds(60);
      
      context = new ElectionContext(group, me);
      algorithm = new SimpleElectionAlgorithm(context);
    }
  }
}

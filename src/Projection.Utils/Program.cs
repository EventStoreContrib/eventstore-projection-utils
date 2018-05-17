using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EventStore.ClientAPI.Common.Log;
using EventStore.ClientAPI.Projections;
using EventStore.ClientAPI.SystemData;
using PowerArgs;

namespace EventStoreContrib.Utils
{
    class Program
    {
        static void Main(string[] args)
        {
            Args.InvokeAction<UtilityProgram>(args);
        }


        
    }

  
}
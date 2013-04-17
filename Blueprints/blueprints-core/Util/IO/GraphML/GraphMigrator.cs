using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.IO.GraphML
{
    /// <summary>
    /// GraphMigrator takes the data in one graph and pipes it to another graph.
    /// </summary>
    public static class GraphMigrator
    {
        /// <summary>
        /// Pipe the data from one graph to another graph.
        /// </summary>
        /// <param name="fromGraph">the graph to take data from</param>
        /// <param name="toGraph">the graph to take data to</param>
        public static void MigrateGraph(Graph fromGraph, Graph toGraph)
        {
            const int PIPE_SIZE = 1024;
            AnonymousPipeServerStream outPipe = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable, PIPE_SIZE);

            using (AnonymousPipeClientStream inPipe = new AnonymousPipeClientStream(PipeDirection.In, outPipe.GetClientHandleAsString()))
            {
                outPipe.DisposeLocalCopyOfClientHandle();

                Task.Factory.StartNew(() =>
                {
                    GraphMLWriter.OutputGraph(fromGraph, outPipe);
                    outPipe.Flush();
                    outPipe.Close();
                });

                GraphMLReader.InputGraph(toGraph, inPipe);
            }
        }
    }
}

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Impls.TG
{
    [TestFixture(Category = "TinkerGraphGraphTestSuite")]
    public class TinkerGraphVertexTestSuite : VertexTestSuite
    {
        [SetUp]
        public void setUp()
        {
            deleteDirectory(TinkerGraphTestImpl.getThinkerGraphDirectory());
        }

        public TinkerGraphVertexTestSuite()
            : base(new TinkerGraphTestImpl())
        {
        }
    }

    [TestFixture(Category = "TinkerGraphGraphTestSuite")]
    public class TinkerGraphEdgeTestSuite : EdgeTestSuite
    {
        [SetUp]
        public void setUp()
        {
            deleteDirectory(TinkerGraphTestImpl.getThinkerGraphDirectory());
        }

        public TinkerGraphEdgeTestSuite()
            : base(new TinkerGraphTestImpl())
        {
        }
    }

    class TinkerGraphTestImpl : GraphTest
    {
        public override Graph generateGraph()
        {
            return generateGraph("graph");
        }

        public override Graph generateGraph(string graphDirectoryName)
        {
            return new TinkerGraph(getThinkerGraphDirectory());
        }

        public static string getThinkerGraphDirectory()
        {
            string directory = System.Environment.GetEnvironmentVariable("tinkerGraphDirectory");
            if (directory == null)
                directory = getWorkingDirectory();
            
            return string.Concat(directory,"/graph");
        }

        static string getWorkingDirectory()
        {
            return Directory.GetCurrentDirectory();
        }
    }
}

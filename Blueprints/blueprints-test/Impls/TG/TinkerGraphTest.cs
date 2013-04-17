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
        public void SetUp()
        {
            DeleteDirectory(TinkerGraphTestImpl.GetThinkerGraphDirectory());
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
        public void SetUp()
        {
            DeleteDirectory(TinkerGraphTestImpl.GetThinkerGraphDirectory());
        }

        public TinkerGraphEdgeTestSuite()
            : base(new TinkerGraphTestImpl())
        {
        }
    }

    class TinkerGraphTestImpl : GraphTest
    {
        public override Graph GenerateGraph()
        {
            return GenerateGraph("graph");
        }

        public override Graph GenerateGraph(string graphDirectoryName)
        {
            return new TinkerGraph(GetThinkerGraphDirectory());
        }

        public static string GetThinkerGraphDirectory()
        {
            string directory = System.Environment.GetEnvironmentVariable("tinkerGraphDirectory");
            if (directory == null)
                directory = GetWorkingDirectory();
            
            return string.Concat(directory,"/graph");
        }

        static string GetWorkingDirectory()
        {
            return Directory.GetCurrentDirectory();
        }
    }
}

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Frontenac.Blueprints.Impls;
using NUnit.Framework;

namespace Frontenac.Blueprints
{
    public abstract class StorageTestSuite : TestSuite
    {
        protected StorageTestSuite(GraphTest graphTest)
            : base("StorageTestSuite", graphTest)
        {
        }

        private static void CreateDirectory(string dir)
        {
            if (Directory.Exists(dir))
            {
                DeleteDirectory(dir);
            }

            Directory.CreateDirectory(dir);
        }

        private string GetDirectory()
        {
            return ComputeTestDataRoot();
        }

        private static IEnumerable<string> FindFilesByExt(string path, string ext)
        {
            return Directory.EnumerateFiles(path, string.Concat("*.", ext), SearchOption.AllDirectories);
        }

        [Test]
        public void TestGmlStorage()
        {
            var path = GetDirectory() + "/" + "storage-test-gml";
            CreateDirectory(path);


            var graph = GraphTest.GenerateGraph();
            graph.CreateTinkerGraph();
            try
            {
                graph.SaveGml(path);
            }
            finally
            {
                graph.Shutdown();
            }

            Assert.AreEqual(1, FindFilesByExt(path, "gml").Count());
        }

        [Test]
        public void TestGraphMlStorage()
        {
            var path = GetDirectory() + "/" + "storage-test-graphml";
            CreateDirectory(path);

            var graph = GraphTest.GenerateGraph();
            graph.CreateTinkerGraph();
            try
            {
                graph.SaveGraphml(path);
            }
            finally
            {
                graph.Shutdown();
            }

            Assert.AreEqual(1, FindFilesByExt(path, "xml").Count());
        }

        [Test]
        public void TestGraphSonStorage()
        {
            var path = GetDirectory() + "/" + "storage-test-graphson";
            CreateDirectory(path);

            var graph = GraphTest.GenerateGraph();
            graph.CreateTinkerGraph();
            try
            {
                graph.SaveGraphson(path);
            }
            finally
            {
                graph.Shutdown();
            }

            Assert.AreEqual(1, FindFilesByExt(path, "json").Count());
        }


        [Test]
        public void TestDotNetStorage()
        {
            var path = GetDirectory() + "/" + "storage-test-dotnet";
            CreateDirectory(path);

            var graph = GraphTest.GenerateGraph();
            graph.CreateTinkerGraph();
            try
            {
                graph.SaveDotNet(path);
            }
            finally
            {
                graph.Shutdown();
            }

            Assert.AreEqual(1, FindFilesByExt(path, "dat").Count());
        }
    }
}
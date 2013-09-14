using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Frontenac.Blueprints.Impls.TG
{
    [TestFixture(Category = "TinkerStorageFactoryTest")]
    public class TinkerStorageFactoryTest : BaseTest
    {
        [Test]
        public void StorageFactoryIsSingleton()
        {
            TinkerStorageFactory factory = TinkerStorageFactory.GetInstance();
            Assert.AreSame(factory, TinkerStorageFactory.GetInstance());
        }

        [Test]
        public void TestGmlStorage()
        {
            string path = GetDirectory() + "/" + "storage-test-gml";
            CreateDirectory(path);

            ITinkerStorage storage = TinkerStorageFactory.GetInstance().GetTinkerStorage(TinkerGraph.FileType.Gml);
            TinkerGraph graph = TinkerGraphFactory.CreateTinkerGraph();
            storage.Save(graph, path);

            Assert.AreEqual(1, FindFilesByExt(path, "gml").Count());
            Assert.AreEqual(1, FindFilesByExt(path, "dat").Count());
        }

        [Test]
        public void TestGraphMlStorage()
        {
            string path = GetDirectory() + "/" + "storage-test-graphml";
            CreateDirectory(path);

            ITinkerStorage storage = TinkerStorageFactory.GetInstance().GetTinkerStorage(TinkerGraph.FileType.Graphml);
            TinkerGraph graph = TinkerGraphFactory.CreateTinkerGraph();
            storage.Save(graph, path);

            Assert.AreEqual(1, FindFilesByExt(path, "xml").Count());
            Assert.AreEqual(1, FindFilesByExt(path, "dat").Count());
        }

        [Test]
        public void TestGraphSonStorageFactory()
        {
            string path = GetDirectory() + "/" + "storage-test-graphson";
            CreateDirectory(path);

            ITinkerStorage storage = TinkerStorageFactory.GetInstance().GetTinkerStorage(TinkerGraph.FileType.Graphson);
            TinkerGraph graph = TinkerGraphFactory.CreateTinkerGraph();
            storage.Save(graph, path);

            Assert.AreEqual(1, FindFilesByExt(path, "json").Count());
            Assert.AreEqual(1, FindFilesByExt(path, "dat").Count());
        }

        [Test]
        public void TestDotNetStorageFactory()
        {
            string path = GetDirectory() + "/" + "storage-test-dotnet";
            CreateDirectory(path);

            ITinkerStorage storage = TinkerStorageFactory.GetInstance().GetTinkerStorage(TinkerGraph.FileType.DotNet);
            TinkerGraph graph = TinkerGraphFactory.CreateTinkerGraph();
            storage.Save(graph, path);

            Assert.AreEqual(1, FindFilesByExt(path, "dat").Count());
        }

        static void CreateDirectory(string dir)
        {
            if (Directory.Exists(dir))
            {
                DeleteDirectory(dir);
            }

            Directory.CreateDirectory(dir);
        }

        string GetDirectory()
        {
            String directory = Environment.GetEnvironmentVariable("tinkerGraphDirectory") ?? GetWorkingDirectory();
            return directory;
        }

        string GetWorkingDirectory()
        {
            return ComputeTestDataRoot();
        }

        static IEnumerable<string> FindFilesByExt(string path, string ext)
        {
            return Directory.EnumerateFiles(path, string.Concat("*.", ext), SearchOption.AllDirectories);
        }
    }
}

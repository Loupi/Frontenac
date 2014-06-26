using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Frontenac.Blueprints.Impls.TG
{
    [TestFixture(Category = "TinkerStorageFactoryTest")]
    public class TinkerStorageFactoryTest : BaseTest
    {
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
            var directory = Environment.GetEnvironmentVariable("tinkerGraphDirectory") ?? GetWorkingDirectory();
            return directory;
        }

        private string GetWorkingDirectory()
        {
            return ComputeTestDataRoot();
        }

        private static IEnumerable<string> FindFilesByExt(string path, string ext)
        {
            return Directory.EnumerateFiles(path, string.Concat("*.", ext), SearchOption.AllDirectories);
        }

        [Test]
        public void StorageFactoryIsSingleton()
        {
            var factory = TinkerStorageFactory.GetInstance();
            Assert.AreSame(factory, TinkerStorageFactory.GetInstance());
        }

        [Test]
        public void TestDotNetStorageFactory()
        {
            var path = GetDirectory() + "/" + "storage-test-dotnet";
            CreateDirectory(path);

            var storage = TinkerStorageFactory.GetInstance().GetTinkerStorage(TinkerGraph.FileType.DotNet);
            var graph = TinkerGraphFactory.CreateTinkerGraph();
            storage.Save(graph, path);

            Assert.AreEqual(1, FindFilesByExt(path, "dat").Count());
        }

        [Test]
        public void TestGmlStorage()
        {
            var path = GetDirectory() + "/" + "storage-test-gml";
            CreateDirectory(path);

            var storage = TinkerStorageFactory.GetInstance().GetTinkerStorage(TinkerGraph.FileType.Gml);
            var graph = TinkerGraphFactory.CreateTinkerGraph();
            storage.Save(graph, path);

            Assert.AreEqual(1, FindFilesByExt(path, "gml").Count());
            Assert.AreEqual(1, FindFilesByExt(path, "dat").Count());
        }

        [Test]
        public void TestGraphMlStorage()
        {
            var path = GetDirectory() + "/" + "storage-test-graphml";
            CreateDirectory(path);

            var storage = TinkerStorageFactory.GetInstance().GetTinkerStorage(TinkerGraph.FileType.Graphml);
            var graph = TinkerGraphFactory.CreateTinkerGraph();
            storage.Save(graph, path);

            Assert.AreEqual(1, FindFilesByExt(path, "xml").Count());
            Assert.AreEqual(1, FindFilesByExt(path, "dat").Count());
        }

        [Test]
        public void TestGraphSonStorageFactory()
        {
            var path = GetDirectory() + "/" + "storage-test-graphson";
            CreateDirectory(path);

            var storage = TinkerStorageFactory.GetInstance().GetTinkerStorage(TinkerGraph.FileType.Graphson);
            var graph = TinkerGraphFactory.CreateTinkerGraph();
            storage.Save(graph, path);

            Assert.AreEqual(1, FindFilesByExt(path, "json").Count());
            Assert.AreEqual(1, FindFilesByExt(path, "dat").Count());
        }
    }
}
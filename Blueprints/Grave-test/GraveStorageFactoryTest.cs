using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Frontenac.Blueprints;
using Grave;
using NUnit.Framework;

namespace Grave_test
{
    public interface IGraveStorage
    {
        GraveGraph Load(string directory);
        void Save(GraveGraph graph, string directory);
    }

    public enum FileType
    {
        DotNet,
        Gml,
        Graphml,
        Graphson
    }

    [TestFixture(Category = "GraveStorageFactoryTest")]
    public class GraveStorageFactoryTest : BaseTest
    {
        [SetUp]
        public void SetUp()
        {
            GraveFactory.Release();
            DeleteDirectory(GraveGraphTestImpl.GetGraveGraphDirectory());
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
            var directory = Environment.GetEnvironmentVariable("graveGraphDirectory") ?? GetWorkingDirectory();
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
            var factory = GraveGraphStorageFactory.GetInstance();
            Assert.AreSame(factory, GraveGraphStorageFactory.GetInstance());
        }

        [Test]
        public void TestGmlStorage()
        {
            var path = GetDirectory() + "/" + "storage-test-gml";
            CreateDirectory(path);

            var storage = GraveGraphStorageFactory.GetInstance().GetGraveStorage(FileType.Gml);
            var graph = GraveFactory.CreateTinkerGraph();
            try
            {
                storage.Save(graph, path);
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

            var storage = GraveGraphStorageFactory.GetInstance().GetGraveStorage(FileType.Graphml);
            var graph = GraveFactory.CreateTinkerGraph();
            try
            {
                storage.Save(graph, path);
            }
            finally
            {
                graph.Shutdown();
            }

            Assert.AreEqual(1, FindFilesByExt(path, "xml").Count());
        }

        [Test]
        public void TestGraphSonStorageFactory()
        {
            var path = GetDirectory() + "/" + "storage-test-graphson";
            CreateDirectory(path);

            var storage = GraveGraphStorageFactory.GetInstance().GetGraveStorage(FileType.Graphson);
            var graph = GraveFactory.CreateTinkerGraph();
            try
            {
                storage.Save(graph, path);
            }
            finally
            {
                graph.Shutdown();
            }

            Assert.AreEqual(1, FindFilesByExt(path, "json").Count());
        }
    }
}
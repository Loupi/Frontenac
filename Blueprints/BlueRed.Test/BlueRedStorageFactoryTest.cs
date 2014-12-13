using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Frontenac.Blueprints;
using NUnit.Framework;

namespace Frontenac.BlueRed.Tests
{
    public interface IBlueRedStorage
    {
        RedisGraph Load(string directory);
        void Save(RedisGraph graph, string directory);
    }

    public enum FileType
    {
        DotNet,
        Gml,
        Graphml,
        Graphson
    }

    [TestFixture(Category = "BlueRedStorageFactoryTest")]
    public class BlueRedStorageFactoryTest : BaseTest
    {
        [SetUp]
        public void SetUp()
        {
            BlueRedFactory.DeleteDb();
            BlueRedFactory.Release();
            DeleteDirectory(BlueRedGraphTestImpl.GetBlueRedGraphDirectory());
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
            var directory = Environment.GetEnvironmentVariable("BlueRedGraphDirectory") ?? GetWorkingDirectory();
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
            var factory = BlueRedGraphStorageFactory.GetInstance();
            Assert.AreSame(factory, BlueRedGraphStorageFactory.GetInstance());
        }

        [Test]
        public void TestGmlStorage()
        {
            var path = GetDirectory() + "/" + "storage-test-gml";
            CreateDirectory(path);

            var storage = BlueRedGraphStorageFactory.GetInstance().GetBlueRedStorage(FileType.Gml);
            var graph = BlueRedFactory.CreateTinkerGraph();
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

            var storage = BlueRedGraphStorageFactory.GetInstance().GetBlueRedStorage(FileType.Graphml);
            var graph = BlueRedFactory.CreateTinkerGraph();
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

            var storage = BlueRedGraphStorageFactory.GetInstance().GetBlueRedStorage(FileType.Graphson);
            var graph = BlueRedFactory.CreateTinkerGraph();
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
using Frontenac.Blueprints.Impls.VG;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VelocityDb.Session;
using VelocityGraph;

namespace Frontenac.Blueprints.Impls.VG
{
    /// <summary>
    /// Implementations are responsible for loading and saving a TinkerGraph data.
    /// </summary>
    public interface IVGStorage
    {
      Graph Load(string directory);
      void Save(Graph graph, string directory);
    }

    public enum FileType
    {
      Java,
      Gml,
      Graphml,
      Graphson
    }

    [TestFixture(Category = "VelocityStorageFactoryTest")]
    public class VelocityStorageFactoryTest : BaseTest
    {
        [Test]
        public void StorageFactoryIsSingleton()
        {
          VelocityGraphStorageFactory factory = VelocityGraphStorageFactory.GetInstance();
            Assert.AreSame(factory, VelocityGraphStorageFactory.GetInstance());
        }

        [Test]
        public void TestGmlStorage()
        {
            string path = GetDirectory() + "/" + "storage-test-gml";
            SessionBase session = new SessionNoServer(GetDirectory());
            IVGStorage storage = VelocityGraphStorageFactory.GetInstance().GetVGStorage(FileType.Gml);
            Graph graph = VelocityGraphFactory.CreateVelocityGraph(session);
            storage.Save(graph, path);

            Assert.AreEqual(1, FindFilesByExt(path, "gml").Count());
        }

        [Test]
        public void TestGraphMlStorage()
        {
            string path = GetDirectory() + "/" + "storage-test-graphml";
            CreateDirectory(path);
            SessionBase session = new SessionNoServer(GetDirectory());
            IVGStorage storage = VelocityGraphStorageFactory.GetInstance().GetVGStorage(FileType.Graphml);
            Graph graph = VelocityGraphFactory.CreateVelocityGraph(session);
            storage.Save(graph, path);

            Assert.AreEqual(1, FindFilesByExt(path, "xml").Count());
        }

        [Test]
        public void TestGraphSonStorageFactory()
        {
            string path = GetDirectory() + "/" + "storage-test-graphson";
            CreateDirectory(path);
            SessionBase session = new SessionNoServer(GetDirectory());
            IVGStorage storage = VelocityGraphStorageFactory.GetInstance().GetVGStorage(FileType.Graphson);
            Graph graph = VelocityGraphFactory.CreateVelocityGraph(session);
            storage.Save(graph, path);

            Assert.AreEqual(1, FindFilesByExt(path, "json").Count());
        }

        [Test]
        public void TestJavaStorageFactory()
        {
            string path = GetDirectory() + "/" + "storage-test-java";
            CreateDirectory(path);
            SessionBase session = new SessionNoServer(GetDirectory());
            IVGStorage storage = VelocityGraphStorageFactory.GetInstance().GetVGStorage(FileType.Java);
            Graph graph = VelocityGraphFactory.CreateVelocityGraph(session);
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
            String directory = Environment.GetEnvironmentVariable("VelocityGraphDirectory") ?? GetWorkingDirectory();
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

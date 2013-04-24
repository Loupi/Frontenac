using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Impls.TG
{
    [TestFixture(Category = "TinkerStorageFactoryTest")]
    public class TinkerStorageFactoryTest : BaseTest
    {
        [Test]
        public void storageFactoryIsSingleton()
        {
            TinkerStorageFactory factory = TinkerStorageFactory.getInstance();
            Assert.AreSame(factory, TinkerStorageFactory.getInstance());
        }

        [Test]
        public void testGMLStorage()
        {
            string path = getDirectory() + "/" + "storage-test-gml";
            createDirectory(path);

            TinkerStorage storage = TinkerStorageFactory.getInstance().getTinkerStorage(TinkerGraph.FileType.GML);
            TinkerGraph graph = TinkerGraphFactory.createTinkerGraph();
            storage.save(graph, path);

            Assert.AreEqual(1, findFilesByExt(path, "gml").Count());
            Assert.AreEqual(1, findFilesByExt(path, "dat").Count());
        }

        [Test]
        public void testGraphMLStorage()
        {
            string path = getDirectory() + "/" + "storage-test-graphml";
            createDirectory(path);

            TinkerStorage storage = TinkerStorageFactory.getInstance().getTinkerStorage(TinkerGraph.FileType.GRAPHML);
            TinkerGraph graph = TinkerGraphFactory.createTinkerGraph();
            storage.save(graph, path);

            Assert.AreEqual(1, findFilesByExt(path, "xml").Count());
            Assert.AreEqual(1, findFilesByExt(path, "dat").Count());
        }

        [Test]
        public void testGraphSONStorageFactory()
        {
            string path = getDirectory() + "/" + "storage-test-graphson";
            createDirectory(path);

            TinkerStorage storage = TinkerStorageFactory.getInstance().getTinkerStorage(TinkerGraph.FileType.GRAPHSON);
            TinkerGraph graph = TinkerGraphFactory.createTinkerGraph();
            storage.save(graph, path);

            Assert.AreEqual(1, findFilesByExt(path, "json").Count());
            Assert.AreEqual(1, findFilesByExt(path, "dat").Count());
        }

        [Test]
        public void testJavaStorageFactory()
        {
            string path = getDirectory() + "/" + "storage-test-java";
            createDirectory(path);

            TinkerStorage storage = TinkerStorageFactory.getInstance().getTinkerStorage(TinkerGraph.FileType.JAVA);
            TinkerGraph graph = TinkerGraphFactory.createTinkerGraph();
            storage.save(graph, path);

            Assert.AreEqual(1, findFilesByExt(path, "dat").Count());
        }

        void createDirectory(string dir)
        {
            if (Directory.Exists(dir))
            {
                deleteDirectory(dir);
            }

            Directory.CreateDirectory(dir);
        }

        string getDirectory()
        {
            String directory = System.Environment.GetEnvironmentVariable("tinkerGraphDirectory");
            if (directory == null)
            {
                directory = this.getWorkingDirectory();
            }
            return directory;
        }

        string getWorkingDirectory()
        {
            return this.computeTestDataRoot();
        }

        static IEnumerable<string> findFilesByExt(string path, string ext)
        {
            return Directory.EnumerateFiles(path, string.Concat("*.", ext), SearchOption.AllDirectories);
        }
    }
}

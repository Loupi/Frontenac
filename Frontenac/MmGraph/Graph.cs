using System;
using System.Collections.Generic;
using System.Linq;
using Frontenac.Blueprints;
using Frontenac.Blueprints.Util;
using Frontenac.Infrastructure;
using MmGraph.Database.Records;
using MmGraph.Database.Repositories;

namespace MmGraph
{
    public class Graph : IGraph, IDisposable
    {
        private static readonly Features GraphFeatures = new Features
        {
            SupportsDuplicateEdges = true,
            SupportsSelfLoops = true,
            SupportsSerializableObjectProperty = true,
            SupportsBooleanProperty = true,
            SupportsDoubleProperty = true,
            SupportsFloatProperty = true,
            SupportsIntegerProperty = true,
            SupportsPrimitiveArrayProperty = true,
            SupportsUniformListProperty = true,
            SupportsMixedListProperty = true,
            SupportsLongProperty = true,
            SupportsMapProperty = true,
            SupportsStringProperty = true,
            IgnoresSuppliedIds = false,
            IsPersistent = true,
            IsRdfModel = false,
            IsWrapper = false,
            SupportsIndices = true,
            SupportsKeyIndices = true,
            SupportsVertexKeyIndex = true,
            SupportsEdgeKeyIndex = true,
            SupportsVertexIndex = true,
            SupportsEdgeIndex = true,
            SupportsTransactions = false,
            SupportsVertexIteration = true,
            SupportsEdgeIteration = true,
            SupportsEdgeRetrieval = true,
            SupportsVertexProperties = true,
            SupportsEdgeProperties = true,
            SupportsThreadedTransactions = false,
            SupportsIdProperty = true,
            SupportsLabelProperty = true
        };

        private readonly StringRepository _stringRepository;
        private readonly VertexRepository _vertexRepository;
        private readonly EdgeRepository _edgeRepository;
        private readonly PropertyManager _vertexPropertyManager;
        private readonly PropertyManager _edgePropertyManager;
        private readonly LabelManager _labelManager;

        private bool _disposed;

        public Graph()
        {
            _stringRepository = new StringRepository();
            _vertexRepository = new VertexRepository();
            _edgeRepository = new EdgeRepository();
            _vertexPropertyManager = new PropertyManager("vertex", _stringRepository);
            _edgePropertyManager = new PropertyManager("edge", _stringRepository);
            _labelManager = new LabelManager();
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Graph()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _stringRepository?.Dispose();
                _vertexRepository?.Dispose();
                _edgeRepository?.Dispose();
                _vertexPropertyManager?.Dispose();
                _edgePropertyManager?.Dispose();
                _labelManager?.Dispose();
            }

            _disposed = true;
        }

        public Features Features => GraphFeatures;

        public override string ToString()
        {
            return this.GraphString("MmGraph");
        }

        public IVertex AddVertex(object notUsed)
        {
            var record = new VertexRecord {NextEdgeId = -1, NextPropertyId = -1};
            var id = _vertexRepository.Create(record);
            return new Vertex(id, this);
        }

        public IVertex GetVertex(object id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            var vertexId = id.TryToInt64();
            if (!vertexId.HasValue) return null;
            var record = _vertexRepository.Read((int) vertexId);
            if (record == null)
                return null;
            return new Vertex(record.Id, this);
        }

        public void RemoveVertex(IVertex vertex)
        {
            var v = vertex as Vertex;
            var record = _vertexRepository.Read((int)v.RawId);
            foreach (var edge in GetEdges(v, Direction.Both))
            {
                RemoveEdge(edge);
            }
            if (record.NextPropertyId != -1)
                _vertexPropertyManager.DeletePropertyList(record.NextPropertyId);
            _vertexRepository.Delete((int)v.RawId);
        }

        public IEnumerable<IVertex> GetVertices()
        {
            // TODO: Read per 64k block size in repository, and iterate in ram
            foreach (var record in _vertexRepository.Scan())
            {
                yield return new Vertex(record.Id, this);
            }
        }

        public IEnumerable<IVertex> GetVertices(string key, object value)
        {
            foreach (var record in _vertexRepository.Scan())
            {
                var vertex = new Vertex(record.Id, this);
                if (vertex.GetPropertyKeys().Any(s => s == key))
                {
                    var property = vertex.GetProperty(key);
                    // TODO: Array comparison
                    if (property == null && value == null || 
                        property != null && property.Equals(value))
                        yield return vertex;
                }
            }
        }

        public IEdge AddEdge(object notUsed, IVertex outVertex, IVertex inVertex, string label)
        {
            var index = _labelManager.CreateOrGet(label);
            
            var outV = (Vertex)outVertex;
            var inV = (Vertex)inVertex;

            var outRecord = _vertexRepository.Read((int)outV.RawId);
            var inRecord = _vertexRepository.Read((int)inV.RawId);
            
            var record = new EdgeRecord
            {
                LabelId = index.Id,
                IsDirected = true,
                InNodeId = (int)(long)inVertex.Id,
                OutNodeId = (int)(long)outVertex.Id,
                OutNodeNextEdgeId = outRecord.NextEdgeId,
                OutNodePreviousEdgeId = -1,
                InNodeNextEdgeId = inRecord.NextEdgeId,
                InNodePreviousEdgeId = -1,
                NextPropertyId = -1
            };

            var id = _edgeRepository.Create(record);

            if (outRecord.NextEdgeId != -1)
            {
                var outNextEdgeRecord = _edgeRepository.Read(outRecord.NextEdgeId);

                if (outNextEdgeRecord.InNodeId == outRecord.Id)
                    outNextEdgeRecord.InNodePreviousEdgeId = id;
                else
                    outNextEdgeRecord.OutNodePreviousEdgeId = id;

                _edgeRepository.Update(outRecord.NextEdgeId, outNextEdgeRecord);
            }

            outRecord.NextEdgeId = id;
            _vertexRepository.Update(outRecord.Id, outRecord);

            if (inRecord.NextEdgeId != -1)
            {
                var inNextEdgeRecord = _edgeRepository.Read(inRecord.NextEdgeId);
                if (inNextEdgeRecord.InNodeId == inRecord.Id)
                    inNextEdgeRecord.InNodePreviousEdgeId = id;
                else
                    inNextEdgeRecord.OutNodePreviousEdgeId = id;

                _edgeRepository.Update(inRecord.NextEdgeId, inNextEdgeRecord);
            }

            inRecord.NextEdgeId = id;
            _vertexRepository.Update(inRecord.Id, inRecord);

            index.IncrementCount();

            return new Edge(id, index.Key, outV, inV, this);
        }

        public IEdge GetEdge(object id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            var edgeId = id.TryToInt64();
            if (!edgeId.HasValue) return null;
            var record = _edgeRepository.Read((int)edgeId);
            if (record == null)
                return null;
            var edge = EdgeFromRecord(record);
            return edge;
        }

        public void RemoveEdge(IEdge edge)
        {
            var e = (Edge) edge;

            var record = _edgeRepository.Read((int) e.RawId);
            if (record.NextPropertyId != -1)
                _edgePropertyManager.DeletePropertyList(record.NextPropertyId);

            _edgeRepository.Delete(record.Id);

            if (record.OutNodePreviousEdgeId != -1)
            {
                var outPreviousEdgeRecord = _edgeRepository.Read(record.OutNodePreviousEdgeId);
                if (outPreviousEdgeRecord.OutNodeId == record.OutNodeId)
                    outPreviousEdgeRecord.OutNodeNextEdgeId = record.OutNodeNextEdgeId;
                else
                    outPreviousEdgeRecord.InNodeNextEdgeId = record.OutNodeNextEdgeId;
                _edgeRepository.Update(outPreviousEdgeRecord.Id, outPreviousEdgeRecord);
            }
            else
            {
                var outVertexRecord = _vertexRepository.Read(record.OutNodeId);
                outVertexRecord.NextEdgeId = record.OutNodeNextEdgeId;
                _vertexRepository.Update(outVertexRecord.Id, outVertexRecord);
            }

            if (record.OutNodeNextEdgeId != -1)
            {
                var outNextEdgeRecord = _edgeRepository.Read(record.OutNodeNextEdgeId);
                if (outNextEdgeRecord.OutNodeId == record.OutNodeId)
                    outNextEdgeRecord.OutNodePreviousEdgeId = record.OutNodePreviousEdgeId;
                else
                    outNextEdgeRecord.InNodePreviousEdgeId = record.OutNodePreviousEdgeId;
                _edgeRepository.Update(outNextEdgeRecord.Id, outNextEdgeRecord);
            }

            if (record.InNodePreviousEdgeId != -1)
            {
                var inPreviousEdgeRecord = _edgeRepository.Read(record.InNodePreviousEdgeId);
                if (inPreviousEdgeRecord.InNodeId == record.InNodeId)
                    inPreviousEdgeRecord.InNodeNextEdgeId = record.InNodeNextEdgeId;
                else
                    inPreviousEdgeRecord.OutNodeNextEdgeId = record.InNodeNextEdgeId;
                _edgeRepository.Update(inPreviousEdgeRecord.Id, inPreviousEdgeRecord);
            }
            else
            {
                var inVertexRecord = _vertexRepository.Read(record.InNodeId);
                inVertexRecord.NextEdgeId = record.InNodeNextEdgeId;
                _vertexRepository.Update(inVertexRecord.Id, inVertexRecord);
            }

            if (record.InNodeNextEdgeId != -1)
            {
                var inNextEdgeRecord = _edgeRepository.Read(record.InNodeNextEdgeId);
                if (inNextEdgeRecord.InNodeId == record.InNodeId)
                    inNextEdgeRecord.InNodePreviousEdgeId = record.InNodePreviousEdgeId;
                else
                    inNextEdgeRecord.OutNodePreviousEdgeId = record.InNodePreviousEdgeId;
                _edgeRepository.Update(inNextEdgeRecord.Id, inNextEdgeRecord);
            }

            var index = _labelManager.CreateOrGet(e.Label);
            index.DecrementCount();
        }

        public IEnumerable<IEdge> GetEdges()
        {
            foreach (var record in _edgeRepository.Scan())
            {
                var edge = EdgeFromRecord(record);
                yield return edge;
            }
        }

        public IEnumerable<IEdge> GetEdges(string key, object value)
        {
            foreach (var record in _edgeRepository.Scan())
            {
                var edge = EdgeFromRecord(record);

                if (edge.GetPropertyKeys().Any(s => s == key))
                {
                    var property = edge.GetProperty(key);
                    // TODO: Array comparison
                    if (property == null && value == null ||
                        property != null && property.Equals(value))
                        yield return edge;
                }
            }
        }

        private Edge EdgeFromRecord(EdgeRecord record)
        {
            var label = _labelManager.GetLabel(record.LabelId);
            var outV = new Vertex(record.OutNodeId, this);
            var inV = new Vertex(record.InNodeId, this);
            var edge = new Edge(record.Id, label.Key, outV, inV, this);
            return edge;
        }

        public IQuery Query()
        {
            throw new NotImplementedException();
        }

        public void Shutdown()
        {
            Dispose();
        }

        internal void SetVertexProperty(long id, string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            var record = _vertexRepository.Read((int) id);
            _vertexPropertyManager.SetProperty(record.NextPropertyId, key, value, out var nextPropertyId);
            if (record.NextPropertyId != nextPropertyId)
            {
                record.NextPropertyId = (int)nextPropertyId;
                _vertexRepository.Update((int) id, record);
            }
        }

        internal object RemoveVertexProperty(long id, string key)
        {
            var record = _vertexRepository.Read((int)id);
            var result = _vertexPropertyManager.RemoveProperty(record.NextPropertyId, key, out var nextPropertyId);

            if (record.NextPropertyId != nextPropertyId)
            {
                record.NextPropertyId = (int)nextPropertyId;
                _vertexRepository.Update((int) id, record);
            }

            return result;
        }

        internal IEnumerable<string> GetVertexPropertyKeys(long id)
        {
            var record = _vertexRepository.Read((int)id);
            return _vertexPropertyManager.GetPropertyKeys(record.NextPropertyId);
        }

        internal object GetVertexProperty(long id, string key)
        {
            var record = _vertexRepository.Read((int)id);
            return _vertexPropertyManager.GetProperty(record.NextPropertyId, key);
        }

        internal void SetEdgeProperty(long id, string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            var record = _edgeRepository.Read((int)id);
            _edgePropertyManager.SetProperty(record.NextPropertyId, key, value, out var nextPropertyId);
            if (record.NextPropertyId != nextPropertyId)
            {
                record.NextPropertyId = (int)nextPropertyId;
                _edgeRepository.Update((int)id, record);
            }
        }

        internal object RemoveEdgeProperty(long id, string key)
        {
            var record = _edgeRepository.Read((int)id);
            var result = _edgePropertyManager.RemoveProperty(record.NextPropertyId, key, out var nextPropertyId);

            if (record.NextPropertyId != nextPropertyId)
            {
                record.NextPropertyId = (int)nextPropertyId;
                _edgeRepository.Update((int)id, record);
            }

            return result;
        }

        internal IEnumerable<string> GetEdgePropertyKeys(long id)
        {
            var record = _edgeRepository.Read((int)id);
            return _edgePropertyManager.GetPropertyKeys(record.NextPropertyId);
        }

        internal object GetEdgeProperty(long id, string key)
        {
            var record = _edgeRepository.Read((int)id);
            return _edgePropertyManager.GetProperty(record.NextPropertyId, key);
        }

        public virtual IEnumerable<IEdge> GetEdges(Vertex vertex, Direction direction, params string[] labels)
        {
            var labelIds = labels.Length == 0 ? null : labels
                .Select(l => _labelManager.GetLabel(l))
                .Where(l => l != null)
                .Select(l => l.Id)
                .ToList();

            var id = (int) vertex.RawId;
            var vertexRecord = _vertexRepository.Read(id);
            var nextEdgeId = vertexRecord.NextEdgeId;

            while (nextEdgeId != -1)
            {
                var edgeRecord = _edgeRepository.Read(nextEdgeId);
                if (edgeRecord.OutNodeId == id)
                {
                    if ((direction == Direction.Both || direction == Direction.Out) &&
                        (labelIds == null || labelIds.Contains(edgeRecord.LabelId)))
                    {
                        var edge = EdgeFromRecord(edgeRecord);
                        yield return edge;
                    }

                    nextEdgeId = edgeRecord.OutNodeNextEdgeId;
                }
                else
                {
                    if ((direction == Direction.Both || direction == Direction.In) &&
                        (labelIds == null || labelIds.Contains(edgeRecord.LabelId)))
                    {
                        var edge = EdgeFromRecord(edgeRecord);
                        yield return edge;
                    }

                    nextEdgeId = edgeRecord.InNodeNextEdgeId;
                }
            }

            /*if (direction == Direction.Both || direction == Direction.Out)
            {
                while (nextEdgeId != -1)
                {
                    var edgeRecord = _edgeRepository.Read(nextEdgeId);
                    if (edgeRecord.OutNodeId == id &&
                        (labelIds == null || labelIds.Contains(edgeRecord.LabelId)))
                    {
                        var edge = EdgeFromRecord(edgeRecord);
                        yield return edge;
                    }

                    nextEdgeId = edgeRecord.InNodeNextEdgeId;
                }
            }
            
            if (direction != Direction.In && direction != Direction.Both) yield break;

            nextEdgeId = vertexRecord.NextEdgeId;
            while (nextEdgeId != -1)
            {
                var edgeRecord = _edgeRepository.Read(nextEdgeId);
                if (edgeRecord.InNodeId == id &&
                    (labelIds == null || labelIds.Contains(edgeRecord.LabelId)) &&
                    !(direction == Direction.Both && edgeRecord.OutNodeId == id))
                {
                    var edge = EdgeFromRecord(edgeRecord);
                    yield return edge;
                }

                nextEdgeId = edgeRecord.OutNodeNextEdgeId;
            }*/
        }
    }
}

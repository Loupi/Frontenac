using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints
{
    /// <summary>
    /// Features provides a listing of the features/qualities/quirks associated with any Graph implementation.
    /// This feature listing can be used to dynamically adjust code to the features of the graph implementation.
    /// For example, this feature listing is used extensively throughout the Blueprints TestSuite to validate behavior of the implementation.
    /// </summary>
    public class Features
    {
        /// <summary>
        /// Does the graph allow for two edges with the same vertices and edge label to exist?
        /// </summary>
        public bool? SupportsDuplicateEdges { get; set; }

        /// <summary>
        /// Does the graph allow an edge to have the same out/tail and in/head vertex?
        /// </summary>
        public bool? SupportsSelfLoops { get; set; }

        /// <summary>
        /// Does the graph allow any serializable object to be used as a property value for a graph element?
        /// </summary>
        public bool? SupportsSerializableObjectProperty { get; set; }

        /// <summary>
        /// Does the graph allows boolean to be used as a property value for a graph element?
        /// </summary>
        public bool? SupportsBooleanProperty { get; set; }

        /// <summary>
        /// Does the graph allows double to be used as a property value for a graph element?
        /// </summary>
        public bool? SupportsDoubleProperty { get; set; }

        /// <summary>
        /// Does the graph allows float to be used as a property value for a graph element?
        /// </summary>
        public bool? SupportsFloatProperty { get; set; }

        /// <summary>
        /// Does the graph allows integer to be used as a property value for a graph element?
        /// </summary>
        public bool? SupportsIntegerProperty { get; set; }

        /// <summary>
        /// Does the graph allows a primitive array to be used as a property value for a graph element?
        /// </summary>
        public bool? SupportsPrimitiveArrayProperty { get; set; }

        /// <summary>
        /// Does the graph allows list (all objects with the list have the same data types) to be used as a property value for a graph element?
        /// </summary>
        public bool? SupportsUniformListProperty { get; set; }

        /// <summary>
        /// Does the graph allows a mixed list (different data types within the same list) to be used as a
        /// property value for a graph element?
        /// </summary>
        public bool? SupportsMixedListProperty { get; set; }

        /// <summary>
        /// Does the graph allows long to be used as a property value for a graph element?
        /// </summary>
        public bool? SupportsLongProperty { get; set; }

        /// <summary>
        /// Does the graph allows map to be used as a property value for a graph element?
        /// </summary>
        public bool? SupportsMapProperty { get; set; }

        /// <summary>
        /// Graph allows string to be used as a property value for a graph element.
        /// </summary>
        public bool? SupportsStringProperty { get; set; }

        /// <summary>
        /// Does the graph ignore user provided ids in graph.addVertex(object id)?
        /// </summary>
        public bool? IgnoresSuppliedIds { get; set; }

        /// <summary>
        /// Does the graph persist the graph to disk after shutdown?
        /// </summary>
        public bool? IsPersistent { get; set; }

        /// <summary>
        /// Is the graph an RDF framework?
        /// Deprecated thus far, isRDFModel describes a collection of features. Use actual features to describe your data model.
        /// </summary>
        public bool? IsRDFModel { get; set; }

        /// <summary>
        /// Does the graph implement WrapperGraph?
        /// </summary>
        public bool? IsWrapper { get; set; }

        /// <summary>
        /// Does the graph implement IndexableGraph?
        /// </summary>
        public bool? SupportsIndices { get; set; }

        /// <summary>
        /// Does the graph support the indexing of vertices by their properties?
        /// </summary>
        public bool? SupportsVertexIndex { get; set; }

        /// <summary>
        /// Does the graph support the indexing of edges by their properties?
        /// </summary>
        public bool? SupportsEdgeIndex { get; set; }

        /// <summary>
        /// Does the graph implement KeyIndexableGraph?
        /// </summary>
        public bool? SupportsKeyIndices { get; set; }

        /// <summary>
        /// Does the graph support key indexing on vertices?
        /// </summary>
        public bool? SupportsVertexKeyIndex { get; set; }

        /// <summary>
        /// Does the graph support key indexing on edges?
        /// </summary>
        public bool? SupportsEdgeKeyIndex { get; set; }

        /// <summary>
        /// Does the graph support graph.getEdges()?
        /// </summary>
        public bool? SupportsEdgeIteration { get; set; }

        /// <summary>
        /// Does the graph support graph.getVertices()?
        /// </summary>
        public bool? SupportsVertexIteration { get; set; }

        /// <summary>
        /// Does the graph support retrieving edges by id, i.e. graph.getEdge(object id)?
        /// </summary>
        public bool? SupportsEdgeRetrieval { get; set; }

        /// <summary>
        /// Does the graph support setting and retrieving properties on vertices?
        /// </summary>
        public bool? SupportsVertexProperties { get; set; }

        /// <summary>
        /// Does the graph support setting and retrieving properties on edges?
        /// </summary>
        public bool? SupportsEdgeProperties { get; set; }

        /// <summary>
        /// Does the graph implement TransactionalGraph?
        /// </summary>
        public bool? SupportsTransactions { get; set; }

        /// <summary>
        /// Does the graph implement ThreadedTransactionalGraph?
        /// </summary>
        public bool? SupportsThreadedTransactions { get; set; }

        /// <summary>
        /// Checks whether the graph supports both vertex and edge properties
        /// </summary>
        /// <returns>whether the graph supports both vertex and edge properties</returns>
        public bool SupportsElementProperties()
        {
            return SupportsVertexProperties.Value && SupportsEdgeProperties.Value;
        }

        public override string ToString()
        {
            var fields = this
                .GetType()
                .GetProperties()
                .Select(t => string.Format("{0}: {1}", t.Name, t.GetValue(this)));

            return string.Join("\n", fields);
        }

        public IDictionary<string, object> ToMap()
        {
            var fields = this
               .GetType()
               .GetProperties()
               .ToDictionary(t => t.Name, t => t.GetValue(this));

            return fields;
        }

        /// <summary>
        /// This method determines whether the full gamut of features have been set by the Graph implementation.
        /// This is useful for implementers to ensure that they did not miss specifying a feature.
        /// Throws InvalidOperationException if a feature was not set
        /// </summary>
        public void CheckCompliance()
        {
            var notCompliant = this
               .GetType()
               .GetProperties()
               .FirstOrDefault(t => t.GetValue(this) == null);

            if (notCompliant != null)
                throw new InvalidOperationException(string.Format("The feature {0} was not specified", notCompliant.Name));
        }

        /// <summary>
        /// This method copies the features in this features object to another feature object.
        /// </summary>
        /// <returns>a feature object with a clone of the features in the prior.</returns>
        public Features CopyFeatures()
        {
            return this.MemberwiseClone() as Features;
        }
    }
}

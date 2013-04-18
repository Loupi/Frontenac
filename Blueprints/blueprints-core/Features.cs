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
        public bool? supportsDuplicateEdges { get; set; }

        /// <summary>
        /// Does the graph allow an edge to have the same out/tail and in/head vertex?
        /// </summary>
        public bool? supportsSelfLoops { get; set; }

        /// <summary>
        /// Does the graph allow any serializable object to be used as a property value for a graph element?
        /// </summary>
        public bool? supportsSerializableObjectProperty { get; set; }

        /// <summary>
        /// Does the graph allows boolean to be used as a property value for a graph element?
        /// </summary>
        public bool? supportsBooleanProperty { get; set; }

        /// <summary>
        /// Does the graph allows double to be used as a property value for a graph element?
        /// </summary>
        public bool? supportsDoubleProperty { get; set; }

        /// <summary>
        /// Does the graph allows float to be used as a property value for a graph element?
        /// </summary>
        public bool? supportsFloatProperty { get; set; }

        /// <summary>
        /// Does the graph allows integer to be used as a property value for a graph element?
        /// </summary>
        public bool? supportsIntegerProperty { get; set; }

        /// <summary>
        /// Does the graph allows a primitive array to be used as a property value for a graph element?
        /// </summary>
        public bool? supportsPrimitiveArrayProperty { get; set; }

        /// <summary>
        /// Does the graph allows list (all objects with the list have the same data types) to be used as a property value for a graph element?
        /// </summary>
        public bool? supportsUniformListProperty { get; set; }

        /// <summary>
        /// Does the graph allows a mixed list (different data types within the same list) to be used as a
        /// property value for a graph element?
        /// </summary>
        public bool? supportsMixedListProperty { get; set; }

        /// <summary>
        /// Does the graph allows long to be used as a property value for a graph element?
        /// </summary>
        public bool? supportsLongProperty { get; set; }

        /// <summary>
        /// Does the graph allows map to be used as a property value for a graph element?
        /// </summary>
        public bool? supportsMapProperty { get; set; }

        /// <summary>
        /// Graph allows string to be used as a property value for a graph element.
        /// </summary>
        public bool? supportsStringProperty { get; set; }

        /// <summary>
        /// Does the graph ignore user provided ids in graph.addVertex(object id)?
        /// </summary>
        public bool? ignoresSuppliedIds { get; set; }

        /// <summary>
        /// Does the graph persist the graph to disk after shutdown?
        /// </summary>
        public bool? isPersistent { get; set; }

        /// <summary>
        /// Is the graph an RDF framework?
        /// Deprecated thus far, isRDFModel describes a collection of features. Use actual features to describe your data model.
        /// </summary>
        public bool? isRdfModel { get; set; }

        /// <summary>
        /// Does the graph implement WrapperGraph?
        /// </summary>
        public bool? isWrapper { get; set; }

        /// <summary>
        /// Does the graph implement IndexableGraph?
        /// </summary>
        public bool? supportsIndices { get; set; }

        /// <summary>
        /// Does the graph support the indexing of vertices by their properties?
        /// </summary>
        public bool? supportsVertexIndex { get; set; }

        /// <summary>
        /// Does the graph support the indexing of edges by their properties?
        /// </summary>
        public bool? supportsEdgeIndex { get; set; }

        /// <summary>
        /// Does the graph implement KeyIndexableGraph?
        /// </summary>
        public bool? supportsKeyIndices { get; set; }

        /// <summary>
        /// Does the graph support key indexing on vertices?
        /// </summary>
        public bool? supportsVertexKeyIndex { get; set; }

        /// <summary>
        /// Does the graph support key indexing on edges?
        /// </summary>
        public bool? supportsEdgeKeyIndex { get; set; }

        /// <summary>
        /// Does the graph support graph.getEdges()?
        /// </summary>
        public bool? supportsEdgeIteration { get; set; }

        /// <summary>
        /// Does the graph support graph.getVertices()?
        /// </summary>
        public bool? supportsVertexIteration { get; set; }

        /// <summary>
        /// Does the graph support retrieving edges by id, i.e. graph.getEdge(object id)?
        /// </summary>
        public bool? supportsEdgeRetrieval { get; set; }

        /// <summary>
        /// Does the graph support setting and retrieving properties on vertices?
        /// </summary>
        public bool? supportsVertexProperties { get; set; }

        /// <summary>
        /// Does the graph support setting and retrieving properties on edges?
        /// </summary>
        public bool? supportsEdgeProperties { get; set; }

        /// <summary>
        /// Does the graph implement TransactionalGraph?
        /// </summary>
        public bool? supportsTransactions { get; set; }

        /// <summary>
        /// Does the graph implement ThreadedTransactionalGraph?
        /// </summary>
        public bool? supportsThreadedTransactions { get; set; }

        /// <summary>
        /// Checks whether the graph supports both vertex and edge properties
        /// </summary>
        /// <returns>whether the graph supports both vertex and edge properties</returns>
        public bool supportsElementProperties()
        {
            return supportsVertexProperties.Value && supportsEdgeProperties.Value;
        }

        public override string ToString()
        {
            var fields = this
                .GetType()
                .GetProperties()
                .Select(t => string.Format("{0}: {1}", t.Name, t.GetValue(this)));

            return string.Join("\n", fields);
        }

        public IDictionary<string, object> toMap()
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
        public void checkCompliance()
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
        public Features copyFeatures()
        {
            return this.MemberwiseClone() as Features;
        }
    }
}

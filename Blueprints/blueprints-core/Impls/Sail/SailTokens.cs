using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Impls.Sail
{
    public static class SailTokens
    {
        public const string NAMESPACE_SEPARATOR = ":";
        public const string FORWARD_SLASH = "/";
        public const string POUND = "#";
        public const string XSD_PREFIX = "xsd";
        public const string XSD_NS = "http://www.w3.org/2001/XMLSchema#";
        public const string RDF_PREFIX = "rdf";
        public const string RDF_NS = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
        public const string RDFS_PREFIX = "rdfs";
        public const string RDFS_NS = "http://www.w3.org/2000/01/rdf-schema#";
        public const string OWL_PREFIX = "owl";
        public const string OWL_NS = "http://www.w3.org/2002/07/owl#";
        public const string FOAF_PREFIX = "foaf";
        public const string FOAF_NS = "http://xmlns.com/foaf/0.1/";
        public const string BLANK_NODE_PREFIX = "_:";
        public const string URN_UUID_PREFIX = "urn:uuid:";

        public const string DATATYPE = "type";
        public const string LANGUAGE = "lang";
        public const string VALUE = "value";
        public const string KIND = "kind";
        public const string NAMED_GRAPH = "ng";

        public const string URI = "uri";
        public const string BNODE = "bnode";
        public const string LITERAL = "literal";

        public const string PREFIX_SPACE = "PREFIX ";
        public const string COLON_LESSTHAN = ": <";
        public const string GREATERTHAN_NEWLINE = ">\n";
    }
}

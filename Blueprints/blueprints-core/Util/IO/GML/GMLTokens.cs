using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.IO.GML
{
    /// <summary>
    /// A collection of tokens used for GML related data.
    /// <p/>
    /// Tokens defined from GML Tags
    /// (http://www.fim.uni-passau.de/fileadmin/files/lehrstuhl/brandenburg/projekte/gml/gml-documentation.tar.gz)
    /// </summary>
    public static class GMLTokens
    {
        public const string GML = "gml";
        public const string ID = "id";
        public const string NAME = "name";
        public const string LABEL = "label";
        public const string COMMENT = "comment";
        public const string CREATOR = "Creator";
        public const string VERSION = "Version";
        public const string GRAPH = "graph";
        public const string NODE = "node";
        public const string EDGE = "edge";
        public const string SOURCE = "source";
        public const string TARGET = "target";
        public const string DIRECTED = "directed"; // directed (0) undirected (1) default is undirected
        public const string GRAPHICS = "graphics";
        public const string LABEL_GRAPHICS = "LabelGraphics";
        public const char COMMENT_CHAR = '#';

        /// <summary>
        /// Special token used to store Blueprint ids as they may not be integers
        /// </summary>
        public const string BLUEPRINTS_ID = "blueprintsId";
    }
}

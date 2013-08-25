namespace Frontenac.Blueprints
{
    /// <summary>
    /// MetaGraph can be implemented as a way to access the underlying native graph engine.
    /// This is useful for those Graph implementations that are not native Blueprints implementations.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IMetaGraph<out T> : IGraph
    {
        /// <summary>
        /// Get the raw underlying graph engine that exposes the Blueprints API.
        /// </summary>
        /// <value>the raw underlying graph engine</value>
        T RawGraph { get; }
    }
}

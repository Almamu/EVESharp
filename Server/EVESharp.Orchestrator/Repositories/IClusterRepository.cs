using EVESharp.Orchestrator.Models;

namespace EVESharp.Orchestrator.Repositories;

public interface IClusterRepository
{
    /// <summary>
    /// Gets the full list of registered nodes in the repository
    /// </summary>
    /// <returns></returns>
    List <Node> FindNodes ();

    /// <summary>
    /// Searches for the node with the given address
    /// </summary>
    /// <param name="address">The node to search for</param>
    /// <returns></returns>
    Node FindByAddress (string address);

    /// <summary>
    /// Searches for the node with the given ID
    /// </summary>
    /// <param name="nodeId">The node to search for</param>
    /// <returns></returns>
    Node FindById (int nodeId);

    /// <summary>
    /// Searches for the registered proxy nodes
    /// </summary>
    /// <returns></returns>
    List <Node> FindProxyNodes ();

    /// <summary>
    /// Searches for normal server nodes
    /// </summary>
    /// <returns></returns>
    List <Node> FindServerNodes ();

    /// <summary>
    /// Registers a new node in the repository
    /// </summary>
    /// <param name="port"></param>
    /// <param name="role"></param>
    /// <returns></returns>
    Node RegisterNode (string ip, ushort port, string role);

    /// <summary>
    /// Searches the repository for the least loaded node and returns it
    /// </summary>
    /// <returns></returns>
    Node GetLeastLoadedNode ();

    /// <summary>
    /// Updates heartbeat information for the given node
    /// </summary>
    /// <param name="address"></param>
    /// <param name="load"></param>
    void Hearbeat (string address, double load);
}
using System.Collections.Frozen;
using System.Collections.ObjectModel;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Persistence14.Construction.Graph;

/// <summary>
/// Defines a complete construction graph.
/// </summary>
public sealed partial class ConstructionGraphPrototype : IPrototype, ISerializationHooks
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    private ConstructionGraphNode[] _nodes = [];
    private readonly Dictionary<string, ConstructionGraphNode> _nodeDict = new();
    public ReadOnlyDictionary<string, ConstructionGraphNode> Nodes => _nodeDict.AsReadOnly();

    void ISerializationHooks.AfterDeserialization()
    {
        _nodeDict.Clear();

        foreach (var node in _nodes)
        {
            _nodeDict.Add(node.ID, node);
        }
    }
}
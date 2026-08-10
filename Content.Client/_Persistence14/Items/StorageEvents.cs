namespace Content.Client._Persistence14.Items;

[ByRefEvent]
public sealed partial class GetStorageVisualsEvent : EntityEventArgs
{
    public readonly List<(string, PrototypeLayerData)> Layers = new();
}
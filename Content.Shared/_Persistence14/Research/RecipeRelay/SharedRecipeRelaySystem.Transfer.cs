using System.Linq;
using Content.Shared._Persistence14.Log;
using Content.Shared.Research.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._Persistence14.Research.RecipeRelay;

public sealed partial class SharedRecipeRelaySystem
{
    /// <summary>
    /// Attempts to transfer a recipe from one recipe container to another.
    /// </summary>
    public bool TryTransferRecipe(EntityUid from, EntityUid to,
        ProtoId<LatheRecipePrototype> recipeId, int count = -1)
    {
        if (!TryGetRecipeContainer(from, out var fromContainer) ||
            !TryGetRecipeContainer(to, out var toContainer) ||
            fromContainer.Owner == toContainer.Owner) // Relays end up pointing to same container.
            return false;

        if (!fromContainer.Comp.UnlockedRecipes.TryGetValue(recipeId, out var qty))
            return false;

        if (count < 0 || count > qty)
            count = qty;

        var ev = new RecipeRelayCompatibilityCheckEvent();
        RaiseLocalEvent(toContainer.Owner, ref ev);
        if (ev.CompatibleRecipes.Count > 0 && !ev.CompatibleRecipes.Contains(recipeId))
            return false;

        var current = 0;
        if (toContainer.Comp.UnlockedRecipes.TryGetValue(recipeId, out var curr))
            current = curr;
        toContainer.Comp.UnlockedRecipes[recipeId] = current + count;
        fromContainer.Comp.UnlockedRecipes[recipeId] -= count;
        if (fromContainer.Comp.UnlockedRecipes[recipeId] <= 0)
            fromContainer.Comp.UnlockedRecipes.Remove(recipeId);

        Dirty(fromContainer);
        Dirty(toContainer);
        return true;
    }

    /// <summary>
    /// Transfers all applicable technologies in the provided quantities from the from entity to the to entity, provided those are different containers.
    /// If a count is negative, all prints will be transfered.
    /// Returns true if any recipes were transfered. Otherwise, false.
    /// </summary>
    public bool TransferRecipes(EntityUid from, EntityUid to, bool ignoreCompatibility, params (ProtoId<LatheRecipePrototype> recipeId, int count)[] recipeIds)
    {
        if (!TryGetRecipeContainer(from, out var fromContainer) ||
            !TryGetRecipeContainer(to, out var toContainer) ||
            fromContainer.Owner == toContainer.Owner) // Relays end up pointing to same container.
            return false;

        var ev = new RecipeRelayCompatibilityCheckEvent();
        if (!ignoreCompatibility)
            RaiseLocalEvent(toContainer.Owner, ref ev);

        var any = false;
        foreach (var (recipeId, count) in recipeIds)
        {
            if (!fromContainer.Comp.UnlockedRecipes.TryGetValue(recipeId, out var supply) ||
                !ev.Check(recipeId))
                continue;

            any = true;

            var qty = count;
            if (qty < 0 || qty > supply)
                qty = supply;
            fromContainer.Comp.UnlockedRecipes[recipeId] -= qty;
            if (fromContainer.Comp.UnlockedRecipes[recipeId] <= 0)
                fromContainer.Comp.UnlockedRecipes.Remove(recipeId);

            if (toContainer.Comp.UnlockedRecipes.TryGetValue(recipeId, out var current))
                qty += current;
            toContainer.Comp.UnlockedRecipes[recipeId] = qty;
        }

        Dirty(fromContainer);
        Dirty(toContainer);
        return any;
    }
    /// <summary>
    /// Transfers all applicable technologies in the provided quantities from the from entity to the to entity, provided those are different containers.
    /// If a count is negative, all prints will be transfered.
    /// Returns true if any recipes were transfered. Otherwise, false.
    /// </summary>
    public bool TransferRecipes(EntityUid from, EntityUid to, params (ProtoId<LatheRecipePrototype> recipeId, int count)[] recipeIds)
        => TransferRecipes(from, to, false, recipeIds);

    /// <summary>
    /// Transfers all recipes from one container to another. Returns true if any recipes were transfered. Otherwise, false.
    /// </summary>
    public bool TransferAllRecipes(EntityUid from, EntityUid to, bool ignoreCompatibility = false)
    {
        if (!TryGetRecipeContainer(from, out var fromContainer) ||
            !TryGetRecipeContainer(to, out var toContainer) ||
            fromContainer.Owner == toContainer.Owner) // Relays end up pointing to same container.
            return false;

        var ev = new RecipeRelayCompatibilityCheckEvent();
        if (!ignoreCompatibility) // If the event is never raised, the filter can never have anything in it.
            RaiseLocalEvent(toContainer.Owner, ref ev);

        bool any = false;
        foreach (var (recipeId, count) in fromContainer.Comp.UnlockedRecipes.ToArray())
        {
            if (!ev.Check(recipeId))
                continue;

            any = true;
            fromContainer.Comp.UnlockedRecipes.Remove(recipeId);

            var qty = count;
            if (toContainer.Comp.UnlockedRecipes.TryGetValue(recipeId, out var current))
                qty += current;
            toContainer.Comp.UnlockedRecipes[recipeId] = qty;
        }

        Dirty(fromContainer);
        Dirty(toContainer);
        return any;
    }

    /// <summary>
    /// Attempts to add an unlockable recipe to the container.
    /// </summary>
    public bool TryAddRecipe(EntityUid uid, ProtoId<LatheRecipePrototype> recipeId, int count = 1)
    {
        if (!TryGetRecipeContainer(uid, out var container))
            return false;

        var current = 0;
        if (container.Comp.UnlockedRecipes.TryGetValue(recipeId, out var curr))
            current = curr;
        container.Comp.UnlockedRecipes[recipeId] = current + count;
        Dirty(container);
        return true;
    }

    /// <summary>
    /// Attempts to remove a specific quantity of an unlocked recipe from a container. 
    /// When count is negative (by default) all of the specified recipe are removed.
    /// </summary>
    public bool TryRemoveRecipe(EntityUid uid, ProtoId<LatheRecipePrototype> recipeId, int count = -1, bool allowOverdraw = true)
        => TryRemoveRecipe(uid, recipeId, out _, count, allowOverdraw);
    /// <summary>
    /// Attempts to remove a specific quantity of an unlocked recipe from a container. 
    /// When count is negative (by default) all of the specified recipe are removed. 
    /// Provides any overflow as an output variable
    /// </summary>
    public bool TryRemoveRecipe(EntityUid uid, ProtoId<LatheRecipePrototype> recipeId, out int overflow, int count = -1, bool allowOverdraw = true)
    {
        overflow = 0;
        if (!TryGetRecipeContainer(uid, out var container))
            return false;

        if (!container.Comp.UnlockedRecipes.TryGetValue(recipeId, out var qty))
            return false;

        if (count >= qty)
        {
            overflow = count - qty;
            if (count > qty && !allowOverdraw) return false;
            container.Comp.UnlockedRecipes.Remove(recipeId);
            Dirty(container);
            return true;
        }

        if (count < 0)
        {
            container.Comp.UnlockedRecipes.Remove(recipeId);
            Dirty(container);
            return true;
        }

        container.Comp.UnlockedRecipes[recipeId] -= count;
        Dirty(container);
        return true;
    }

    /// <summary>
    /// Removes all recipes from the container.
    /// </summary>
    public void ClearRecipes(EntityUid uid)
    {
        if (!TryGetRecipeContainer(uid, out var container))
            return; // No container to clear

        container.Comp.UnlockedRecipes.Clear();
        Dirty(container);
    }

    /// <summary>
    /// Copies the recipes from one container to another.
    /// </summary>
    public void CopyTo(Entity<RecipeContainerComponent> root, Entity<RecipeContainerComponent> copy)
    {
        foreach (var (key, qty) in root.Comp.UnlockedRecipes)
        {
            var current = 0;
            if (copy.Comp.UnlockedRecipes.TryGetValue(key, out var curr))
                current = curr;
            copy.Comp.UnlockedRecipes[key] = current + qty;
        }

        Dirty(copy);
    }
}
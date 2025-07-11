// SPDX-FileCopyrightText: 2022 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DEATHB4DEFEAT <77995199+DEATHB4DEFEAT@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Zoldorf <silvertorch5@gmail.com>
// SPDX-FileCopyrightText: 2024 Cojoke <83733158+Cojoke-dot@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2024 to4no_fix <156101927+chavonadelal@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 CybersunBot <cybersunbot@proton.me>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 SX-7 <92227810+SX-7@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 paige404 <59348003+paige404@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Diagnostics.CodeAnalysis;
using Content.Shared.Clothing.EntitySystems;
using Content.Shared.DoAfter;
using Content.Shared.Inventory;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Clothing.Components;

/// <summary>
///     This handles entities which can be equipped.
/// </summary>
//[Access(typeof(ClothingSystem), typeof(InventorySystem))] - Fuck yo access - Goob
[RegisterComponent, NetworkedComponent]
public sealed partial class ClothingComponent : Component
{
    [DataField("clothingVisuals")]
    public Dictionary<string, List<PrototypeLayerData>> ClothingVisuals = new();

    /// <summary>
    /// The name of the layer in the user that this piece of clothing will map to
    /// </summary>
    [DataField]
    public string? MappedLayer;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("quickEquip")]
    public bool QuickEquip = true;

    /// <summary>
    /// The slots in which the clothing is considered "worn" or "equipped". E.g., putting shoes in your pockets does not
    /// equip them as far as clothing related events are concerned.
    /// </summary>
    /// <remarks>
    /// Note that this may be a combination of different slot flags, not a singular bit.
    /// </remarks>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField(required: true)]
    // [Access(typeof(ClothingSystem), typeof(InventorySystem), Other = AccessPermissions.ReadExecute)] // Goobstation - FUCK YOUR ACCESS! WE GOIDA IN THIS BITCH
    public SlotFlags Slots = SlotFlags.NONE;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("equipSound")]
    public SoundSpecifier? EquipSound;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("unequipSound")]
    public SoundSpecifier? UnequipSound;

    [Access(typeof(ClothingSystem))]
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("equippedPrefix")]
    public string? EquippedPrefix;

    /// <summary>
    /// Allows the equipped state to be directly overwritten.
    /// useful when prototyping INNERCLOTHING items into OUTERCLOTHING items without duplicating/modifying RSIs etc.
    /// </summary>
    [Access(typeof(ClothingSystem))]
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("equippedState")]
    public string? EquippedState;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("sprite")]
    public string? RsiPath;

    /// <summary>
    /// Name of the inventory slot the clothing is currently in.
    /// Note that this being non-null does not mean the clothing is considered "worn" or "equipped" unless the slot
    /// satisfies the <see cref="Slots"/> flags.
    /// </summary>
    [DataField]
    public string? InSlot;
    // TODO CLOTHING
    // Maybe keep this null unless its in a valid slot?
    // To lazy to figure out ATM if that would break anything.
    // And when doing this, combine InSlot and InSlotFlag, as it'd be a breaking change for downstreams anyway

    /// <summary>
    /// Slot flags of the slot the clothing is currently in. See also <see cref="InSlot"/>.
    /// </summary>
    [DataField]
    public SlotFlags? InSlotFlag;
    // TODO CLOTHING
    // Maybe keep this null unless its in a valid slot?
    // And when doing this, combine InSlot and InSlotFlag, as it'd be a breaking change for downstreams anyway

    [Access(typeof(ClothingSystem), typeof(InventorySystem), Other = AccessPermissions.ReadWrite)]
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan EquipDelay = TimeSpan.Zero;

    [Access(typeof(ClothingSystem), typeof(InventorySystem), Other = AccessPermissions.ReadWrite)]
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan UnequipDelay = TimeSpan.Zero;

    /// <summary>
    /// Offset for the strip time for an entity with this component.
    /// Only applied when it is being equipped or removed by another player.
    /// </summary>
    [DataField]
    public TimeSpan StripDelay = TimeSpan.Zero;
}

[Serializable, NetSerializable]
public sealed class ClothingComponentState : ComponentState
{
    public string? EquippedPrefix;

    public ClothingComponentState(string? equippedPrefix)
    {
        EquippedPrefix = equippedPrefix;
    }
}

public enum ClothingMask : byte
{
    NoMask = 0,
    UniformFull,
    UniformTop
}

[Serializable, NetSerializable]
public sealed partial class ClothingEquipDoAfterEvent : DoAfterEvent
{
    public string Slot;

    public ClothingEquipDoAfterEvent(string slot)
    {
        Slot = slot;
    }

    public override DoAfterEvent Clone() => this;
}

[Serializable, NetSerializable]
public sealed partial class ClothingUnequipDoAfterEvent : DoAfterEvent
{
    public string Slot;

    public ClothingUnequipDoAfterEvent(string slot)
    {
        Slot = slot;
    }

    public override DoAfterEvent Clone() => this;
}

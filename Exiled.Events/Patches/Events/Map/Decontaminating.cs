// -----------------------------------------------------------------------
// <copyright file="Decontaminating.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.Patches.Events.Map
{
#pragma warning disable SA1118
    using System.Collections.Generic;
    using System.Reflection.Emit;

    using Exiled.Events.EventArgs;

    using HarmonyLib;

    using LightContainmentZoneDecontamination;

    using NorthwoodLib.Pools;

    using static HarmonyLib.AccessTools;

    /// <summary>
    /// Patches <see cref="DecontaminationController.FinishDecontamination"/>.
    /// Adds the <see cref="Decontaminating"/> event.
    /// </summary>
    [HarmonyPatch(typeof(DecontaminationController), nameof(DecontaminationController.FinishDecontamination))]
    internal static class Decontaminating
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

            Label returnLabel = newInstructions[newInstructions.Count - 1].labels[0];

            newInstructions.InsertRange(0, new CodeInstruction[]
            {
                new(OpCodes.Ldc_I4_1),
                new(OpCodes.Newobj, GetDeclaredConstructors(typeof(DecontaminatingEventArgs))[0]),
                new(OpCodes.Dup),
                new(OpCodes.Call, Method(typeof(Handlers.Map), nameof(Handlers.Map.OnDecontaminating))),
                new(OpCodes.Callvirt, PropertyGetter(typeof(DecontaminatingEventArgs), nameof(DecontaminatingEventArgs.IsAllowed))),
                new(OpCodes.Brfalse, returnLabel),
            });

            for (int z = 0; z < newInstructions.Count; z++)
                yield return newInstructions[z];

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }
    }
}

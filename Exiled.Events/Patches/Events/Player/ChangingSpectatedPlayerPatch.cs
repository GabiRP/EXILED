// -----------------------------------------------------------------------
// <copyright file="ChangingSpectatedPlayerPatch.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

#pragma warning disable SA1118 // Parameter should not span multiple lines

namespace Exiled.Events.Patches.Events.Player
{
    using System.Collections.Generic;
    using System.Reflection.Emit;

    using Exiled.Events.EventArgs;

    using HarmonyLib;
    using NorthwoodLib.Pools;

    using Player = Exiled.Events.Handlers.Player;

    /// <summary>
    /// Patches <see cref="SpectatorManager.CurrentSpectatedPlayer"/> setter.
    /// Adds the <see cref="Player.ChangingSpectatedPlayer"/>.
    /// </summary>
    [HarmonyPatch(typeof(SpectatorManager), nameof(SpectatorManager.CurrentSpectatedPlayer), MethodType.Setter)]
    internal static class ChangingSpectatedPlayerPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);
            Label continueLabel = generator.DefineLabel();
            Label endLabel = generator.DefineLabel();
            Label elseLabel = generator.DefineLabel();

            LocalBuilder player = generator.DeclareLocal(typeof(API.Features.Player));
            LocalBuilder ev = generator.DeclareLocal(typeof(ChangingSpectatedPlayerEventArgs));

            int index = newInstructions.FindIndex(x => x.opcode == OpCodes.Ret) + 1;

            CodeInstruction firstLabel = new CodeInstruction(OpCodes.Ldarg_0).MoveLabelsFrom(newInstructions[index]);
            newInstructions[index].labels.Add(endLabel);

            newInstructions.InsertRange(index, new[]
            {
                    /*
                     *  var player = Player.Get(__instance._hub);
                     *  if (player is not null)
                     *  {
                     *      var ev = new ChangingSpectatedPlayerEventArgs(player, Player.Get(__instance.CurrentSpectatedPlayer), Player.Get(value));
                     *
                     *      Exiled.Events.Handlers.Player.OnChangingSpectatedPlayer(ev);
                     *
                     *      if(!ev.IsAllowed) return;
                     *
                     *      value = ev.NewTarget?.ReferenceHub ?? ev.Player.ReferenceHub;
                     *  }
                     */

                // var player = Player.Get(__instance._hub);
                firstLabel,
                new(OpCodes.Ldfld, AccessTools.Field(typeof(SpectatorManager), nameof(SpectatorManager._hub))),
                new(OpCodes.Call, AccessTools.Method(typeof(API.Features.Player), nameof(API.Features.Player.Get), new[] { typeof(ReferenceHub) })),
                new(OpCodes.Dup),

                // if (player is not null)
                new(OpCodes.Stloc, player),
                new(OpCodes.Brfalse_S, endLabel),
                new(OpCodes.Ldloc, player),

                // Player.Get(__instance.CurrentSpectatedPlayer)
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldfld, AccessTools.Field(typeof(SpectatorManager), nameof(SpectatorManager._currentSpectatedPlayer))),
                new(OpCodes.Call, AccessTools.Method(typeof(API.Features.Player), nameof(API.Features.Player.Get), new[] { typeof(ReferenceHub) })),

                // Player.Get(value)
                new(OpCodes.Ldarg_1),
                new(OpCodes.Call, AccessTools.Method(typeof(API.Features.Player), nameof(API.Features.Player.Get), new[] { typeof(ReferenceHub) })),

                // var ev = new ChangingSpectatedPlayerEventArgs(player, Player.Get(__instance.CurrentSpectatedPlayer), Player.Get(value))
                new(OpCodes.Ldc_I4_1),
                new(OpCodes.Newobj, AccessTools.GetDeclaredConstructors(typeof(ChangingSpectatedPlayerEventArgs))[0]),
                new(OpCodes.Dup),
                new(OpCodes.Dup), new(OpCodes.Stloc, ev),

                // Exiled.Events.Handlers.Player.OnChangingSpectatedPlayer(ev);
                new(OpCodes.Call, AccessTools.Method(typeof(Player), nameof(Player.OnChangingSpectatedPlayer))),

                // if(!ev.IsAllowed) return;
                new(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(ChangingSpectatedPlayerEventArgs), nameof(ChangingSpectatedPlayerEventArgs.IsAllowed))),
                new(OpCodes.Brtrue_S, continueLabel),
                new(OpCodes.Ret),

                // ev.NewTarget;
                new CodeInstruction(OpCodes.Ldloc, ev).WithLabels(continueLabel),
                new(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(ChangingSpectatedPlayerEventArgs), nameof(ChangingSpectatedPlayerEventArgs.NewTarget))),

                // if(ev.NewTarget is null)
                new(OpCodes.Dup),
                new(OpCodes.Brtrue_S, elseLabel),

                // value = ev.Player.ReferenceHub;
                new(OpCodes.Pop),
                new(OpCodes.Ldloc, ev),
                new(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(ChangingSpectatedPlayerEventArgs), nameof(ChangingSpectatedPlayerEventArgs.Player))),

                // value = ev.NewTarget.ReferenceHub;
                new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(API.Features.Player), nameof(API.Features.Player.ReferenceHub))).WithLabels(elseLabel),
                new(OpCodes.Starg_S, 1),
            });

            for (int z = 0; z < newInstructions.Count; z++)
                yield return newInstructions[z];

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }
    }
}

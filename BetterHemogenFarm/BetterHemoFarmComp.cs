using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace BetterHemogenFarm
{
    public class BetterHemoFarmComp : ThingComp
    {
        private Pawn Pawn => (Pawn)this.parent;
        private bool shouldFarmHemogen;

        // Hoisted from:
        // RimWorld.Pawn_GuestTracker
        // public void GuestTrackerTick()
        public override void CompTick()
        {
            Pawn pawn = Pawn;
            if (ModsConfig.BiotechActive && pawn.Spawned && pawn.IsHashIntervalTick(15000) && pawn.BillStack != null && !pawn.BillStack.Bills.Any((Bill x) => x.recipe == RecipeDefOf.ExtractHemogenPack) && RecipeDefOf.ExtractHemogenPack.Worker.AvailableOnNow(pawn) && !pawn.health.hediffSet.HasHediff(HediffDefOf.BloodLoss))
            {
                HealthCardUtility.CreateSurgeryBill(pawn, RecipeDefOf.ExtractHemogenPack, null, null, sendMessages: false);
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo item in base.CompGetGizmosExtra())
            {
                yield return item;
            }
            Pawn pawn = Pawn;
            if (pawn.IsColonist || pawn.IsPrisonerOfColony)
            {
                Command_Toggle command_Toggle2 = new Command_Toggle();
                command_Toggle2.defaultLabel = "HemogenFarmLabel";
                command_Toggle2.defaultDesc = "HemogenFarmDescription";
                command_Toggle2.hotKey = null;
                command_Toggle2.icon = DefDatabase<ThingDef>.GetNamed("HemogenPack").uiIcon;
                command_Toggle2.isActive = (() => shouldFarmHemogen);
                command_Toggle2.toggleAction = delegate
                {
                    shouldFarmHemogen = !shouldFarmHemogen;
                };
                yield return command_Toggle2;
            }
        }
    }
}

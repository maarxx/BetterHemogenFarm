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
        private Pawn Pawn => (Pawn) this.parent;
        public override void CompTick()
        {
            //Log.Error("Let's error on every tick!");
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
                command_Toggle2.defaultLabel = "CommandFireAtWillLabel".Translate();
                command_Toggle2.defaultDesc = "CommandFireAtWillDesc".Translate();
                command_Toggle2.hotKey = KeyBindingDefOf.Misc6;
                //command_Toggle2.icon = TexCommand.ToggleVent;
                command_Toggle2.icon = ContentFinder<Texture2D>.Get("UI/Commands/AssignOwner");
                command_Toggle2.isActive = (() => ShouldFarmHemogen);
                command_Toggle2.toggleAction = delegate
                {
                    ShouldFarmHemogen = !ShouldFarmHemogen;
                };
                yield return command_Toggle2;
            }
        }

        /// <summary>
        /// By default props returns the base CompProperties class.
        /// You can get props and cast it everywhere you use it, 
        /// or you create a Getter like this, which casts once and returns it.
        /// Careful of case sensitivity!
        /// </summary>
        public BetterHemoFarmCompProperties Props => (BetterHemoFarmCompProperties)this.props;

        public bool ShouldFarmHemogen
        {
            get
            {
                return Props.shouldFarmHemogen;
            }
            set
            {
                Props.shouldFarmHemogen = value;
            }
        }
    }

    public class BetterHemoFarmCompProperties : CompProperties
    {
        public bool shouldFarmHemogen;

        /// <summary>
        /// These constructors aren't strictly required if the compClass is set in the XML.
        /// </summary>
        public BetterHemoFarmCompProperties()
        {
            this.compClass = typeof(BetterHemoFarmComp);
            this.shouldFarmHemogen = false;
        }

        public BetterHemoFarmCompProperties(Type compClass) : base(compClass)
        {
            this.compClass = compClass;
            this.shouldFarmHemogen = false;
        }
    }
}

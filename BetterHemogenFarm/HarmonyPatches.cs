using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace BetterHemogenFarm
{
    public static class HarmonyPatches
    {
        [StaticConstructorOnStartup]
        class Main
        {
            static Main()
            {
                var harmony = new Harmony("com.github.harmony.rimworld.maarx.betterhemogenfarm");
                Log.Message("Hello from Harmony in scope: com.github.harmony.rimworld.maarx.betterhemogenfarm");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
        }

        [HarmonyPatch(typeof(HealthCardUtility), "DrawOverviewTab")]
        public static class Patch_HealthCardUtility_HealthCardUtility
        {
            public static bool Prefix(ref float __result, Rect leftRect, Pawn pawn, float curY)
            {
                //Log.Message("betterhemogenfarm - HealthCardUtility - DrawOverviewTab - 1");
                curY += 4f;
                Text.Font = GameFont.Tiny;
                Text.Anchor = TextAnchor.UpperLeft;
                GUI.color = new Color(0.9f, 0.9f, 0.9f);
                string str = (pawn.gender == Gender.None) ? ((string)"PawnSummary".Translate(pawn.Named("PAWN"))) : ((string)"PawnSummaryWithGender".Translate(pawn.Named("PAWN")));
                Rect rect = new Rect(0f, curY, leftRect.width, 34f);
                Widgets.Label(rect, str.CapitalizeFirst());
                if (Mouse.IsOver(rect))
                {
                    TooltipHandler.TipRegion(rect, () => pawn.ageTracker.AgeTooltipString, 73412);
                    Widgets.DrawHighlight(rect);
                }
                GUI.color = Color.white;
                curY += 34f;
                if (pawn.foodRestriction != null && pawn.foodRestriction.Configurable && !pawn.DevelopmentalStage.Baby())
                {
                    Rect rect2 = new Rect(0f, curY, leftRect.width * 0.42f, 23f);
                    Text.Anchor = TextAnchor.MiddleLeft;
                    Widgets.Label(rect2, Text.TinyFontSupported ? "FoodRestriction".Translate() : "FoodRestrictionShort".Translate());
                    GenUI.ResetLabelAlign();
                    if (Widgets.ButtonText(new Rect(rect2.width, curY, leftRect.width - rect2.width, 23f), pawn.foodRestriction.CurrentFoodRestriction.label))
                    {
                        List<FloatMenuOption> list = new List<FloatMenuOption>();
                        List<FoodRestriction> allFoodRestrictions = Current.Game.foodRestrictionDatabase.AllFoodRestrictions;
                        for (int i = 0; i < allFoodRestrictions.Count; i++)
                        {
                            FoodRestriction localRestriction = allFoodRestrictions[i];
                            list.Add(new FloatMenuOption(localRestriction.label, delegate
                            {
                                pawn.foodRestriction.CurrentFoodRestriction = localRestriction;
                            }));
                        }
                        list.Add(new FloatMenuOption("ManageFoodRestrictions".Translate(), delegate
                        {
                            Find.WindowStack.Add(new Dialog_ManageFoodRestrictions(null));
                        }));
                        Find.WindowStack.Add(new FloatMenu(list));
                    }
                    curY += 23f;
                }
                if (pawn.IsColonist || pawn.IsPrisonerOfColony)
                {
                    bool shouldSiphon = true;
                    Rect rect3 = new Rect(0f, curY, leftRect.width, 24f);
                    //Widgets.CheckboxLabeled(rect3, "SelfTend".Translate(), ref pawn.playerSettings.selfTend);
                    Widgets.CheckboxLabeled(rect3, "Siphon", ref shouldSiphon);
                    curY += 28f;
                }
                if (pawn.IsColonist && !pawn.Dead && !pawn.DevelopmentalStage.Baby())
                {
                    bool selfTend = pawn.playerSettings.selfTend;
                    Rect rect3 = new Rect(0f, curY, leftRect.width, 24f);
                    Widgets.CheckboxLabeled(rect3, "SelfTend".Translate(), ref pawn.playerSettings.selfTend);
                    if (pawn.playerSettings.selfTend && !selfTend)
                    {
                        if (pawn.WorkTypeIsDisabled(WorkTypeDefOf.Doctor))
                        {
                            pawn.playerSettings.selfTend = false;
                            Messages.Message("MessageCannotSelfTendEver".Translate(pawn.LabelShort, pawn), MessageTypeDefOf.RejectInput, historical: false);
                        }
                        else if (pawn.workSettings.GetPriority(WorkTypeDefOf.Doctor) == 0)
                        {
                            Messages.Message("MessageSelfTendUnsatisfied".Translate(pawn.LabelShort, pawn), MessageTypeDefOf.CautionInput, historical: false);
                        }
                    }
                    if (Mouse.IsOver(rect3))
                    {
                        TooltipHandler.TipRegion(rect3, "SelfTendTip".Translate(Faction.OfPlayer.def.pawnsPlural, 0.7f.ToStringPercent()).CapitalizeFirst());
                    }
                    curY += 28f;
                }
                if (pawn.RaceProps.IsFlesh && (pawn.Faction == Faction.OfPlayer || pawn.HostFaction == Faction.OfPlayer || (pawn.NonHumanlikeOrWildMan() && pawn.InBed() && pawn.CurrentBed().Faction == Faction.OfPlayer)) && pawn.playerSettings != null && !pawn.Dead && Current.ProgramState == ProgramState.Playing)
                {
                    MedicalCareUtility.MedicalCareSetter(new Rect(0f, curY, 140f, 28f), ref pawn.playerSettings.medCare);
                    if (Widgets.ButtonText(new Rect(leftRect.width - 70f, curY, 70f, 28f), "MedGroupDefaults".Translate()))
                    {
                        Find.WindowStack.Add(new Dialog_MedicalDefaults());
                    }
                    curY += 32f;
                }
                Text.Font = GameFont.Small;
                //Log.Message("betterhemogenfarm - HealthCardUtility - DrawOverviewTab - 2");
                if (pawn.def.race.IsFlesh)
                {
                    Pair<string, Color> painLabel = HealthCardUtility.GetPainLabel(pawn);
                    string painTip = HealthCardUtility.GetPainTip(pawn);
                    //curY = DrawLeftRow(leftRect, curY, "PainLevel".Translate(), painLabel.First, painLabel.Second, painTip);
                    //class HealthCardUtility
                    //private static float DrawLeftRow(Rect leftRect, float curY, string leftLabel, string rightLabel, Color rightLabelColor, TipSignal tipSignal)
                    MethodInfo dynMethod = typeof(HealthCardUtility).GetMethod("DrawLeftRow", BindingFlags.Static | BindingFlags.NonPublic);
                    //curY = DrawLeftRow(leftRect, curY, "PainLevel".Translate(), painLabel.First, painLabel.Second, painTip);
                    curY = (float)dynMethod.Invoke(obj: null, parameters: new object[] { leftRect, curY, "PainLevel".Translate().ToString(), painLabel.First, painLabel.Second, new TipSignal(painTip) });
                }
                Log.Message("betterhemogenfarm - HealthCardUtility - DrawOverviewTab - 3");
                if (!pawn.Dead)
                {
                    IEnumerable<PawnCapacityDef> source = pawn.def.race.Humanlike ? (from x in DefDatabase<PawnCapacityDef>.AllDefs
                                                                                     where x.showOnHumanlikes
                                                                                     select x) : ((!pawn.def.race.Animal) ? DefDatabase<PawnCapacityDef>.AllDefs.Where((PawnCapacityDef x) => x.showOnMechanoids) : DefDatabase<PawnCapacityDef>.AllDefs.Where((PawnCapacityDef x) => x.showOnAnimals));
                    {
                        foreach (PawnCapacityDef item in from act in source
                                                         orderby act.listOrder
                                                         select act)
                        {
                            if (PawnCapacityUtility.BodyCanEverDoCapacity(pawn.RaceProps.body, item))
                            {
                                PawnCapacityDef activityLocal = item;
                                Pair<string, Color> efficiencyLabel = HealthCardUtility.GetEfficiencyLabel(pawn, item);
                                Func<string> textGetter = delegate
                                {
                                    if (!pawn.Dead)
                                    {
                                        return HealthCardUtility.GetPawnCapacityTip(pawn, activityLocal);
                                    }
                                    return "";
                                };
                                //curY = DrawLeftRow(leftRect, curY, item.GetLabelFor(pawn.RaceProps.IsFlesh, pawn.RaceProps.Humanlike).CapitalizeFirst(), efficiencyLabel.First, efficiencyLabel.Second, new TipSignal(textGetter, pawn.thingIDNumber ^ item.index));
                                //class HealthCardUtility
                                //private static float DrawLeftRow(Rect leftRect, float curY, string leftLabel, string rightLabel, Color rightLabelColor, TipSignal tipSignal)
                                MethodInfo dynMethod = typeof(HealthCardUtility).GetMethod("DrawLeftRow", BindingFlags.Static | BindingFlags.NonPublic);
                                //curY = DrawLeftRow(leftRect, curY, item.GetLabelFor(pawn.RaceProps.IsFlesh, pawn.RaceProps.Humanlike).CapitalizeFirst(), efficiencyLabel.First, efficiencyLabel.Second, new TipSignal(textGetter, pawn.thingIDNumber ^ item.index));
                                curY = (float)dynMethod.Invoke(obj: null, parameters: new object[] { leftRect, curY, item.GetLabelFor(pawn.RaceProps.IsFlesh, pawn.RaceProps.Humanlike).CapitalizeFirst(), efficiencyLabel.First, efficiencyLabel.Second, new TipSignal(textGetter, pawn.thingIDNumber ^ item.index) });
                            }
                        }
                        Log.Message("betterhemogenfarm - HealthCardUtility - DrawOverviewTab - 4");
                        __result = curY;
                        return false;
                    }
                }
                Log.Message("betterhemogenfarm - HealthCardUtility - DrawOverviewTab - 5");
                __result = curY;
                return false;
            }
        }
    }
}

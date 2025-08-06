using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Obeliskial_Content;
using Obeliskial_Essentials;
using UnityEngine;
using System.Runtime.CompilerServices;
using System.Collections;

namespace TraitMod
{
    [HarmonyPatch]
    internal class Traits
    {

        public static string[] myTraitList = { "shazixnarnightstalker", "shazixnarsilentfang", "shazixnarvenomousshade", "shazixnarvenomshadow" };

        public static void myDoTrait(string _trait, ref Trait __instance)
        {
            // get info you may need
            Enums.EventActivation _theEvent = Traverse.Create(__instance).Field("theEvent").GetValue<Enums.EventActivation>();
            Character _character = Traverse.Create(__instance).Field("character").GetValue<Character>();
            Character _target = Traverse.Create(__instance).Field("target").GetValue<Character>();
            int _auxInt = Traverse.Create(__instance).Field("auxInt").GetValue<int>();
            string _auxString = Traverse.Create(__instance).Field("auxString").GetValue<string>();
            CardData _castedCard = Traverse.Create(__instance).Field("castedCard").GetValue<CardData>();
            Traverse.Create(__instance).Field("character").SetValue(_character);
            Traverse.Create(__instance).Field("target").SetValue(_target);
            Traverse.Create(__instance).Field("theEvent").SetValue(_theEvent);
            Traverse.Create(__instance).Field("auxInt").SetValue(_auxInt);
            Traverse.Create(__instance).Field("auxString").SetValue(_auxString);
            Traverse.Create(__instance).Field("castedCard").SetValue(_castedCard);
            TraitData traitData = Globals.Instance.GetTraitData(_trait);
            List<CardData> cardDataList = new List<CardData>();
            List<string> heroHand = MatchManager.Instance.GetHeroHand(_character.HeroIndex);
            Hero[] teamHero = MatchManager.Instance.GetTeamHero();
            NPC[] teamNpc = MatchManager.Instance.GetTeamNPC();

            // activate traits
            if (_trait == "shazixnarnightstalker")
            {
                _character.SetAuraTrait(_character, "stealth", 2);

                int traitNum = 2;
                if (_character.HaveTrait("shazixnarvenomousshade")) traitNum += 2;
                if (_character.HaveTrait("shazixnarshadowexecution")) traitNum += 2;
                traitData.TimesPerTurn = traitNum;

                if (_character.HeroItem != null)
                {
                    _character.HeroItem.ScrollCombatText(Texts.Instance.GetText("traits_shazixnarnightstalker", ""), Enums.CombatScrollEffectType.Trait);
                    EffectsManager.Instance.PlayEffectAC("stealth", true, _character.HeroItem.CharImageT, false, 0f);
                }

                MatchManager.Instance.activatedTraits["shazixnarnightstalker"] = 0;
                
                _character.HeroItem?.ScrollCombatText(
                    Texts.Instance.GetText("traits_shazixnarnightstalker", "") +
                    Functions.TextChargesLeft(0, traitNum),  // 初始化为0
                    Enums.CombatScrollEffectType.Trait);

                MatchManager.Instance.SetTraitInfoText();
                return;
            }

            else if (_trait == "shazixnarsilentfang")
            {
                if (MatchManager.Instance != null && _castedCard != null)
                {
                    int traitNum = 6;
                    if (_character.HaveTrait("shazixnarvenomshadow"))
                    {
                        traitNum += 2;
                    }
                    traitData.TimesPerTurn = traitNum;

                    if (MatchManager.Instance.activatedTraits != null && MatchManager.Instance.activatedTraits.ContainsKey("shazixnarsilentfang") && MatchManager.Instance.activatedTraits["shazixnarsilentfang"] > traitNum)
                    {
                        return;
                    }
                    if (_castedCard.GetCardTypes().Contains(Enums.CardType.Small_Weapon) && _character.HeroData != null)
                    {
                        if (!MatchManager.Instance.activatedTraits.ContainsKey("shazixnarsilentfang"))
                        {
                            MatchManager.Instance.activatedTraits.Add("shazixnarsilentfang", 1);
                        }
                        else
                        {
                            Dictionary<string, int> activatedTraits = MatchManager.Instance.activatedTraits;
                            activatedTraits["shazixnarsilentfang"] = activatedTraits["shazixnarsilentfang"] + 1;
                        }
                        float probability = 0.95f;
                        if (_character.HaveTrait("shazixnarvenomshadow"))
                        {
                            probability = 0.9f;
                        }
                        string cardId = UnityEngine.Random.value < probability ? "silentfangnick" : "silentfangnickrare";
                        MatchManager.Instance.GenerateNewCard(1, cardId, true, Enums.CardPlace.Hand, null, null, -1, true, 0);
                        _character.HeroItem.ScrollCombatText(Texts.Instance.GetText("traits_shazixnarsilentfang", "") + Functions.TextChargesLeft(MatchManager.Instance.activatedTraits["shazixnarsilentfang"], traitNum), Enums.CombatScrollEffectType.Trait);
                        MatchManager.Instance.SetTraitInfoText();
                    }
                }
                return;
            }

            else if (_trait == "shazixnarvenomousshade")
            {
                if (MatchManager.Instance != null && _auxString == "stealth")
                {
                    NPC[] teamNPC = MatchManager.Instance.GetTeamNPC();
                    for (int i = 0; i < teamNPC.Length; i++)
                    {
                        if (teamNPC[i] != null && teamNPC[i].Alive)
                        {
                            teamNPC[i].SetAuraTrait(_character, "poison", 5);
                            _character.HeroItem.ScrollCombatText(Texts.Instance.GetText("traits_shazixnarvenomousshade", ""), Enums.CombatScrollEffectType.Trait);
                        }
                    }
                }
                return;
            }

            else if (_trait == "shazixnarvenomshadow")
            {
                if (_target != null && _target.Alive)
                {
                    _target.SetAuraTrait(_character, "poison", 1);
                    _character.HeroItem.ScrollCombatText(Texts.Instance.GetText("traits_shazixnarvenomousshade", ""), Enums.CombatScrollEffectType.Trait);
                }
                return;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Trait), "DoTrait")]
        public static bool DoTrait(Enums.EventActivation _theEvent, string _trait, Character _character, Character _target, int _auxInt, string _auxString, CardData _castedCard, ref Trait __instance)
        {
            if ((UnityEngine.Object)MatchManager.Instance == (UnityEngine.Object)null)
                return false;
            Traverse.Create(__instance).Field("character").SetValue(_character);
            Traverse.Create(__instance).Field("target").SetValue(_target);
            Traverse.Create(__instance).Field("theEvent").SetValue(_theEvent);
            Traverse.Create(__instance).Field("auxInt").SetValue(_auxInt);
            Traverse.Create(__instance).Field("auxString").SetValue(_auxString);
            Traverse.Create(__instance).Field("castedCard").SetValue(_castedCard);
            if (Content.medsCustomTraitsSource.Contains(_trait) && myTraitList.Contains(_trait))
            {
                myDoTrait(_trait, ref __instance);
                return false;
            }
            return true;
        }

        public static string TextChargesLeft(int currentCharges, int chargesTotal)
        {
            int cCharges = currentCharges;
            int cTotal = chargesTotal;
            return "<br><color=#FFF>" + cCharges.ToString() + "/" + cTotal.ToString() + "</color>";
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(AtOManager), "GlobalAuraCurseModificationByTraitsAndItems")]
        public static void GlobalAuraCurseModificationByTraitsAndItemsPostfix(ref AtOManager __instance, ref AuraCurseData __result, string _type, string _acId, Character _characterCaster, Character _characterTarget)
        {
            bool flag = false;
            bool flag2 = false;
            if (_characterCaster != null && _characterCaster.IsHero)
            {
                flag = _characterCaster.IsHero;
            }
            if (_characterTarget != null && _characterTarget.IsHero)
            {
                flag2 = true;
            }
            if (_acId == "poison")
            {
                if (_type == "set")
                {
                    if (!flag2)
                    {
                        if (__instance.TeamHaveTrait("shazixnarsilentfang"))
                        {
                            __result.DamageWhenConsumedPerCharge = 0.5f;
                            __result.ConsumedAtTurnBegin = true;
                        }
                        if (__instance.TeamHaveTrait("shazixnarvenomousshade"))
                        {
                            __result.ResistModifiedPercentagePerStack = -0.3f;
                            __result.ResistModified = Enums.DamageType.All;
                            __result.DamageWhenConsumedPerCharge = 0;
                            __result.ConsumedAtTurn = false;
                        }
                        if (__instance.TeamHaveTrait("shazixnarvenomshadow"))
                        {
                            __result.IncreasedDamageReceivedType = Enums.DamageType.All;
                            __result.IncreasedDirectDamageReceivedPerStack = 0.1f;
                        }
                    }
                }
                else if (_type == "consume")
                {
                    if (!flag)
                    {
                        if (__instance.TeamHaveTrait("shazixnarsilentfang"))
                        {
                            __result.DamageWhenConsumedPerCharge = 0.5f;
                            __result.ConsumedAtTurnBegin = true;
                        }
                        if (__instance.TeamHaveTrait("shazixnarvenomousshade"))
                        {
                            __result.ResistModifiedPercentagePerStack = -0.3f;
                            __result.ResistModified = Enums.DamageType.All;
                            __result.DamageWhenConsumedPerCharge = 0;
                            __result.ConsumedAtTurn = false;
                        }
                        if (__instance.TeamHaveTrait("shazixnarvenomshadow"))
                        {
                            __result.IncreasedDamageReceivedType = Enums.DamageType.All;
                            __result.IncreasedDirectDamageReceivedPerStack = 0.1f;
                        }
                    }
                }
            }
        }

        // 当前卡牌
        public static CardData currentCard;

        // 每回合使用次数（key 为 subClassName）
        public static Dictionary<string, int> stealthUsesThisTurn = new Dictionary<string, int>();

        // 每场战斗使用一次的近战攻击标记
        public static HashSet<string> shadowExecutionUsed = new HashSet<string>();

        // 1. CastCard Patch
        [HarmonyPatch(typeof(MatchManager), "CastCard")]
        [HarmonyPrefix]
        public static void CastCard_Prefix(ref CardItem theCardItem, ref CardData _card)
        {
            CardData _cardActive = theCardItem != null ? theCardItem.CardData : _card;

            // 复制源代码中的过滤逻辑，确保 currentCard 是主动使用的牌
            if (!_cardActive.AutoplayDraw &&
                !_cardActive.AutoplayEndTurn &&
                (_cardActive.CardClass != Enums.CardClass.Special || (_cardActive.CardClass == Enums.CardClass.Special && _cardActive.Playable)))
            {
                currentCard = _cardActive;
            }
        }

        // 2. HealAuraCurse Patch
        [HarmonyPatch(typeof(Character), "HealAuraCurse")]
        [HarmonyPrefix]
        public static bool HealAuraCurse_Prefix(AuraCurseData AC, Character __instance)
        {
            if (AC != null && AC.Id == "stealth" && __instance is Hero hero && currentCard != null && __instance.HaveTrait("shazixnarnightstalker"))
            {
                HeroItem heroItem = Traverse.Create(__instance).Field("heroItem").GetValue<HeroItem>();
                string subClassName = Traverse.Create(__instance).Field("subclassName").GetValue<string>();
                bool hasShadowExecution = __instance.HaveTrait("shazixnarshadowexecution");
                bool isMelee = currentCard.HasCardType(Enums.CardType.Melee_Attack);
                bool isSkillOrEnchant = currentCard.HasCardType(Enums.CardType.Skill) || currentCard.HasCardType(Enums.CardType.Enchantment);

                // 近战攻击一次性豁免
                if (hasShadowExecution && isMelee && !shadowExecutionUsed.Contains(subClassName))
                {
                    shadowExecutionUsed.Add(subClassName);
                    if (!MatchManager.Instance.activatedTraits.ContainsKey("shazixnarshadowexecution"))
                    {
                        MatchManager.Instance.activatedTraits["shazixnarshadowexecution"] = 1;
                    }
                    else
                    {
                        MatchManager.Instance.activatedTraits["shazixnarshadowexecution"]++;
                    }
                    heroItem.ScrollCombatText(Texts.Instance.GetText("traits_shazixnarshadowexecution", "") +
                    Functions.TextChargesLeft(MatchManager.Instance.ItemExecutedInThisCombat(subClassName, "shazixnarshadowexecution"), 1),
                    Enums.CombatScrollEffectType.Trait);
                    Debug.Log($"[潜行保留] 触发 shazixnarshadowexecution");
                    MatchManager.Instance.SetTraitInfoText();
                    return false;
                }

                // 每回合次数豁免
                if (__instance.HaveTrait("shazixnarnightstalker") && isSkillOrEnchant)
                {
                    int limit = 2;
                    if (__instance.HaveTrait("shazixnarvenomousshade")) limit += 2;
                    if (__instance.HaveTrait("shazixnarshadowexecution")) limit += 2;
                    Globals.Instance.GetTraitData("shazixnarnightstalker").TimesPerTurn = limit;

                    if (!stealthUsesThisTurn.ContainsKey(subClassName)) stealthUsesThisTurn[subClassName] = 0;

                    if (stealthUsesThisTurn[subClassName] < limit)
                    {
                        stealthUsesThisTurn[subClassName]++;
                        if (!MatchManager.Instance.activatedTraits.ContainsKey("shazixnarnightstalker"))
                        {
                            MatchManager.Instance.activatedTraits["shazixnarnightstalker"] = 1;
                        }
                        else
                        {
                            MatchManager.Instance.activatedTraits["shazixnarnightstalker"]++;
                        }

                        heroItem?.ScrollCombatText(
                            Texts.Instance.GetText("traits_shazixnarnightstalker", "") +
                            Functions.TextChargesLeft(MatchManager.Instance.activatedTraits["shazixnarnightstalker"], limit),
                            Enums.CombatScrollEffectType.Trait);
                        MatchManager.Instance.SetTraitInfoText();

                        Debug.Log($"[潜行保留] 使用次数 {stealthUsesThisTurn[subClassName]}/{limit}");
                        return false;
                    }
                }
            }

            return true; // 不阻止正常清除
        }

        [HarmonyPatch(typeof(Character), "BeginTurn")]
        [HarmonyPostfix]
        public static void Character_BeginTurn_Postfix(Character __instance)
        {
            if (__instance is Hero hero && hero.HeroData != null && __instance.HaveTrait("shazixnarnightstalker"))
            {
                string subClassName = Traverse.Create(__instance).Field("subclassName").GetValue<string>();
                stealthUsesThisTurn[subClassName] = 0;
                Debug.Log($"[潜行次数已重置] {subClassName}");
            }
        }
    }
}

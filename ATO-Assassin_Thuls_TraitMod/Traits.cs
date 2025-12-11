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

        private static readonly Traits _instance = new Traits();

        public static void myDoTrait(
            string trait,
            Enums.EventActivation evt,
            Character character,
            Character target,
            int auxInt,
            string auxString,
            CardData castedCard)
        {
            switch(trait)
            {
                case "shazixnarnightstalker":
                    _instance.shazixnarnightstalker(evt, character, target, auxInt, auxString, castedCard, trait);
                    break;
                    
                case "shazixnarsilentfang":
                    _instance.shazixnarsilentfang(evt, character, target, auxInt, auxString, castedCard, trait);
                    break;

                case "shazixnarvenomousshade":
                    _instance.shazixnarvenomousshade(evt, character, target, auxInt, auxString, castedCard, trait);
                    break;

                case "shazixnarvenomshadow":
                    _instance.shazixnarvenomshadow(evt, character, target, auxInt, auxString, castedCard, trait);
                    break;
            }
        }

            // activate traits
        public void shazixnarnightstalker(
            Enums.EventActivation evt,
            Character character, Character target,
            int auxInt, string auxString,
            CardData castedCard, string trait)
        {
            // 0. 防御：如果 character 是 null，直接跳过
            if (character == null) return;

            // 1. 只在 BeginCombat 时触发初始化
            if (evt == Enums.EventActivation.BeginCombat)
            {
                // 设置2层潜行
                character.SetAuraTrait(character, "stealth", 2);

                // 计算次数
                int traitNum = 2;
                if (character.HaveTrait("shazixnarvenomousshade")) traitNum += 2;
                if (character.HaveTrait("shazixnarshadowexecution")) traitNum += 2;

                // 初始化 activatedTraits
                MatchManager.Instance.activatedTraits[trait] = 0;

                // 显示提示
                if (character.HeroItem != null)
                {
                    character.HeroItem.ScrollCombatText(
                        Texts.Instance.GetText("traits_shazixnarnightstalker", "")
                        + Functions.TextChargesLeft(0, traitNum),
                        Enums.CombatScrollEffectType.Trait
                    );

                    EffectsManager.Instance.PlayEffectAC("stealth", true, character.HeroItem.CharImageT, false, 0f);
                }

                MatchManager.Instance.SetTraitInfoText();
                return;
            }

            // 2. 在 CastCard 时检测是否扣次数（或者是保留潜行）
            if (evt == Enums.EventActivation.CastCard && castedCard != null)
            {
                // 判断是否技能/附魔
                bool isSkill = castedCard.HasCardType(Enums.CardType.Skill);
                bool isEnchant = castedCard.HasCardType(Enums.CardType.Enchantment);

                if (isSkill || isEnchant)
                {
                    // 注意：不要在这里再初始化 ActivatedTraits
                    // 因为 BeginCombat 里已经做过

                    int used = MatchManager.Instance.activatedTraits[trait];
                    int max = Globals.Instance.GetTraitData(trait).TimesPerTurn;

                    if (used < max)
                    {
                        // 触发效果：不消耗潜行 → 就不调用 HealAuraCurse
                        MatchManager.Instance.activatedTraits[trait] = used + 1;

                        // 显示更新
                        character.HeroItem?.ScrollCombatText(
                            Texts.Instance.GetText("traits_shazixnarnightstalker", "")
                            + Functions.TextChargesLeft(used + 1, max),
                            Enums.CombatScrollEffectType.Trait
                        );

                        MatchManager.Instance.SetTraitInfoText();
                        return;
                    }
                }
            }
        }

        public void shazixnarsilentfang(
            Enums.EventActivation evt,
            Character character, Character target,
            int auxInt, string auxString,
            CardData card, string trait)
        {
            if (character == null || card == null) return;

            // 只在使用卡牌时触发
            if (evt != Enums.EventActivation.CastCard) return;

            // 必须是小型武器
            bool isSmallWeapon = card.GetCardTypes().Contains(Enums.CardType.Small_Weapon);
            if (!isSmallWeapon) return;

            // 必须是英雄
            if (character.HeroData == null) return;

            TraitData data = Globals.Instance.GetTraitData(trait);
            int max = data.TimesPerTurn;

            // 如果有 venomshadow，次数 +2
            if (character.HaveTrait("shazixnarvenomshadow"))
                max += 2;

            // 读取已用次数
            int used = 0;
            if (MatchManager.Instance.activatedTraits.ContainsKey(trait))
                used = MatchManager.Instance.activatedTraits[trait];

            // 超过次数则不触发
            if (used >= max) return;

            // 更新次数
            MatchManager.Instance.activatedTraits[trait] = used + 1;

            // 判定概率
            float probability = character.HaveTrait("shazixnarvenomshadow") ? 0.90f : 0.95f;
            string cardId = UnityEngine.Random.value < probability ? "silentfangnick" : "silentfangnickrare";

            // 生成卡牌
            MatchManager.Instance.GenerateNewCard(1, cardId, true, Enums.CardPlace.Hand, null, null, -1, true, 0);

            // combat text
            character.HeroItem?.ScrollCombatText(
                Texts.Instance.GetText("traits_shazixnarsilentfang", "")
                + Functions.TextChargesLeft(used + 1, max),
                Enums.CombatScrollEffectType.Trait);

            MatchManager.Instance.SetTraitInfoText();
        }

        public void shazixnarvenomousshade(
            Enums.EventActivation evt,
            Character character, Character target,
            int auxInt, string auxString,
            CardData card, string trait)
        {
            if (character == null) return;

            // 游戏内部判断获得 stealth 时，auxString = "stealth"
            if (auxString != "stealth") return;

            NPC[] npcs = MatchManager.Instance.GetTeamNPC();
            foreach (var npc in npcs)
            {
                if (npc != null && npc.Alive)
                {
                    npc.SetAuraTrait(character, "poison", 5);
                }
            }

            character.HeroItem?.ScrollCombatText(
                Texts.Instance.GetText("traits_shazixnarvenomousshade", ""),
                Enums.CombatScrollEffectType.Trait);
        }

        public void shazixnarvenomshadow(
            Enums.EventActivation evt,
            Character character, Character target,
            int auxInt, string auxString,
            CardData card, string trait)
        {
            if (character == null || target == null || !target.Alive) return;

            // 通常是 Hitted 事件，你可按需限制 evt
            target.SetAuraTrait(character, "poison", 1);

            character.HeroItem?.ScrollCombatText(
                Texts.Instance.GetText("traits_shazixnarvenomousshadow", ""), 
                Enums.CombatScrollEffectType.Trait);
        }

        [HarmonyPatch(typeof(Trait), "DoTrait")]
        public static class Trait_DoTrait_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(
                Enums.EventActivation __0,   // theEvent
                string __1,                  // trait id
                Character __2,               // character
                Character __3,               // target
                int __4,                     // auxInt
                string __5,                  // auxString
                CardData __6,                // castedCard
                Trait __instance)
            {
                string trait = __1;

                // 如果是自定义 trait，就直接调用我们的逻辑
                if (myTraitList.Contains(trait))
                {
                    myDoTrait(
                        trait,
                        __0,        // event
                        __2,        // character
                        __3,        // target
                        __4,        // auxInt
                        __5,        // auxString
                        __6         // castedCard
                    );

                    // 返回 false = 阻止原版 DoTrait 执行
                    return false;
                }

                // 否则走原版逻辑
                return true;
            }
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

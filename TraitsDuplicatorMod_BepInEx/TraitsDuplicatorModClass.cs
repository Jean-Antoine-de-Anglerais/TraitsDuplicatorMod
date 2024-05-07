using BepInEx;
using HarmonyLib;
using ReflectionUtility;
using System.Collections.Generic;
using static ConstantNamespace.ConstantClass;

namespace TraitsDuplicatorMod_BepInEx
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class TraitsDuplicatorModClass : BaseUnityPlugin
    {
        public static Harmony harmony = new Harmony(pluginName);
        private bool _initialized = false;

        public void Awake()
        {
            Logger.LogMessage("ХООООООООЙ");
        }

        public void Start()
        {
            if (global::Config.gameLoaded)
            {
                Logger.LogMessage("Пропатчено");
            }
        }

        // Метод, запускающийся каждый кадр (в моём случае он зависим от загрузки игры)
        public void Update()
        {
            if (global::Config.gameLoaded)
            {
            }

            if (global::Config.gameLoaded && !_initialized)
            {
                foreach (var trait in AssetManager.traits.list)
                {
                    trait.can_be_given = true;
                    trait.can_be_removed = true;
                }

                foreach (var actorAsset in AssetManager.actor_library.list)
                {
                    actorAsset.can_edit_traits = true;
                    actorAsset.canBeInspected = true;
                }

                Localizer.Localization("en", "trait_editor_remove", "Trait Editor now removes traits from a creature");
                Localizer.Localization("ru", "trait_editor_remove", "Теперь редактор черт удаляет черты у существа");

                Localizer.Localization("en", "trait_editor_add", "Trait Editor now adds traits to the creature");
                Localizer.Localization("ru", "trait_editor_add", "Теперь редактор черт добавляет черты существу");

                harmony.Patch(AccessTools.Method(typeof(TraitsWindow), "useTraitOnActor"),
                prefix: new HarmonyMethod(AccessTools.Method(typeof(Patches), "useTraitOnActor_Prefix")));

                harmony.Patch(AccessTools.Method(typeof(RaceClick), "click"),
                postfix: new HarmonyMethod(AccessTools.Method(typeof(Patches), "click_Postfix")));

                _initialized = true;
            }
        }

    }

    public class Patches
    {
        public static bool useTraitOnActor_Prefix(TraitsWindow __instance, ActorTrait pTrait)
        {
            Actor currentActor = (Actor)Reflection.CallMethod(__instance, "getCurrentActor");
            if (!StaticStuff.isTraitRemoverOn)
            {
                Reflection.CallMethod(currentActor, "updateStats");
                StaticStuff.addTrait(currentActor, pTrait.id, false);
            }

            else
            {
                if (currentActor.hasTrait(pTrait.id))
                {
                    if (pTrait.can_be_removed)
                    {
                        currentActor.removeTrait(pTrait.id);
                    }
                }
            }

            Reflection.CallMethod(currentActor, "updateStats");
            Reflection.CallMethod(__instance, "checkUnitTraits");
            return false;
        }

        public static void click_Postfix()
        {
            StaticStuff.isTraitRemoverOn = !StaticStuff.isTraitRemoverOn;

            if (StaticStuff.isTraitRemoverOn)
            {
                WorldTip.showNow("trait_editor_remove", true, "top", 1f);
            }
            else if (!StaticStuff.isTraitRemoverOn)
            {
                WorldTip.showNow("trait_editor_add", true, "top", 1f);
            }
        }
    }

    public static class StaticStuff
    {
        public static bool addTrait(ActorBase instance, string pTrait, bool pRemoveOpposites = false)
        {
            var data = (ActorData)Reflection.GetField(instance.GetType(), instance, "data");
            //if (instance.hasTrait(pTrait))
            //{
            //return false;
            //}
            if (AssetManager.traits.get(pTrait) == null)
            {
                return false;
            }
            //if (pRemoveOpposites)
            //{
            //instance.removeOppositeTraits(pTrait);
            //}
            //if (instance.hasOppositeTrait(pTrait))
            //{
            //return false;
            //}

            data.traits.Add(pTrait);
            instance.setStatsDirty();
            return true;
        }

        public static bool isTraitRemoverOn = false;
    }


    public static class Localizer
    {
        public static void Localization(string planguage, string id, string name)
        {
            string language = Reflection.GetField(LocalizedTextManager.instance.GetType(), LocalizedTextManager.instance, "language") as string;
            string templanguage;

            templanguage = language;

            if (templanguage != "ru" && templanguage != "en")
            {
                templanguage = "en";
            }

            if (planguage == templanguage)
            {
                Dictionary<string, string> localizedText = Reflection.GetField(LocalizedTextManager.instance.GetType(), LocalizedTextManager.instance, "localizedText") as Dictionary<string, string>;
                if (!localizedText.ContainsKey(id))
                {
                    localizedText.Add(id, name);
                }
                else if (localizedText.ContainsKey(id))
                {
                    localizedText.Remove(id);
                    localizedText.Add(id, name);
                }
            }
        }
    }
}



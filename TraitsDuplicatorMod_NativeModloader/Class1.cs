using DG.Tweening;
using HarmonyLib;
using ReflectionUtility;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace TraitsDuplicatorMod_NativeModloader
{
    public class WorldBoxMod : MonoBehaviour
    {
        public void Awake()
        {
            Debug.Log("Traits Duplicator Mod loading...");
            string path = Path.Combine(Application.streamingAssetsPath, "Mods");
            path = Path.Combine(path, "stuffthatjeansmodsuse");
            if (!Directory.Exists(path))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(path);
                directoryInfo.Create();
                directoryInfo.Attributes |= FileAttributes.Hidden;
            }

            string text2 = Path.Combine(path, "Mono.Cecil.dll");
            string text3 = Path.Combine(path, "0Harmony.dll");
            string text4 = Path.Combine(path, "MonoMod.RuntimeDetour.dll");
            string text5 = Path.Combine(path, "MonoMod.Utils.dll");

            File.WriteAllBytes(text3, TraitsDuplicatorMod_NativeModloader.Properties.Resources._0Harmony);
            File.WriteAllBytes(text2, TraitsDuplicatorMod_NativeModloader.Properties.Resources.Mono_Cecil);
            File.WriteAllBytes(text4, TraitsDuplicatorMod_NativeModloader.Properties.Resources.MonoMod_Utils);
            File.WriteAllBytes(text5, TraitsDuplicatorMod_NativeModloader.Properties.Resources.MonoMod_RuntimeDetour);

            Assembly.LoadFrom(text3);
            Assembly.LoadFrom(text2);
            Assembly.LoadFrom(text4);
            Assembly.LoadFrom(text5);

            Debug.Log("Traits Duplicator Mod loaded!");
            GameObject gameObject = new GameObject("TraitsDuplicatorMod_NativeModloader");
            UnityEngine.Object.DontDestroyOnLoad(gameObject);
            gameObject.AddComponent<Patches>();
        }
    }

    internal class Patches : MonoBehaviour
    {
        public static Harmony harmony = new Harmony("TraitsDuplicatorMod_NativeModloader");
        private bool _initialized = false;

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
                Localizer.Localization("cz", "trait_editor_remove", "已切换为移除特质模式，点击特质可删除");
                Localizer.Localization("ch", "trait_editor_remove", "已切換為移除特質模式，點擊特質可刪除");

                Localizer.Localization("en", "trait_editor_add", "Trait Editor now adds traits to the creature");
                Localizer.Localization("ru", "trait_editor_add", "Теперь редактор черт добавляет черты существу");
                Localizer.Localization("cz", "trait_editor_add", "已切换为添加特质模式，点击特质可叠加");
                Localizer.Localization("ch", "trait_editor_add", "已切換為添加特質模式，點擊特質可疊加");

                harmony.Patch(AccessTools.Method(typeof(TraitsWindow), "useTraitOnActor"),
                prefix: new HarmonyMethod(AccessTools.Method(typeof(Patches), "useTraitOnActor_Prefix")));

                harmony.Patch(AccessTools.Method(typeof(UiCreature), "click"),
                prefix: new HarmonyMethod(AccessTools.Method(typeof(Patches), "click_Prefix")));

                _initialized = true;
            }
        }

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

        public static bool click_Prefix(UiCreature __instance, ref bool ___dropped, ref Tweener ___tweener_scale, ref float ___initScale, ref Tweener ___tweener_rotation)
        {
            if (Config.selectedUnit != null && __instance.doScale)
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

                if (___dropped)
                {
                    return false;
                }
                if (__instance.doPlayPunch)
                {
                    MusicBox.playSound("event:/SFX/OTHER/Punch", -1f, -1f, false, false);
                }
                if (__instance.doSfx != "none" && !string.IsNullOrEmpty(__instance.doSfx) && __instance.doSfx.Contains("event:"))
                {
                    MusicBox.playSound(__instance.doSfx, -1f, -1f, false, false);
                }
                if (__instance.doScale)
                {
                    if (___tweener_scale != null && ___tweener_scale.active)
                    {
                        ___tweener_scale.Kill(false);
                    }
                    float num = ___initScale * 1.2f;
                    __instance.transform.localScale = new Vector3(num, num, num);
                    ___tweener_scale = __instance.transform.DOScale(new Vector3(___initScale, ___initScale, ___initScale), 0.3f).SetEase(Ease.OutBack);
                }
                return false;
            }
            return true;
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

            if (templanguage != "ru" && templanguage != "en" && templanguage != "cz" && templanguage != "ch")
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

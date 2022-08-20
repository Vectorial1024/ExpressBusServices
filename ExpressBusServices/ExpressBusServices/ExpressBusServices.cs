using CitiesHarmony.API;
using ColossalFramework.UI;
using ICities;
using UnityEngine;

namespace ExpressBusServices
{
    public class ExpressBusServices : LoadingExtensionBase, IUserMod
    {
        public virtual string Name
        {
            get
            {
                return "Express Bus Services";
            }
        }

        public virtual string Description
        {
            get
            {
                return "Unlock the peak efficiency of buses; now also improves trams!";
            }
        }

        /// <summary>
        /// Executed whenever a level completes its loading process.
        /// This mod the activates and patches the game using Hramony library.
        /// </summary>
        /// <param name="mode">The loading mode.</param>
        public override void OnLevelLoaded(LoadMode mode)
        {
            /*
             * This function can still be called when loading up the asset editor,
             * so we have to check where we are right now.
             */

            switch (mode)
            {
                case LoadMode.LoadGame:
                case LoadMode.NewGame:
                case LoadMode.LoadScenario:
                case LoadMode.NewGameFromScenario:
                    break;

                default:
                    return;
            }

            UnifyHarmonyVersions();
            PatchController.Activate();
        }

        /// <summary>
        /// Executed whenever a map is being unloaded.
        /// This mod then undoes the changes using the Harmony library.
        /// </summary>
        public override void OnLevelUnloading()
        {
            UnifyHarmonyVersions();
            PatchController.Deactivate();
        }

        private void UnifyHarmonyVersions()
        {
            if (HarmonyHelper.IsHarmonyInstalled)
            {
                // this code will redirect our Harmony 2.x version to the authoritative version stipulated by CitiesHarmony
                // I will make it such that the game will throw hard error if Harmony is not found,
                // as per my usual software deployment style
                // the user will have to subscribe to Harmony by themselves. I am not their parent anyways.
                // so this block will have to be empty.
            }
        }

        // It seems they will dynamically find whether a certain method that matches some criteria
        // exists, and then apply UI settings to it.
        // This is kinda like an in-house Harmony Lib except it targets some very specific areas.
        public void OnSettingsUI(UIHelperBase helper)
        {
            UIHelperBase group = helper.AddGroup("Express Bus Services: Settings");
            ModSettingController.Touch();
            int selectedIndex_ExpressBus = (int)EBSModConfig.CurrentExpressBusMode;
            bool usesSelfBalancing = EBSModConfig.UseServiceSelfBalancing;
            bool selfBalCanTargetMiddle = EBSModConfig.ServiceSelfBalancingCanDoMiddleStop;
            bool canUseMinibusMode = EBSModConfig.CanUseMinibusMode;
            var dropdown = group.AddDropdown("EBS Unbunching Mode",
                new string[] {
                    "Prudential (Legacy)",
                    "Aggressive",
                    "Experimental" },
                0,
                (index) => {
                    EBSModConfig.CurrentExpressBusMode = (EBSModConfig.ExpressMode)index;
                    Debug.Log($"Express Bus Services: (express bus) received index {index}");
                    ModSettingController.WriteSettings();
                });
            var toggleSelfBalancing = group.AddCheckbox("Enable Service Self-Balancing", true, (newValue) =>
            {
                EBSModConfig.UseServiceSelfBalancing = newValue;
                Debug.Log($"Express Bus Services: (self balancing) received value {newValue}");
                ModSettingController.WriteSettings();
            });
            var toggleSelfBalCanTargetMid = group.AddCheckbox("Service Self-Balancing can target most-pax middle-of-line stops", true, (newValue) =>
            {
                EBSModConfig.ServiceSelfBalancingCanDoMiddleStop = newValue;
                Debug.Log($"Express Bus Services: (self balancing middle target) received value {newValue}");
                ModSettingController.WriteSettings();
            });
            var toggleCanUseMinibusMode = group.AddCheckbox("Enable \"minibus behavior\" for buses", true, (newValue) =>
            {
                EBSModConfig.ServiceSelfBalancingCanDoMiddleStop = newValue;
                Debug.Log($"Express Bus Services: (minibus mode) received value {newValue}");
                ModSettingController.WriteSettings();
            });
            UIDropDown properDropdownObject = dropdown as UIDropDown;
            if (properDropdownObject != null)
            {
                properDropdownObject.selectedIndex = selectedIndex_ExpressBus;
            }
            UICheckBox toggleObjectSelfBalancing = toggleSelfBalancing as UICheckBox;
            if (toggleObjectSelfBalancing != null)
            {
                toggleObjectSelfBalancing.isChecked = usesSelfBalancing;
            }
            UICheckBox toggleObjectSelfBalCanTargetMid = toggleSelfBalCanTargetMid as UICheckBox;
            if (toggleObjectSelfBalCanTargetMid != null)
            {
                toggleObjectSelfBalCanTargetMid.isChecked = selfBalCanTargetMiddle;
            }
            UICheckBox toggleObjectCanUseMinibusMode = toggleCanUseMinibusMode as UICheckBox;
            if (toggleObjectCanUseMinibusMode != null)
            {
                toggleObjectCanUseMinibusMode.isChecked = canUseMinibusMode;
            }

            UIHelperBase expressTramGroup = helper.AddGroup("Express Tram Services: Settings");
            int selectedIndex_ExpressTram = (int)EBSModConfig.CurrentExpressTramMode;
            var dropdownExpressTramMode = expressTramGroup.AddDropdown("ETS Unbunching Mode",
                new string[] {
                    "Disabled",
                    "Light Rail Mode",
                    "True Tram Mode" },
                0,
                (index) => {
                    EBSModConfig.CurrentExpressTramMode = (EBSModConfig.ExpressTramMode)index;
                    Debug.Log($"Express Bus Services: (express tram) received index {index}");
                    ModSettingController.WriteSettings();
                });
            UIDropDown dropdownExpressTramModeObject = dropdownExpressTramMode as UIDropDown;
            if (dropdownExpressTramModeObject != null)
            {
                dropdownExpressTramModeObject.selectedIndex = selectedIndex_ExpressTram;
            }
        }
    }
}

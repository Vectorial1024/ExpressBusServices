using ICities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
                return "Unlock the peak efficiency of buses.";
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

            PatchController.Activate();
        }

        /// <summary>
        /// Executed whenever a map is being unloaded.
        /// This mod then undoes the changes using the Harmony library.
        /// </summary>
        public override void OnLevelUnloading()
        {
            PatchController.Deactivate();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using Microsoft.Xna.Framework;
using StardewValley.Menus;
using StardewValley.Locations;
using xTile;

namespace SkullCavernElevator
{
    public class ModEntry : Mod
    {

        private IModHelper helper;
        private ModConfig config;

        /*********
        ** Public methods
        *********/
        /// <summary>Initialise the mod.</summary>
        /// <param name="helper">Provides methods for interacting with the mod directory, such as read/writing a config file or custom JSON files.</param>
        public override void Entry(IModHelper helper)
        {
            config = helper.ReadConfig<ModConfig>();
            this.helper = helper;
            ControlEvents.MouseChanged += ControlEvents_MouseChanged;
        }

        
        private void ControlEvents_MouseChanged(object sender, EventArgsMouseStateChanged e)
        {
            //Check for MapPage to be open
            if (!(Game1.activeClickableMenu is GameMenu) || (Game1.activeClickableMenu as GameMenu).currentTab != 3)
            {
                return;
            }
            if (e.NewState.LeftButton != Microsoft.Xna.Framework.Input.ButtonState.Pressed)
            {
                return;
            }
            if (!Game1.oldKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift))
            {
                return;
            }
            Game1.enterMine(true, 200, null);

            MineShaft ms = Game1.currentLocation as MineShaft;
            Map map = map = Game1.content.CreateTemporary().Load<Map>("Maps\\Mines\\" + (object)1);
            map.TileSheets[0].ImageSource = "Maps\\Mines\\mine";
            map.LoadTileSheets(Game1.mapDisplayDevice);
            
            Vector2 ladder = helper.Reflection.GetPrivateField<Vector2>(ms, "tileBeneathLadder").GetValue();
            ms.setMapTileIndex((int) ladder.X +1, (int) ladder.Y +1, 80, "Buildings");
            helper.Reflection.GetPrivateMethod(Game1.currentLocation, "prepareElevator").Invoke();
            Point tile = Utility.findTile(Game1.currentLocation, 80, "Buildings");
            this.Monitor.Log("x "+tile.X + " y "+tile.Y, LogLevel.Info);
            //(Game1.currentLocation as MineShaft).pr
        }
    }
}

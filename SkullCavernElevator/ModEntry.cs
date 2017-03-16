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
using Microsoft.Xna.Framework.Graphics;
using SkullCavernElevator.SkullCavernElevator;

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
            this.helper = helper;
            MineEvents.MineLevelChanged += MineEvents_MineLevelChanged;
            MenuEvents.MenuChanged += MenuChanged;
            SaveEvents.AfterLoad += SetUpSkullCave;
            this.config = helper.ReadConfig<ModConfig>();
        }

        private void SetUpSkullCave(object sender, EventArgs e)
        {
            if (!Game1.hasLoadedGame || Game1.CurrentEvent != null)
                return;
            GameEvents.UpdateTick -= SetUpSkullCave;
            GameLocation skullCave = Game1.getLocationFromName("SkullCave");
            GameLocation mine = Game1.getLocationFromName("Mine");
            xTile.Tiles.TileSheet parent = mine.map.GetTileSheet("untitled tile sheet");

            skullCave.map.AddTileSheet(new xTile.Tiles.TileSheet("z_path_objects_custom_sheet", skullCave.map, parent.ImageSource, parent.SheetSize, parent.TileSize));
            skullCave.map.DisposeTileSheets(Game1.mapDisplayDevice);
            skullCave.map.LoadTileSheets(Game1.mapDisplayDevice);

            skullCave.setMapTileIndex(4, 1 + 2, 112, "Buildings", 2);
            skullCave.setMapTileIndex(4, 1 + 1, 96, "Front", 2);
            skullCave.setMapTileIndex(4, 1, 80, "Front", 2);
            skullCave.setMapTile(4, 1 + 2, 112, "Buildings", "MineElevator", 2);
            skullCave.setMapTile(4, 1 + 1, 96, "Front", "MineElevator", 2);
            skullCave.setMapTile(4, 1, 80, "Front", "MineElevator", 2);
        }

        private void MenuChanged(object sender, EventArgsClickableMenuChanged e)
        {
            if (!(e.NewMenu is MineElevatorMenu) || Game1.currentLocation.name == "Mine" || e.NewMenu is MyElevatorMenu || e.NewMenu is MyElevatorMenuWithScrollbar)
            {
                return;
            }
            if (Game1.currentLocation is MineShaft && (Game1.currentLocation as MineShaft)?.mineLevel <= 120)
            {
                return;
            }
            /*if (Game1.player.deepestMineLevel > 120+121*5)
            {
                Game1.activeClickableMenu = new MyElevatorMenuWithScrollbar(config.elevatorStep);
            } */
            Game1.activeClickableMenu = new MyElevatorMenu(config.elevatorStep);
        }

        private void MineEvents_MineLevelChanged(object sender, EventArgsMineLevelChanged e)
        {
            if (!Game1.hasLoadedGame || Game1.mine == null || Game1.mine.mineLevel % config.elevatorStep != 0 || Game1.mine.mineLevel <= 120 || !(Game1.currentLocation is MineShaft))
            {
                return;
            }
            //Game1.player.deepestMineLevel = 1560;
            MineShaft ms = Game1.currentLocation as MineShaft;

            GameLocation mine = Game1.getLocationFromName("Mine");
            xTile.Tiles.TileSheet parent = mine.map.GetTileSheet("untitled tile sheet");

            ms.map.AddTileSheet(new xTile.Tiles.TileSheet("z_path_objects_custom_sheet", ms.map, parent.ImageSource, parent.SheetSize, parent.TileSize));
            ms.map.DisposeTileSheets(Game1.mapDisplayDevice);
            ms.map.LoadTileSheets(Game1.mapDisplayDevice);


            Vector2 ladder = findLadder(ms);
            int elevatorX = (int)ladder.X + 1;
            int elevatorY = (int)ladder.Y - 3;
            helper.Reflection.GetPrivateField<Vector2>(ms, "tileBeneathElevator").SetValue(new Vector2(elevatorX, elevatorY + 2));

            ms.setMapTileIndex(elevatorX, elevatorY + 2, 112, "Buildings", 1);
            ms.setMapTileIndex(elevatorX, elevatorY + 1, 96, "Front", 1);
            ms.setMapTileIndex(elevatorX, elevatorY, 80, "Front", 1);
            ms.setMapTile(elevatorX, elevatorY, 80, "Front", "MineElevator", 1);
            ms.setMapTile(elevatorX, elevatorY + 1, 96, "Front", "MineElevator", 1);
            ms.setMapTile(elevatorX, elevatorY + 2, 112, "Buildings", "MineElevator", 1);


            helper.Reflection.GetPrivateMethod(ms, "prepareElevator").Invoke();
            Point tile = Utility.findTile(ms, 80, "Buildings");
            this.Monitor.Log("x " + tile.X + " y " + tile.Y, LogLevel.Info);
        }

        private Vector2 findLadder(MineShaft ms)
        {
            Map map = ms.map;
            for (int yTile = 0; yTile < map.GetLayer("Buildings").LayerHeight; ++yTile)
            {
                for (int xTile = 0; xTile < map.GetLayer("Buildings").LayerWidth; ++xTile)
                {
                    if (map.GetLayer("Buildings").Tiles[xTile, yTile] != null)
                    {
                        int tileIndex = map.GetLayer("Buildings").Tiles[xTile, yTile].TileIndex;
                        if (tileIndex == 115)
                        {
                            return new Vector2((float)xTile, (float)(yTile + 1));
                        }
                    }
                }
            }
            return helper.Reflection.GetPrivateValue<Vector2>(ms, "tileBeneathLadder");
        }

    }
}

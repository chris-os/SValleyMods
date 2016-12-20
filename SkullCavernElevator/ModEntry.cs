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
            MineEvents.MineLevelChanged += MineEvents_MineLevelChanged;
            MenuEvents.MenuChanged += MenuChanged;
            GameEvents.UpdateTick += SetUpSkullCave;
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
            skullCave.setMapTile(4, 1, 80, "Front", "MineElevator", 2);
            skullCave.setMapTile(4, 1 + 1, 96, "Front", "MineElevator", 2);
            skullCave.setMapTile(4, 1 + 2, 112, "Buildings", "MineElevator", 2);
        }

        private void MenuChanged(object sender, EventArgsClickableMenuChanged e)
        {
            if (! (e.NewMenu is MineElevatorMenu) || Game1.currentLocation.name == "Mine")
            {
                return;
            }
            if (Game1.currentLocation is MineShaft && (Game1.currentLocation as MineShaft)?.mineLevel <= 120)
            {
                return;
            }
            Game1.activeClickableMenu = new MyElevatorMenu();
        }

        private void MineEvents_MineLevelChanged(object sender, EventArgsMineLevelChanged e)
        {
            if (!Game1.hasLoadedGame || Game1.mine == null || Game1.mine.mineLevel % 5 != 0 || Game1.mine.mineLevel <= 120 || !(Game1.currentLocation is MineShaft))
            {
                return;
            }
            MineShaft ms = Game1.currentLocation as MineShaft;

            GameLocation mine = Game1.getLocationFromName("Mine");
            xTile.Tiles.TileSheet parent = mine.map.GetTileSheet("untitled tile sheet");

            ms.map.AddTileSheet(new xTile.Tiles.TileSheet("z_path_objects_custom_sheet", ms.map, parent.ImageSource, parent.SheetSize, parent.TileSize));
            ms.map.DisposeTileSheets(Game1.mapDisplayDevice);
            ms.map.LoadTileSheets(Game1.mapDisplayDevice);


            Vector2 ladder = helper.Reflection.GetPrivateValue<Vector2>(ms, "tileBeneathLadder");
            int elevatorX = (int)ladder.X + 1;
            int elevatorY = (int)ladder.Y -3;
            helper.Reflection.GetPrivateField<Vector2>(ms, "tileBeneathElevator").SetValue(new Vector2(elevatorX, elevatorY + 2));

            ms.setMapTileIndex(elevatorX, elevatorY + 2, 112, "Buildings",1);
            ms.setMapTileIndex(elevatorX, elevatorY + 1, 96, "Front", 1);
            ms.setMapTileIndex(elevatorX, elevatorY, 80, "Front",1);
            ms.setMapTile(elevatorX, elevatorY, 80, "Front", "MineElevator",1);
            ms.setMapTile(elevatorX, elevatorY + 1, 96, "Front", "MineElevator", 1);
            ms.setMapTile(elevatorX, elevatorY + 2, 112, "Buildings", "MineElevator",1);
            
            
            helper.Reflection.GetPrivateMethod(ms, "prepareElevator").Invoke();
            Point tile = Utility.findTile(ms, 80, "Buildings");
            this.Monitor.Log("x " + tile.X + " y " + tile.Y, LogLevel.Info);
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
               
            //(Game1.currentLocation as MineShaft).pr
        }
    }

    public class MyElevatorMenu : MineElevatorMenu
    {
        public MyElevatorMenu()
    {
            this.initialize(0,0,0,0,true);
            if ((int)Game1.gameMode != 3 || Game1.player == null || Game1.eventUp)
                return;
            Game1.player.Halt();
            this.elevators.Clear();
            int num = (Game1.player.deepestMineLevel-120) / 5;
            this.width = num > 50 ? (Game1.tileSize * 3 / 4 - 4) * 11 + IClickableMenu.borderWidth * 2 : Math.Min((Game1.tileSize * 3 / 4 - 4) * 5 + IClickableMenu.borderWidth * 2, num * (Game1.tileSize * 3 / 4 - 4) + IClickableMenu.borderWidth * 2);
            this.height = Math.Max(Game1.tileSize + IClickableMenu.borderWidth * 3, num * (Game1.tileSize * 3 / 4 - 4) / (this.width - IClickableMenu.borderWidth) * (Game1.tileSize * 3 / 4 - 4) + Game1.tileSize + IClickableMenu.borderWidth * 3);
            this.xPositionOnScreen = Game1.viewport.Width / 2 - this.width / 2;
            this.yPositionOnScreen = Game1.viewport.Height / 2 - this.height / 2;
            Game1.playSound("crystal");
            int x1 = this.xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder * 3 / 4;
            int y = this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.borderWidth / 3;
            this.elevators.Add(new ClickableComponent(new Rectangle(x1, y, Game1.tileSize * 3 / 4 - 4, Game1.tileSize * 3 / 4 - 4), string.Concat((object)0)));
            int x2 = x1 + Game1.tileSize - 20;
            if (x2 > this.xPositionOnScreen + this.width - IClickableMenu.borderWidth)
            {
                x2 = this.xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder * 3 / 4;
                y += Game1.tileSize - 20;
            }
            for (int index = 1; index <= num; ++index)
            {
                this.elevators.Add(new ClickableComponent(new Rectangle(x2, y, Game1.tileSize * 3 / 4 - 4, Game1.tileSize * 3 / 4 - 4), string.Concat((object)(index * 5))));
                x2 = x2 + Game1.tileSize - 20;
                if (x2 > this.xPositionOnScreen + this.width - IClickableMenu.borderWidth)
                {
                    x2 = this.xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder * 3 / 4;
                    y += Game1.tileSize - 20;
                }
            }
            this.initializeUpperRightCloseButton();
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (this.isWithinBounds(x, y))
            {
                bool gotSelected = false;
                foreach (ClickableComponent elevator in this.elevators)
                {
                    if (elevator.containsPoint(x, y))
                    {
                        if ((Game1.currentLocation as MineShaft)?.mineLevel == Convert.ToInt32(elevator.name) + 120)
                        {
                            return;
                        }
                        Game1.playSound("smallSelect");
                        if (Convert.ToInt32(elevator.name) == 0)
                        {

                            if (!Game1.currentLocation.Equals((object)Game1.mine))
                                return;
                            Game1.warpFarmer("SkullCave", 3, 4, 2);
                            Game1.exitActiveMenu();
                            Game1.changeMusicTrack("none");
                            gotSelected = true;
                        }
                        else
                        {
                            if (Game1.currentLocation.Equals((object)Game1.mine) && Convert.ToInt32(elevator.name) == Game1.mine.mineLevel)
                                return;
                            Game1.player.ridingMineElevator = true;
                            int mineLevel = Convert.ToInt32(elevator.name) + 120;
                            Game1.enterMine(true, mineLevel, null);
                            Game1.exitActiveMenu();
                            gotSelected = true;
                        }
                    }
                }
                if (!gotSelected)
                {
                    base.receiveLeftClick(x, y, true);
                }
            }
            else
                Game1.exitActiveMenu();
        }

        public override void draw(SpriteBatch b)
        {
            base.draw(b);
            foreach (ClickableComponent elevator in this.elevators)
            {
                Vector2 position = new Vector2((float)(elevator.bounds.X + 16 + NumberSprite.numberOfDigits(Convert.ToInt32(elevator.name)) * 6), (float)(elevator.bounds.Y + Game1.pixelZoom * 6 - NumberSprite.getHeight() / 4));
                NumberSprite.draw(Convert.ToInt32(elevator.name), b, position, Game1.mine.mineLevel == Convert.ToInt32(elevator.name)+120 && Game1.currentLocation.Equals((object)Game1.mine) || Convert.ToInt32(elevator.name) == 0 && !Game1.currentLocation.Equals((object)Game1.mine) ? Color.Gray * 0.75f : Color.Gold, 0.5f, 0.86f, 1f, 0, 0);
            }
        }
    }
}

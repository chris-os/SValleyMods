using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkullCavernElevator
{
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
        public class MyElevatorMenu : MineElevatorMenu
        {
            public MyElevatorMenu(int elevatorStep)
            {
                this.initialize(0, 0, 0, 0, true);
                if ((int)Game1.gameMode != 3 || Game1.player == null || Game1.eventUp)
                    return;
                Game1.player.Halt();
                this.elevators.Clear();
                int num = (Game1.player.deepestMineLevel - 120) / elevatorStep;
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
                    this.elevators.Add(new ClickableComponent(new Rectangle(x2, y, Game1.tileSize * 3 / 4 - 4, Game1.tileSize * 3 / 4 - 4), string.Concat((object)(index * elevatorStep))));
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
                                Game1.mine.mineLevel = 0;
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
                    NumberSprite.draw(Convert.ToInt32(elevator.name), b, position, Game1.mine.mineLevel == Convert.ToInt32(elevator.name) + 120 && Game1.currentLocation.Equals((object)Game1.mine) || Convert.ToInt32(elevator.name) == 0 && !Game1.currentLocation.Equals((object)Game1.mine) ? Color.Gray * 0.75f : Color.Gold, 0.5f, 0.86f, 1f, 0, 0);
                }
            }
        }
    }

}

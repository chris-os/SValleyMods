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
    public class MyElevatorMenuWithScrollbar : MineElevatorMenu
    {
        private const int SCROLLSTEP = 11;
        public ClickableTextureComponent upArrow;
        public ClickableTextureComponent downArrow;
        public ClickableTextureComponent scrollBar;
        public Rectangle scrollBarRunner;
        private int currentItemIndex = 0;
        private int elevatorStep = 5;
        private int maxElevators;
        private bool scrolling;

        public MyElevatorMenuWithScrollbar(int elevatorStep)
        {
            this.initialize(0, 0, 0, 0, true);
            this.elevatorStep = elevatorStep;
            maxElevators = (Game1.player.deepestMineLevel - 120) / elevatorStep;
            if ((int)Game1.gameMode != 3 || Game1.player == null || Game1.eventUp)
                return;
            Game1.player.Halt();
            this.elevators.Clear();
            int num = 120;
            this.width = num > 50 ? (Game1.tileSize * 3 / 4 - 4) * 11 + IClickableMenu.borderWidth * 2 : Math.Min((Game1.tileSize * 3 / 4 - 4) * 5 + IClickableMenu.borderWidth * 2, num * (Game1.tileSize * 3 / 4 - 4) + IClickableMenu.borderWidth * 2);
            this.height = Math.Max(Game1.tileSize + IClickableMenu.borderWidth * 3, num * (Game1.tileSize * 3 / 4 - 4) / (this.width - IClickableMenu.borderWidth) * (Game1.tileSize * 3 / 4 - 4) + Game1.tileSize + IClickableMenu.borderWidth * 3);
            this.xPositionOnScreen = Game1.viewport.Width / 2 - this.width / 2;
            this.yPositionOnScreen = Game1.viewport.Height / 2 - this.height / 2;
            Game1.playSound("crystal");
            this.upArrow = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + width + Game1.tileSize / 4, this.yPositionOnScreen + Game1.tileSize, 11 * Game1.pixelZoom, 12 * Game1.pixelZoom), Game1.mouseCursors, new Rectangle(421, 459, 11, 12), (float)Game1.pixelZoom, false);
            this.downArrow = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + width + Game1.tileSize / 4, this.yPositionOnScreen + height - Game1.tileSize, 11 * Game1.pixelZoom, 12 * Game1.pixelZoom), Game1.mouseCursors, new Rectangle(421, 472, 11, 12), (float)Game1.pixelZoom, false);
            this.scrollBar = new ClickableTextureComponent(new Rectangle(this.upArrow.bounds.X + Game1.pixelZoom * 3, this.upArrow.bounds.Y + this.upArrow.bounds.Height + Game1.pixelZoom, 6 * Game1.pixelZoom, 10 * Game1.pixelZoom), Game1.mouseCursors, new Rectangle(435, 463, 6, 10), (float)Game1.pixelZoom, false);
            this.scrollBarRunner = new Rectangle(this.scrollBar.bounds.X, this.upArrow.bounds.Y + this.upArrow.bounds.Height + Game1.pixelZoom, this.scrollBar.bounds.Width, height - Game1.tileSize * 2 - this.upArrow.bounds.Height - Game1.pixelZoom * 2);
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
                x2 += Game1.tileSize - 20;
                if (x2 > this.xPositionOnScreen + this.width - IClickableMenu.borderWidth)
                {
                    x2 = this.xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder * 3 / 4;
                    y += Game1.tileSize - 20;
                }
            }
            this.initializeUpperRightCloseButton();
        }

        private void setScrollBarToCurrentIndex()
        {
            if (this.elevators.Count <= 0)
                return;
            this.scrollBar.bounds.Y = this.scrollBarRunner.Height / Math.Max(1, maxElevators) * this.currentItemIndex + this.upArrow.bounds.Bottom + Game1.pixelZoom;
            if (this.currentItemIndex != maxElevators)
                return;
            this.scrollBar.bounds.Y = this.downArrow.bounds.Y - this.scrollBar.bounds.Height - Game1.pixelZoom;
        }

        public override void receiveScrollWheelAction(int direction)
        {
            if (GameMenu.forcePreventClose)
                return;
            base.receiveScrollWheelAction(direction);
            if (direction > 0 && this.currentItemIndex > 0)
            {
                this.upArrowPressed();
                Game1.playSound("shiny4");
            }
            else
            {
                if (direction >= 0 || this.currentItemIndex >= Math.Max(0, maxElevators - SCROLLSTEP))
                    return;
                this.downArrowPressed();
                Game1.playSound("shiny4");
            }
        }

        private void downArrowPressed()
        {
            this.downArrow.scale = this.downArrow.baseScale;
            this.currentItemIndex = this.currentItemIndex + SCROLLSTEP;
            this.setScrollBarToCurrentIndex();
        }

        private void upArrowPressed()
        {
            this.upArrow.scale = this.upArrow.baseScale;
            this.currentItemIndex = this.currentItemIndex - SCROLLSTEP;
            this.setScrollBarToCurrentIndex();
        }

        public override void leftClickHeld(int x, int y)
        {
            if (GameMenu.forcePreventClose)
                return;
            base.leftClickHeld(x, y);
            if (this.scrolling)
            {
                int y1 = this.scrollBar.bounds.Y;
                this.scrollBar.bounds.Y = Math.Min(this.yPositionOnScreen + this.height - Game1.tileSize - Game1.pixelZoom * 3 - this.scrollBar.bounds.Height, Math.Max(y, this.yPositionOnScreen + this.upArrow.bounds.Height + Game1.pixelZoom * 5));
                this.currentItemIndex = Math.Min(maxElevators - this.currentItemIndex, Math.Max(0, (int)((double)this.elevators.Count * (double)((float)(y - this.scrollBarRunner.Y) / (float)this.scrollBarRunner.Height))));
                this.setScrollBarToCurrentIndex();
                int y2 = this.scrollBar.bounds.Y;
                if (y1 == y2)
                    return;
                Game1.playSound("shiny4");
            }
        }

        public override void performHoverAction(int x, int y)
        {

            if (GameMenu.forcePreventClose)
                return;
            this.upArrow.tryHover(x, y, 0.1f);
            this.downArrow.tryHover(x, y, 0.1f);
            this.scrollBar.tryHover(x, y, 0.1f);
            int num = this.scrolling ? 1 : 0;
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (this.downArrow.containsPoint(x, y))
            {
                if (this.currentItemIndex < Math.Max(0, maxElevators - SCROLLSTEP))
                {
                    this.downArrowPressed();
                    Game1.playSound("shwip");
                }
            }
            else if (this.upArrow.containsPoint(x, y))
            {
                if (this.currentItemIndex > 0)
                {
                    this.upArrowPressed();
                    Game1.playSound("shwip");
                }
            }
            else if (this.scrollBar.containsPoint(x, y))
                this.scrolling = true;
            else if (!this.downArrow.containsPoint(x, y) && x > this.xPositionOnScreen + this.width && (x < this.xPositionOnScreen + this.width + Game1.tileSize * 2 && y > this.yPositionOnScreen) && y < this.yPositionOnScreen + this.height)
            {
                this.scrolling = true;
                this.leftClickHeld(x, y);
                this.releaseLeftClick(x, y);
            }
            else if (this.isWithinBounds(x, y))
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
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
            Game1.drawDialogueBox(this.xPositionOnScreen, this.yPositionOnScreen - Game1.tileSize + Game1.tileSize / 8, this.width + Game1.tileSize / 3, this.height + Game1.tileSize, false, true, (string)null, false);
            this.drawMouse(b);
            this.upperRightCloseButton.draw(b);
            this.upArrow.draw(b);
            this.downArrow.draw(b);
            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), this.scrollBarRunner.X, this.scrollBarRunner.Y, this.scrollBarRunner.Width, this.scrollBarRunner.Height, Color.White, (float)Game1.pixelZoom, false);
            this.scrollBar.draw(b);
            for (int i = 0; i < 121; i++)
            {
                ClickableComponent elevator = elevators.ElementAt(i);
                elevator.name = "" + ((i + currentItemIndex) * elevatorStep);
                drawElevator(b, elevator);
            }
        }

        private static void drawElevator(SpriteBatch b, ClickableComponent elevator)
        {
            b.Draw(Game1.mouseCursors, new Vector2((float)(elevator.bounds.X - Game1.pixelZoom), (float)(elevator.bounds.Y + Game1.pixelZoom)), new Rectangle?(new Rectangle((double)elevator.scale > 1.0 ? 267 : 256, 256, 10, 10)), Color.Black * 0.5f, 0.0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, 0.865f);
            b.Draw(Game1.mouseCursors, new Vector2((float)elevator.bounds.X, (float)elevator.bounds.Y), new Rectangle?(new Rectangle((double)elevator.scale > 1.0 ? 267 : 256, 256, 10, 10)), Color.White, 0.0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, 0.868f);
            Vector2 position = new Vector2((float)(elevator.bounds.X + 16 + NumberSprite.numberOfDigits(Convert.ToInt32(elevator.name)) * 6), (float)(elevator.bounds.Y + Game1.pixelZoom * 6 - NumberSprite.getHeight() / 4));
            NumberSprite.draw(Convert.ToInt32(elevator.name), b, position, Game1.mine.mineLevel == Convert.ToInt32(elevator.name) + 120 && Game1.currentLocation.Equals((object)Game1.mine) || Convert.ToInt32(elevator.name) == 0 && !Game1.currentLocation.Equals((object)Game1.mine) ? Color.Gray * 0.75f : Color.Gold, 0.5f, 0.86f, 1f, 0, 0);
        }
    }
}

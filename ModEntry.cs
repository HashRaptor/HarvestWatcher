using System;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Globalization;
using System.Data.Entity.Design.PluralizationServices;

namespace HarvestWatcher
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        private StardewValley.TerrainFeatures.HoeDirt terrain = null;
        PluralizationService pluralizationService = null;

        public ModEntry()
        {
            pluralizationService = PluralizationService.CreateService(new CultureInfo("en-us"));
        }

        public override void Entry(IModHelper helper)
        {
            GraphicsEvents.OnPostRenderEvent += on_post;
            ControlEvents.MouseChanged += on_post_render_event;
        }

        private void on_post(object sender, EventArgs e)
        {
            if(terrain != null)
            {
                StardewValley.Crop crop = terrain.crop;
                if (crop != null)
                {
                    String cropName = Game1.objectInformation[crop.indexOfHarvest].Split('/')[0];
                    drawHoverTextBox(Game1.smallFont, $"These {cropName}(s) will be ready in {crop.dayOfCurrentPhase} day(s)");
                }
            }
        }

        private void on_post_render_event(object sender, EventArgsMouseStateChanged e )
        {
            if (Game1.activeClickableMenu != null)
            {
                terrain = null;
                return;
            }

            if (terrain != null && ButtonState.Pressed.Equals(e.PriorState.RightButton) && ButtonState.Released.Equals(e.NewState.RightButton))
            {
                this.Monitor.Log("removed terrain");
                terrain = null;
            }

            Vector2 vector2 = (double)Game1.mouseCursorTransparency == 0.0 ? Game1.player.GetToolLocation(false) : new Vector2((float)(Game1.getOldMouseX() + Game1.viewport.X), (float)(Game1.getOldMouseY() + Game1.viewport.Y));

            if (Game1.currentLocation != null)
            {
                Vector2 index = new Vector2((float)((Game1.getOldMouseX() + Game1.viewport.X) / Game1.tileSize), (float)((Game1.getOldMouseY() + Game1.viewport.Y) / Game1.tileSize));

                SerializableDictionary<Vector2, StardewValley.TerrainFeatures.TerrainFeature> terrainMap = Game1.currentLocation.terrainFeatures;
                if (terrainMap != null && terrainMap.ContainsKey(index))
                {
                    if (terrainMap[index] is StardewValley.TerrainFeatures.HoeDirt && (terrain == null || terrain != terrainMap[index]))
                    {
                        terrain = (terrainMap[index] as StardewValley.TerrainFeatures.HoeDirt);
                        StardewValley.Crop crop = terrain.crop;
                        if (crop != null)
                        {
                            String cropName = Game1.objectInformation[crop.indexOfHarvest].Split('/')[0];
                            bool isCropHarvestable = crop.currentPhase >= crop.phaseDays.Count - 1 && (!crop.fullyGrown || crop.dayOfCurrentPhase <= 0);

                            String cropInformation = $"\nCrop Information: \n\tname: {cropName}\n\tisHarvestable: {isCropHarvestable}";
                            foreach (System.Reflection.FieldInfo field in crop.GetType().GetFields())
                            {
                                cropInformation += $"\n\t{field.Name}: ";

                                if (field.GetValue(crop) is System.Collections.Generic.List<String>)
                                {
                                    System.Collections.Generic.List<String> asd = field.GetValue(crop) as System.Collections.Generic.List<String>;

                                    cropInformation += String.Join(", ", asd.ToArray());
                                } else if (field.GetValue(crop) is System.Collections.Generic.List<Int32>)
                                {
                                    System.Collections.Generic.List<Int32> asd = field.GetValue(crop) as System.Collections.Generic.List<Int32>;
                                    cropInformation += String.Join(", ", asd.ToArray());
                                } else
                                {
                                    cropInformation += $"{field.GetValue(crop).ToString()}";
                                }
                            }
                            this.Monitor.Log(cropInformation);
                        }
                    }
                    else if (!(terrainMap[index] is StardewValley.TerrainFeatures.HoeDirt))
                    {
                        terrain = null;
                    }
                }
            }
        }

        private void drawHoverTextBox(SpriteFont font, String text)
        {
            Vector2 bounds = font.MeasureString(text);
            int width = (int)bounds.X + Game1.tileSize / 2;
            int height = (int)font.MeasureString(text).Y + Game1.tileSize / 3 + 5;

            int x = (int)(Mouse.GetState().X / Game1.options.zoomLevel) - Game1.tileSize / 2 - width;
            int y = (int)(Mouse.GetState().Y / Game1.options.zoomLevel) + Game1.tileSize / 2;

            if (x < 0)
            {
                x = 0;
            }
            if (y + height > Game1.graphics.GraphicsDevice.Viewport.Height)
            {
                y = Game1.graphics.GraphicsDevice.Viewport.Height - height;
            }

           IClickableMenu.drawTextureBox(Game1.spriteBatch, Game1.menuTexture, new Rectangle(0, 256, 60, 60), x, y, width, height, Color.White, 1f, true);

            Utility.drawTextWithShadow(Game1.spriteBatch, text, font, new Vector2((float)(x + Game1.tileSize / 4), (float)(y + Game1.tileSize / 4)), Game1.textColor, 1f, -1f, -1, -1, 1F, 3);
        }
    }
}
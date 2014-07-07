using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.IO;

namespace WindowsGame2
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D wall, red, blue, green;
        Dictionary<int, Texture2D> tileTextures;
        Dictionary<string, Texture2D> textures;
        Vector4 viewport;
        Rectangle map;
        byte[,] tiles;
        string towerSelected;
        List<string> towerStrings;

        // For zooming in and out
        MouseState ms;
        float previousWheelValue;
        float currentWheelValue;

        bool mousePressed;
        Vector2 mousePressedLocation;

        int hudWidth = 75;
        int hudHeight = 25;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            this.Window.AllowUserResizing = true;
            graphics.IsFullScreen = false;
            graphics.PreferredBackBufferWidth = 512 + hudWidth;
            graphics.PreferredBackBufferHeight = 512 + hudHeight;

            ms = Mouse.GetState();
            previousWheelValue = ms.ScrollWheelValue;
            currentWheelValue = ms.ScrollWheelValue;

            this.IsMouseVisible = true;

        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            Stream filestream = new FileStream("level1\\tiles.png", FileMode.Open);
            Texture2D sprites = Texture2D.FromStream(graphics.GraphicsDevice, filestream);
            Texture2D[] holder = {blue, green, red, wall};
            for (int i = 0; i < holder.Length; i++){
                int width = sprites.Width/holder.Length;
                Rectangle source = new Rectangle(i * width, 0, width, width);
                holder[i] = new Texture2D(GraphicsDevice, width, width);
                Color[] data = new Color[width * width];
                sprites.GetData(0, source, data, 0, data.Length);
                holder[i].SetData(data);
            }
            blue = holder[0];
            green = holder[1];
            red = holder[2];
            wall = holder[3];
            base.Initialize();
            map = new Rectangle(0, 0, 12800, 12800);
            viewport = new Vector4(0, 0, 128 * 10, 128 * 10);

            mousePressed = false;
            mousePressedLocation = new Vector2();

            tiles = new byte[100, 100];
            int xIndex = 0;
            int yIndex = 0;
           Char[] xmlString = System.IO.File.ReadAllText("level1\\level1.tmx").ToCharArray();
            for (int i = 10; i < xmlString.Length; i++)
            {
                if (xmlString[i - 1] == '=' && xmlString[i - 2] == 'd' && xmlString[i - 3] == 'i' && xmlString[i - 4] == 'g' && xmlString[i - 5] == ' ')
                {
                    if (xmlString[i + 1] == '1')
                    {
                        tiles[xIndex, yIndex] = 1;
                    }
                    else if (xmlString[i + 1] == '2')
                    {
                        tiles[xIndex, yIndex] = 2; 
                    }
                    else if (xmlString[i + 1] == '3')
                    {
                        tiles[xIndex, yIndex] = 3;
                    }
                    else if (xmlString[i + 1] == '4')
                    {
                        tiles[xIndex, yIndex] = 4;
                    }
                    xIndex++;
                    if (xIndex == 100)
                    {
                        xIndex = 0;
                        yIndex++;
                    }
                }
            }
            tileTextures = new Dictionary<int, Texture2D>();
            tileTextures.Add(1, blue);
            tileTextures.Add(2, green);
            tileTextures.Add(3, red);
            tileTextures.Add(4, wall);

            towerStrings = new List<string>();
            towerStrings.Add("tower_basic");
            towerStrings.Add("tower_fire");
            towerStrings.Add("tower_lightning");

            textures = new Dictionary<string, Texture2D>();
            textures.Add("hud", Texture2D.FromStream(graphics.GraphicsDevice, new FileStream("images\\hud.png", FileMode.Open)));
            foreach (string towerString in towerStrings)
            {
                textures.Add(towerString, Texture2D.FromStream(graphics.GraphicsDevice, new FileStream("images\\" + towerString + ".png", FileMode.Open)));
            }
            textures.Add("selected", Texture2D.FromStream(graphics.GraphicsDevice, new FileStream("images\\selected.png", FileMode.Open)));
            towerSelected = "";
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here

            KeyboardState keyState = Keyboard.GetState();
            float scrollx = 0, scrolly = 0;

            if (keyState.IsKeyDown(Keys.Left))
                scrollx = -1;
            if (keyState.IsKeyDown(Keys.Right))
                scrollx = 1;
            if (keyState.IsKeyDown(Keys.Up))
                scrolly = 1;
            if (keyState.IsKeyDown(Keys.Down))
                scrolly = -1;

            viewport.X -= scrollx * 3;
            viewport.Y += scrolly * 3;

            ms = Mouse.GetState();
            previousWheelValue = currentWheelValue;
            currentWheelValue = ms.ScrollWheelValue;
            if (currentWheelValue != previousWheelValue)
            {
                float gameViewportX = screenToGameX(viewport.X);
                float gameViewportY = screenToGameY(viewport.Y);
                viewport.W -= .001f * viewport.W * (currentWheelValue - previousWheelValue);
                viewport.Z -= .001f * viewport.Z * (currentWheelValue - previousWheelValue);
                viewport.X = gameToScreenX(gameViewportX);
                viewport.Y = gameToScreenY(gameViewportY);
            }

            checkAndFixBounds();
            
            bool oldMousePressed = mousePressed;
            mousePressed = (ms.LeftButton == ButtonState.Pressed);
            if (mousePressed && !oldMousePressed)
            {
                mousePressedLocation.X = screenToGameX(ms.X - viewport.X);
                mousePressedLocation.Y = screenToGameY(ms.Y - viewport.Y);
                if(ms.X < 0 || ms.Y < hudHeight || ms.X > screenWidth() || ms.Y > screenHeight()){
                    // Press not on map, check if on hud buttons
                    towerSelected = "";
                    for (int i = 0; i < towerStrings.Count; i++)
                    {
                        if (ms.X >= this.GraphicsDevice.Viewport.Width - (int)(hudWidth * .9 + .5) && ms.X <= this.GraphicsDevice.Viewport.Width - (int)(hudWidth * .1 + .5) && ms.Y >= this.GraphicsDevice.Viewport.Height - (int)(hudWidth * .9 + .5) - (i * hudWidth) && ms.Y <= (this.GraphicsDevice.Viewport.Height - (int)(hudWidth * .9 + .5) - (i * hudWidth)) + (hudWidth)){
                            towerSelected = towerStrings[i];
                        }
                    }

                }   
            }
            if (mousePressed)
            {
                viewport.X = (gameToScreenX(mousePressedLocation.X) - ms.X) * -1;
                viewport.Y = (gameToScreenY(mousePressedLocation.Y) - ms.Y) * -1;
                checkAndFixBounds();
            }

            base.Update(gameTime);
        }

        protected void checkAndFixBounds()
        {
            if (viewport.W > 12800)
            {
                viewport.W = 12800;
            }
            if (viewport.W < 128 * 2)
            {
                viewport.W = 128 * 2;
            }
            if (viewport.Z > 12800)
            {
                viewport.Z = 12800;
            }
            if (viewport.Z < 128 * 2)
            {
                viewport.Z = 128 * 2;
            }
            if (viewport.X > 0)
            {
                viewport.X = 0;
            }
            if (viewport.Y > 0)
            {
                viewport.Y = 0;
            }
            if (screenToGameX(viewport.X) - viewport.W < -12800)
            {
                viewport.X = gameToScreenX(-12800 + viewport.W);
            }
            if (screenToGameY(viewport.Y) - viewport.Z < -12800)
            {
                viewport.Y = gameToScreenY(-12800 + viewport.Z);
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            // TODO: Add your drawing code here
            spriteBatch.Begin();
            ////////////////////////////////

            float width = screenWidth();
            float height = screenHeight();
            float blockWidth = width / (viewport.W / 128);
            float blockHeight = height / (viewport.Z / 128);

            // Draw the blocks 
            for (int i = 0; i < 100; i++)
            {
                for (int j = 0; j < 100; j++)
                {
                    int w = (int)blockWidth;
                    if ((int)(viewport.X + (i+1)*blockWidth) - (int)(viewport.X + i*blockWidth) > w || i == 99)
                    {
                        w++;
                    }
                    int h = (int)blockHeight;
                    if ((int)(viewport.Y + (j+1)*blockHeight) - (int)(viewport.Y + j*blockHeight) > h || j == 99)
                    {
                        h++;
                    }
                    spriteBatch.Draw(tileTextures[tiles[i,j]], new Rectangle((int)(viewport.X + i*blockWidth), hudHeight + (int)(viewport.Y + j*blockHeight), w, h), Color.White);
                }
            }

            // Draw the HUD
            spriteBatch.Draw(textures["hud"], new Rectangle(this.GraphicsDevice.Viewport.Width - hudWidth, 0, hudWidth, this.GraphicsDevice.Viewport.Height), Color.White);
            spriteBatch.Draw(textures["hud"], new Rectangle(0, 0, this.GraphicsDevice.Viewport.Width, hudHeight), Color.White);

            for (int i = 0; i < towerStrings.Count; i++)
            {
                if (towerSelected == towerStrings[i])
                {
                    spriteBatch.Draw(textures["selected"], new Rectangle(this.GraphicsDevice.Viewport.Width - (int)(hudWidth * .9 + .5), this.GraphicsDevice.Viewport.Height - (int)(hudWidth * .9 + .5) - (i * hudWidth), (int)(hudWidth * .8 + .5), (int)(hudWidth * .8 + .5)), Color.White);
                }
                spriteBatch.Draw(textures[towerStrings[i]], new Rectangle(this.GraphicsDevice.Viewport.Width - (int)(hudWidth * .9 + .5), this.GraphicsDevice.Viewport.Height - (int)(hudWidth * .9 + .5) - (i * hudWidth), (int)(hudWidth * .8 + .5), (int)(hudWidth * .8 + .5)), Color.White);
            }

            ////////////////////////////////
            spriteBatch.End();

            base.Draw(gameTime);
        }
        public float screenToGameX(float x)
        {
            return (x / screenWidth()) * viewport.W;
        }
        public float screenToGameY(float y)
        {
            return (y / screenHeight()) * viewport.Z;
        }
        public float gameToScreenX(float x)
        {
            return (x / viewport.W) * screenWidth();
        }
        public float gameToScreenY(float y)
        {
            return (y / viewport.Z) * screenHeight();
        }
        public int screenWidth()
        {
            return this.GraphicsDevice.Viewport.Width - hudWidth;
        }
        public int screenHeight()
        {
            return this.GraphicsDevice.Viewport.Height - hudHeight;
        }
    }
}

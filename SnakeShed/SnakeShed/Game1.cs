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

namespace SnakeShed
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        // Set initial value for speed.
        Vector2[] spriteSpeed = new Vector2[10];
        // This is a texture we can render.
        Texture2D headTexture;
        Texture2D bodyTexture;
        Texture2D pelletTexture;
        // Set the coordinates to draw the sprite at.
        Vector2[] arrayVector = new Vector2[10];
        Vector2[,] headSpeeds = new Vector2[10,10];
        Vector2[,] turnPoints = new Vector2[10,10];
        int seg; //number of segments initialised
        int[] count = new int[10]; //number of turns saved
        Vector2 pelletPos;
        //font
        Vector2 FontPosition;
        SpriteFont Font1;
        Double time;
        int score;
        //input
        KeyboardState oldState;
        float rotationAngle;
        Vector2 origin;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            spriteSpeed[0] = new Vector2(-1,0);
            Vector2 FontPosition = Vector2.Zero;
            time = 0.0d;
            score = 0;
            seg = 1;
            oldState = Keyboard.GetState();
            origin.Y = 16;
            origin.X = 16;
            pelletPos = new Vector2(900, 700);

            //move all the body parts off screen at start
            for (int i = 0; i < 10; i++)
            {
                if (i != 0)
                {
                    arrayVector[i] = new Vector2(900, 700);
                }
                else
                {
                    arrayVector[i] = new Vector2(400, 300);
                }
            }

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            headTexture = Content.Load<Texture2D>("SnakeHead");
            bodyTexture = Content.Load<Texture2D>("SnakeBody");
            pelletTexture = Content.Load<Texture2D>("Pellet");

            //font
            Font1 = Content.Load<SpriteFont>("SpriteFont1");

            // TODO: Load your game content here            
            FontPosition = new Vector2(graphics.GraphicsDevice.Viewport.Width / 2,
                graphics.GraphicsDevice.Viewport.Height / 2);

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
            time += gameTime.ElapsedGameTime.TotalSeconds;

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();


            if (pelletPos == new Vector2(900, 700))
            {
                System.Random RandNum = new System.Random();
                int x = RandNum.Next(64, 736);
                int y = RandNum.Next(64, 336);
                pelletPos = new Vector2(x, y);
            }

            if (approxEquals(arrayVector[0], pelletPos))
            {
                score += 1;
                pelletPos = new Vector2(900, 700);
                if (seg < 9)
                {
                    arrayVector[seg] = (-spriteSpeed[seg - 1] * new Vector2(32, 32)) + arrayVector[seg - 1];
                    spriteSpeed[seg] = spriteSpeed[seg - 1];
                    for (int j = 0; j < 10; j++)
                    {
                        turnPoints[seg,j] = turnPoints[(seg - 1),j];
                        headSpeeds[seg, j] = headSpeeds[(seg - 1), j];
                    }
                    count[seg] = count[seg - 1];

                    seg += 1;
                }  
            }

            this.UpdateInput();
            this.UpdateSprite(gameTime);
            

            base.Update(gameTime);
        }

        private void UpdateSprite(GameTime gameTime)
        {
            //Move the sprite by speed, scaled by elapsed time.
            arrayVector[0] += spriteSpeed[0] * 2;

            int MaxX =
                graphics.GraphicsDevice.Viewport.Width - headTexture.Width + 32;
            int MinX = 0;
            int MaxY =
                graphics.GraphicsDevice.Viewport.Height - headTexture.Height + 32;
            int MinY = 0;

            // Check for bounce.
            for (int i = 0; i < seg; i++)
            {
                if (arrayVector[i].X > MaxX)
                {
                    arrayVector[i].X = MinX;
                }

                else if (arrayVector[i].X < MinX)
                {
                    arrayVector[i].X = MaxX;
                }

                if (arrayVector[i].Y > MaxY)
                {
                    arrayVector[i].Y = MinY;
                }

                else if (arrayVector[i].Y < MinY)
                {
                    arrayVector[i].Y = MaxY;
                }

                if (i != 0)
                {
                    if (arrayVector[i] == turnPoints[i,0])
                        {
                            spriteSpeed[i] = headSpeeds[i,0];
                            for (int j = 0; j < count[i]; j++)
                            {
                                if (j == (count[i] - 1))
                                {
                                    headSpeeds[i,j] = new Vector2(0, 0);
                                    turnPoints[i,j] = new Vector2(0, 0);
                                }
                                else
                                {
                                    headSpeeds[i,j] = headSpeeds[i,j + 1];
                                    turnPoints[i,j] = turnPoints[i,j + 1];
                                }
                                 
                            }
                            count[i] -= 1; 
                        }
                    arrayVector[i] += spriteSpeed[i] * 2;
                }

            }
           

        }

        private void UpdateInput()
        {
            KeyboardState newState = Keyboard.GetState();

            // Is the Up key down?
            if (newState.IsKeyDown(Keys.Up))
            {
                // If not down last update, key has just been pressed.
                if (!oldState.IsKeyDown(Keys.Up) && spriteSpeed[0] != new Vector2(0, 1))
                {
                    spriteSpeed[0] = new Vector2(0, -1);
                    if (score % 10 != 0)
                    {
                        for (int i = 1; i < seg; i++)
                        {
                            turnPoints[i, count[i]] = arrayVector[0];
                            headSpeeds[i, count[i]] = spriteSpeed[0];
                            count[i] += 1;
                        }
                    }
                    rotationAngle = (float)(Math.PI / 2);
                    
                }
            }
            else if (newState.IsKeyDown(Keys.Down))
            {
                if (!oldState.IsKeyDown(Keys.Down) && spriteSpeed[0] != new Vector2(0, -1))
                {
                    spriteSpeed[0] = new Vector2(0, 1);
                    if (score % 10 != 0)
                    {
                        for (int i = 1; i < seg; i++)
                        {
                            turnPoints[i, count[i]] = arrayVector[0];
                            headSpeeds[i, count[i]] = spriteSpeed[0];
                            count[i] += 1;
                        }
                    }
                    rotationAngle = (float)((3 * Math.PI)/ 2);
                }
            }
            else if (newState.IsKeyDown(Keys.Left))
            {
                if (!oldState.IsKeyDown(Keys.Left) && spriteSpeed[0] != new Vector2(1, 0))
                {
                    spriteSpeed[0] = new Vector2(-1, 0);
                    if (score % 10 != 0)
                    {
                        for (int i = 1; i < seg; i++)
                        {
                            turnPoints[i, count[i]] = arrayVector[0];
                            headSpeeds[i, count[i]] = spriteSpeed[0];
                            count[i] += 1;
                        }
                    }
                    rotationAngle = (float)(2*Math.PI);
                }
            }
            else if (newState.IsKeyDown(Keys.Right))
            {
                if (!oldState.IsKeyDown(Keys.Right) && spriteSpeed[0] != new Vector2(-1, 0))
                {

                    spriteSpeed[0] = new Vector2(1, 0);
                    if (score % 10 != 0)
                    {
                        for (int i = 1; i < seg; i++)
                        {
                            turnPoints[i, count[i]] = arrayVector[0];
                            headSpeeds[i, count[i]] = spriteSpeed[0];
                            count[i] += 1;
                        }
                    }
                    rotationAngle = (float)(Math.PI);
                }
            }
            // Update saved state.
            oldState = newState;
        }

        private bool approxEquals(Vector2 a, Vector2 b)
        {
            bool check = (a.X <= b.X + 24 && a.X >= b.X - 24) && (a.Y <= b.Y + 24 && a.Y >= b.Y - 24);
            return check;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            Int32 rem;
            Math.DivRem((int)time, 2, out rem);

            // Draw the sprite.
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
            spriteBatch.Draw(headTexture, arrayVector[0], null, Color.White, rotationAngle, origin, 1.0f, SpriteEffects.None, 0.0f);
            for (int i = 1; i < 10; i++)
            {
                spriteBatch.Draw(bodyTexture, arrayVector[i], null, Color.White, 0.0f, origin, 1.0f, SpriteEffects.None, 0.0f);
            }
            spriteBatch.Draw(pelletTexture, pelletPos, null, Color.White, 0.0f, origin, 1.0f, SpriteEffects.None, 0.0f);

            // Draw time passed
            string output = "Score: " + score.ToString(); //time.ToString();
            // Find the center of the string
            Vector2 FontOrigin = Font1.MeasureString(output) / 2;
            // Draw the string
            spriteBatch.DrawString(Font1, output, FontPosition, Color.LightGreen,
                0, FontOrigin, 1.0f, SpriteEffects.None, 0.5f);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
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
        Vector2[] spriteSpeed;
        // This is a texture we can render.
        Texture2D headTexture;
        Texture2D bodyTexture;
        Texture2D pelletTexture;
        Texture2D deadTexture;
        // Set the coordinates to draw the sprite at.
        Vector2[] arrayVector;
        Vector2[,] headSpeeds;
        Vector2[,] turnPoints;
        List<Vector2> tails;
        List<Vector2> pellets;
        int seg; //number of segments initialised
        int[] count; //number of turns saved
        //Vector2 pelletPos;
        //font
        Vector2 FontPosition;
        SpriteFont Font1;
        Double time;
        int score;
        //input
        KeyboardState oldState;
        float rotationAngle;
        Vector2 origin;
        bool gameOver;
        int lives;
        //sound
        bool deadPlay;
        SoundEffect EAT;
        SoundEffect DIE;
        SoundEffect SHED;

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
            //reset or load gamestate
            spriteSpeed = new Vector2[10];
            arrayVector = new Vector2[10];
            headSpeeds = new Vector2[10, 10];
            turnPoints = new Vector2[10, 10];
            tails = new List<Vector2>();
            count = new int[10];
            rotationAngle = 0.0f;
            gameOver = false;
            lives = 3;

            spriteSpeed[0] = new Vector2(-1,0);
            Vector2 FontPosition = Vector2.Zero;
            time = 0.0d;
            score = 0;
            seg = 1;
            oldState = Keyboard.GetState();
            origin.Y = 16;
            origin.X = 16;
            pellets = new List<Vector2>();
            pellets.Add(new Vector2(900, 700));

            //sound playing
            DIE = Content.Load<SoundEffect>("SnakeDie");
            deadPlay = false;
            EAT = Content.Load<SoundEffect>("SnakeEat");
            SHED = Content.Load<SoundEffect>("SnakeShed");

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
            deadTexture = Content.Load<Texture2D>("SnakeDead");

            //font
            Font1 = Content.Load<SpriteFont>("SpriteFont1");         
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

            for (int i = 0; i < pellets.Count; i++)
            {
                //spawn a pellet if off screen
                if (pellets[i] == new Vector2(900, 700))
                {
                    bool shedCollision = true;
                    //while loop to make sure new pellets aren't inside shed skins
                    while ((pellets[i] == new Vector2(900, 700)) || shedCollision)
                    {
                        shedCollision = false;
                        System.Random RandNum = new System.Random();
                        int x = 768;
                        int y = RandNum.Next(64, 436);
                        List<Vector2> body = new List<Vector2>(arrayVector);
                        var combined = body.Union(tails).Union(pellets).ToList();
                        pellets[i] = new Vector2(x, y);
                        foreach (Vector2 elem in combined)
                        {
                            if (approxEquals(pellets[i], elem))
                            {
                                shedCollision = true;
                            }
                        }
                    }
                }
            
                //checks if snake has hit a pellet
                if (approxEquals(arrayVector[0], pellets[i]))
                {
                    score += 1;
                    pellets[i] = new Vector2(900, 700);
                    for (int k = 0; k < (score / 5); k++)
                    {
                        if (pellets.Count < (score / 5) + 1)
                        {
                            pellets.Add(new Vector2(900, 700));
                        }
                    }
                    //if it is less than length ten, lengthen tail
                    if (seg < 10)
                    {
                        arrayVector[seg] = (-spriteSpeed[seg - 1] * new Vector2(32, 32)) + arrayVector[seg - 1];
                        spriteSpeed[seg] = spriteSpeed[seg - 1];
                        for (int j = 0; j < 10; j++)
                        {
                            turnPoints[seg, j] = turnPoints[(seg - 1), j];
                            headSpeeds[seg, j] = headSpeeds[(seg - 1), j];
                        }
                        count[seg] = count[seg - 1];

                        seg += 1;
                        EAT.Play();
                    }
                    //otherwise make tail dead
                    else
                    {
                        var tail = new List<Vector2>(arrayVector);
                        tail.RemoveAt(0);
                        tails.AddRange(tail);
                        seg = 1;
                        for (int j = 1; j < 10; j++)
                        {
                            arrayVector[j] = new Vector2(900, 700);
                        }
                        while (approxEquals(arrayVector[0], tails[tails.Count - 9]))
                        {
                            arrayVector[0] += spriteSpeed[0] * 2;
                        }
                        Array.Clear(turnPoints, 0, 100);
                        Array.Clear(headSpeeds, 0, 100);
                        Array.Clear(spriteSpeed, 1, 9);
                        Array.Clear(count, 0, 10);
                        SHED.Play();
                    }
                }
            }

            //checks if snake has collided with dead skins or its tail
            var tale = new List<Vector2>(arrayVector);
            tale.RemoveAt(0);
            tale.RemoveAt(0);
            var coll = tale.Union(tails).ToList();
            foreach (Vector2 collision in coll)
            {
                if (approxEquals(arrayVector[0], collision))
                {
                    Array.Clear(spriteSpeed, 0, 10);
                    gameOver = true;
                    if (!deadPlay)
                    {
                        DIE.Play();
                        deadPlay = true;
                    }
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

                //when not looping on head
                if (i != 0)
                {
                    if (arrayVector[i] == turnPoints[i,0])
                        {
                            spriteSpeed[i] = headSpeeds[i,0];
                            for (int j = 0; j < count[i]; j++)
                            {
                                if (j == (count[i] - 1))
                                {
                                    headSpeeds[i,j] = new Vector2(0, 0); //reset headspeed and turnpoints at end of list to 0
                                    turnPoints[i,j] = new Vector2(0, 0);
                                }
                                else
                                {
                                    headSpeeds[i,j] = headSpeeds[i,j + 1]; //shuffle other headspeed and turnpoints up one
                                    turnPoints[i,j] = turnPoints[i,j + 1];
                                }
                                 
                            }
                            count[i] -= 1; 
                        }
                    arrayVector[i] += spriteSpeed[i] * 2; //move rest of body
                }

            }
            //move pellets
            for (int k = 0; k < pellets.Count; k++)
            {
                if (pellets[k].X < 0)
                {
                    lives -= 1;
                    if (lives < 1)
                    {
                        Array.Clear(spriteSpeed, 0, 10);
                        gameOver = true;
                        if (!deadPlay)
                        {
                            DIE.Play();
                            deadPlay = true;
                        }
                    }
                    pellets[k] = new Vector2(900, 700);
                }
                else if (pellets[k] != new Vector2(900,700) && !gameOver)
                {
                    pellets[k] += new Vector2(-1, 0);
                }

                
            }

        }

        private void UpdateInput()
        {
            KeyboardState newState = Keyboard.GetState();
            if (!gameOver)
            {
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
                        rotationAngle = (float)((3 * Math.PI) / 2);
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
                        rotationAngle = (float)(2 * Math.PI);
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
            }
            else
            {
                if (newState.IsKeyDown(Keys.Enter) && !oldState.IsKeyDown(Keys.Enter))
                {
                    this.Initialize();
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

            // Draw the snake.
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
            spriteBatch.Draw(headTexture, arrayVector[0], null, Color.White, rotationAngle, origin, 1.0f, SpriteEffects.None, 0.0f);
            for (int i = 1; i < 10; i++)
            {
                spriteBatch.Draw(bodyTexture, arrayVector[i], null, Color.White, 0.0f, origin, 1.0f, SpriteEffects.None, 0.0f);
            }
            foreach (Vector2 pos in pellets) //draw all pellets
            {
                spriteBatch.Draw(pelletTexture, pos, null, Color.White, 0.0f, origin, 1.0f, SpriteEffects.None, 0.0f);
            }
            //draw all dead tails
            foreach (Vector2 pos in tails)
            {
                spriteBatch.Draw(deadTexture, pos, null, Color.White, 0.0f, origin, 1.0f, SpriteEffects.None, 0.0f);
            }

            // Draw score
            string output;
            if (gameOver)
            {
                output = "Game over! You scored: " + score.ToString();
            }
            else
            {
                output = "Score: " + score.ToString();
            }
            
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

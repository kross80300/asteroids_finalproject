namespace asteroids_finalproject;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

public class Powerup
{
    
    private int frameCount = 6;
    private float frameTime = 0.25f;
    private int currFrame = 0; 
    private int frameW = 95;
    private int frameH = 101;
    private Rectangle img = new Rectangle(0, 0, 95, 101);

    private Vector2 position;
    private int type;
    private Texture2D texture;
    private Vector2 velocity;
    private Color color = new Color(0.9f, 0.5f, 0.5f);


    public Powerup(Vector2 pos, Vector2 vel, Texture2D spritesheet, int type)
    {
        position = pos;
        velocity = vel;
        texture = spritesheet;
        if (type == 0)
            color = new Color(0.9f, 0.3f, 0.3f);
        else if (type == 1)
            color = new Color(0.3f, 0.3f, 0.9f);
        else
            color = new Color(0.3f, 0.9f, 0.2f);
    }

    public int getType()
    {
        return type;
    }

    public void Update(GameTime time)
    {
        position += velocity;

        frameTime += (float)time.ElapsedGameTime.TotalSeconds;
        if (frameTime >= 0.15f)
        {
            frameTime = 0f;
            if (currFrame >= frameCount)
            {
                currFrame = 0;
            }
            img = new Rectangle(currFrame * frameW, 0, frameW, frameH);
            currFrame++;
        }
    }

    public Rectangle GetBoundingBox()
    {

        return new Rectangle(
            (int)(position.X + 10),
            (int)(position.Y + 10),
            frameW - 20,
            frameH - 20
        );
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(texture, position, img, color, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
    }











}
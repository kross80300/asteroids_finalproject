namespace asteroids_finalproject;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


public class Powerup
{
    
    private int frameCount = 6;
    private float frameTime = 0.25f;
    private int currFrame = 0; 
    private int frameW = 94;
    private int frameH = 101;
    private Rectangle img = new Rectangle(0, 0, 94, 101);

    private Vector2 position;
    private int type;
    private Texture2D texture;
    private Vector2 velocity;
    private Color color = new Color(0.9f, 0.5f, 0.5f);
    private float scale = 0.7f;


    public Powerup(Vector2 pos, Vector2 vel, Texture2D spritesheet, int type)
    {
        position = pos;
        velocity = vel;
        texture = spritesheet;
        this.type = type;
        if (type == 0)
            color = new Color(0.9f, 0.4f, 0.4f);
        else if (type == 1)
            color = new Color(0.9f, 0.9f, 0.9f);
        else if (type == 2)
            color = new Color(0.4f, 0.4f, 0.9f);
        else
            color = new Color(0.4f, 0.9f, 0.4f);
    }

    public int getType()
    {
        return type;
    }

    public void Update(GameTime time)
    {
        position += velocity;

        frameTime += (float)time.ElapsedGameTime.TotalSeconds;
        if (frameTime >= 0.1f)
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
            (int)(position.X + scale * 10),
            (int)(position.Y + scale * 10),
            (int)(frameW * scale) - 10,
            (int)(frameH * scale) - 10
        );
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(texture, position, img, color, 0.0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
    }











}

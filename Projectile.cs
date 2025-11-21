using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace asteroids_finalproject;

public class Projectile
{
    public Vector2 position;
    private Vector2 velocity;
    private Texture2D texture;
    private float scale = 2.5f;
    private int width = 4;
    private int height = 15;

    public Projectile(Texture2D texture, Vector2 position, Vector2 velocity)
    {
        this.texture = texture;
        this.position = position;
        this.velocity = velocity;
    }

    public void Update(GameTime time)
    {
        position += velocity;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(
            texture,
            new Rectangle((int)position.X, (int)position.Y, width, height),
            Color.Yellow
        );
    }

    public Rectangle GetBounds()
    {
        return new Rectangle((int)position.X, (int)position.Y, width, height);
    }

    public bool IsOffScreen(int height)
    {
        return position.Y < -20 || position.Y > height + 20;
    }
}
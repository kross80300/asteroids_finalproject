namespace asteroids_finalproject;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


public class Shield
{
    
    Texture2D texture;
    public Vector2 position;
    private Color color;

    public Shield(Vector2 pos, Texture2D tex)
    {
        position = pos;
        texture = tex;
    }

    public void Update(float ptimer)
    {
        color = Color.LightGoldenrodYellow * 0.7f;
        {
            if (ptimer < 5f && ptimer >= 3f)
            {
                if (ptimer % 1f < 0.5f)
                {
                    color *= 0.4f;
                }
            }
            else if (ptimer >= 5.5f)
            {
                if (ptimer % 0.5f < 0.25f)
                {
                    color *= .4f;
                }
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch, Spaceship spaceship)
    {
        spriteBatch.Draw(
            texture, 
            new Vector2(spaceship.position.X + spaceship.frame / 2, spaceship.position.Y + spaceship.frame / 2), 
            null, 
            color, 
            0f, 
            new Vector2(texture.Width / 2, texture.Height / 2), 
            0.75f, 
            SpriteEffects.None, 
            0f);

        spriteBatch.Draw(
            texture, 
            new Vector2(spaceship.position.X + spaceship.frame / 2, spaceship.position.Y + spaceship.frame / 2), 
            null, 
            Color.Black * 0.7f, 
            0f, 
            new Vector2(texture.Width / 2, texture.Height / 2), 
            0.67f, 
            SpriteEffects.None, 
            0f);
    }




}

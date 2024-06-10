using FinalFantasyV.Content;
using FinalFantasyV.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FinalFantasyV.GameStates.Menus;

public class MagicMenu : MenuState
{
    public MagicMenu(ContentManager cm) : base(cm)
    {
        menuSelectors = new MenuSelector[] { };
    }

    public override void OnEnter(PartyState ps, object _)
    {
        Menu.SetBox(tileData, 1, 1, 7, 3);
        Menu.SetBox(tileData, 8, 1, 23, 3);
        Menu.SetBox(tileData, 1, 4, 30, 5);
        Menu.SetBox(tileData, 1, 1, 7, 6, 1);
        Menu.SetBox(tileData, 1, 7, 7, 8, 1);
        
        
        //Menu.SetBox(tileData, 1, 9, 30, 14);
        //Menu.SetBox(tileData, 0, 3, 32, 4);
        //Menu.SetBox(tileData, 0, 7, 32, 30-7);
    }

    public override void Render(SpriteBatch spriteBatch, PartyState ps)
    {
        base.Render(spriteBatch, ps);
        spriteBatch.Begin();
        var h = ps.Slots[slotIndex];
        Menu.DrawManyString(spriteBatch, menuSpritesheet, 
            new []{$"{PartyState.GetName(h.Hero)}   LV{Menu.PadNumber(h.Level,2)}", 
                $"HP {Menu.PadNumber(h.CurrHP, 4)}/{Menu.PadNumber(h.MaxHP(), 4)}", 
                $"MP {Menu.PadNumber(h.CurrMP, 4)}/{Menu.PadNumber(h.MaxMP(), 4)}"}, new Vector2(8*6, 8*5), 8);
        
        Menu.DrawManyString(spriteBatch, menuSpritesheet, new []{"White","Black","Effct"}, new Vector2(8*2,8*2), 14);
        Menu.DrawManyString(spriteBatch, menuSpritesheet, new []{"Call","Sword","Blue", "Song"}, new Vector2(8*2,8*8), 14);
        spriteBatch.End();
        

    }
}
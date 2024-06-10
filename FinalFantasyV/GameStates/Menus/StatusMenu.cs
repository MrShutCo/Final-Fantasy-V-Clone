using System.Linq;
using FinalFantasyV.Content;
using FinalFantasyV.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FinalFantasyV.GameStates.Menus
{
	public class StatusMenu : MenuState
	{
        public StatusMenu(ContentManager cm) : base(cm)
        {
            menuSelectors = new MenuSelector[] { };
        }


        public override void OnEnter(PartyState ps, object _)
        {
            Menu.SetBox(tileData, 0, 1, 32, 28);
            Menu.SetBox(tileData, 25, 0, 7, 3);
            Menu.DrawText(tileData, 26, 1, "Stats");

            var hero = ps.Slots[slotIndex];

            Menu.DrawText(tileData, 2, 3, PartyState.GetName(hero.Hero));
            Menu.DrawText(tileData, 10, 3, $"LV{Menu.PadNumber(hero.Level, 3)}");

            Menu.DrawText(tileData, 2, 5, "Normal"); // TODO fix
            Menu.DrawText(tileData, 3, 10, $"HP{Menu.PadNumber(hero.CurrHP, 4)}/{Menu.PadNumber(hero.MaxHP(), 4)}");
            Menu.DrawText(tileData, 3, 11, $"MP{Menu.PadNumber(hero.CurrMP, 4)}/{Menu.PadNumber(hero.MaxMP(), 4)}");

            Menu.DrawText(tileData, 3, 13, "Experience");
            Menu.DrawText(tileData, 5, 14, Menu.PadNumber(hero.Exp, 10));

            Menu.DrawText(tileData, 3, 16, "Next level");
            //Menu.DrawText(tileData, 13, 3, Menu.PadNumber(hero.Ne, 10)); // TODO

            Menu.DrawText(tileData, 3, 19, "Abilities");
            // TODO

        }

        public override void Render(SpriteBatch spriteBatch, PartyState ps)
        {
            var h = ps.Slots[slotIndex];
            base.Render(spriteBatch, ps);
            spriteBatch.Begin();
            Menu.DrawManyString(spriteBatch, menuSpritesheet, new []{"Strgth.", "Agility", "Vitality", "Mag.Pwr", "Attack", "Defense", "Evade %", "Mag.Def", "EqpWgt"}, new Vector2(8*16,8*14-4),14);

            var stats = new[]
            {
                h.Strength, h.Agility, h.Vitality, h.MagicPower, h.Attack, h.Defense, h.Evade, h.MagicDefense, h.Weight
            }.Select(s => ":" + Menu.PadNumber(s, 3)).ToArray();
            stats[^3] = ":" + Menu.PadNumber(h.Evade, 2) + "%";
            Menu.DrawManyString(spriteBatch, menuSpritesheet, stats, new Vector2(8 * 27, 8 * 14 - 4), 14);
            spriteBatch.End();
        }

        public override void Update(GameTime gameTime, PartyState ps)
        {
            if (InputHandler.KeyPressed(Keys.Back)) stateStack.Pop();
            base.Update(gameTime, ps);
        }
    }
}


using System.Collections.Generic;
using FinalFantasyV.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FinalFantasyV.GameStates
{
    public delegate void BattleCallback();

    public class BattleState : IState
    {

        enum EBattleState
        {
            TimeFlowing,
            UnitActing,
            Victory,
            GameOver
        }

        public StateStack stateStack { get; set; }

        TilesetInfo tilesetInfo;
        Map tileData;
        List<BattleUnit> units;
        SpriteSheet background;
        InputHandler input;
        //MenuSelector menuSelector;

        EBattleState battleState;

        public event BattleCallback Ended;

        SpriteSheet[] heros;

        Menu menu;

        BattleUnit actingUnit;
        const int ATBPerSecond = 50;

        public BattleState(ContentManager cm)
        {
            var bartzTex = cm.Load<Texture2D>("Bartz");
            var lennaTex = cm.Load<Texture2D>("Lenna");
            var galufTex = cm.Load<Texture2D>("Galuf");
            var farisTex = cm.Load<Texture2D>("Faris");
            var menuTex = cm.Load<Texture2D>("fontmenu");
            tileData = MapIO.ReadMap("Tilemaps/battle.tmj");
            tilesetInfo = MapIO.ReadTileSet(cm, "Tilemaps/Menu.tsj");

            var backgroundTex = cm.Load<Texture2D>("backgrounds");
            background = new SpriteSheet(backgroundTex, 240, 160, new Vector2(2,2), new Vector2(4, 6));
            background.SetTile(1, 0);
            battleState = EBattleState.TimeFlowing;

            menu = new();

            input = new InputHandler();
            //menuSelector = new MenuSelector(new Vector2[] { new(16*10, 16*10), new(160, 16*11)},
            //    new Action[] { });

            heros = new[]
            {
                new SpriteSheet(bartzTex, 30, 30, Vector2.Zero, Vector2.Zero),
                new SpriteSheet(lennaTex, 30, 30, Vector2.Zero, Vector2.Zero),
                new SpriteSheet(galufTex, 30, 30, Vector2.Zero, Vector2.Zero),
                new SpriteSheet(farisTex, 30, 30, Vector2.Zero, Vector2.Zero)
            };
            
        }

        public void OnEnter(PartyState ps)
        {
            tileData.SetLayerVisible(0, true);
            tileData.SetLayerVisible(1, false);
            Ended += ActionFinished;
            units = new List<BattleUnit>();
            units.Add(new BattleHero(heros[0], ps.Slots[0], new Vector2(16*13, 16*5)));
            units.Add(new BattleHero(heros[1], ps.Slots[1], new Vector2(16 * 13, 16 * 7)));
            units.Add(new BattleHero(heros[2], ps.Slots[2], new Vector2(16 * 13, 16 * 9)));
            units.Add(new BattleHero(heros[3], ps.Slots[3], new Vector2(16 * 13, 16 * 11)));
        }

        private void ActionFinished()
        {
            battleState = EBattleState.TimeFlowing;
            actingUnit.OnActionFinished -= ActionFinished;
            actingUnit = null;

            units = new();


        }

        public void OnExit()
        {
        }

        public void Render(SpriteBatch spriteBatch, PartyState ps)
        {
            spriteBatch.Begin();

            tileData.DrawTileSet(spriteBatch, tilesetInfo);
            background.Draw(spriteBatch, new Vector2(8, 0));
            //units[0].Draw(spriteBatch, ps.Bartz, new Vector2(16 * 13, 26*2));
            //units[1].Draw(spriteBatch, ps.Lenna, new Vector2(16 * 13, 26*3));
            //units[2].Draw(spriteBatch, ps.Galuf, new Vector2(16 * 13, 26*4));
            //units[3].Draw(spriteBatch, ps.Faris, new Vector2(16 * 13, 26*5));
            foreach (var unit in units)
            {
                unit.Draw(spriteBatch);
            }

            for (int i = 0; i < 3; i++)
            {
                DrawCharacterHealth(i, ps.Slots[i].CurrHP);
                Menu.DrawString(spriteBatch, background, PartyState.GetName(ps.Slots[i].Hero), new Vector2(8 * 13, 8*(22+i*2)));
            }

            spriteBatch.End();
        }

        void DrawCharacterHealth(int slot, int health)
        {
            int dig1 = health % 10;
            int dig2 = (health / 10) % 10;
            int dig3 = (health / 100) % 10;
            int dig4 = (health / 1000) % 10;
            tileData.SetTileAt(0, 23, 22+slot*2, 52+dig1);
            tileData.SetTileAt(0, 22, 22 + slot * 2, 52 + dig2);
            tileData.SetTileAt(0, 21, 22 + slot * 2, 52 + dig3);
            tileData.SetTileAt(0, 20, 22 + slot * 2, 52 + dig4);
        }

        void DrawCharacterSelection(int slot)
        {
        }

        public void Update(GameTime gameTime, PartyState ps)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Back)) stateStack.Pop();

            switch (battleState)
            {
                case EBattleState.TimeFlowing:
                    foreach (var unit in units)
                    {
                        unit.AdvanceATB(gameTime);
                        if (unit.ATB == 255)
                        {
                            unit.BeginAction();
                            battleState = EBattleState.UnitActing;
                            actingUnit = unit;
                            actingUnit.OnActionFinished += ActionFinished;
                            break;
                        }
                    }

                    break;
                case EBattleState.UnitActing:
                    actingUnit.Update(gameTime, ps);
                    break;
            }
        }
    }
}


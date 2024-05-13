﻿using System;
using Final_Fantasy_V.Models;
using FinalFantasyV.GameStates;
using Microsoft.Xna.Framework;

namespace FinalFantasyV.Sprites
{
	public class BattleEnemy : BattleUnit
	{
		public BattleEnemy(SpriteSheet enemySheet, Enemy enemy, Vector2 position) :base(enemySheet, enemy, position)
		{
		}

        public override void BeginAction()
        {
            Console.WriteLine($"Enemy {Unit} is acting");
        }


        public override void Update(GameTime gameTime, PartyState ps)
        {
            
        }
    }
}


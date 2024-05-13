// See https://aka.ms/new-console-template for more information
using Final_Fantasy_V.Models;
using Final_Fantasy_V.Algorithms;
using Engine.RomReader;
using Engine;
using System.Collections;
using System.Text;

Console.WriteLine("Hello, World!");

var butz = new Character(Hero.Butz);
var lenna = new Character(Hero.Lenna);
var galuf = new Character(Hero.Galuf);
var faris = new Character(Hero.Faris);
Console.WriteLine(butz.MaxHP());
Console.WriteLine(lenna.MaxHP());
Console.WriteLine(galuf.MaxHP());
Console.WriteLine(faris.MaxHP());

var goblin = Utility.Enemies["Goblin"];

butz.LeftHand = Utility.Weapons["Broad Sword"];
butz.Body = Utility.BodyWear["Leather"];

var dmg = Attacks.SwordsAttack(butz, goblin, Command.Fight);
Console.WriteLine(dmg);


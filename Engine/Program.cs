// See https://aka.ms/new-console-template for more information
using Final_Fantasy_V.Models;
using Final_Fantasy_V.Algorithms;
using Engine.RomReader;
using Engine;
using System.Collections;
using System.Text;

Console.WriteLine("Hello, World!");

/*var butz = new Character(Hero.Butz);
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
*/
static string BinaryToHex(string binary)
{
    // Ensure the length of the binary string is a multiple of 4
    int remainder = binary.Length % 4;
    if (remainder != 0)
    {
        binary = new string('0', 4 - remainder) + binary;
    }

    StringBuilder hex = new StringBuilder(binary.Length / 4);

    
    // Process each 4-bit chunk
    for (int i = 0; i < binary.Length; i += 4)
    {
        string fourBitChunk = binary.Substring(i, 4);
        hex.Append(Convert.ToString(Convert.ToByte(fourBitChunk, 2), 16).ToUpper());
    }

    return hex.ToString();
}

HashSet<int> indices = [8, 9, 25, 26, 32, 40, 50, 51, 74, 86, 87, ];
string binary = "";
for (int i = 0; i < 80; i++)
{
    binary += indices.Contains(i) ? '1' : '0';
}

Console.WriteLine(BinaryToHex(binary));



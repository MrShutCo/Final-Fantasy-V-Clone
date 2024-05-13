using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Final_Fantasy_V.Models;
namespace Engine
{
	public class Utility
	{
		static readonly JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true, };

        public static readonly Dictionary<string, Job>? Jobs;
        public static readonly Dictionary<string, Enemy>? Enemies;

        public static Level GetLevel(int level) => levels[level - 1];
		static readonly Level[]? levels;

        public static readonly Dictionary<string, Weapon>? Weapons;
			

        public static readonly Dictionary<string, Body>? BodyWear;

        public static readonly Dictionary<string, Head>? HeadGear;

        public static readonly Dictionary<string, Accessory>? Accesories;

        static Utility()
        {

            levels = JsonSerializer.Deserialize<Level[]>(File.ReadAllText("Data/levels.json"), options);

            Enemies = JsonSerializer.Deserialize<Dictionary<string, Enemy>>(File.ReadAllText("Data/enemies.json"), options);
            foreach (var item in Enemies)
                item.Value.Name = item.Key;

            Jobs = JsonSerializer.Deserialize<Dictionary<string, Job>>(File.ReadAllText("Data/jobs.json"), options);
            foreach (var item in Jobs)
                item.Value.Name = item.Key;

            Weapons = JsonSerializer.Deserialize<Dictionary<string, Weapon>>(File.ReadAllText("Data/weapons.json"), options);
            foreach (var item in Weapons)
                item.Value.Name = item.Key;
            Weapons.Add("Empty", new Weapon());

            BodyWear = JsonSerializer.Deserialize<Dictionary<string, Body>>(File.ReadAllText("Data/bodywear.json"), options);
            foreach (var item in BodyWear)
                item.Value.Name = item.Key;
            BodyWear.Add("None", new Body());

            HeadGear = JsonSerializer.Deserialize<Dictionary<string, Head>>(File.ReadAllText("Data/headgear.json"), options);
            foreach (var item in HeadGear)
                item.Value.Name = item.Key;
            HeadGear.Add("None", new Head());

            Accesories = JsonSerializer.Deserialize<Dictionary<string, Accessory>>(File.ReadAllText("Data/accesories.json"), options);

            foreach (var item in Accesories)
                item.Value.Name = item.Key;
            Accesories.Add("None", new Accessory());
        }

    }
}


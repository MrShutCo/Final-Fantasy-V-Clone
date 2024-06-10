using Engine.RomReader;

namespace Final_Fantasy_V.Models;

public class MonsterAI
{
    private List<byte> _aiData;
    public List<string> AiText;
    
    public MonsterAI(List<byte> aiData)
    {
        _aiData = aiData;
        AiText = [];
        
        for (int i = 0; i < aiData.Count; i++)
        {
            bool hasTab = true;
            var action = aiData[i];
            // Top level condition checks
            if (action == 0x00)
            {
                AiText.Add("Normal:");
                i += 3;
                hasTab = false;
            }
            else if (action == 0x06) // React
            {
                if (_aiData[i+2] == 0x04) AiText.Add("React: Fight");
                if (_aiData[i+1] == 0x01 && aiData[i+2] == 0x2B) AiText.Add("React: Physical");
                if (_aiData[i+2] == 0x2B && aiData[i+3] == 0x00) AiText.Add("React: Magic");
                if (_aiData[i+2] == 0x2B && aiData[i+3] == 0x01) AiText.Add("React: Elemental Attack: fire");
                i += 3;
            }
            else if (action == 0x03)  // Condition VXX == YY
            {
                AiText.Add($"If: V{aiData[i + 2]} = {aiData[i + 3]}");
                i += 3;
            } 
            else if (action == 0x02) // Condition HP < XXXXX
            {
                var hp = aiData[i + 2] + aiData[i + 3] * 0x100;
                AiText.Add($"Condition: HP < {hp}");
                i += 3;
            }
            else if (action == 0x08)
            {
                AiText.Add($"React: Magic: {GetAttackName(_aiData[i+2])}");
                i += 3;
            }
            else if (action == 0x09)
            {
                AiText.Add($"React: Item: 0x{_aiData[i+2].ToString("X2")}");
                i += 3;
            }
            else if (action == 0x0E) //React:HP Damage
            {
                AiText.Add("React: HP Damage");
                i += 3;
            }
            else if (action == 0x0F)
            {
                AiText.Add("React: Death");
                i += 3;
            }
            
            // We are parsing out actual commands
            else if (action == 0xFE)
            {
                i++;
                while (aiData[i] != 0xFF && aiData[i] != 0xFE)
                {
                    if (aiData[i] == 0xFD)
                    {
                        // We have a special command
                        if (aiData[i + 1] >= 0xF0)
                        {
                            //Console.WriteLine("Special Command");
                            if (_aiData[i + 1] == 0xF2)
                            {
                                AddLine("Unhide Monster", hasTab);
                                i += 3;
                            }

                            if (_aiData[i + 1] == 0xF6)
                            {
                                AddLine("Show Text", hasTab);
                                i += 3;
                            }

                            if (_aiData[i + 1] == 0xF7)// No interrupt
                            {
                                AddLine("No Interrupt", hasTab);
                                i += 3;
                            }
                            //i += 4;
                        }
                        // Do one of 3 actions
                        else
                        {
                            AddLine("{" + $"{GetAttackName(aiData[i+1])},{GetAttackName(aiData[i+2])},{GetAttackName(aiData[i+3])}" + "}", hasTab);
                            i += 3;
                        }
                    }
                    // Do actions sequentially
                    else
                    {
                        AddLine(GetAttackName(aiData[i]), hasTab);
                    }

                    i++;
                }
            }
        }
    }

    void AddLine(string s, bool hasTab)
    {
        if (hasTab) s = "  " + s;
        AiText.Add(s);
    }
    
    string GetAttackName(byte id)
    {
        var b = _attackNames.TryGetValue(id, out string atk);
        return b ? atk : "0x" + id.ToString("X2");
    }
    
    Dictionary<byte, string> _attackNames = new Dictionary<byte, string>
        {
            {0x12, "Cure"},
            {0x13, "Libra"},
            {0x14, "Poisona"},
            {0x15, "Silence"},
            {0x16, "Protect"},
            {0x17, "Mini"},
            {0x18, "Cura"},
            {0x19, "Raise"},
            {0x1A, "Confuse"},
            {0x1B, "Blink"},
            {0x1C, "Shell"},
            {0x1D, "Esuna"},
            {0x1E, "Curaga"},
            {0x1F, "Reflect"},
            {0x20, "Berserk"},
            {0x21, "Arise"},
            {0x22, "Holy"},
            {0x23, "Dispel"},
            {0x24, "Fire"},
            {0x25, "Blizzard"},
            {0x26, "Thunder"},
            {0x27, "Poison"},
            {0x28, "Sleep"},
            {0x29, "Toad"},
            {0x2A, "Fira"},
            {0x2B, "Blizara"},
            {0x2C, "Thundara"},
            {0x2D, "Drain"},
            {0x2E, "Break"},
            {0x2F, "Bio"},
            {0x30, "Firaga"},
            {0x31, "Bilzzaga"},
            {0x32, "Thundaga"},
            {0x33, "Flare"},
            {0x34, "Death"},
            {0x35, "Osmose"},
            {0x36, "Speed"},
            {0x37, "Slow"},
            {0x38, "Regen"},
            {0x39, "Mute"},
            {0x3A, "Haste"},
            {0x3B, "Float"},
            {0x3C, "Gravity"},
            {0x3D, "Stop"},
            {0x3E, "Teleport"},
            {0x3F, "Comet"},
            {0x40, "Slowga"},
            {0x41, "Return"},
            {0x42, "Graviga"},
            {0x43, "Hastega"},
            {0x44, "Old"},
            {0x45, "Meteor"},
            {0x46, "Quick"},
            {0x47, "Banish"},
            {0x48, "Choco Kick"},
            {0x49, "Slypa"},
            {0x4A, "Constrict"},
            {0x4B, "Diamond Dust"},
            {0x4C, "Judgment Bolt"},
            {0x4D, "Hellfire"},
            {0x4E, "Gaia's Wrath"},
            {0x4F, "Earthen wall"},
            {0x50, "Demon Eye"},
            {0x51, "Ruby Light"},
            {0x52, "Item"},
            {0x53, "Grungier"},
            {0x54, "Phoenix"},
            {0x55, "Tsunami"},
            {0x56, "Mega flare"},
            {0x57, "Sinewy Etude"},
            {0x58, "Swift Song"},
            {0x59, "Mighty March"},
            {0x5A, "Mana's Paean"},
            {0x5B, "Hero's Rime"},
            {0x5C, "Requiem"},
            {0x5D, "Romeo's Ballad"},
            {0x5E, "Alluring Air"},
            {0x5F, "Chocobo Kick"},
            {0x70, "Flames of rebirth"},
            {0x71, "Drain spear"},
            {0x72, "Osmose Lance"},
            {0x73, "Egg chop"},
            {0x74, "Silver Harp"},
            {0x75, "Dream Harp"},
            {0x76, "Lamia's harp"},
            {0x77, "Apollo's Harp"},
            {0x78, "Dummy attack"},
            {0x79, "Mystery Waltz"},
            {0x7A, "Jitterbug"},
            {0x7B, "Tempting Tango"},
            {0x7C, "Magic shell"},
            {0x7D, "Sword Dance"},
            {0x7E, "Ice Aura"},
            {0x7F, "Entangle"},
            {0x80, "Fight"},
            {0x81, "Critical"},
            {0x82, "Doom"},
            {0x83, "Roulette"},
            {0x84, "Aqua Breath"},
            {0x85, "Level 5 Death"},
            {0x86, "Level 4 Gravia"},
            {0x87, "Level 2 old"},
            {0x88, "Level 3 Flare"},
            {0x89, "Pond's chorus"},
            {0x8A, "Lilliputain lyric"},
            {0x8B, "Flash"},
            {0x8C, "TimeSlip"},
            {0x8D, "Moon Flute"},
            {0x8E, "Death Claw"},
            {0x8F, "Aero"},
            {0x90, "Aera"},
            {0x91, "Aeroga"},
            {0x92, "Flame Thrower"},
            {0x93, "Goblin Punch"},
            {0x94, "Dark Spark"},
            {0x95, "Off Guard"},
            {0x96, "Transfusion"},
            {0x97, "Mind Blast"},
            {0x98, "Vampire"},
            {0x99, "Magic Hammer"},
            {0x9A, "Mighty Guard"},
            {0x9B, "Self destruct"},
            {0x9C, "???"},
            {0x9D, "1000 Needles"},
            {0x9E, "White Wind"},
            {0x9F, "Missile"},
            {0xA0, "Ribbit"},
            {0xA1, "????"},
            {0xA2, "Flee"},
            {0xA3, "Bugged self kill/freeze"},  // Modified due to lack of specific key handling in C#
            {0xA4, "Bugged self kill/freeze"},  // Modified due to lack of specific key handling in C#
            {0xA5, "Bugged self kill/freeze"},  // Modified due to lack of specific key handling in C#
            {0xA6, "Grand Cross"},
            {0xA7, "Delta Attack"},
            {0xA8, "Interceptor"},
            {0xA9, "Barrier Change"},
            {0xAA, "Nothing"},
            {0xAB, "Wind Slash"},
            {0xAC, "Skip Slot"},
            {0xAD, "Search"},
            {0xAE, "100G's"},
            {0xAF, "Vanish"},
            {0xB0, "Reaper's Sword"},
            {0xB1, "Destruct"},
            {0xB2, "Blaster"},
            {0xB3, "Beak"},
            {0xB4, "Embrace"},
            {0xB5, "Spore"},
            {0xB6, "Poison breath"},
            {0xB7, "Dance Mercaba"},
            {0xB8, "Zombie powder"},
            {0xB9, "Zombie breath"},
            {0xBA, "Paraclet"},
            {0xBB, "Entice"},
            {0xBC, "Entangle"},
            {0xBD, "Rainbow wind"},
            {0xBE, "Dazzling Daze"},
            {0xBF, "Gamma Ray"},
            {0xC0, "White Hole"},
            {0xC1, "Needle"},
            {0xC2, "Maelstrom"},
            {0xC3, "Special/Bone"},
            {0xC4, "Tail Screw"},
            {0xC5, "Digestive Acid"},
            {0xC6, "Rocket Punch"},
            {0xC7, "Mustard Bomb"},
            {0xC8, "Algamast"},
            {0xC9, "Quicksand"},
            {0xCA, "Atomic Ray"},
            {0xCB, "Frost Bite"},
            {0xCC, "Ice Strom"},
            {0xCD, "Frost"},
            {0xCE, "Electrocute"},
            {0xCF, "Earth Shaker"},
            {0xD0, "Zentegusi"},
            {0xD1, "Tidal Wave"},
            {0xD2, "Mega flare"},
            {0xD3, "Discord"},
            {0xD4, "Web"},
            {0xD5, "Slimer"},
            {0xD6, "Earthquake"},
            {0xD7, "OHKO"},
            {0xD8, "Pancea"},
            {0xD9, "Image"},
            {0xDA, "BreathWing"},
            {0xDB, "Blaze"},
            {0xDC, "Lightning"},
            {0xDD, "Wave Cannon"},
            {0xDE, "Physical?"},
            {0xDF, "????"},
            {0xE0, "Rocket (causes Old)"},
            {0xE1, "Giga Flare"},
            {0xE2, "Encircle"},
            {0xE3, "Worm hole"},
            {0xE4, "Posses"},
            {0xE5, "Reverse Polarity"},
            {0xE6, "Magnet"},
            {0xE7, "????"},
            {0xE8, "Jump"},
            {0xE9, "Banish (Used on Gilgemesh)"},
            {0xEA, "Hurricane"},
            {0xEB, "Evil Eye"},
            {0xEC, "Bug delete unwinnable?"},  // Modified due to lack of specific key handling in C#
            {0xED, "Bug delete unwinnable?"},  // Modified due to lack of specific key handling in C#
            {0xEE, "Bug delete unwinnable?"},  // Modified due to lack of specific key handling in C#
            {0xEF, "Battle ends"},
            {0xF0, "Item"},
            {0xF1, "Item"},
            {0xF2, "Item"},
            {0xF3, "Drain Touch"},
            {0xF4, "Dark Haze"},
            {0xF5, "Deep Freeze"},
            {0xF6, "Evil Mist"},
            {0xF7, "Meltdown"},
            {0xF8, "Hell Wind"},
            {0xF9, "Chaos Drive"},
            {0xFA, "Curse"},
            {0xFB, "Dark Flare"}
        };
}
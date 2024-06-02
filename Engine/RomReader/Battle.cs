using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.RomReader;

public class MapBattles
{
    
}

public class BattleGroup
{
    private List<byte> _monsters;
    private byte _visibiltyMask;
    private byte _palette1;
    private byte _palette2;
    private ushort _music;

    public List<Vector2> MonsterPositions;

    private List<Monster> _monsterSprites;
    
    public BattleGroup(byte[] battleData)
    {
        _monsters = battleData[4..0xB].ToList();
        _visibiltyMask = battleData[3];
        _palette1 = battleData[0xC];
        _palette1 = battleData[0xD];
        _music = (ushort)(battleData[0xE] + battleData[0xF] * 0x100);
        MonsterPositions = [];
        _monsterSprites = [];
    }

    public void LoadMonsterPositions(byte[] posData)
    {
        //Console.WriteLine(posData);
        for (int i = 0; i < posData.Length; i++)
        {
            int y = posData[i] & 0x0F;
            int x = (posData[i] & 0xF0) >> 4;
            MonsterPositions.Add(new Vector2(x, y) * 8);
        }
    }

    public void LoadMonsterData(GraphicsDevice gd, RomGame rom)
    {
        for (int i = 0; i < _monsters.Count; i++)
        {
            if (_monsters[i] != 0xFF)
            {
                _monsterSprites.Add(rom.GetMonster(gd, _monsters[i]));
            }
        }
    }
    
    public void Draw(SpriteBatch sb)
    {
        for (int i = 0; i < _monsterSprites.Count; i++)
        {
            if (((_visibiltyMask >> (7-i)) & 1) == 1)
                _monsterSprites[i].Draw(sb, MonsterPositions[i]);
        }
    }
    
}
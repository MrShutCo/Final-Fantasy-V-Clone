using Engine.RomReader;
using Microsoft.Xna.Framework;

namespace FinalFantasyV.Sprites;

public class WorldNPC : WorldCharacter
{
    private NPC _npcData;

    private int _movementType1;
    private int _movementType2;
    private bool _isStationary;
    private int _speed;

    private Timer waitTimer;
    
    public WorldNPC(SpriteSheet spriteSheet, Vector2 pos, NPC npcData) : base(spriteSheet, pos)
    {
        _npcData = npcData;
        _movementType1 = (_npcData.x & 0xC0) >> 6;
        _movementType2 = (_npcData.y & 0xC0) >> 6;
        _isStationary = (npcData.walkingParam & 0x4) >> 2 != 0;
        var speedCategory = npcData.walkingParam & 0x03;
        _speed = speedCategory switch
        {
            0 => NormalWalkingSpeed,
            1 => SlowWalkingSpeed,
            2 => SlowestWalkingSpeed,
            3 => FastWalkingSpeed,
            _ => NormalWalkingSpeed
        };
        waitTimer = new Timer(2*_speed);
        
    }

    public override void Update(GameTime gt)
    {
        waitTimer.Update(gt);
        if (_movementType2 == 0 && !_isStationary)
        {
            if (!IsMoving && waitTimer.IsDone)
            {
                var move = (ECharacterMove)r.Next(3);
                //if (CanWalkHere(move, ))
                //Move(move, _speed);
                waitTimer.Reset();
            }
        }
        base.Update(gt);
    }
}
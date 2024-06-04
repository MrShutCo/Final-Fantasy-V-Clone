using Engine.RomReader;
using FinalFantasyV.Utilities;
using Microsoft.Xna.Framework;

namespace FinalFantasyV.Sprites;

public class WorldNPC : WorldCharacter
{
    private NPC _npcData;

    private int _movementType1;
    private int _movementType2;
    private bool _isStationary;
    private int _animationType;
    public int Speed { get; private set; }
    private ECharacterMove _originalDirection;

    private Timer waitTimer;
    
    public WorldNPC(SpriteSheet spriteSheet, Vector2 pos, NPC npcData) : base(spriteSheet, pos)
    {
        _npcData = npcData;
        _movementType1 = (_npcData.x & 0xC0) >> 6;
        _movementType2 = (_npcData.y & 0xC0) >> 6;
        _isStationary = (npcData.walkingParam & 0x4) >> 2 != 0;
        var speedCategory = npcData.walkingParam & 0x03;
        Speed = speedCategory switch
        {
            0 => NormalWalkingSpeed,
            1 => SlowWalkingSpeed,
            2 => SlowestWalkingSpeed,
            3 => FastWalkingSpeed,
            _ => NormalWalkingSpeed
        };

        _originalDirection = (ECharacterMove)npcData.direction;
        
        waitTimer = new Timer(2*Speed);
        if (_isStationary)
        {
            _animationType = (npcData.walkingParam & 0x70) >> 4;
            var hasExtraFrame = ((npcData.walkingParam & 0b0100) >> 2) == 1;
            if (_animationType == 0 && _npcData.actionId != 128)
            {
                Face(_originalDirection);
                VisualAnimation = new Animation([new Vector2(Character.X, Character.Y), new Vector2(Character.X+1, Character.Y)], [Speed, Speed], true);
            }
                
            else if (_animationType == 1)
                VisualAnimation = new Animation([new Vector2(Character.X, Character.Y)], [1.0f], true);
            else if (_animationType == 2)
            {
                VisualAnimation = new Animation([new Vector2(Character.X, Character.Y), new Vector2(Character.X+1, Character.Y)], [Speed, Speed], true);
            }
            else if (_animationType == 3)
            {
                // TODO: this is a hack for dancers
                VisualAnimation = new Animation([
                    (new (0, 0), false),
                    (new (2, 0), true),
                    (new (4,0), false),
                    (new (5, 0), true)], [Speed/2, Speed/2, Speed/2, Speed/2], true);
            }

            if (hasExtraFrame)
            {
                
            }
            VisualAnimation?.StartAnimation();
        }
    }

    public override void Move(ECharacterMove move, int milliseconds, bool playAnimation=true)
    {
        base.Move(move, milliseconds, _animationType == 0);
    }
    
    public string GetDialogue()
    {
        return _npcData.dialogues[0].Item1;
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
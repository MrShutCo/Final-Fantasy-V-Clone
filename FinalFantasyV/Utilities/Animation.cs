using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace FinalFantasyV.Utilities;

public class Animation
{
    private List<(Vector2, bool isFlipped)> _frames;
    private List<float> _timeInFrame;

    private float _timePassed;
    private int _activeIndex;

    public bool IsActive;
    public bool IsRepeating { get; private set; }
    public bool IsLerpingFrames;
    
    public Animation(List<Vector2> frames, List<float> timeInFrame, bool repeats)
    {
        _frames = [];
        foreach (var t in frames)
        {
            _frames.Add((t, false));
        }
        _timeInFrame = timeInFrame;
        IsRepeating = repeats;
    }
    
    public Animation(List<(Vector2, bool)> frames, List<float> timeInFrame, bool repeats)
    {
        _frames = frames;
        _timeInFrame = timeInFrame;
        IsRepeating = repeats;
    }

    public void StartAnimation() => IsActive = true;
    public void StopAnimation() => IsActive = false;

    public void Update(GameTime gt)
    {
        if (!IsActive) return;

        _timePassed += gt.ElapsedGameTime.Milliseconds;
        if (_timePassed >= _timeInFrame[_activeIndex])
        {
            _activeIndex++;
            _timePassed = 0;
            if (_activeIndex == _timeInFrame.Count)
            {
                if (IsRepeating) _activeIndex = 0;
                else
                {
                    IsActive = false;
                    _activeIndex = _frames.Count - 1;
                }
            }
        }
    }

    public (Vector2, bool isFlipped) GetAnimationFrame()
    {
        return _frames[_activeIndex];
    }
}
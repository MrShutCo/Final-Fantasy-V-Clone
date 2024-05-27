using System;
using Microsoft.Xna.Framework;

namespace FinalFantasyV;

public class Timer
{
    public bool IsDone;
    
    double _timeMilliseconds;
    double _timeElapsed;

    public Timer(double timeMilliseconds)
    {
        _timeMilliseconds = timeMilliseconds;
        _timeElapsed = 0;
    }

    public void Update(GameTime gt)
    {
        _timeElapsed += gt.ElapsedGameTime.Milliseconds;
        _timeElapsed = Math.Min(_timeElapsed, _timeMilliseconds);
        if (_timeElapsed >= _timeMilliseconds)
        {
            IsDone = true;
        }
    }

    public void Reset()
    {
        _timeElapsed = 0;
        IsDone = false;
    }
}
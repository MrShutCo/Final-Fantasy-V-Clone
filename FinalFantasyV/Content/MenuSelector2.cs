using FinalFantasyV.GameStates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FinalFantasyV.Content;

public class MenuScreen
{
    private MenuSelector _mainSelector;
    private MenuSelector _subSelector;

    private bool _isMainSelector;
    
    public MenuScreen(MenuSelector mainSelector, MenuSelector subSelector)
    {
        _mainSelector = mainSelector;
        _subSelector = subSelector;
    }

    public void Update(GameTime gt, PartyState ps)
    {
        if (InputHandler.KeyPressed(Keys.Up))
        {
            if (_isMainSelector) _mainSelector.MoveCursorUp();
            else _mainSelector.MoveCursorUp();
        }
        if (InputHandler.KeyPressed(Keys.Down))
        {
            if (_isMainSelector) _mainSelector.MoveCursorDown();
            else _mainSelector.MoveCursorDown();
        }
        if (InputHandler.KeyPressed(Keys.Left))
        {
            if (_isMainSelector) _mainSelector.MoveCursorLeft();
            else _mainSelector.MoveCursorLeft();
        }
        if (InputHandler.KeyPressed(Keys.Right))
        {
            if (_isMainSelector) _mainSelector.MoveCursorRight();
            else _mainSelector.MoveCursorRight();
        }

        if (_isMainSelector && InputHandler.KeyPressed(Keys.Enter))
        {
            
        }
    }
    
}
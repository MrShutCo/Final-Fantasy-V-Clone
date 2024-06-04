using System;
using System.Collections.Generic;
using FinalFantasyV.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FinalFantasyV.Menus;

public class TextPopup
{
    private Menu _menu;
    private TilesetInfo _tilesetInfo;
    private Map tileData;
    public bool IsActive;

    private SpriteSheet menuSpritesheet;
    private List<string> text;

    private int _currentTextIndex;
    
    public TextPopup()
    {
        tileData = MapIO.ReadMap("Tilemaps/mainmenu.tmj");
        menuSpritesheet = new SpriteSheet(FF5.MenuTexture, 8, 8, Vector2.Zero, Vector2.Zero);
        _tilesetInfo = FF5.MenuTilesetInfo;
        Menu.SetBox(tileData, 1, 1, 30, 10);
        _currentTextIndex = 0;
    }
    
    public void ShowText(string text)
    {
        IsActive = true;
        _currentTextIndex = 0;
        SplitUpText(text);
    }

    private void SplitUpText(string texts)
    {
        var s = texts.Split("[EOL]");
        //Console.WriteLine(s);
        text = new();
        for (int i = 0; i < s.Length; i += 4)
        {
            string textLine = s[i];
            if (i + 1 < s.Length) textLine += "[EOL]" + s[i + 1];
            if (i + 2 < s.Length) textLine += "[EOL]" + s[i + 2];
            if (i + 3 < s.Length) textLine += "[EOL]" + s[i + 3];
            text.Add(textLine);
        }
    }
    
    public void Render(SpriteBatch sb)
    {
        if (!IsActive) return;
        
        sb.Begin();
        tileData.DrawTileSet(sb, _tilesetInfo);
        sb.End();
            
        sb.Begin(transformMatrix: Matrix.CreateScale(0.75f,1f,1));
        Menu.DrawString(sb, menuSpritesheet, text[_currentTextIndex], new Vector2(20,16));
        sb.End();
    }

    public void NextDialogue()
    {
        _currentTextIndex++;
        if (_currentTextIndex == text.Count)
        {
            IsActive = false;
        }
    }
    
    public void Update(GameTime gt)
    {
    }
}
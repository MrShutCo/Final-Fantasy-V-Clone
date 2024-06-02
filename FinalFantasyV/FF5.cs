using System;
using System.IO;
using Engine.RomReader;
using Final_Fantasy_V.Models;
using FinalFantasyV.GameStates;
using FinalFantasyV.GameStates.Menus;
using FinalFantasyV.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;

namespace FinalFantasyV;

public class FF5 : Game
{
    public static  GraphicsDeviceManager Graphics;
    private SpriteBatch _spriteBatch;

    public static Texture2D NPCTexture;
    public static Texture2D Bartz;
    public static Texture2D Lenna;
    public static Texture2D Galuf;
    public static Texture2D Faris;

    public static Texture2D MenuTexture;
    public static TilesetInfo MenuTilesetInfo;

    private SimpleFps _fpsCounter;

    BattleUnit bartz;
    WorldCharacter worldCharacter;
    Character bartzC;
    RenderTarget2D target;
    Texture2D worldMap;
    Menu menu;

    StateStack stateStack;

    TiledMap tilemap;
    TiledMapRenderer tileRenderer;

    PartyState partyState;

    private Texture2D tex;
    private SpriteFont font;

    private RomGame _romGame; 
    
    public FF5()
    {
        Graphics = new GraphicsDeviceManager(this);
        Graphics.PreferMultiSampling = false;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Graphics.PreferredBackBufferWidth = 256 * 3;
        Graphics.PreferredBackBufferHeight = 240 * 3;
        //MapExtractor.ReadMap(1);
        _fpsCounter = new SimpleFps();
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here
        TargetElapsedTime = TimeSpan.FromSeconds(1d / 60d);
        Graphics.ApplyChanges();
        bartzC = new Character(Hero.Butz);
        base.Initialize();
    }

    protected override void LoadContent()
    {
        using (var br = new BinaryReader(File.Open("FF5.sfc", FileMode.Open)))
        {
            var offset = RomGame.CheckSNESHeader(br);
            RomData.Instantiate(br, offset.Item2);
        }


        _romGame = new RomGame();
        partyState = new PartyState(Content);
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        stateStack = new StateStack();
        NPCTexture = Content.Load<Texture2D>("npcs");
        MenuTexture = Content.Load<Texture2D>("fontmenu");
        MenuTilesetInfo = MapIO.ReadTileSet(Content, "Tilemaps/Menu.tsj");
        Bartz = Content.Load<Texture2D>("Bartz");
        Lenna = Content.Load<Texture2D>("Lenna");
        Galuf = Content.Load<Texture2D>("Galuf");
        Faris = Content.Load<Texture2D>("Faris");
        font = Content.Load<SpriteFont>("File");

        partyState.AddItem(RomData.GetGearByName("[Shld]Leather"));
        partyState.AddItem(RomData.GetGearByName("[Shld]Leather"));
        partyState.AddItem(RomData.GetGearByName("[Shld]Leather"));
        
        stateStack.Add("world", new WorldState(Content, _romGame));
        stateStack.Add("menu", new CharacterMenu(Content));
        stateStack.Add("battle", new BattleState(Content, _romGame));
        stateStack.Add("menu.equipment", new EquipmentMenu(Content));
        stateStack.Add("menu.item", new ItemMenu(Content));
        stateStack.Add("menu.status", new StatusMenu(Content));
        stateStack.Add("menu.magic", new MagicMenu(Content));
        stateStack.Add("menu.job", new JobMenu(Content));
        stateStack.Push("world", partyState);

    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
        
        _fpsCounter.Update(gameTime);

        InputHandler.GetState();
        
        stateStack.Update(gameTime, partyState);
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        target = new RenderTarget2D(GraphicsDevice, 256, 240);
        GraphicsDevice.SetRenderTarget(target);
        stateStack.Render(_spriteBatch, partyState);
        _spriteBatch.Begin();
        _spriteBatch.End();
        
        GraphicsDevice.SetRenderTarget(null);

        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.Default);
        _spriteBatch.Draw(target, new Rectangle(0, 0, Graphics.PreferredBackBufferWidth, Graphics.PreferredBackBufferHeight), Color.White);
        
        _fpsCounter.DrawFps(_spriteBatch, font, new Vector2(16,16), Color.White);
        
        _spriteBatch.End();
        base.Draw(gameTime);
    }

    Vector2 TopLeftScreen() => worldCharacter.Position - new Vector2(128, 120);

}


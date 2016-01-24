using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

namespace CultureBattle
{
    /// <summary>
    /// 這是會實作 IUpdateable 的遊戲元件。
    /// </summary>
    public class GameMenu : Microsoft.Xna.Framework.DrawableGameComponent
    {
        SpriteBatch sBatch;
        GraphicsDevice device;
        Texture2D Texture_PM1;
        Texture2D Texture_PM2;
        Game game;
        //CultureChoose CChoose;

        MouseState oldMouseState;
        MouseState mouseState;

        int Mx, My;
        //MyAudio audio;

        public GameMenu(Game game, Texture2D PM1, Texture2D PM2)
            : base(game)
        {
            // TODO: 在此建構任何子元件
            this.game = game;
            device = game.GraphicsDevice;
            this.Texture_PM1 = PM1;
            this.Texture_PM2 = PM2;
            
            
        }

        /// <summary>
        /// 允許遊戲元件在開始執行前執行所需的任何初始化項目。
        /// 這是元件能夠查詢所需服務，以及載入內容的地方。
        /// </summary>
        public override void Initialize()
        {
           // audio = new MyAudio();
            //audio.PlayAudio(0);
            //CChoose = new CultureChoose(game, game.Content.Load<Texture2D>("pCC1"),
                                          //game.Content.Load<Texture2D>("pCC2"), game.Content.Load<Texture2D>("pChoose"));
            //game.Components.Add(CChoose);
            //audio = new MyAudio();
            //audio.PlayAudio(0);
            base.Initialize();

        }

        /// <summary>
        /// 允許遊戲元件自我更新。
        /// </summary>
        /// <param name="gameTime">提供時間值的快照。</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: 在此新增更新程式碼
            //if (GameStateClass.currentGameState != GameStateClass.GameState.Menu)
                //return;
            
            mouseState = Mouse.GetState();

            //主選單(換頁功能)

            if (mouseState.LeftButton == ButtonState.Pressed &&
                oldMouseState.LeftButton == ButtonState.Released)   //滑鼠的左鍵被按下
            {

                Mx = mouseState.X;
                My = mouseState.Y;
               // for (int i = 0; i < 4; i++)
                //{
                    //M1 長220 寬 57 405,625,448,505 
                    //M2 長232 寬 68 400,632,442,510
                    //if (Mx >= 405 && Mx <= 625 && My >= 448 + i * (57 + 15) && My <= 505 + i * (57 + 15))
                    //{
                    //    if (i == 0)
                    //    {
                    //        audio.StopAudio(0);
                    //        GameStateClass.changeState(GameStateClass.GameState.Game1, game);
                            
                    //    }
                    //    if (i == 1)
                    //        GameStateClass.changeState(GameStateClass.GameState.Explain, game);
                    //    if (i == 2)
                    //    {
                    //        GameStateClass.changeState(GameStateClass.GameState.Made, game);
                    //    }
                    //    if (i == 3) game.Exit();
                    //}
                //}
            }
            oldMouseState = mouseState;

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            //if (GameStateClass.currentGameState != GameStateClass.GameState.Menu)
            //    return;
            //device.Clear(Color.White);
            //sBatch = (SpriteBatch)Game.Services.GetService(typeof(SpriteBatch));
            //if (sBatch == null) return;


            mouseState = Mouse.GetState();  //得到目前滑鼠的狀態
            Mx = mouseState.X;
            My = mouseState.Y;

            sBatch.Begin();

            //主選單(按鈕光圈)
            sBatch.Draw(Texture_PM1, new Vector2(0, 0), Color.White);
            for (int i = 0; i < 4; i++)
            {
                //M1 長220 寬 57 405,625,448,505 
                //M2 長232 寬 68 400,632,442,510
                if (Mx >= 405 && Mx <= 625 && My >= 448 + i * (57 + 15) && My <= 505 + i * (57 + 15))
                {
                    Rectangle R = new Rectangle(400, 442 + i * (68 + 4), 232, 68);

                    sBatch.Draw(Texture_PM2, R, R, Color.White);
                }
            }


            sBatch.End();
            base.Draw(gameTime);

        }
    }
}

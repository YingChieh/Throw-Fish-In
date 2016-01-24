
//倒數計時(未完成)
// http://createps.pixnet.net/blog/post/32590018-xna-%E6%99%82%E9%96%93%E8%A8%88%E6%99%82-record

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Timers;

namespace WindowsGame_Level2_ver2
{
    class Time_Bar : Microsoft.Xna.Framework.DrawableGameComponent
    {
        Game game;
        Texture2D Texture_Bar; // 內部的白色紋理圖

        public Vector2 Pos = Vector2.One; // 擺放的位置

        public int time_Max = 30;       // 限制的時間值 
        public int time = 0;            // 目前的的時間值 0 ~ time_Max

        public Color colorForground = Color.Red;  // 前景顏色
        public Color colorBackground = Color.Silver;// 背景顏色

        public int Width = 256; // 紋理圖 寬
        public int Height = 16; // 紋理圖 高

        public int sec, h, m, s;


        SpriteFont Font1; // 文字

        public Time_Bar(Game game, SpriteFont Font1) : base(game)
        {
            // TODO: Construct any child components here
            this.game = game;
            this.Font1 = Font1;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here
            // 新增一張白色、不透明紋理圖 (Width * Height)
            Texture_Bar = new Texture2D(game.GraphicsDevice, Width, Height, true, SurfaceFormat.Color);
            Color[] color = new Color[Width * Height];
            for (int i = 0; i < Width * Height; i++)
                color[i] = new Color(255, 255, 255, 255);
            Texture_Bar.SetData(color); // 白色 不透明 紋理圖
            base.Initialize();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: Add your update code here
            this.time += gameTime.ElapsedGameTime.Milliseconds;
            if (this.time >= 1000)
            {
                this.time = 0;
                this.s += 1;
                //if (s >= 60)
                //{
                //    s = 0;
                //    m += 1;
                //    if (m >= 60)
                //        h += 1;
                //}
                //if (h > 0)
                //    Console.WriteLine("時間計時:" + h + "小時" + m + "分" + s + "秒");
                //else if (m > 0)
                //    Console.WriteLine("時間計時:" + m + "分" + s + "秒");
                //else
                    Console.WriteLine("時間計時:" + s + "秒");
                    if (s > time_Max)
                        s = time_Max;
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch =
                (SpriteBatch)Game.Services.GetService(typeof(SpriteBatch));

            if (spriteBatch == null) return;

            // 將分數限制在 (0, score_Max) 之間
            s= (int)MathHelper.Clamp( 0, s, time_Max);

            Rectangle Dest1 = new Rectangle( // 前景區域
                                (int)Pos.X,
                                (int)Pos.Y,
                                (int)(Texture_Bar.Width * s / time_Max),
                                Texture_Bar.Height);
            Rectangle Dest2 = new Rectangle( // 背景區域
                                (int)(Pos.X + Texture_Bar.Width * s * (1.0f / time_Max)),
                                (int)Pos.Y,
                                (int)(Texture_Bar.Width * (time_Max - s) * (1.0f / time_Max)),
                                Texture_Bar.Height);
            Rectangle Src1 = new Rectangle(0, 0, (int)(Texture_Bar.Width * s * (1.0f / time_Max)), Texture_Bar.Height);
            Rectangle Src2 = new Rectangle(0, 0, (int)(Texture_Bar.Width * (time_Max - s) * (1.0f / time_Max)), Texture_Bar.Height);

            // 字型設定
            string output = "Time: " + Convert.ToString(time_Max-s); // 顯示出來的文字
            Vector2 FontOrigin = Font1.MeasureString(output) / 2; // Find the center of the string
            Vector2 FontPos = new Vector2(Pos.X + Texture_Bar.Width * 0.5f, Pos.Y + Texture_Bar.Height + 20);

            spriteBatch.Begin(); //SpriteSortMode.BackToFront, BlendState.AlphaBlend);

            spriteBatch.Draw(Texture_Bar, Dest1, Src1, colorForground);
            spriteBatch.Draw(Texture_Bar, Dest2, Src2, colorBackground);

            spriteBatch.DrawString(Font1,     // 字型
                              output,         // 字串
                              FontPos,        // 位置
                              Color.Black, // 字的顏色
                              0,           // 旋轉角度
                              FontOrigin,  // 字串中心點
                              1.0f,        // 縮放倍數
                              SpriteEffects.None,
                              1);       // 圖層深度 0.0 ~ 1.0 (後)

            spriteBatch.End();

            base.Update(gameTime);
        }    
    }
}

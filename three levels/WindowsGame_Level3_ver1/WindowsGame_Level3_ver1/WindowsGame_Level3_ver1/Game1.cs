using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Diagnostics;

namespace WindowsGame_Level3_ver1
{    
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;


        SoundEffect bgm;//音樂
        SoundEffectInstance bgmInstance;
        SoundEffect back;//音樂
        SoundEffectInstance backInstance;

        public Texture2D BG_texture; //背景

        public Texture2D win_texture; //贏得遊戲畫面
        public Texture2D lose_texture; //輸了遊戲畫面

        public Texture2D Fishtank; //魚缸
        Vector2 position;

        Texture2D[] mySpriteTexture = new Texture2D[32]; //魚
        Point[] mySpritePos = new Point[32];
        Vector2 vect;
        bool[] flag = new bool[32]; //有無進魚缸

        //Texture2D[] badfishTexture = new Texture2D[7]; //魚(扣分)
        //Point[] badfishPos = new Point[10];
        Vector2 vect_bad;
        //bool[] flag_bad = new bool[7]; //有無進魚缸
        Vector2 vect_2;
        Vector2 vect_bad2;

        MouseState oldMouseState;  // 宣告一個 MouseState 結構的變數
        bool dragging = false;
        int drag_N = -1;
        int drag_Num = 0;
        int Dx, Dy;
        int max = 32;
        //int Offset_X = 0;  // X 軸 偏移値

        int off = 0;
        Rectangle[,] srcRect = new Rectangle[1, 4];
        int FishWidth = 228; // 主角的寬
        int FishHeight = 170; // 主角的高
        int Dir = 0; // 走路的方向 往左
        int Seq = 0; // 該方向走路的第幾個動作
        double StepDuration = 0;

        //int off_bad = 0;
        Rectangle[,] badRect = new Rectangle[1, 4];
        int badFishWidth = 329; // 主角的寬
        int badFishHeight = 210; // 主角的高
        
        //int off_2 = 0;
        Rectangle[,] Rect2 = new Rectangle[1, 4];
        int Fish2Width = 200; // 主角的寬
        int Fish2Height = 180; // 主角的高

        Rectangle[,] badRect2 = new Rectangle[1, 4];
        int badFishWidth2 = 191; // 主角的寬
        int badFishHeight2 = 160; // 主角的高

        double[] myThetaList = new double[32];//上下起伏效果
        /******************************************************************************************/
        double G = 1; 		// 重力
        int V = 40; 		// 移動速度
        double degree = 45;	// 發射角度
        double rad;         // 將角度換算成電腦的單位，PI = 90度
        
        double move_x;	// 水平位移
        double move_y;	// 垂直位移
        double v_temp;  // 用拖曳距離和時間計算出的速率
        /******************************************************************************************/
        int kState = 0;
        int old_disX, old_disY, last_positionX, last_positionY;
        double dis = 0;  // 魚的拖曳移動距離
        Stopwatch watch; // 拖曳時間
        /******************************************************************************************/
        Random rand = new Random();
        G2D_Bar bar2D;    // 宣告一個長條圖物件
        //Time_Bar barTime;    // 宣告一個倒數計時長條圖物件
        SpriteFont Font1; // 文字

        Time_Bar bartime;

        double gamesecond;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
           

            Content.RootDirectory = "Content";

            this.IsMouseVisible = true;  // 呈現滑鼠游標
            this.Window.AllowUserResizing = true; // 允許視窗縮放
            graphics.PreferredBackBufferWidth = 1300;  //視窗的預設寬
            graphics.PreferredBackBufferHeight = 800; //視窗的預設高

            this.Window.Title = "Fish (滑鼠)";

            for (int j = 0; j < 4; j++) // 魚的分解動畫圖3張
            {
                srcRect[0, j] = new Rectangle(j * FishWidth, 0 * FishHeight, FishWidth, FishHeight);
                badRect[0, j] = new Rectangle(j * badFishWidth, 0 * badFishHeight, badFishWidth, badFishHeight);
                Rect2[0, j] = new Rectangle(j * Fish2Width, 0 * Fish2Height, Fish2Width, Fish2Height);
                badRect2[0, j] = new Rectangle(j * badFishWidth2, 0 * badFishHeight2, badFishWidth2, badFishHeight2);
            }
           
        }
        
        protected override void Initialize()
        {            
            Font1 = Content.Load<SpriteFont>("Courier New");
            bar2D = new G2D_Bar(this, Font1); // 分數長條圖            
            bar2D.Pos = new Vector2(10, 10);  // 擺放的位置            
            bar2D.score_Max = 700;  // 設定最高的分數值
            bar2D.score = 0;  // 設定目前的分數值
            Components.Add(bar2D);  // 變成 Game1 的遊戲元件

            bartime = new Time_Bar(this, Font1); //倒數計時長條圖
            bartime.Pos = new Vector2(800, 10);
            bartime.time_Max = 30;  // 設定最高的分數值
            bartime.time = 0;  // 設定目前的分數值
            Components.Add(bartime);
            base.Initialize();
        }
        
        protected override void LoadContent()
        {
            bgm = Content.Load<SoundEffect>(@"bm");
            bgmInstance = bgm.CreateInstance();
            back = Content.Load<SoundEffect>(@"back");
            backInstance = back.CreateInstance();


            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            BG_texture = Content.Load<Texture2D>("W");  // 水背景
            win_texture = Content.Load<Texture2D>("win");  // 贏得遊戲
            lose_texture = Content.Load<Texture2D>("lose");  // 輸了遊戲

            for (int i = 0; i < max-3; i++)
            {
                mySpriteTexture[i] = Content.Load<Texture2D>("fish1");
                i++;
                mySpriteTexture[i] = Content.Load<Texture2D>("fish4");
                i++;
                mySpriteTexture[i] = Content.Load<Texture2D>("fish2");
                i++;
                mySpriteTexture[i] = Content.Load<Texture2D>("fish8");
            }
            for (int i = 0; i < mySpritePos.Length-3; i++)
            {
                mySpritePos[i] = new Point(600 + 120 * i, 650 + 15 * i);
                i++;
                mySpritePos[i] = new Point(600 + rand.Next(100, 120) * i, 650 + rand.Next(15) * i);
                i++;
                mySpritePos[i] = new Point(rand.Next(-30, 10) * i, 650 + rand.Next(0,10) * i);
                i++;
                mySpritePos[i] = new Point(rand.Next(-30, 10) * i, 650 + rand.Next(0, 10) * i);
            }       
            
            Fishtank = Content.Load<Texture2D>("glassss");  // 魚缸

            Services.AddService(typeof(SpriteBatch), spriteBatch); // Game1 要將 spriteBatch 放到服務區
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();
            
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            /*********************************/
            StepDuration += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (StepDuration > 200) // 魚的分解動畫圖3張
            {
                StepDuration = 0;
                Seq++;  // 下一張
                Seq = Seq % 3; // 每一方向只有三張
            }

            MouseState mouseState = Mouse.GetState();  //得到目前滑鼠的狀態
            //Offset_X += -1; // 向左

            jump(mouseState, gameTime); // 拋物線

            /*********************************/

            // 游出視窗，從另一邊再游出來
            for (int i = 0; i < max; i++)
            {
                if ((mySpritePos[i].X) > graphics.GraphicsDevice.Viewport.Width)
                    if(i%4 == 2)
                        mySpritePos[i].X = 0 + Fish2Width / 2;
                    else if(i%4 == 3)
                        mySpritePos[i].X = 0 + badFishWidth2 / 2;
                else if((mySpritePos[i].X) < 0)
                    if (i % 4 == 0)
                        mySpritePos[i].X = 1300 + FishWidth / 2;
                    else if (i % 4 == 1)
                        mySpritePos[i].X = 1300 + badFishWidth / 2;
            }

            oldMouseState = mouseState;
            
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            backInstance.Play();
            GraphicsDevice.Clear(Color.CornflowerBlue);

            int W = graphics.GraphicsDevice.Viewport.Width;
            int H = graphics.GraphicsDevice.Viewport.Height;

            spriteBatch.Begin();

            //背景
            for (int i = 0; i <= (W / BG_texture.Width); i++)
            {
                for (int j = 3; j < H / 2 / BG_texture.Height + 5; j++)
                {                    
                    Vector2 pos = new Vector2(i * BG_texture.Width, j * BG_texture.Height);
                    spriteBatch.Draw(BG_texture, pos, Color.White);
                }
            }

            // 魚缸
            Rectangle dest = new Rectangle(450, 200, 400, 366);//縮小為400*366
            spriteBatch.Draw(Fishtank, dest, Color.White);
            position = new Vector2(400, 200);

            //金魚
            
            gamesecond += gameTime.ElapsedGameTime.TotalSeconds;
            if (gamesecond < 10)
                max = 10;
            else if (gamesecond >= 10 && gamesecond < 20)
                max = 20;
            else
                max = 32;
            for (int i = 0; i < max; i++)
            {
                if (i % 4 == 0)
                {
                    vect = new Vector2(mySpritePos[i].X - FishWidth / 2,
                                       mySpritePos[i].Y - FishHeight / 2);
                    if (flag[i] != true)
                    {
                        myThetaList[i] += 0.1; //sin() 要用到的角度
                        mySpritePos[i].X -= 3 + rand.Next(2) * rand.Next(-1, 1);
                        mySpritePos[i].Y = mySpritePos[i].Y + (int)(6 * Math.Sin(myThetaList[i]) * rand.Next(2));
                    }

                    spriteBatch.Draw(mySpriteTexture[i], vect, srcRect[Dir, Seq], Color.White, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);
                }
                else if (i % 4 == 1)
                {
                    vect_bad = new Vector2(mySpritePos[i].X - badFishWidth / 2,
                                       mySpritePos[i].Y - badFishHeight / 2);
                    if (flag[i] != true)
                    {
                        myThetaList[i] += 0.1; //sin() 要用到的角度
                        mySpritePos[i].X -= 3 + rand.Next(2) * rand.Next(-1, 1);
                        mySpritePos[i].Y = mySpritePos[i].Y + (int)(6 * Math.Sin(myThetaList[i]) * rand.Next(2));
                    }

                    spriteBatch.Draw(mySpriteTexture[i], vect_bad, badRect[Dir, Seq], Color.White, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);
                }
                else if (i % 4 == 2)
                {
                    vect_2 = new Vector2(mySpritePos[i].X - Fish2Width / 2,
                                       mySpritePos[i].Y - Fish2Height / 2);
                    if (flag[i] != true)
                    {
                        myThetaList[i] += 0.1; //sin() 要用到的角度
                        mySpritePos[i].X += 3 + rand.Next(2) * rand.Next(-1, 1);
                        mySpritePos[i].Y = mySpritePos[i].Y + (int)(6 * Math.Sin(myThetaList[i]) * rand.Next(2));
                    }

                    spriteBatch.Draw(mySpriteTexture[i], vect_2, Rect2[Dir, Seq], Color.White, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);
                }
                else
                {
                    vect_bad2 = new Vector2(mySpritePos[i].X - badFishWidth2 / 2,
                                       mySpritePos[i].Y - badFishHeight2 / 2);
                    if (flag[i] != true)
                    {
                        myThetaList[i] += 0.1; //sin() 要用到的角度
                        mySpritePos[i].X += 3 + rand.Next(2) * rand.Next(-1, 1);
                        mySpritePos[i].Y = mySpritePos[i].Y + (int)(6 * Math.Sin(myThetaList[i]) * rand.Next(2));
                    }

                    spriteBatch.Draw(mySpriteTexture[i], vect_bad2, badRect2[Dir, Seq], Color.White, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);
                }
            }

            // 贏得遊戲
            if (bar2D.score == 700)
            {
                Vector2 win_pos = new Vector2(450, 200);
                spriteBatch.Draw(win_texture, win_pos, Color.White);
                bartime.s--; 
            }

            //輸了遊戲
            if (bartime.s == bartime.time_Max && bar2D.score != 700)
            {
                Vector2 lose_pos = new Vector2(450, 200);
                spriteBatch.Draw(lose_texture, lose_pos, Color.White);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }       

        public void jump(MouseState mouseState, GameTime gameTime) // 拋物線
        {
            //得到目前滑鼠的狀態 mouseState
            //Offset_X += -1; // 向左

            if (mouseState.LeftButton == ButtonState.Released) // 放開 拖曳結束
            {
                dragging = false;

                move_x = V * Math.Cos(rad);
                move_y = V * Math.Sin(rad);
            }
            /*********************************/
            if (mouseState.LeftButton == ButtonState.Pressed &&
                oldMouseState.LeftButton == ButtonState.Released)   // 初次點按 
            {
                Rectangle rect;
                for (int i = 0; i < mySpritePos.Length; i++)
                {
                    if(i%2 == 0)
                        rect = new Rectangle(mySpritePos[i].X - FishWidth / 2, mySpritePos[i].Y - FishHeight / 2,
                                         FishWidth / 2, FishHeight / 2);
                    else
                        rect = new Rectangle(mySpritePos[i].X - badFishWidth / 2, mySpritePos[i].Y - badFishHeight / 2,
                                         badFishWidth / 2, badFishHeight / 2);
                    if (rect.Contains(mouseState.X, mouseState.Y)) //點到圖形
                    {
                        //計算點擊魚的時間間隔
                        watch = new Stopwatch();
                        watch.Start();

                        old_disX = mySpritePos[i].X;
                        old_disY = mySpritePos[i].Y;
                        kState = 1;
                        dragging = true;  // 要開始 拖曳了

                        drag_N = i; //拖曳第幾張圖形
                        drag_Num = drag_N;
                        Dx = mouseState.X - mySpritePos[i].X; // 滑鼠游標和圖的偏移
                        Dy = mouseState.Y - mySpritePos[i].Y;

                        break;
                    }
                }
            }
            else if (mouseState.LeftButton == ButtonState.Pressed)  //連續的滑鼠按鍵輸入
            {
                if (dragging) // 拖曳中
                {
                    mySpritePos[drag_N].X = mouseState.X - Dx - off; // 移動圖形
                    mySpritePos[drag_N].Y = mouseState.Y - Dy;

                    last_positionX = mySpritePos[drag_N].X;
                    last_positionY = mySpritePos[drag_N].Y;


                    rad = degree / (180 / Math.PI);  //rad：將角度換算成電腦的單位，PI = 90度
                    Console.WriteLine("rad" + rad);

                }
            }
            else
            {
                if (!dragging)
                {
                    if (kState == 1 && (old_disY - last_positionY) > 0)
                    {
                        watch.Stop();
                        // Console.WriteLine("時間間隔為:{0}秒", watch.ElapsedMilliseconds / 1000.0f);

                        int dis_x = Math.Abs(old_disX - last_positionX);
                        int dis_y = Math.Abs(old_disY - last_positionY);

                        dis = Math.Sqrt(dis_x * dis_x + dis_y * dis_y); //魚的移動距離
                        //Console.WriteLine("距離為:" + dis);
                        v_temp = dis / (watch.ElapsedMilliseconds / 1000.0f); //速率 = 距離除以時間
                        V = (int)v_temp / 45;
                        Console.WriteLine("速率為:" + V);

                        if (rad > 0)
                        {
                            if (old_disX - last_positionX < 0)//往右
                            {
                                mySpritePos[drag_Num].X = (int)((move_x * 1)) + last_positionX;
                                move_y += G; 	// 重力加速度會越來越快
                                mySpritePos[drag_Num].Y = (int)(move_y + G) + last_positionY;
                                last_positionX = mySpritePos[drag_Num].X;
                                last_positionY = mySpritePos[drag_Num].Y;
                            }
                            else if (old_disX - last_positionX > 0)//往左
                            {
                                mySpritePos[drag_Num].X = (int)((move_x * -1)) + last_positionX;
                                move_y += G; 	// 重力加速度會越來越快
                                mySpritePos[drag_Num].Y = (int)(move_y + G) + last_positionY;
                                last_positionX = mySpritePos[drag_Num].X;
                                last_positionY = mySpritePos[drag_Num].Y;
                            }

                        }
                    }

                    if (Collides(mySpritePos[drag_Num]) || Collides_bad(mySpritePos[drag_Num]))//碰到魚缸
                    {
                        flag[drag_Num] = true;
                        if (drag_Num % 4 == 0 || drag_Num % 4 == 2)
                            bar2D.score += 100; // 目前的分數值 加100
                        else
                            bar2D.score -= 100; // 目前的分數值 減100
                        
                        //Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!");
                        int stop = 450/* + rand.Next(40)*/;
                        bgmInstance.Play();

                        while (mySpritePos[drag_Num].Y < stop)
                        {
                            mySpritePos[drag_Num].Y = (int)(move_y) + last_positionY;
                            last_positionY = mySpritePos[drag_Num].Y;
                            if (old_disX - last_positionX < 0)//往右
                            {
                                mySpritePos[drag_Num].X = (int)((move_x * 1)) + last_positionX;
                                last_positionX = mySpritePos[drag_Num].X;
                            }
                            else if (old_disX - last_positionX > 0)//往左
                            {
                                mySpritePos[drag_Num].X = (int)((move_x * -1)) + last_positionX;
                                last_positionX = mySpritePos[drag_Num].X;
                            }
                            if (drag_Num % 2 == 0)
                            {
                                if (mySpritePos[drag_Num].X > position.X + Fishtank.Width - 100)
                                    mySpritePos[drag_Num].X = (int)position.X + FishWidth + 100;
                                else if (mySpritePos[drag_Num].X < position.X)
                                    mySpritePos[drag_Num].X = (int)position.X + FishWidth + 100;
                            }
                            else
                            {
                                if (mySpritePos[drag_Num].X > position.X + Fishtank.Width - 100)
                                    mySpritePos[drag_Num].X = (int)position.X + badFishWidth + 100;
                                else if (mySpritePos[drag_Num].X < position.X)
                                    mySpritePos[drag_Num].X = (int)position.X + badFishWidth + 100;
                            }
                        }
                        
                        kState = 0;
                        dragging = false;
                    }
                    

                    if (mySpritePos[drag_Num].Y > (graphics.GraphicsDevice.Viewport.Height))//拋物線最後停下的高度
                    {
                        mySpritePos[drag_Num].Y = graphics.GraphicsDevice.Viewport.Height - 50 + rand.Next(15);
                        kState = 0;
                    }
                    else if (mySpritePos[drag_Num].Y < 0)//超出視窗高度
                    {
                        mySpritePos[drag_Num].Y = 650 + rand.Next(15);
                        ;
                        kState = 0;
                        dragging = false;
                    }
                }
            }
        }

        private bool Collides_bad(Point point)
        {
            return (position.X < point.X && position.X + Fishtank.Width > point.X + badFishWidth
                && position.Y + 5 > point.Y && position.Y - 5 < point.Y + badFishHeight);//碰到魚缸上緣
        }

        //檢查碰撞魚缸
        public bool Collides(Point pic)
        {
            return (position.X < pic.X && position.X + Fishtank.Width > pic.X + FishWidth
                && position.Y + 5 > pic.Y && position.Y - 5 < pic.Y + FishHeight);//碰到魚缸上緣
        }
    }
}
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


namespace CultureBattle
{
    /// <summary>
    /// 這是會實作 IUpdateable 的遊戲元件。
    /// </summary>
    public class BattlePage : Microsoft.Xna.Framework.DrawableGameComponent
    {
        static SpriteBatch sBatch;
        static GraphicsDevice device;
        Game game;

        SpriteFont MFontTitle;
        SpriteFont SDataFont;
        Color PlayerColor;
        string title;//標題
        Color WinColor;
        static int[,] Pking = GameStateClass.getPKing();//玩家 統帥、勇武、敏捷
        static int[, ,] BattleData = GameStateClass.getBattleData();//X座標、Y座標、(0兵種、1數量、2順序、3戰鬥指令、4回合、5血量、6玩家)
        static int[, ,] SoldierDataP = GameStateClass.getSoldierDataP();
        static int[,] PCN = GameStateClass.getPCN();
        int[, ,] Battlechess = new int[17, 9, 10];
        Texture2D[] army = new Texture2D[21];
        Texture2D pBattlestage;
        Texture2D pChoose;
        //棋盤貼圖
        static int chessX = 35, chessY = 154, chessDis = 7, chessPic = 50;

        int counter = 1;//回合
        double SeqTime;//士兵動作
        int SeqX = 0;//X格子轉換
        int SearchI;//目前掃描的格子
        int SearchJ;//目前掃描的格子
        int PlayerN = 0;//輪到哪個玩家
        int Seqence = 0;//目前的優先度

        static Boolean Over ;//判斷平手狀況
        Boolean draw;//確定發生平手狀況

        KeyboardState oldState;
        int speed;
        MyAudio audio = new MyAudio();

        public BattlePage(Game game)
            : base(game)
        {
            // TODO: 在此建構任何子元件
            this.game = game;
            device = game.GraphicsDevice;
        }

        /// <summary>
        /// 允許遊戲元件在開始執行前執行所需的任何初始化項目。
        /// 這是元件能夠查詢所需服務，以及載入內容的地方。
        /// </summary>
        public override void Initialize()
        {
            // TODO: 在此新增初始化程式碼
            title = "";
            SeqTime = 0;//士兵動作
            Over = true;//判斷平手狀況
            draw = false;//確定發生平手狀況
            speed = 500;//遊戲速度
            WinColor = Color.Black;
            MFontTitle = game.Content.Load<SpriteFont>("PlayerSetTitle");
            SDataFont = game.Content.Load<SpriteFont>("SFont");
            pBattlestage = game.Content.Load<Texture2D>("pBattlestage");
            pChoose = game.Content.Load<Texture2D>("pChoose");
            for (int i = 0; i < 21; i++)
                army[i] = game.Content.Load<Texture2D>("p" + i.ToString());
            audio.PlayAudio(3);
            base.Initialize();
        }

        /// <summary>
        /// 允許遊戲元件自我更新。
        /// </summary>
        /// <param name="gameTime">提供時間值的快照。</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: 在此新增更新程式碼
            if (GameStateClass.currentGameState != GameStateClass.GameState.GameWar)
                return;

            //返回首頁
            KeyboardState newState = Keyboard.GetState();
            if (oldState.IsKeyUp(Keys.Escape) && newState.IsKeyDown(Keys.Escape))
            {
                audio.StopAudio(3);
                GameStateClass.changeState(GameStateClass.GameState.Menu, game);
            }
            if (oldState.IsKeyUp(Keys.Up) && newState.IsKeyDown(Keys.Up) && speed > 100)//加速
                speed -= 50;
            if (oldState.IsKeyUp(Keys.Down) && newState.IsKeyDown(Keys.Down) && speed < 5000)//減速
                speed += 50;

            oldState = newState;

            if (BattleData[0, 4, 0] != 0 || BattleData[16, 4, 0] != 0 || draw)//GameOver
            {
                SeqTime += gameTime.ElapsedGameTime.TotalMilliseconds;
                if (SeqTime > 10000)
                {
                    audio.StopAudio(3);
                    GameStateClass.changeState(GameStateClass.GameState.Menu, game);
                }
            }
            else
            {
                //優先度相同時處理
                if (PlayerN == 0)
                    SeqX = 16 - SearchI;
                else
                    SeqX = SearchI;
                //X座標、Y座標、(0兵種、1數量、2順序、3戰鬥指令、4回合、5血量、6玩家)
                while ((BattleData[SeqX, SearchJ, 0] < 0
                    || BattleData[SeqX, SearchJ, 4] != counter
                    || BattleData[SeqX, SearchJ, 6] != PlayerN
                    || BattleData[SeqX, SearchJ, 2] != Seqence
                    || BattleData[SeqX, SearchJ, 0] == 9
                    || BattleData[SeqX, SearchJ, 0] == 10)
                    )
                {
                    //檢查下一格
                    if (SearchJ < 8)
                        SearchJ++;
                    else if (SearchI < 16 && SearchJ == 8)//Y到底
                    {
                        SearchI++;
                        SearchJ = 0;
                    }
                    else if (SearchI == 16 && SearchJ == 8)//檢查完
                    {
                        SearchI = 0;
                        SearchJ = 0;
                        if (Seqence < 72)//優先度+1
                            Seqence++;
                        else if (PlayerN == 0)//換P2
                        {
                            Seqence = 0;
                            PlayerN++;
                        }
                        else if (PlayerN == 1)//下一回合
                        {
                            Seqence = 0;
                            PlayerN--;
                            if (Over)
                            {
                                draw = true;
                                counter++;
                            }
                            else
                            {
                                Over = true;
                                counter++;
                            }
                        }
                    }
                    if (PlayerN == 0)
                        SeqX = 16 - SearchI;
                    else
                        SeqX = SearchI;

                }

                SeqTime += gameTime.ElapsedGameTime.TotalMilliseconds;//士兵動作
                if (SeqTime > speed && !draw && BattleData[0, 4, 0] == 0 && BattleData[16, 4, 0] == 0)
                {
                    if (BattleData[0, 4, 5] > 0 && BattleData[16, 4, 5] > 0 && BattleData[0, 4, 0] == 0 && BattleData[16, 4, 0] == 0)//雙方的英雄都還活著就繼續
                    {
                        if (BattleData[SeqX, SearchJ, 3] == 0)//偷襲
                        {
                            int[] G = Move(SeqX, SearchJ, PlayerN, game, gameTime);
                            if (G[0] != -1 && G[1] != -1)
                                if (G[0] != SeqX || G[1] != SearchJ)//有移動
                                {
                                    Atk(G[0], G[1], PlayerN, game, gameTime);
                                    BattleData[G[0], G[1], 4]++;

                                }
                                else//沒移動
                                {
                                    Atk(G[0], G[1], PlayerN, game, gameTime);//先打
                                    Move(G[0], G[1], PlayerN, game, gameTime);//再走
                                    BattleData[G[0], G[1], 4]++;
                                }
                        }
                        else if (BattleData[SeqX, SearchJ, 3] == 1)//進攻
                        {
                            int A = Atk(SeqX, SearchJ, PlayerN, game, gameTime);
                            if (A >= 0)
                            {//有打
                                int[] G = Move(SeqX, SearchJ, PlayerN, game, gameTime);

                                if (G[0] != -1 && G[1] != -1)
                                    BattleData[G[0], G[1], 4]++;
                            }
                            else
                            {//沒打
                                int[] G = Move(SeqX, SearchJ, PlayerN, game, gameTime);
                                if (G[0] != -1 && G[1] != -1)
                                {
                                    Atk(G[0], G[1], PlayerN, game, gameTime);
                                    BattleData[G[0], G[1], 4]++;
                                }
                            }
                        }
                        else if (BattleData[SeqX, SearchJ, 3] == 2)//待機
                        {
                            Atk(SeqX, SearchJ, PlayerN, game, gameTime);
                            BattleData[SeqX, SearchJ, 4]++;
                        }
                    }
                    else//GAME OVER
                    {
                    }
                    SeqTime = 0;
                }
            }
            base.Update(gameTime);
        }


        public override void Draw(GameTime gameTime)
        {
            if (GameStateClass.currentGameState != GameStateClass.GameState.GameWar)
                return;
            sBatch = (SpriteBatch)Game.Services.GetService(typeof(SpriteBatch));
            device.Clear(Color.White);
            sBatch.Begin();
            sBatch.Draw(pBattlestage, new Vector2(0, 0), Color.White);

            //目標士兵
            if (BattleData[0, 4, 0] == 0 && BattleData[16, 4, 0] == 0 && !draw)
                sBatch.Draw(pChoose,
                    new Rectangle(chessX + (chessPic + chessDis) * SeqX, chessY + (chessPic + chessDis) * SearchJ, chessPic, chessPic),
                    new Rectangle(0, 0, 54, 45),
                    new Color(255, 255, 0, 180));

            Rectangle Ra1 = new Rectangle(0, 0, 500, 500);
            Rectangle Ra2 = new Rectangle(0, 500, 500, 500);
            //士兵
            for (int i = 0; i < 17; i++)
                for (int j = 0; j < 9; j++)
                    if (BattleData[i, j, 0] >= 0)
                    {
                        Rectangle Ra = new Rectangle(chessX + (chessPic + chessDis) * i, chessY + (chessPic + chessDis) * j, chessPic, chessPic);
                        if (BattleData[i, j, 6] == 0)
                            sBatch.Draw(army[BattleData[i, j, 0]], Ra, Ra1, Color.White);
                        else if (BattleData[i, j, 6] == 1)
                            sBatch.Draw(army[BattleData[i, j, 0]], Ra, Ra2, Color.White);
                    }


            //文字區
            //貼標題

            if (BattleData[0, 4, 0] != 0)
            {
                title = " Red Win";
                WinColor = Color.Red;
            }
            else if (BattleData[16, 4, 0] != 0)
            {
                title = "Blue Win";
                WinColor = Color.Blue;
            }
            else if (draw)
                title = "  DRAW  ";

            sBatch.DrawString(MFontTitle,   // 字型
                                  title,  // 字串
                                  new Vector2(80, 250), // 位置
                                  WinColor, // 字的顏色
                                  0,         // 旋轉角度
                                  new Vector2(0, 0),// 字串中心點
                                  5.0f,      // 縮放倍數
                                  SpriteEffects.None,// 旋轉校果
                                  0.1f); // 圖層深度 0.0 ~ 1.0 (後)
            //士兵資料
            for (int i = 0; i < 17; i++)
                for (int j = 0; j < 9; j++)
                    if (BattleData[i, j, 0] >= 0)
                    {
                        if (BattleData[i, j, 6] == 0)
                            PlayerColor = Color.Blue;
                        else
                            PlayerColor = Color.Red;
                        sBatch.DrawString(SDataFont,   // 字型
                                          BattleData[i, j, 1].ToString(),  //棋盤  #兵種、數量、順序、戰鬥指令
                                          new Vector2(chessX - 5 + (chessPic + chessDis) * i, chessY - 5 + (chessPic + chessDis) * j), // 位置
                                          PlayerColor, // 字的顏色
                                          0,         // 旋轉角度
                                          new Vector2(0, 0),// 字串中心點
                                          1.0f,      // 縮放倍數
                                          SpriteEffects.None,// 旋轉校果
                                          0.1f); // 圖層深度 0.0 ~ 1.0 (後)
                        sBatch.DrawString(SDataFont,   // 字型
                                  BattleData[i, j, 2].ToString(),  //棋盤  #兵種、數量、順序、戰鬥指令、點擊
                                  new Vector2(chessX - 5 + (chessPic + chessDis) * i, chessY + 40 + (chessPic + chessDis) * j), // 位置
                                  PlayerColor, // 字的顏色
                                  0,         // 旋轉角度
                                  new Vector2(0, 0),// 字串中心點
                                  1.0f,      // 縮放倍數
                                  SpriteEffects.None,// 旋轉校果
                                  0.1f); // 圖層深度 0.0 ~ 1.0 (後)
                    }


            sBatch.End();
            base.Draw(gameTime);
        }

        private static int[] Move(int SeqX, int j, int PlayerN, Game game, GameTime gameTime)//移動
        {
            int GoalX = SeqX, GoalY = j;
            int M;
            for (int move = 1; move <= SoldierDataP[PlayerN, BattleData[SeqX, j, 0], 7]; move++)
            {

                if (PlayerN == 1)
                    M = -1;
                else
                    M = 1;

                //前方移動
                if (GoalX + M >= 0 && GoalX + M < 17 && (BattleData[GoalX + M, j, 0] < 0 || BattleData[GoalX + M, j, 0] == 9))//沒出界而且沒敵人或自己人
                {
                    Over = false;
                    if (BattleData[GoalX + M, j, 0] == 9)//採到陷阱
                    {
                        for (int i = 0; i < 7; i++)
                        {
                            BattleData[GoalX + M, j, i] = -1;
                            BattleData[SeqX, j, i] = -1;
                        }
                        return new int[] { -1, -1 };
                    }
                    else
                        GoalX += M;
                }
                else if (GoalX == 0 || GoalX == 16)//碰壁
                {
                    if (GoalY < 4 && (BattleData[GoalX, GoalY + 1, 0] < 0 || BattleData[GoalX, GoalY + 1, 0] == 9))//上半部，下方沒人
                    {
                        Over = false;
                        if (BattleData[GoalX, GoalY + 1, 0] == 9)//採到陷阱
                        {
                            for (int i = 0; i < 7; i++)
                            {
                                BattleData[GoalX, GoalY + 1, 0] = -1;
                                BattleData[SeqX, j, i] = -1;
                            }
                            return new int[] { -1, -1 };
                        }

                        else
                            GoalY++;
                    }
                    else if (GoalY > 4 && (BattleData[GoalX, GoalY - 1, 0] < 0 || BattleData[GoalX, GoalY - 1, 0] == 9))//下半部，上方沒人
                    {
                        Over = false;
                        if (BattleData[GoalX, GoalY - 1, 0] == 9)//採到陷阱
                        {
                            for (int i = 0; i < 7; i++)
                            {
                                BattleData[GoalX, GoalY - 1, 0] = -1;
                                BattleData[SeqX, j, i] = -1;
                            }
                            return new int[] { -1, -1 };
                        }

                        else
                            GoalY--;
                    }
                }

            }
            if (GoalX != SeqX || GoalY != j)//有移動
            {
                for (int a = 0; a < 7; a++)
                {
                    BattleData[GoalX, GoalY, a] = BattleData[SeqX, j, a];
                    BattleData[SeqX, j, a] = -1;
                }
            }
            return new int[] { GoalX, GoalY };
        }



        private static int Atk(int SeqX, int j, int PlayerN, Game game, GameTime gameTime)//攻擊
        {
            //BattleData[17, 9, 7]  X座標、Y座標、(0兵種、1數量、2順序、3戰鬥指令、4回合、5血量、6玩家)
            //SoldierDataP[2, 21, 10]  0type, 1造價, 2血量,3攻擊,4防禦,  5射程(MIN),6射程(MAX),7移動,8數量限制,9目前的量
            Random rd = new Random();
            int A = -1;//攻擊狀態
            int attackCount = 0;
            if ((BattleData[SeqX, j, 0] == 2 || BattleData[SeqX, j, 0] == 15)
                && PCN[PlayerN, 1] == 1
                && (int)(rd.NextDouble() * 100) < 38)//連續(槍兵技能)
                attackCount = 1;
            else if (BattleData[SeqX, j, 0] == 17 && (int)(rd.NextDouble() * 100) < 30)//蒙古突騎
                attackCount = 1;
            else if (BattleData[SeqX, j, 0] == 20)//中國連弩兵
                attackCount = 2;

            for (int a = 0; a <= attackCount; a++)
            {
                A = AtkChoose(SeqX, j, PlayerN, 1, game, gameTime);//先打前面
                if (j <= 4 && A == -1)//中、上方部隊，優先攻擊下方部隊
                {
                    A = AtkChoose(SeqX, j, PlayerN, 2, game, gameTime);
                    if (A == -1)
                        A = AtkChoose(SeqX, j, PlayerN, 3, game, gameTime);
                }
                if (j > 4 && A == -1)
                {
                    A = AtkChoose(SeqX, j, PlayerN, 3, game, gameTime);
                    if (A == -1)
                        A = AtkChoose(SeqX, j, PlayerN, 2, game, gameTime);
                }
                if (A == -1)//打後面
                    A = AtkChoose(SeqX, j, PlayerN, 4, game, gameTime);
            }

            return A;
        }
        private static int AtkChoose(int SeqX, int j, int PlayerN, int Choose, Game game, GameTime gameTime)//攻擊方向判定 Choose=1前方，2下方，3上方，4後方
        {
            int A;//回傳狀況
            if (Choose == 1)
                for (int dis = SoldierDataP[PlayerN, BattleData[SeqX, j, 0], 5]; dis <= SoldierDataP[PlayerN, BattleData[SeqX, j, 0], 6]; dis++)//前方攻擊
                {
                    int Pdis = dis;
                    if (PlayerN == 1)
                        Pdis = -Pdis;
                    if (SeqX + Pdis >= 0 && SeqX + Pdis < 17 && BattleData[SeqX + Pdis, j, 0] >= 0 && BattleData[SeqX + Pdis, j, 6] != PlayerN && BattleData[SeqX + Pdis, j, 0] != 9)//先判斷前方
                    {
                        A = Hurt(SeqX, j, SeqX + Pdis, j, PlayerN, 1, game, gameTime);//1為額外攻擊加成數
                        if (BattleData[SeqX, j, 0] == 18)//戰象
                        {
                            if (j + 1 <= 8 && BattleData[SeqX, j + 1, 0] >= 0 && BattleData[SeqX, j + 1, 6] != PlayerN)//下
                                Hurt(SeqX, j, SeqX, j + 1, PlayerN, 0.5, game, gameTime);
                            if (j - 1 >= 0 && BattleData[SeqX, j - 1, 0] >= 0 && BattleData[SeqX, j - 1, 6] != PlayerN)//上
                                Hurt(SeqX, j, SeqX, j - 1, PlayerN, 0.5, game, gameTime);
                            if (SeqX - Pdis >= 0 && SeqX - Pdis < 17 && BattleData[SeqX - Pdis, j, 0] >= 0 && BattleData[SeqX - Pdis, j, 6] != PlayerN)//後
                                Hurt(SeqX, j, SeqX - Pdis, j, PlayerN, 0.5, game, gameTime);
                        }
                        if (A == -1)//前方無法攻擊(建築)
                            return -1;
                        if (A == 0)//成功攻擊
                        {
                            Over = false;
                            return 0;
                        }
                    }
                }
            else if (Choose == 2)//下
                for (int dis = SoldierDataP[PlayerN, BattleData[SeqX, j, 0], 5]; dis <= SoldierDataP[PlayerN, BattleData[SeqX, j, 0], 6]; dis++)//前方攻擊
                {
                    int Pdis = dis;
                    if (j + Pdis >= 0 && j + Pdis < 9 && BattleData[SeqX, j + Pdis, 0] >= 0 && BattleData[SeqX, j + Pdis, 6] != PlayerN && BattleData[SeqX, j + Pdis, 0] != 9)//先判斷前方
                    {
                        A = Hurt(SeqX, j, SeqX, j + Pdis, PlayerN, 1, game, gameTime);
                        if (BattleData[SeqX, j, 0] == 18)//戰象
                        {
                            if (j - 1 >= 0 && BattleData[SeqX, j - 1, 0] >= 0 && BattleData[SeqX, j - 1, 6] != PlayerN)//上
                                Hurt(SeqX, j, SeqX, j - 1, PlayerN, 0.5, game, gameTime);
                            if (SeqX - Pdis >= 0 && SeqX - Pdis < 17 && BattleData[SeqX - Pdis, j, 0] >= 0 && BattleData[SeqX - Pdis, j, 6] != PlayerN)//後
                                Hurt(SeqX, j, SeqX - Pdis, j, PlayerN, 0.5, game, gameTime);
                        }
                        if (A == -1)//無法攻擊(建築)
                            return -1;
                        if (A == 0)//成功攻擊
                        {
                            Over = false;
                            return 0;
                        }
                    }
                }
            else if (Choose == 3)//上
                for (int dis = SoldierDataP[PlayerN, BattleData[SeqX, j, 0], 5]; dis <= SoldierDataP[PlayerN, BattleData[SeqX, j, 0], 6]; dis++)//前方攻擊
                {
                    int Pdis = -dis;
                    if (j + Pdis >= 0 && j + Pdis < 9 && BattleData[SeqX, j + Pdis, 0] >= 0 && BattleData[SeqX, j + Pdis, 6] != PlayerN && BattleData[SeqX, j + Pdis, 0] != 9)//先判斷前方
                    {
                        if (BattleData[SeqX, j, 0] == 18)//戰象
                        {
                            if (j + 1 <= 8 && BattleData[SeqX, j + 1, 0] >= 0 && BattleData[SeqX, j + 1, 6] != PlayerN)//下
                                Hurt(SeqX, j, SeqX, j + 1, PlayerN, 0.5, game, gameTime);
                            if (SeqX - Pdis >= 0 && SeqX - Pdis < 17 && BattleData[SeqX - Pdis, j, 0] >= 0 && BattleData[SeqX - Pdis, j, 6] != PlayerN)//後
                                Hurt(SeqX, j, SeqX - Pdis, j, PlayerN, 0.5, game, gameTime);
                        }
                        A = Hurt(SeqX, j, SeqX, j + Pdis, PlayerN, 1, game, gameTime);
                        if (A == -1)//無法攻擊(建築)
                            return -1;
                        if (A == 0)//成功攻擊
                        {
                            Over = false;
                            return 0;
                        }
                    }
                }
            else if (Choose == 4)//後
                for (int dis = SoldierDataP[PlayerN, BattleData[SeqX, j, 0], 5]; dis <= SoldierDataP[PlayerN, BattleData[SeqX, j, 0], 6]; dis++)//後方攻擊
                {
                    int Pdis = dis;
                    if (PlayerN == 0)
                        Pdis = -Pdis;
                    if (SeqX + Pdis >= 0 && SeqX + Pdis < 17 && BattleData[SeqX + Pdis, j, 0] >= 0 && BattleData[SeqX + Pdis, j, 6] != PlayerN && BattleData[SeqX + Pdis, j, 0] != 9)//先判斷前方
                    {
                        A = Hurt(SeqX, j, SeqX + Pdis, j, PlayerN, 1, game, gameTime);
                        if (A == -1)//前方無法攻擊(建築)
                            return -1;
                        if (A == 0)//成功攻擊
                        {
                            Over = false;
                            return 0;
                        }
                    }
                }
            return -1;//沒攻擊
        }

        private static int Hurt(int a1, int a2, int b1, int b2, int a1P, double extra, Game game, GameTime gameTime)//傷害
        {
            //return   -1 無法攻擊 ， 0成功攻擊，1 迴避，


            int PlayerAtk = a1P;
            int PlayerDef = Math.Abs(1 - a1P);
            double Atk;//攻擊力
            int Def = (int)((SoldierDataP[PlayerDef, BattleData[b1, b2, 0], 4] + 20) / 2);//防禦力
            int dis = Math.Abs(a1 - b1) + Math.Abs(a2 - b2);//距離
            Boolean Ignore = false;//無視(重騎兵技能)
            int hurt;
            int extraCritical = 0;
            Random rd = new Random();


            if (SoldierDataP[PlayerDef, BattleData[b1, b2, 0], 0] == 0)//英雄迴避
            {
                if ((int)(rd.NextDouble() * 100) < Pking[PlayerDef, 2])//迴避發動
                    return 1;
            }

            if (PCN[PlayerDef, 2] == 4
                && SoldierDataP[PlayerDef, BattleData[b1, b2, 0], 0] == 5)//迴避(輕騎兵技能)
            {

                if ((int)(rd.NextDouble() * 4) == 0)//迴避發動
                    return 1;
            }

            if (BattleData[a1, a2, 0] != 6 && dis > 1 && BattleData[b1, b2, 0] >= 7 && BattleData[b1, b2, 0] <= 10)
                return -1;


            if (PCN[PlayerDef, 2] == 3
                 && SoldierDataP[PlayerAtk, BattleData[a1, a2, 0], 0] == 3
                 && BattleData[a1, a2, 1] * 5 <= Pking[PlayerAtk, 0] * 5 + 50)//底力(重騎兵技能)

                Atk = (SoldierDataP[PlayerAtk, BattleData[a1, a2, 0], 3] * (Pking[PlayerAtk, 0] * 5 + 50) * 0.7);
            else//沒發動
                Atk = SoldierDataP[PlayerAtk, BattleData[a1, a2, 0], 3] * BattleData[a1, a2, 1];

            //文明加成
            if (PCN[PlayerAtk, 0] == 1)//哥德
            {
                if (SoldierDataP[PlayerAtk, BattleData[a1, a2, 0], 0] == 1
                    && SoldierDataP[PlayerDef, BattleData[b1, b2, 0], 0] == 7)//步兵對建築+20%傷害
                    Atk = (Atk * 1.2);
            }
            else if (PCN[PlayerAtk, 0] == 3)//賽爾特
            {
                if (BattleData[a1, a2, 0] == 6
                    && SoldierDataP[PlayerDef, BattleData[b1, b2, 0], 0] == 7)//巨投對建築+30%傷害
                    Atk = (Atk * 1.3);
            }
            else if (PCN[PlayerAtk, 0] == 7)//波斯
            {
                if ((SoldierDataP[PlayerAtk, BattleData[a1, a2, 0], 0] == 3
                    || SoldierDataP[PlayerAtk, BattleData[a1, a2, 0], 0] == 5)
                    && SoldierDataP[PlayerDef, BattleData[b1, b2, 0], 0] == 4)//騎兵對弓兵+10%
                    Atk = (Atk * 1.1);
            }

            //兵種相剋攻擊成數
            if (BattleData[a1, a2, 0] == 1 || BattleData[a1, a2, 0] == 12 || BattleData[a1, a2, 0] == 14 || BattleData[a1, a2, 0] == 16 || BattleData[a1, a2, 0] == 19)//劍勇
            {
                if (SoldierDataP[PlayerDef, BattleData[b1, b2, 0], 0] == 3 || SoldierDataP[PlayerDef, BattleData[b1, b2, 0], 0] == 5)//遇到騎兵
                    Atk = (Atk * 0.7);
            }
            else if (BattleData[a1, a2, 0] == 2 || BattleData[a1, a2, 0] == 15)//戟兵
            {
                if (SoldierDataP[PlayerDef, BattleData[b1, b2, 0], 0] == 1)//遇到步兵
                    Atk = (Atk * 0.7);
            }
            else if (BattleData[a1, a2, 0] == 3 || BattleData[a1, a2, 0] == 18)//遊俠
            {
                if (SoldierDataP[PlayerDef, BattleData[b1, b2, 0], 0] == 2)//遇到槍兵
                    Atk = (Atk * 0.7);
            }
            else if (BattleData[a1, a2, 0] == 4 || BattleData[a1, a2, 0] == 7 || BattleData[a1, a2, 0] == 13 || BattleData[a1, a2, 0] == 20)//弩兵、箭塔
            {
                if (SoldierDataP[PlayerDef, BattleData[b1, b2, 0], 0] == 2)//遇到槍兵
                    Atk = (Atk * 1.3);
                else if (SoldierDataP[PlayerDef, BattleData[b1, b2, 0], 0] == 0)//遇到英雄
                    Atk = (Atk * 0.8);
                else if (BattleData[b1, b2, 0] == 12)//遇到哥德衛隊
                    Atk = (Atk * 0.8);
            }
            else if (BattleData[a1, a2, 0] == 5 || BattleData[a1, a2, 0] == 11)//輕騎兵、韃靼騎兵
            {
                if (SoldierDataP[PlayerDef, BattleData[b1, b2, 0], 0] == 2)//遇到槍兵
                    Atk = (Atk * 0.7);
                else if (SoldierDataP[PlayerDef, BattleData[b1, b2, 0], 0] == 0)//遇到英雄
                    Atk = (Atk * 1.15);
            }
            else if (BattleData[a1, a2, 0] == 6)//巨投
            {
                if (SoldierDataP[PlayerDef, BattleData[b1, b2, 0], 0] == 7)//遇到建築物
                    Atk = (Atk * 3);
                else if (SoldierDataP[PlayerDef, BattleData[b1, b2, 0], 0] == 0)//遇到英雄
                    Atk = (Atk * 0.2);
            }
            else if (BattleData[a1, a2, 0] == 7)//建塔
            {
                if (SoldierDataP[PlayerDef, BattleData[b1, b2, 0], 0] == 7)//遇到建築物
                    Atk = (Atk * 1.5);
                else if (SoldierDataP[PlayerDef, BattleData[b1, b2, 0], 0] == 0)//遇到英雄
                    Atk = (Atk * 0.5);
            }

            //特殊兵種額外加成
            if (BattleData[a1, a2, 0] == 11 || BattleData[a1, a2, 0] == 14 || BattleData[a1, a2, 0] == 15)//韃靼騎兵、菘藍武士、擲斧兵
            {
                if (SoldierDataP[PlayerDef, BattleData[b1, b2, 0], 0] == 7)//遇到建築
                    Atk = (Atk * 1.2);
            }
            else if (BattleData[a1, a2, 0] == 16)//日本武士
            {
                if (SoldierDataP[PlayerDef, BattleData[b1, b2, 0], 0] >= 11)//遇到特殊兵種
                    Atk = (Atk * 1.3);
                else if (SoldierDataP[PlayerDef, BattleData[b1, b2, 0], 0] == 0)//遇到英雄
                    Atk = (Atk * 1.2);
            }
            else if (BattleData[a1, a2, 0] == 19)//條頓武士
            {
                if (SoldierDataP[PlayerDef, BattleData[b1, b2, 0], 0] == 0)//遇到英雄
                    Atk = (Atk * 1.1);
            }

            //距離減傷
            if (dis > 1)
            {
                if (BattleData[a1, a2, 0] == 13 || BattleData[a1, a2, 0] == 17)//長弓兵、蒙古突騎
                    Atk = (Atk * (1 - 0.1 * (dis - 1)));
                else if (BattleData[a1, a2, 0] == 20 || BattleData[a1, a2, 0] == 7)//箭塔
                    Atk = (Atk * (1 - 0.12 * (dis - 1)));
                else if (BattleData[a1, a2, 0] == 4)//弩兵
                    Atk = (Atk * (1 - 0.15 * (dis - 1)));
                else if (BattleData[a1, a2, 0] == 5 || BattleData[a1, a2, 0] == 11)//輕騎兵、韃靼騎兵
                    Atk = (Atk * (1 - 0.2 * (dis - 1)));
                else if (BattleData[a1, a2, 0] == 6)//巨投
                    Atk = (Atk * (1 - 0.1 * (dis - 10)));
                else if (BattleData[a1, a2, 0] == 8)//砲塔
                    Atk = (Atk * (1 - 0.1 * (dis - 2)));
            }


            //兵種額外爆擊
            if (BattleData[a1, a2, 0] == 13 || BattleData[a1, a2, 0] == 17 || BattleData[a1, a2, 0] == 20)//長弓兵、蒙古突騎、中國連弩兵爆擊
                extraCritical = 5;
            else if (BattleData[a1, a2, 0] == 16 && BattleData[b1, b2, 0] >= 11)//日本武士爆擊
                extraCritical = 10;
            else if (BattleData[a1, a2, 0] == 19)//條頓武士爆擊
                extraCritical = 20;


            //爆擊率
            if (BattleData[a1, a2, 0] == 0 && (int)(rd.NextDouble() * 100) < Pking[PlayerAtk, 1])//英雄爆擊
                Atk *= 2.5;
            else if ((BattleData[a1, a2, 0] <= 5 || BattleData[a1, a2, 0] >= 11)//士兵才能發動
                && (BattleData[b1, b2, 0] <= 6 || BattleData[b1, b2, 0] >= 11)//目標不可為建築
                && (int)(rd.NextDouble() * 100) < (int)(Pking[PlayerAtk, 1] / 2) + extraCritical)//士兵爆擊
                Atk *= 2.5;


            Atk = (Atk * extra);//額外參數傷害

            if (PCN[PlayerDef, 2] == 2
                && SoldierDataP[PlayerDef, BattleData[b1, b2, 0], 0] == 1)//大盾(步兵技能)
            {
                if ((int)(rd.NextDouble() * 100) < 15)
                {
                    Atk /= 2;
                }
            }

            if (PCN[PlayerAtk, 1] == 3
                && SoldierDataP[PlayerAtk, BattleData[a1, a2, 0], 0] == 3
                && SoldierDataP[PlayerDef, BattleData[b1, b2, 0], 0] != 7)//無視(重騎兵技能)
            {
                if ((int)(rd.NextDouble() * 2) == 0)
                {//無視防禦發動
                    Ignore = true;
                }
            }

            //損失生命
            if (Ignore)
                hurt = (int)Atk;
            else
                hurt = (int)(Atk - (Def / 100));

            if (hurt >= 0)
            {
                int realdeduce = 0;//真實損失兵數

                if (BattleData[b1, b2, 5] - hurt <= 0)//陣亡
                {
                    realdeduce = BattleData[b1, b2, 5];
                    for (int i = 0; i < 7; i++)
                        BattleData[b1, b2, i] = -1;
                }
                else
                {

                    //玩家 統帥、勇武、敏捷

                    //BattleData[17, 9, 7]  X座標、Y座標、(0兵種、1數量、2順序、3戰鬥指令、4回合、5血量、6玩家)
                    //SoldierDataP[2, 21, 10]  0type, 1造價, 2血量,3攻擊,4防禦,  5射程(MIN),6射程(MAX),7移動,8數量限制,9目前的量
                    BattleData[b1, b2, 5] -= hurt;//扣血
                    if (BattleData[b1, b2, 0] <= 5 || BattleData[b1, b2, 0] >= 11)
                    {
                        if (BattleData[b1, b2, 0] == 0)
                        {
                            BattleData[b1, b2, 1] = (int)Math.Ceiling((double)BattleData[b1, b2, 5] / (double)(Pking[PlayerDef, 0] * 6 + 50));

                        }
                        else
                        {
                            BattleData[b1, b2, 1] = (int)Math.Ceiling((double)BattleData[b1, b2, 5] / (double)(Pking[PlayerDef, 0] * 5 + 50));
                        }
                    }

                    if (PCN[PlayerDef, 1] == 2
                          && SoldierDataP[PlayerDef, BattleData[b1, b2, 0], 0] == 1)//反彈(步兵技能)
                    {
                        int hurt2 = hurt / 2 * SoldierDataP[PlayerDef, BattleData[b1, b2, 0], 4] / 100;
                        hurt2 = hurt2 - SoldierDataP[PlayerAtk, BattleData[a1, a2, 0], 4] / 100;
                        if (BattleData[a1, a2, 5] <= hurt2)//陣亡
                        {
                            for (int i = 0; i < 7; i++)
                                BattleData[a1, a2, i] = -1;
                        }
                        else
                        {
                            BattleData[a1, a2, 5] -= hurt2;//扣血
                            if (BattleData[a1, a2, 0] <= 5 || BattleData[a1, a2, 0] >= 11)
                            {
                                if (BattleData[a1, a2, 0] == 0)//英雄
                                {
                                    BattleData[a1, a2, 1] = BattleData[a1, a2, 5] / (Pking[PlayerAtk, 0] * 6 + 50);
                                    if (BattleData[a1, a2, 5] % (Pking[PlayerAtk, 0] * 6 + 50) >= 0)
                                        BattleData[a1, a2, 1]++;
                                }
                                else
                                {
                                    BattleData[a1, a2, 1] = BattleData[a1, a2, 5] / (Pking[PlayerAtk, 0] * 5 + 50);
                                    if (BattleData[a1, a2, 5] % (Pking[PlayerAtk, 0] * 5 + 50) >= 0)
                                        BattleData[a1, a2, 1]++;
                                }
                            }
                        }
                    }
                }
                if (PCN[PlayerAtk, 2] == 1
                && SoldierDataP[PlayerAtk, BattleData[a1, a2, 0], 0] == 2)//抽取(槍兵技能)
                {
                    BattleData[a1, a2, 5] += (int)(realdeduce / 5);
                    if (BattleData[a1, a2, 5] > Pking[PlayerAtk, 0] * 5 + 50)
                    {
                        BattleData[a1, a2, 5] = Pking[PlayerAtk, 0] * 5 + 50;
                    }
                }

            }
            return 0;
        }

    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace HuntTheWumpus
{
    public class Control
    {
        enum ControlState
        {
            MainMenu,
            Cave,
            LastWindow,
            ScoreList,
            PickCave
        }
        enum StoryMG
        {
            Empty,
            Bat,
            Pit,
            Wumpus,
            BuyArrow,
            BuyHint
        }
        enum Hint
        {
            Wumpus,
            Pit,
            Bat,
            NoLuck,
            Empty
        }

        private ControlState state = ControlState.MainMenu;
        private ControlState OldState = ControlState.MainMenu;

        private int Width, Height;

        private bool MiniGameEnd = true;
        private MiniGame minigame;
        private bool CheckDanger;
        private StoryMG StoryMiniGame;
        private bool UseMiniGame;

        private Scores score;

        private List<string> HintMessage;

        int num = 0;
        private Map[] MapForPiсk;
        private Map map;
        private int[] seeds;
        private string seed = "";

        private bool IsWin;
        public Player player;

        public View view;

        public Control(int width, int height)
        {
            view = new View(width, height);
            Width = width;
            Height = height;
            view.InitEvent(KeyDown, MouseDown, MouseUp, MouseMove);
            view.ClearConsole();
            MapForPiсk = new Map[5];
            seeds = new int[5];
            HintMessage = new List<string>();
            HintMessage.Add("Wumpus in ");
            HintMessage.Add("Pit in ");
            HintMessage.Add("Bat in ");
            HintMessage.Add("You have bad luck...");
        }

        public void UpDate(long time)
        {
            if (state == ControlState.Cave)
            {
                view.DrawCave(map.graph, map.isActive, map.GetDangerList(), map.danger, map.Room, player.Coins, player.Arrow);
                if (!MiniGameEnd)
                {
                    minigame.TickTime();
                    minigame.DrawMiniGame(view.Graphics);
                    if (!minigame.Is_playing)
                    {
                        List<string> listachiv = new List<string>();
                        minigame.GetAchievement(listachiv);
                        score.getAchievement(listachiv);
                        if (!minigame.Is_Winner && StoryMiniGame != StoryMG.BuyArrow && StoryMiniGame != StoryMG.BuyHint)//не покупка 
                        {
                            IsWin = false;
                            EndGame();
                        }
                        if (minigame.Is_Winner && StoryMiniGame == StoryMG.BuyHint)
                        {
                            int rnd = Utily.Next() % HintMessage.Count;
                            Hint NowHint = (Hint)rnd;
                            int HintData = 0;
                            if (NowHint == Hint.Bat)
                                HintData = map.GetBat();
                            if (NowHint == Hint.Pit)
                                HintData = map.GetPit();
                            if (NowHint == Hint.Wumpus)
                                HintData = map.Wumpus;
                            if (NowHint != Hint.NoLuck)
                                view.AddComand(HintMessage[(int)NowHint] + HintData, true);
                            else
                                view.AddComand(HintMessage[(int)NowHint], true);
                        }
                        if (minigame.Is_Winner && StoryMiniGame == StoryMG.BuyArrow)
                        {
                            player.GiveArrows();
                            view.AddComand("You received 2 arrows", true);
                        }
                        if (minigame.Is_Winner && StoryMG.Wumpus == StoryMiniGame)
                        {
                            map.WumpusGoAway();
                            view.AddComand("Wumpus run away", true);
                        }
                        if (minigame.Is_Winner)
                        {
                            score.AddScores(50);
                        }
                        MiniGameEnd = true;
                    }
                }
                if (!CheckDanger && !view.IsBatAnimated && !view.IsAnimated)
                {
                    CheckDanger = true;
                    if (map.danger == Danger.Pit)
                    {
                        minigame = new MiniGame(Width, Height);
                        StoryMiniGame = StoryMG.Pit;
                        minigame.InitializeMiniGame(2);
                        UseMiniGame = true;
                        MiniGameEnd = false;
                    }
                    if (map.danger == Danger.Bat)
                    {
                        view.AddComand("You met BAT", true);
                        map.Respaw();
                    }
                    if (map.danger == Danger.Wumpus)
                    {
                        minigame = new MiniGame(Width, Height);
                        StoryMiniGame = StoryMG.Wumpus;
                        MiniGameEnd = false;
                        UseMiniGame = true;
                        minigame.InitializeMiniGame(3);
                    }
                    Danger dangerabout = map.GetDangerAbout();
                    if (dangerabout == Danger.Bat)
                        view.AddComand("Bats Nearby(" + (map.Room + 1) + ")", true);
                    if (dangerabout == Danger.Pit)
                        view.AddComand("I feel a draft(" + (map.Room + 1) + ")", true);
                    if (dangerabout == Danger.Wumpus)
                        view.AddComand("I smell a Wumpus!(" + (map.Room + 1) + ")", true);
                }
                if (map.IsWin && view.IsEndAnimation())
                {
                    IsWin = true;
                    EndGame();
                }
                else if (player.Arrow == 0 && view.IsEndAnimation())
                {
                    IsWin = false;
                    EndGame();
                }
            }

            if (state == ControlState.MainMenu)
            {
                view.DrawMainMenu();
            }

            if (state == ControlState.PickCave)
            {
                view.DrawPickCave(MapForPiсk[num].graph, MapForPiсk[num].isActive, num, seed);
            }

            if (time > 0)
                view.DrawText((1000 / time).ToString(), 5, 5, 10);
            if (score != null)
            {
                score.TickTime();
                score.Draw(view.Graphics);
            }
        }

        void ContinueMenu()
        {
            state = OldState;
            if (state == ControlState.Cave && minigame.Is_playing)
                minigame.Pause(false);
        }

        void NewGame()
        {
            for (int i = 0; i < 5; ++i)
            {
                seeds[i] = Utily.Next();
                Utily.ChangeSeed(seeds[i]);
                MapForPiсk[i] = new Map();
            }
            num = 0;
            MiniGameEnd = true;
            minigame = new MiniGame(Width, Height);
            player = new Player();
            score = new Scores(Width, Height);
            score.active = ScoreState.Achievements;
            CheckDanger = false;
            IsWin = false;
            StoryMiniGame = StoryMG.Empty;
            view.UpdateImage();
            view.ClearConsole();
            seed = "";
            state = ControlState.PickCave;
            UseMiniGame = false;
        }

        private void EndGame()
        {
            state = ControlState.LastWindow;
            if (!UseMiniGame)
            {
                List<string> ls = new List<string>();
                ls.Add("MG1.png/namedlyaneuzalMiniGame#mbvseponatno");
                score.getAchievement(ls);
            }
            score.SetFinalState(false);
        }

        public void KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                if (state == ControlState.MainMenu)
                    ContinueMenu();
                else if (state == ControlState.Cave)
                {
                    OldState = ControlState.Cave;
                    state = ControlState.MainMenu;
                    if (minigame.Is_playing)
                        minigame.Pause(true);
                }
                else if (state == ControlState.LastWindow)
                    state = ControlState.ScoreList;
                else if (state == ControlState.ScoreList)
                    state = ControlState.MainMenu;
                else if (state == ControlState.PickCave)
                {
                    state = ControlState.MainMenu;
                    OldState = ControlState.PickCave;
                }
            }
            if (e.KeyCode >= Keys.D0 && e.KeyCode <= Keys.D9)
            {
                if (state == ControlState.PickCave && seed.Length < 16)
                {
                    seed += (e.KeyCode - Keys.D0);
                }
            }
            if (e.KeyCode == Keys.Back)
            {
                if (state == ControlState.PickCave)
                {
                    if (seed.Length > 0)
                        seed = seed.Remove(seed.Length - 1);
                }
                score.KeyDown("del");
            }
            if (e.KeyCode == Keys.Enter)
            {
                score.KeyDown("enter");
            }
            if (score != null)
            {
                score.KeyDown((new KeysConverter()).ConvertToString(e.KeyCode));
            }
        }

        public void MouseDown(object sender, MouseEventArgs e)
        {
            if (state == ControlState.Cave && !MiniGameEnd)
            {
                minigame.Down(e);
            }
            if (state == ControlState.Cave && MiniGameEnd && view.IsEndAnimation())
            {
                view.StopAnimation();
                RegionCave rg = view.GetRegionCave(e.X, e.Y);
                if (rg >= 0 && (int)rg < 6 && map.isActive[map.Room][(int)rg])
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        int add = map.Move((int)rg);
                        player.AddCoins(add);
                        score.AddScores(5 * add);
                        if (add == 1)
                            view.AddComand("You pick up coin", true);
                        else if (add > 1)
                            view.AddComand("You pick up " + add + " coins", true);
                        if (map.danger == Danger.Bat)
                            view.StartBatAnimation();
                        view.StartMoveAnimation((int)rg);
                        CheckDanger = false;
                        List<string> achiv = new List<string>();
                        map.GetAchievement(achiv);
                        score.getAchievement(achiv);
                    }
                    else
                    {
                        player.PushArrow();
                        map.PushArrow((int)rg);
                        view.StartArrowAnimation((int)rg);
                        List<string> achiv = new List<string>();
                        player.GetAchievement(achiv);
                        score.getAchievement(achiv);
                    }
                }
                if (rg == RegionCave.BuyArrow)
                {
                    if (player.CanBuyArrow())
                    {
                        StoryMiniGame = StoryMG.BuyArrow;
                        MiniGameEnd = false;
                        minigame = new MiniGame(Width, Height);
                        minigame.InitializeMiniGame(1);
                        UseMiniGame = true;
                        player.BuyArrows();
                    }
                    else
                        view.AddComand("Not enough coins(need " + player.NeedForBuyArrows().ToString() + ")", true);
                }
                if (rg == RegionCave.BuyHint)
                {
                    if (player.CanBuyHint())
                    {
                        StoryMiniGame = StoryMG.BuyHint;
                        MiniGameEnd = false;
                        minigame = new MiniGame(Width, Height);
                        minigame.InitializeMiniGame(2);
                        UseMiniGame = true;
                        player.BuyHint();
                    }
                    else
                        view.AddComand("Not enough coins(need " + player.NeedForBuyHint().ToString() + ")", true);
                }
                if (rg == RegionCave.UpConsole)
                    view.ChangeIndex(1);
                if (rg == RegionCave.DownConsole)
                    view.ChangeIndex(-1);
            }
            if (state == ControlState.MainMenu)
            {
                RegionMenu rg = view.GetRegionMainMenu(e.X, e.Y);
                if (rg == RegionMenu.NewGame)
                {
                    NewGame();
                }
                if (rg == RegionMenu.Continue)
                {
                    ContinueMenu();
                }
                if (rg == RegionMenu.ScoreList)
                {
                    state = ControlState.ScoreList;
                }
                if (rg == RegionMenu.Exit)
                {
                    Application.Exit();
                }
            }
            if (state == ControlState.PickCave)
            {
                RegionPickCave rg = view.GetRegionPickCave(e.X, e.Y);
                if ((int)rg < 5)
                {
                    num = (int)rg;
                }
                if (rg == RegionPickCave.Play)
                {
                    long nowseed = seeds[num];
                    if (seed != "")
                        nowseed = long.Parse(seed);
                    Utily.ChangeSeed(nowseed);
                    map = new Map();
                    state = ControlState.Cave;
                    view.AddComand("Map's seed " + nowseed, false);
                }
            }
        }

        public void MouseUp(object sender, MouseEventArgs e)
        {
            if (state == ControlState.Cave && !MiniGameEnd)
            {
                minigame.Up(e);
            }
            if (state == ControlState.LastWindow)
                score.MouseUp(e);
        }

        public void MouseMove(object sender, MouseEventArgs e)
        {
            if (state == ControlState.Cave && !MiniGameEnd)
            {
                minigame.Move(e);
            }
            if (state == ControlState.LastWindow)
                score.MouseMove(e);
        }
    }
}


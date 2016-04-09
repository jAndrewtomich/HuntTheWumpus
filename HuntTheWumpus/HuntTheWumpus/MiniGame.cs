﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

namespace HuntTheWumpus
{
    public class MiniGame
    {
        // This is TRUE, if player winner at last game, else it's FALSE
        public bool Is_Winner { get; private set; }
        // This is TRUE, if now game initialized and playing, else it's FALSE
        public bool Is_playing { get; private set; }

		// This difficult of last game
		public int Difficult { get; private set; }

        // If now analytics method is running it's TRUE
        // for locking mouse events
        private bool Is_Analytics = false;

		// This is TRUE, if mini-game paused else it's FALSE
		private bool Is_Pause = false;

		// This is true, if user began drawing
		private bool Is_Mouse_Down = false;

        // While this bigger than zero, game playing
        private int PlayerPoints = 500;

		// This timer can say milliseconds with last called time
		private Stopwatch Event_timer = new Stopwatch();

		// This array have figures file names
        List<string> files_Difficulties = new List<string>();

        // This integers for set height and width canvas
        // and correct drawing figure
        private int CanvasHeight;
        private int CanvasWidth;

        // Coordinats of circles points
        private List<int> CircleCoordinateX = new List<int>();
        private List<int> CircleCoordinateY = new List<int>();

        // Coordinats of mouse cursors positions,
        // when user draw a figure
        private List<int> MousePositionsX = new List<int>();
        private List<int> MousePositionsY = new List<int>();

        // Radius of point
        private const int radius = 10;

        private const int MaxPointsFromOneFigure = 1200;
        private const float Speed_Change_Scale_Distance = 1.0f;

        // Scale for transform figure position to screen position 
        private float Scale_Distance = 20.0f;

		// Timer that we must get 500 points
		private int LifeTimer = 15000;

		// Constant of Max time for LifeTimer
		private const int MaxLife = 15000;

		// Very very much animations properties...
		// ...without comments

		private int ProgressBarSettedAngle = 0;
		private int ProgressBarDrawingAngle = 0;
		private int ProgressBarSpeedChangeAngle = 288;

		private const int MaximalNegative = 255;
		private const int MinimalNegative = 85;
		private int NowNegative = 255;
		private int NowChange = -1;
		private const int SpeedChangeNegative = 250;

		private bool Faster = false;
		private bool Reactivity = false;
		private bool Zinger = false;

		private bool Accurate = false;
		private bool NoName76 = false;
		private bool Sniper = false;

        /// Class constructor, say me window's width and height
        public MiniGame(int DrawingWidth, int DrawingHeight)
        {
            Is_Winner = true;
            CanvasWidth = DrawingWidth / 2;
            CanvasHeight = DrawingHeight / 2;
        }

        /// Read and apply info about figure
        private void Set_Figure(string filename)
        {
			//* Clear memory about last drawing figure... *//
			Scale_Distance = 20.0f;
            CircleCoordinateX.Clear();
            CircleCoordinateY.Clear();
            MousePositionsX.Clear();
            MousePositionsY.Clear();
            //* ...and read info about new figure */
            StreamReader file = new StreamReader(@"" + filename);
            string line = "";
            while ((line = file.ReadLine()) != null)
            {
                string[] points = line.Split(' ').ToArray();
                CircleCoordinateX.Add(-int.Parse(points[0]));
                CircleCoordinateY.Add(-int.Parse(points[1]));
            }
            Scale_Distance = 20.0f;
        }

        /// Call this method for create new game
        public void InitializeMiniGame(int difficult)
        {
			Difficult = difficult;
			//* Read info about game level and set random figure *//
			LifeTimer = MaxLife;
            files_Difficulties.Clear();
            Is_playing = true;
            StreamReader file = new StreamReader(@"Difficulties" + difficult.ToString() + ".txt");
            string line = "";
            while ((line = file.ReadLine()) != null)
            {
                files_Difficulties.Add(line);
            }
            file.Close();
            Random range = new Random();
            Set_Figure(files_Difficulties[range.Next(0, files_Difficulties.Count)]);
			Event_timer = new Stopwatch();
			Event_timer.Start();
        }

        /// Call this method, then game playing
        public void DrawMiniGame(Graphics g)
        {
            //* If game not initialized, then we not drawing *//
            if (!Is_playing || Is_Pause)
            {
                return;
            }
            //* Clear window and drawing the points *//
            g.FillRectangle(new SolidBrush(Color.FromArgb(80, 0, 0, 0)), 0, 0, CanvasWidth * 2, CanvasHeight * 2);
           
            for (int i = 0; i < CircleCoordinateX.Count && !Is_Analytics; ++i)
            {
                g.DrawEllipse(Pens.White, new Rectangle(
                    (int)(CircleCoordinateX[i] * Scale_Distance) + CanvasWidth,
                    (int)(CircleCoordinateY[i] * Scale_Distance) + CanvasHeight,
                    2 * radius, 2 * radius));
            }
			/* Here we drawing user's line */
			Pen line_marker = new Pen(Color.FromArgb(150, 0, 255, 0));
			line_marker.Width = 5;
			for (int i = 1; i < MousePositionsX.Count && !Is_Analytics; ++i) {
				g.DrawLine(line_marker, MousePositionsX[i - 1], MousePositionsY[i - 1], MousePositionsX[i], MousePositionsY[i]);
			}
			Font f = new Font("Arial", 40);
			Brush brush = new SolidBrush(Color.FromArgb(50, 0, 0, 255));
			g.DrawString(PlayerPoints.ToString(), f, brush, CanvasWidth - 50, CanvasHeight - 20);
			line_marker.Color = Color.FromArgb(255 * (360 - ProgressBarDrawingAngle) / 360, 255 * ProgressBarDrawingAngle / 360, 0);
			g.DrawArc(line_marker, new Rectangle(CanvasWidth / 2, CanvasHeight / 6, CanvasHeight * 3 / 2, CanvasHeight * 3 / 2), -90, ProgressBarDrawingAngle);
			line_marker.Color = Color.FromArgb(NowNegative, 255 * (MaxLife - LifeTimer) / MaxLife, 255 * LifeTimer / MaxLife, 0);
			g.DrawArc(line_marker, new Rectangle(CanvasWidth / 2 - 10, CanvasHeight / 6 - 10, CanvasHeight * 3 / 2 + 20, CanvasHeight * 3 / 2 + 20), -90, LifeTimer * 360 / MaxLife);
        }

        public void TickTime()
        {
			long Milliseconds = Event_timer.ElapsedMilliseconds;
            if (Is_Analytics || !Is_playing || Is_Pause)
            {
                return;
            }
			if (ProgressBarDrawingAngle < ProgressBarSettedAngle) {
				ProgressBarDrawingAngle += ProgressBarSpeedChangeAngle * (int)Milliseconds / 1000;
				ProgressBarDrawingAngle = Math.Min(ProgressBarDrawingAngle, 360);
			}
			LifeTimer -= (int)Milliseconds;
			if (LifeTimer <= 0) {
				Is_Winner = false;
				Is_playing = false;
			}
			if (LifeTimer < MaxLife / 2) {
				if (NowNegative >= MaximalNegative) {
					NowChange = -1;
				}
				if (NowNegative <= MinimalNegative) {
					NowChange = 1;
				}
				NowNegative += NowChange * SpeedChangeNegative * (int)Milliseconds / 1000;
				NowNegative = Math.Min(NowNegative, 255);
				NowNegative = Math.Max(NowNegative, 0);
			}
			Scale_Distance += Speed_Change_Scale_Distance * Milliseconds / 1000;
			Event_timer.Restart();
        }

        private void Analytics()
        {
			Event_timer.Stop();
            int points = MaxPointsFromOneFigure;
            List<int> EndCoordinatsX = new List<int>();
            List<int> EndCoordinatsY = new List<int>();
            /* Set now coordinats, because tickTime can change coordinats */
            for (int i = 0; i < CircleCoordinateX.Count; ++i)
            {
                EndCoordinatsX.Add((int)(CircleCoordinateX[i] * Scale_Distance) + CanvasWidth);
                EndCoordinatsY.Add((int)(CircleCoordinateY[i] * Scale_Distance) + CanvasHeight);
            }
            /* Get how good drawing the figure */
            for (int i = 0; i < EndCoordinatsX.Count; ++i)
            {
                double minimal_distance = 100000.0f;
                for (int j = 0; j < MousePositionsX.Count; ++j)
                {
                    double now_distance = Math.Sqrt(Math.Pow(EndCoordinatsX[i] - MousePositionsX[j], 2) + Math.Pow(EndCoordinatsY[i] - MousePositionsY[j], 2));
                    minimal_distance = Math.Min(now_distance, minimal_distance);
                }
                if (minimal_distance >= radius)
                {
                    points -= MaxPointsFromOneFigure / EndCoordinatsX.Count;
                }
                else {
                    points -= (int)(MaxPointsFromOneFigure * (minimal_distance / radius) / EndCoordinatsX.Count);
                }
            }
            /* "Added" points 
				if points bigger than zero
				create new figure
			*/
            PlayerPoints -= points;
			ProgressBarSettedAngle = 360 * (500 - PlayerPoints) / 500;
			ProgressBarSettedAngle = Math.Min(ProgressBarSettedAngle, 360);
			if (PlayerPoints <= 0)
            {
				Faster = Faster || (Difficult == 1 && LifeTimer >= 0);
				Reactivity = Reactivity || (Difficult == 2 && LifeTimer >= 0);
				Zinger = Zinger || (Difficult == 3 && LifeTimer >= 0);
				Accurate = Accurate || (Difficult == 1 && points >= 500 && PlayerPoints == 500 - points);
				NoName76 = NoName76 || (Difficult == 2 && points >= 500 && PlayerPoints == 500 - points);
				Sniper = Sniper || (Difficult == 3 && points >= 500 && PlayerPoints == 500 - points);
				Is_playing = false;
                Is_Winner = true;
            }
            else {
                Random range = new Random();
                Set_Figure(files_Difficulties[range.Next(0, files_Difficulties.Count)]);
            }
            Is_Analytics = false;
        }

		public void Pause(bool setting) {
			Is_Pause = setting;
			if (setting) {
				Event_timer.Stop();
			} else {
				Event_timer.Start();
			}
		}

        public void Down(MouseEventArgs e)
        {
			if (Is_Pause || Is_Analytics || !Is_playing) {
				return;
			}
			Is_Mouse_Down = true;
			MousePositionsX.Add(e.X);
			MousePositionsY.Add(e.Y);
        }

        public void Up(MouseEventArgs e)
        {
			MousePositionsX.Add(e.X);
			MousePositionsY.Add(e.Y);
			Is_Analytics = true;
			Is_Mouse_Down = false;
			Analytics();
        }

        public void Move(MouseEventArgs e)
        {
			if (Is_Pause || Is_Analytics || !Is_playing || !Is_Mouse_Down) {
				return;
			}
			MousePositionsX.Add(e.X);
			MousePositionsY.Add(e.Y);
        }

		public void GetAchievement(List<string> acv) {
			if (Faster) {
				acv.Add("Bottle.png/БЫСТРЫЙ#Пройти миниигру#на 1 уровне#сложности менее чем#за 7 секунд");
			}
			if (Reactivity) {
				acv.Add("Sunduk.png/РЕАКТИВНЫЙ#Пройти миниигру#на 2 уровне#сложности менее чем#за 7 секунд");
			}
			if (Zinger) {
				acv.Add("Demon.png/ЖИВЧИК#Пройти миниигру#на 3 уровне#сложности менее чем#за 7 секунд");
			}
			if (Accurate) {
				acv.Add("Bottle.png/ТОЧНЫЙ#Завершить миниигру#на 1 уровне#сложности нарисовав#одну фигуру");
			}
			if (NoName76) {
				acv.Add("Sunduk.png/NoName76#Завершить миниигру#на 2 уровне#сложности нарисовав#одну фигуру");
			}
			if (Sniper) {
				acv.Add("Demon.png/СНАЙПЕР#Завершить миниигру#на 3 уровне#сложности нарисовав#одну фигуру");
			}
		}
    }
}

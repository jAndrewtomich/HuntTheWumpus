﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;
using System.Collections.Specialized;

namespace HuntTheWumpus {

	enum StateFinal {
		Result,
		VKcomWaiting,
		FaceBookWaiting
	}

	public class Scores {
		// Player score
		public int Score { get; private set; }
		public bool Final { get; set; }

		private int CanvasWidth;
		private int CanvasHeight;

		// Queue at draw achievements
		private List<string> Queue = new List<string>();
		private List<string> WasAchievements = new List<string>();

		private Image activeImage = null;
		private Image BackGround = null;
		private Image FinalPicture = null;

		private const double BeginDrawingPosition = -100;
		private const double EndDrawingPosition = 0;
		private const int TimerShowStarting = 3000;

		private string MessageAchievement = "";

		private Stopwatch Event_timer = new Stopwatch();

		private double AchievementDrawingPosition = 0;
		private double SpeedChangeDrawingPosition = 45;
		private int TimerShowAchievement = 3000;
		private int NowChange = 1;
		private StateFinal state;
		private VKApi vk;
		private bool Winner = false;

		// This properties for draw button "Share in VK"
		private int ShareVKbuttonX;
		private int ShareVKbuttonY;
		private int ShareVKbuttonSize = 30;
		private CompressionImage Vk;

		public Scores(int Width, int Height) {
			CanvasWidth = Width;
			CanvasHeight = Height;
			ShareVKbuttonX = 50;
			ShareVKbuttonY = Height - ShareVKbuttonSize - 40;
			Vk = new CompressionImage("data/ShareVK.png", ShareVKbuttonSize, ShareVKbuttonSize);
			BackGround = Image.FromFile("data/Achievements/BackGround.png");
			List<string> bb = new List<string>();
			Event_timer.Start();
		}

		/// Adding score
		public void AddScores(int add) {
			Score += add;
		}

		public void getAchievement(List<string> achievements) {
			for (int i = 0; i < achievements.Count; ++i) {
				if (WasAchievements.IndexOf(achievements[i]) == -1) {
					Queue.Add(achievements[i]);
					AddScores(100);
					WasAchievements.Add(achievements[i]);
				}
			}
		}

		public void TickTime() {
			if (Final && vk != null) {
				vk.Access_authorize();
				return;
			}
			long Milliseconds = Event_timer.ElapsedMilliseconds;
			if (Queue.Count > 0 && activeImage == null) {
				activeImage = Image.FromFile("data/Achievements/" + Queue[0].Split('/')[0]);
				MessageAchievement = Queue[0].Split('/')[1];
				AchievementDrawingPosition = BeginDrawingPosition;
				TimerShowAchievement = TimerShowStarting;
			}
			if (TimerShowAchievement <= 0) {
				NowChange = -1;
			} else {
				NowChange = 1;
			}
			if (activeImage != null && EndDrawingPosition >= AchievementDrawingPosition && (TimerShowAchievement <= 0 || TimerShowAchievement == TimerShowStarting)) {
				AchievementDrawingPosition += NowChange * SpeedChangeDrawingPosition * Milliseconds / 1000.0f;
				AchievementDrawingPosition = Math.Min(AchievementDrawingPosition, EndDrawingPosition);
			}
			if (AchievementDrawingPosition == EndDrawingPosition && activeImage != null) {
				AchievementDrawingPosition = EndDrawingPosition;
				TimerShowAchievement -= (int)Milliseconds;
			}
			if (AchievementDrawingPosition < BeginDrawingPosition) {
				Queue.Remove(Queue[0]);
				activeImage = null;
				AchievementDrawingPosition = BeginDrawingPosition;
			}
			Event_timer.Restart();
		}

		public void DrawScores(Graphics g) {
			Font f = new Font("Arial", 15);
			Brush brush = new SolidBrush(Color.Yellow);
			//g.DrawString("Score: " + Score.ToString(), f, brush, 0, 20);
			int width = CanvasWidth / 5; //6
			int height = CanvasHeight / 8; //10
			if (activeImage != null) {
				g.DrawImage(BackGround, new Rectangle(CanvasWidth - width, (int)AchievementDrawingPosition, width, height));
				g.DrawImage(activeImage, new Rectangle(CanvasWidth - width + 5, (int)AchievementDrawingPosition + height / 4, height / 2, height / 2));
				g.DrawString("New achievement", new Font("Colibri", 8), new SolidBrush(Color.LimeGreen), CanvasWidth - width / 3 * 2, (float)AchievementDrawingPosition + height / 6);
				string[] strings = MessageAchievement.Split('#');
				for (int i = 0; i < strings.Length; ++i) {
					g.DrawString(strings[i], new Font("Colibri", 8), new SolidBrush(Color.LimeGreen), CanvasWidth - width / 3 * 2, (float)AchievementDrawingPosition + height / 3 + i * 12);
				}
            }
            
		}

		public void SetFinalState(bool isWinner) {
			Winner = isWinner;
			Final = true;
			FinalPicture = Image.FromFile("data/Final.png");
		}

		private void DrawFinalForShare() {
			Bitmap b = new Bitmap(CanvasWidth,CanvasHeight);
			Graphics g = Graphics.FromImage(b);
			g.DrawImage(FinalPicture, 0, 0);
			string WinStatus = "";
			Brush StatusBrush;
			if (Winner) {
				WinStatus = "Победа";
				StatusBrush = new SolidBrush(Color.Green);
			} else {
				WinStatus = "Поражение";
				StatusBrush = new SolidBrush(Color.Red);
			}
			g.DrawString(WinStatus, new Font("Arial", 35), StatusBrush, 78, 78);
			g.DrawString("Очков набрано " + Score.ToString(), new Font("Arial", 20),  new SolidBrush(Color.SteelBlue), 75, 230);
			int activestring = 0;
			int DrawAchivements = 0;
			for (int i = 0; i < WasAchievements.Count; ++i) {
				if (DrawAchivements >= 3) {
					++activestring;
					DrawAchivements = 0;
				}
				Image img = Image.FromFile("data/Achievements/" + WasAchievements[i].Split('#')[0]);
				g.DrawImage(img, 80 + DrawAchivements * 140, 430 + activestring * 46, 140, 46);
				++DrawAchivements;
			}
			b.Save("data/Share.jpg");
		}

		public void DrawFinal(Graphics g) {
			g.DrawImage(FinalPicture, 0, 0);
			string WinStatus = "";
			Brush StatusBrush;
			if (Winner) {
				WinStatus = "Победа";
				StatusBrush = new SolidBrush(Color.Green);
			} else {
				WinStatus = "Поражение";
				StatusBrush = new SolidBrush(Color.Red);
			}
			g.DrawString(WinStatus, new Font("Arial", 35), StatusBrush, 78, 78);
			g.DrawString("Очков набрано " + Score.ToString(), new Font("Arial", 20),  new SolidBrush(Color.SteelBlue), 75, 230);
			int activestring = 0;
			int DrawAchivements = 0;
			for (int i = 0; i < WasAchievements.Count; ++i) {
				if (DrawAchivements >= 3) {
					++activestring;
					DrawAchivements = 0;
				}
				Image img = Image.FromFile("data/Achievements/" + WasAchievements[i].Split('#')[0]);
				g.DrawImage(img, 80 + DrawAchivements * 140, 430 + activestring * 46, 140, 46);
				++DrawAchivements;
			}
			Vk.Draw(g, ShareVKbuttonX, ShareVKbuttonY);
		}

		bool Clicked(int buttonX, int buttonY, int buttonSize, int x, int y) {
			return (buttonX < x && buttonX + buttonSize > x && buttonY < y && buttonY + buttonSize > y);
		}

		public void MouseUp(MouseEventArgs e) {
			if (Final && e.Button == MouseButtons.Left && Clicked(ShareVKbuttonX, ShareVKbuttonY, ShareVKbuttonSize, e.X, e.Y)) {
				DrawFinalForShare();
				vk = new VKApi();
				vk.OauthAutorize();
			}
		}

	}
}

﻿using System;
using System.IO;
using System.Windows.Forms;
using Freeserf.Renderer.OpenTK;
using Orientation = Freeserf.Renderer.OpenTK.Orientation;
using Freeserf.Render;

namespace Freeserf
{
    public partial class FreeserfForm : Form
    {
        GameView gameView = null;
        Game game = null;

        public FreeserfForm()
        {
            InitializeComponent();
        }

        private void FreeserfForm_Load(object sender, EventArgs e)
        {
            Log.SetFile(File.Create(Path.Combine(Program.ExecutablePath, "log.txt")));
            Log.SetLevel(Log.Level.Verbose);

            // TODO: for now we just load DOS data (test path)
            DataSourceDos dosData = new DataSourceDos(Path.Combine(Program.ExecutablePath, "SPAE.PA"));

            if (!dosData.Load())
            {
                MessageBox.Show(this, "Error loading DOS data.", "Error");
                Close();
            }

            gameView = new GameView(dosData, new Size(1024, 768), DeviceType.Desktop, SizingPolicy.FitRatio, OrientationPolicy.Fixed);

            gameView.Resize(RenderControl.Width, RenderControl.Height, Orientation.LandscapeLeftRight);          

            var random = new Random();
            var gameInfo = new GameInfo(random);

            if (!GameManager.Instance.StartGame(gameInfo.GetMission(29), gameView))
                throw new ExceptionFreeserf("Failed to start game.");

            game = GameManager.Instance.GetCurrentGame();

            game.Map.AttachToRenderLayer(gameView.GetLayer(Layer.Landscape), dosData);

            var pos = game.GetPlayer(0u).CastlePos;
            uint column = game.Map.PosColumn(pos);
            uint row = game.Map.PosRow(pos);

            game.Map.ScrollTo(column, row);

            RenderControl.MouseWheel += RenderControl_MouseWheel;

            FrameTimer.Start();
        }

        private void RenderControl_MouseWheel(object sender, MouseEventArgs e)
        {
            if (gameView.Zoom > 0.0f && e.Delta < 0)
                gameView.Zoom -= 0.5f;
            else if (gameView.Zoom < 4.0f && e.Delta > 0)
                gameView.Zoom += 0.5f;
        }

        private void FrameTimer_Tick(object sender, EventArgs e)
        {
            RenderControl.MakeCurrent();

            game.Update();
            gameView.Render();

            RenderControl.SwapBuffers();
        }

        int lastX = int.MinValue;
        int lastY = int.MinValue;

        private void RenderControl_MouseMove(object sender, MouseEventArgs e)
        {
            Position pos = gameView.ScreenToView(new Position(e.X, e.Y));

            if (e.Button == MouseButtons.Right)
            {
                if (pos == null || lastX == int.MinValue)
                    return;

                int diffX = pos.X - lastX;
                int diffY = pos.Y - lastY;
                int scrollX = diffX / 32;
                int scrollY = diffY / 20;

                game.Map.Scroll(-scrollX, -scrollY);

                int remainingX = diffX % 32;
                int remainingY = diffY % 20;

                lastX = pos.X - remainingX;
                lastY = pos.Y - remainingY;
            }
            else
            {
                lastX = int.MinValue;
            }
        }

        private void RenderControl_MouseDown(object sender, MouseEventArgs e)
        {
            lastX = int.MinValue;

            Position pos = gameView.ScreenToView(new Position(e.X, e.Y));

            if (pos == null)
                return;

            lastX = pos.X;
            lastY = pos.Y;
        }
    }
}

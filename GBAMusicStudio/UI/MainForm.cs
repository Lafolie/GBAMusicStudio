﻿using GBAMusicStudio.Core;
using System;
using System.IO;
using System.Windows.Forms;

namespace GBAMusicStudio.UI
{
    public partial class MainForm : Form
    {
        internal static readonly byte RefreshRate = 60;

        bool stopUI = false;

        public MainForm()
        {
            InitializeComponent();
            FormClosing += MainForm_FormClosing;
            timer1.Tick += Timer1_Tick;
            MusicPlayer.Instance.SongEnded += () => stopUI = true;
            codeLabel.Text = gameLabel.Text = creatorLabel.Text = tempoLabel.Text = "";
        }

        void Timer1_Tick(object sender, EventArgs e)
        {
            if (stopUI)
            {
                Stop(null, null);
                return;
            }
            var (tempo, positions, volumes, delays, notes, velocities, voices, mods, bends, pans, types) = MusicPlayer.Instance.GetTrackStates();
            trackInfoControl1.ReceiveData(positions, volumes, delays, notes, velocities, voices, mods, bends, pans, types);
            tempoLabel.Text = string.Format("Tempo - {0}", tempo);
        }

        void OpenROM(object sender, EventArgs e)
        {
            var d = new OpenFileDialog
            {
                Title = "Open GBA ROM",
                Filter = "GBA files|*.gba",
            };
            if (d.ShowDialog() != DialogResult.OK) return;

            Stop(null, null);
            new ROM(d.FileName);

            // Set song numerical num
            Text = "GBA Music Studio - " + Path.GetFileName(d.FileName); // Make song name instead
            codeLabel.Text = ROM.Instance.GameCode;
            gameLabel.Text = ROM.Instance.Config.GameName;
            creatorLabel.Text = ROM.Instance.Config.CreatorName;
            playButton.Enabled = true;
        }

        void Play(object sender, EventArgs e)
        {
            pauseButton.Enabled = stopButton.Enabled = true;
            MusicPlayer.Instance.Play((ushort)songNumerical.Value);
            timer1.Interval = (int)(1000f / RefreshRate);
            timer1.Start();
        }
        void Pause(object sender, EventArgs e)
        {
            stopButton.Enabled = MusicPlayer.Instance.State != State.Playing;
            MusicPlayer.Instance.Pause();
        }
        void Stop(object sender, EventArgs e)
        {
            stopUI = pauseButton.Enabled = stopButton.Enabled = false;
            tempoLabel.Text = "";
            timer1.Stop();
            MusicPlayer.Instance.Stop();
            trackInfoControl1.Invalidate();
        }
        
        void MainForm_FormClosing(object sender, FormClosingEventArgs e) => Stop(null, null);
    }
}

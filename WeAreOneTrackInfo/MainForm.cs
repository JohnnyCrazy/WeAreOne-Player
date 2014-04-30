using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Speech.Synthesis;
using Timer = System.Timers.Timer;
using WMPLib;
using System.Net;
using Un4seen.Bass;

namespace WeAreOneTrackInfo
{
    public partial class MainForm : Form
    {
        WeAreOne wao;
        Timer timer;

        int iconindex = 0;
        Timer icontimer;
        
        Radio current;
        String labeltext = "Song: {0}" + Environment.NewLine + "Artist: {1}" +
            Environment.NewLine + "Show: {2}" + 
            Environment.NewLine + "Moderator: {3}" + 
            Environment.NewLine + "Ende der Show: {4}" +
            Environment.NewLine + "Listener: {5}";

        LoadingForm loadingscreen;
        WindowsMediaPlayer wplayer;
        WindowsMediaPlayer wplayerSongCalls;

        public MainForm()
        {
            InitializeComponent();
            SettingsManager.Load();

            this.Icon = Properties.Resources.logo1;

            wplayer = new WindowsMediaPlayer();
            wplayerSongCalls = new WindowsMediaPlayer();
            wplayer.settings.volume = SettingsManager.VolumeMain;
            wplayerSongCalls.settings.volume = SettingsManager.VolumeSpeech;
            trackBar1.Value = SettingsManager.VolumeMain;
            trackBar2.Value = SettingsManager.VolumeSpeech;
            

            notifyIcon1.Icon = this.Icon;
            notifyIcon1.Visible = true;
            notifyIcon1.Text = "WeAreOne Infos";
            notifyIcon1.DoubleClick += notifyIcon1_DoubleClick;
            notifyIcon1.ContextMenu = new ContextMenu();
            notifyIcon1.ContextMenu.MenuItems.Add(new MenuItem("Erstellt von Johnny")
            {
                Enabled = false
            });

            notifyIcon1.ContextMenu.MenuItems.Add(new MenuItem("-"));
            notifyIcon1.ContextMenu.MenuItems.Add(new MenuItem("Beenden", (sender, e) => this.Close())
                {
                    DefaultItem = true
                });


            timer = new Timer();
            timer.Interval = 3000;
            timer.SynchronizingObject = this;
            timer.Elapsed += timer_callback;
            timer.AutoReset = true;
            timer.Start();

            icontimer = new Timer();
            icontimer.Interval = 500;
            icontimer.SynchronizingObject = this;
            icontimer.Elapsed += icontimer_Elapsed;
            icontimer.AutoReset = true;
            icontimer.Start();

            wao = new WeAreOne();
            wao.OnResponseReceived += wao_OnResponseReceived;
            wao.OnImageReceived += wao_OnImageReceived;

            linkLabel1.Click += (sender, e) => Process.Start(current.release);

            loadingscreen = new LoadingForm();
            loadingscreen.ShowDialog();
        }

        void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            Visible = true;
            WindowState = FormWindowState.Normal;
        }
        void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SettingsManager.Save();
        }

        void icontimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (wplayer.playState != WMPPlayState.wmppsPlaying)
                return;
            switch(iconindex)
            {
                case 0:
                    notifyIcon1.Icon = Properties.Resources.logo1;
                    break;
                case 1:
                    notifyIcon1.Icon = Properties.Resources.logo2;
                    break;
                case 2:
                    notifyIcon1.Icon = Properties.Resources.logo3;
                    break;
                case 3:
                    notifyIcon1.Icon = Properties.Resources.logo4;
                    break;
            }
            iconindex++;
            if (iconindex > 3)
                iconindex = 0;
        }

        void wao_OnImageReceived(ImageReceiveEventArgs e)
        {
            pictureBox1.Image = e.response;
        }
        private void disableNotifications(object sender,TimeSpan span)
        {
            MenuItem menuitem = (MenuItem)sender;
            
        }
        public void Trigger(TriggerTypes type,Radio rad)
        {
            if(type == TriggerTypes.SHOW)
            {
                notifyIcon1.ShowBalloonTip(3000, "Neue Show", rad.show + " von " + rad.moderator + " (" + rad.style + ")", ToolTipIcon.Info);
            }
            else if(type == TriggerTypes.SONG)
            {
                notifyIcon1.ShowBalloonTip(3000, "Neuer Track",rad.song + " || " + rad.artist, ToolTipIcon.Info);
                wplayerSongCalls.URL = "http://translate.google.com/translate_tts?tl=en&q=" + WebUtility.UrlEncode(Translate(rad.song + " by " + rad.artist));
                wplayerSongCalls.controls.play();
            }
            else if(type == TriggerTypes.PICTURE)
            {
                wao.RequestPicture(rad);
            }
        }

        private string Translate(string song)
        {
            return song.Replace("feat.", "featuring").Replace("&","and");
        }
        private void onExitPressed(object sender, EventArgs e)
        {
            this.Close();
        }

        void wao_OnResponseReceived(ResponseReceiveEventArgs e)
        {
            
            if (!timer.Enabled)
                timer.Start();
            if (loadingscreen != null)
            {
                loadingscreen.Invoke(new Action(() => loadingscreen.Close()));
                loadingscreen.Invoke(new Action(() => loadingscreen.Dispose()));
                loadingscreen = null;
                comboBox1.SelectedItem = SettingsManager.Station;
            }
            if (comboBox1.InvokeRequired)
            {
                comboBox1.Invoke(new Action(() => wao_OnResponseReceived(e)));
                return;
            }
            foreach(Radio rad in e.response.radio)
            {
                if(rad.name == (String)comboBox1.SelectedItem)
                {
                    if(current == null)
                    {
                        current = rad;
                        wplayerSongCalls.URL = "http://translate.google.com/translate_tts?tl=en&q=" + WebUtility.UrlEncode(Translate(rad.song + " by " + rad.artist));
                        wplayerSongCalls.controls.play();
                        UpdateGUI(rad);
                        return;
                    }
                    if (current.song != rad.song && current.name == rad.name)
                        Trigger(TriggerTypes.SONG, rad);
                    if (current.show != rad.show && current.name == rad.name)
                        Trigger(TriggerTypes.SHOW, rad);
                    if (current.picture != rad.picture)
                        Trigger(TriggerTypes.PICTURE, rad);
                    UpdateGUI(rad);
                    current = rad;
                }
            }
        }

        private void UpdateGUI(Radio rad)
        {
            if (pictureBox1.Image == null)
                wao.RequestPicture(rad);
            if (rad.release != "")
            {
                if (linkLabel1.InvokeRequired) 
                    linkLabel1.Invoke(new Action(() => linkLabel1.Text = "(Song Release)")); 
                else
                    linkLabel1.Text = "(Song Release)";
            }
            else
            {
                if (linkLabel1.InvokeRequired) linkLabel1.Invoke(new Action(() => linkLabel1.Text = ""));
                else
                    linkLabel1.Text = "";
            }
            //Idk why we need an Invoke here...
            if (label1.InvokeRequired)
                label1.Invoke(new Action(() => label1.Text = String.Format(labeltext, rad.song, rad.artist, rad.show, rad.moderator, rad.endtime + ":00",rad.listener)));
            else
                label1.Text = String.Format(labeltext, rad.song, rad.artist, rad.show, rad.moderator, rad.endtime + ":00",rad.listener);
            
        }

        private void timer_callback(object sender, System.Timers.ElapsedEventArgs e)
        {
            wao.RequestResponse();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(button1.Text == "Play")
            {
                switch((String)comboBox1.SelectedItem)
                {
                    case "HouseTime":
                        wplayer.URL = "http://listen.housetime.fm/dsl.asx";
                        break;
                    case "TechnoBase":
                        wplayer.URL = "http://listen.technobase.fm/dsl.asx";
                        break;
                    case "HardBase":
                        wplayer.URL = "http://listen.hardbase.fm/dsl.asx";
                        break;
                    case "TranceBase":
                        wplayer.URL = "http://listen.trancebase.fm/dsl.asx";
                        break;
                    case "CoreTime":
                        wplayer.URL = "http://listen.coretime.fm/dsl.asx";
                        break;
                    case "ClubTime":
                        wplayer.URL = "http://listen.clubtime.fm/dsl.asx";
                        break;
                }
                wplayer.controls.play();
                wplayer.settings.volume = trackBar1.Value;
                button1.Text = "Lade...";
                button1.Enabled = false;
                while (wplayer.playState != WMPPlayState.wmppsPlaying)
                {
                    Application.DoEvents();
                    Thread.Sleep(200);
                }
                button1.Text = "Pause";
                button1.Enabled = true;
            }
            else
            {
                wplayer.controls.pause();
                button1.Text = "Play";
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            wplayer.settings.volume = trackBar1.Value;
            SettingsManager.VolumeMain = trackBar1.Value;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            SettingsManager.Station = (String)comboBox1.SelectedItem;
            wplayer.controls.pause();
            button1.Text = "Play";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            new AboutForm().ShowDialog();
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Visible = false;
                ShowInTaskbar = false;
            }
            if(WindowState == FormWindowState.Normal)
            {
                Visible = true;
                ShowInTaskbar = true;
            }
                
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            wplayerSongCalls.settings.volume = trackBar2.Value;
            SettingsManager.VolumeSpeech = trackBar2.Value;
        }
    }
    public enum TriggerTypes
    {
        SHOW,SONG,PICTURE
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using EEMSGS;
using PlayerIOClient;

namespace HelperBot
{
    public partial class Form1 : Form
    {
        Connection con;
        Dictionary<int, string> players = new Dictionary<int, string>();
        string worldOwner;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadAdminFile();
            LoadBanFile();
            LoadLoginInfo();
            AdminCommands.Initialize();
            if (!File.Exists("usablecommands.txt"))
                foreach (var index in AdminCommands.Index)
                {
                    UsableAdminCmds.SetItemChecked(index.Value, true);
                }
            LoadUsableCommands();

            label7.Text = "Created By: " + BotInfo.Creator;
            label8.Text = "Version: " + BotInfo.VersionString;
        }

        #region AdminList
        private void button2_Click(object sender, EventArgs e)
        {
            string player = textBox4.Text;
            admin_toggle(player);
        }

        private void admin_toggle(string player)
        {
            player = player.ToLower();
            if (player == "" || player.Contains(" "))
                return;

            if (listBox1.Items.Contains(player))
                listBox1.Items.Remove(player);
            else
                listBox1.Items.Add(player);

            if (checkBox1.Checked)
                SaveAdminFile();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox4.Text = (string)listBox1.SelectedItem;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SaveAdminFile();
        }

        private void LoadAdminFile()
        {
            if (File.Exists("admins.txt"))
            {
                string[] items = File.ReadAllLines("admins.txt");
                foreach (var item in items)
                {
                    listBox1.Items.Add(item.ToLower());
                }
            }
        }

        private void SaveAdminFile()
        {
            button2.Enabled = false;
            button3.Enabled = false;
            StreamWriter SaveAdminFile = new StreamWriter("admins.txt");
            foreach (var item in listBox1.Items)
            {
                SaveAdminFile.WriteLine(item.ToString());
            }

            SaveAdminFile.Close();
            button2.Enabled = true;
            button3.Enabled = true;
        }

        private bool isAdmin(string player)
        {
            return (listBox1.Items.Contains(player) || player == worldOwner);
        }
        #endregion

        #region BanList
        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox5.Text = (string)listBox2.SelectedItem;
        }

        private void button6_Click(object sender, EventArgs e)
        {

            string player = textBox5.Text;
            ban_toggle(player);
        }

        private void ban_toggle(string player)
        {
            player = player.ToLower();
            if (player == "" || player.Contains(" "))
                return;

            if (listBox2.Items.Contains(player))
                listBox2.Items.Remove(player);
            else
                listBox2.Items.Add(player);

            if (checkBox2.Checked)
                SaveBanFile();
        }

        private void ban_toggle(string player, bool on)
        {
            player = player.ToLower();
            if (player == "" || player.Contains(" "))
                return;

            if (listBox2.Items.Contains(player) && !on)
                listBox2.Items.Remove(player);
            else if (on)
                listBox2.Items.Add(player);

            if (checkBox2.Checked)
                SaveBanFile();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            SaveBanFile();
        }

        private void SaveBanFile()
        {
            button5.Enabled = false;
            button6.Enabled = false;
            StreamWriter SaveBanFile = new StreamWriter("bans.txt");
            foreach (var item in listBox2.Items)
            {
                SaveBanFile.WriteLine(item.ToString());
            }

            SaveBanFile.Close();
            button5.Enabled = true;
            button6.Enabled = true;
        }

        private void LoadBanFile()
        {
            if (File.Exists("bans.txt"))
            {
                string[] items = File.ReadAllLines("bans.txt");
                foreach (var item in items)
                {
                    listBox2.Items.Add(item.ToLower());
                }
            }
        }

        private bool isBanned(string player)
        {
            return listBox2.Items.Contains(player);
        }
        #endregion

        #region Connection
        private void button1_Click(object sender, EventArgs e)
        {
            if (con != null)
                if (con.Connected)
                    con.Disconnect();
                else
                    connect(textBox1.Text, textBox2.Text, textBox3.Text);
            else
                connect(textBox1.Text, textBox2.Text, textBox3.Text);
        }

        private void connect(string u, string p, string r)
        {
            PlayerIO.QuickConnect.SimpleConnect("everybody-edits-su9rn58o40itdbnw69plyw", u, p,
                    delegate(Client c)
                    {
                        try
                        {
                            con = c.Multiplayer.JoinRoom(r, null);
                            con.Send("init");
                            con.OnMessage += delegate(object sender, PlayerIOClient.Message m)
                            {
                                switch (m.Type)
                                {
                                    #region Init
                                    case "init":
                                        Invoke((MethodInvoker)(() => button1.Text = "Disconnect"));

                                        if (!m.GetBoolean((uint)EEMessage.Init.PlayerIsOwner))
                                        {
                                            con.Disconnect();
                                            MessageBox.Show("You are not the World Owner");
                                            break;
                                        }

                                        worldOwner = m.GetString(EEMessage.Init.RoomOwner);
                                        players.Add(m.GetInt(EEMessage.Init.PlayerUserID), m.GetString(EEMessage.Init.PlayerUserName));
                                        con.Send("init2");
                                        con.Send("access", "");
                                        break;
                                    #endregion
                                    #region Add
                                    case "add":
                                        players.Add(m.GetInt(EEMessage.Add.ID), m.GetString(EEMessage.Add.UserName));
                                        string owner = worldOwner;

                                        if (m.GetString(EEMessage.Add.UserName) == "timothyji")
                                        {
                                            con.Send("say", "/pm timothyji [Bot] Hello Maker! " + BotInfo.VersionString);
                                            Thread.Sleep(750);
                                        }

                                        if (isBanned(m.GetString(EEMessage.Add.UserName)))
                                        {
                                            con.Send("say", "/kick " + m.GetString(1) + " [Bot] Banned");
                                            Thread.Sleep(750);
                                        }
                                        break;
                                    #endregion
                                    #region Left
                                    case "left":
                                        players.Remove(m.GetInt(EEMessage.Left.UserID));
                                        break;
                                    #endregion
                                    #region Say
                                    case "say":
                                        try
                                        {
                                            string player = players[m.GetInt(EEMessage.Say.UserID)];
                                            string message = m.GetString(EEMessage.Say.Text).ToLower();
                                            string[] msg = message.Split(' ');

                                            Commands(m.GetInt(EEMessage.Say.UserID), message);
                                        }
                                        catch (Exception)
                                        {
                                            
                                        }
                                        break;
                                    #endregion
                                }
                            };
                            con.OnDisconnect += delegate(object sender, string reason)
                            {
                                Invoke((MethodInvoker)(() => button1.Text = "Connect"));
                                players.Clear();
                            };
                        }
                        catch (PlayerIOError error)
                        {
                            MessageBox.Show("Error: " + error.Message);
                        }
                    },
                    delegate(PlayerIOError error)
                    {
                        MessageBox.Show("Error: " + error.Message);
                    });
        }
        #endregion

        #region LoginInfo
        private void LoadLoginInfo()
        {
            if (File.Exists("login.txt"))
            {
                string[] loginFile = File.ReadAllLines("login.txt");
                if (loginFile.Length == 2)
                {
                    textBox1.Text = Rot13.derot(loginFile[0]);
                    textBox2.Text = Rot13.derot(loginFile[1]);
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            button4.Enabled = false;
            StreamWriter SaveFile = new StreamWriter("login.txt");

            SaveFile.WriteLine(Rot13.derot(textBox1.Text));
            SaveFile.WriteLine(Rot13.derot(textBox2.Text));

            SaveFile.Close();
            button4.Enabled = true;
        }
        #endregion

        #region Commands
        #region CustomCommandHelp
        private void button7_Click(object sender, EventArgs e)
        {
            string[] customCommandHelpMSGBOXResponseArray = 
            {
                "Custom Command Help",
                "--------------------",
                "\"$player\" = output player name"
            };

            string CustomCommandHelpResponse = "";
            foreach (string line in customCommandHelpMSGBOXResponseArray)
            {
                CustomCommandHelpResponse += line + Environment.NewLine;
            }

            MessageBox.Show(CustomCommandHelpResponse, "Help");
        }
        #endregion
        #region CanUseCommand
        private bool CanUseCommand(string arg1, string arg2)
        {
            char prefix = '!';
            string command = prefix + arg2;

            if (arg1.StartsWith(command) && UsableAdminCmds.GetItemChecked(AdminCommands.Index[arg2]))
                return true;
            return false;
        }
        #endregion
        #region Commands
        private void Commands(int UserID, string message)
        {
            string player = players[UserID];
            string[] msg = message.Split(' ');

            //Admin+ Only Commands
            #region Admin+ Commands
            if (worldOwner == player || isAdmin(player))
            {
                #region Help
                if (CanUseCommand(message, "help"))
                {
                    string help = "";
                    foreach (var item in UsableAdminCmds.Items)
                    {
                        if (UsableAdminCmds.GetItemChecked(AdminCommands.Index[item.ToString()]))
                            help += " !" + item.ToString();
                    }
                    con.Send("say", "/pm " + player + " [Bot] Commands:" + help);
                    Thread.Sleep(750);
                }
                #endregion

                #region Load
                if (CanUseCommand(message, "load"))
                {
                    con.Send("say", "/loadlevel");
                    Thread.Sleep(750);
                }
                #endregion

                #region Clear
                if (CanUseCommand(message, "clear"))
                {
                    con.Send("clear");
                }
                #endregion

                #region Save
                if (CanUseCommand(message, "save"))
                {
                    con.Send("save");
                }
                #endregion

                #region Kick
                if (CanUseCommand(message, "kick") && msg.Length >= 2)
                {
                    if (players.ContainsValue(msg[1]) && !isAdmin(msg[1]))
                    {
                        string kickReason = "";
                        if (msg.Length >= 3)
                        {
                            int on = 0;
                            foreach (string str in msg)
                            {
                                if (on >= 2)
                                    kickReason += str + " ";
                                on++;
                            }
                        }
                        con.Send("say", "/kick " + msg[1] + " " + kickReason);
                        Thread.Sleep(750);
                    }
                    else
                    {
                        con.Send("say", "/pm " + player + " [Bot] Failed to kick " + msg[1]);
                    }
                }
                #endregion

                #region Ban
                if (CanUseCommand(message, "ban") && msg.Length >= 2)
                {
                    if (!isAdmin(msg[1]) && checkBox1.Checked)
                    {
                        if (isBanned(msg[1]))
                            return;

                        string kickReason = "";
                        if (msg.Length >= 3)
                        {
                            int on = 0;
                            foreach (string str in msg)
                            {
                                if (on >= 2)
                                    kickReason += str + " ";
                                on++;
                            }
                        }
                        con.Send("say", "/kick " + msg[1] + " " + kickReason);
                        Thread.Sleep(750);
                        Invoke((MethodInvoker)(() => ban_toggle(msg[1], true)));
                    }
                    else
                    {
                        con.Send("say", "/pm " + player + " [Bot] Failed to ban " + msg[1] + ". Reason: " + msg[1] + " is an Admin");
                    }
                }
                #endregion

                #region Unban
                if (CanUseCommand(message, "unban") && msg.Length >= 2)
                {
                    if (checkBox1.Checked && isBanned(msg[1]))
                    {
                        Invoke((MethodInvoker)(() => ban_toggle(msg[1], false)));
                    }
                }
                #endregion
            }
            #endregion

            #region Ping
            if (message.StartsWith("!ping"))
            {
                con.Send("say", textBoxPingResponse.Text.Replace("$player", player));
                Thread.Sleep(750);
            }
            #endregion

            #region Help
            if (message.StartsWith("!help"))
            {
                con.Send("say", "/pm " + player + " [Bot] Commands: !ping !download !isAdmin !isBanned !version " + "!" + textBoxCustomCMD.Text);
                Thread.Sleep(750);
            }
            #endregion

            #region Download
            if (message.StartsWith("!download"))
            {
                con.Send("say", "/pm " + player + " [Bot] timothyji.weebly.com/helper-bot.html");
                Thread.Sleep(750);
            }
            #endregion

            #region Version
            if (message.StartsWith("!version"))
            {
                con.Send("say", "/pm " + player + " [Bot] " + BotInfo.VersionString + " By: " + BotInfo.Creator);
                Thread.Sleep(750);
            }
            #endregion

            #region IsAdmin
            if (message.StartsWith("!isadmin"))
            {
                if (msg.Length >= 2)
                {
                    if (isAdmin(msg[1]))
                        con.Send("say", "/pm " + player + " [Bot] " + msg[1] + " is an Admin.");
                    else
                        con.Send("say", "/pm " + player + " [Bot] " + msg[1] + " is NOT an Admin.");
                }
                else
                {
                    if (isAdmin(player))
                        con.Send("say", "/pm " + player + " [Bot] You are an Admin.");
                    else
                        con.Send("say", "/pm " + player + " [Bot] You are NOT an Admin.");
                }
                Thread.Sleep(750);
            }
            #endregion

            #region IsBanned
            if (message.StartsWith("!isbanned") && msg.Length >= 2)
            {
                if (isBanned(msg[1]))
                    con.Send("say", "/pm " + player + " [Bot] " + msg[1] + " is Banned.");
                else
                    con.Send("say", "/pm " + player + " [Bot] " + msg[1] + " is NOT Banned.");
                Thread.Sleep(750);
            }
            #endregion

            #region CustomCommand A
            if (message.StartsWith("!" + textBoxCustomCMD.Text))
            {
                con.Send("say", textBoxCustomRSPNSE.Text.Replace("$player", player));
                Thread.Sleep(750);
            }
            #endregion
        }
        #endregion
        #endregion
        #region UsableAdminCommands
        private void button8_Click(object sender, EventArgs e)
        {
            button8.Enabled = false;
            StreamWriter SaveFile = new StreamWriter("usablecommands.txt");
            foreach (var item in UsableAdminCmds.Items)
            {
                SaveFile.WriteLine(UsableAdminCmds.Items[AdminCommands.Index[(string)item]] + " " + UsableAdminCmds.GetItemChecked(AdminCommands.Index[(string)item]));
            }

            SaveFile.Close();
            button8.Enabled = true;
        }

        private void LoadUsableCommands()
        {
            if (File.Exists("usablecommands.txt"))
            {
                string[] UsableCommands = File.ReadAllLines("usablecommands.txt");
                foreach (string line in UsableCommands)
                {
                    string[] sLine = line.Split(' ');
                    try
                    {
                        int index = AdminCommands.Index[sLine[0]];
                        bool chked = Convert.ToBoolean(sLine[1]);

                        UsableAdminCmds.SetItemChecked(index, chked);
                    }
                    catch
                    {
                    }
                }
            }
        }
        #endregion
    }
}

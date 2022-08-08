using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Threading;

namespace TICTACTOEv2
{
    public partial class GameWindow : Form
    {
        private bool BotCheck = false;
        private int mvCount = 0;
        private int rndMove;
        private char PlayerChar; // Символы игрока
        private char OpponentChar; // Символы оппонента
        private Socket socket;
        private BackgroundWorker MessageReciever = new BackgroundWorker();
        private BackgroundWorker BotClick = new BackgroundWorker();
        private TcpListener server = null;
        private TcpClient client;
        private AutoResetEvent _resetEvent = new AutoResetEvent(false);
        public GameWindow(bool HostCheck, bool bot, string IPtext = null)
        {
            InitializeComponent();
            BotCheck = bot;
            MessageReciever.DoWork += MessageReciever_DoWork;

            CheckForIllegalCrossThreadCalls = false;

            if (HostCheck)
            {
                PlayerChar = 'X';
                OpponentChar = 'O';
                server = new TcpListener(System.Net.IPAddress.Any, 5732);
                server.Start();
                socket = server.AcceptSocket();
                if (BotCheck)
                {
                    Random rnd = new Random();
                    rndMove = rnd.Next(1, 10);
                    mvCount++;
                    BotLogic();
                    BotClick.DoWork += BotClick_DoWork;
                    BotClick.RunWorkerAsync();
                }
            }
            else
            {
                PlayerChar = 'O';
                OpponentChar = 'X';
                try
                {
                    client = new TcpClient(IPtext, 5732);
                    socket = client.Client;
                    MessageReciever.RunWorkerAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    Close();
                }
            }
        }

        private void BotClick_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                _resetEvent.WaitOne();
                if (BotCheck)
                {
                    mvCount++;
                    BotLogic();
                }
                else break;
            }
        }

        private void MessageReciever_DoWork(object sender, DoWorkEventArgs e)
        {
            if (CheckState())
                return;
            FreezeBoard();
            LableState.Text = "Ход противника";
            GetMove();
            LableState.Text = "Ваш ход!";
            if (!CheckState())
                UnFreezeBoard();
            _resetEvent.Set();
        }

        private bool CheckState()
        {
            //Горизонтали и вертикали

            if (button1.Text == button2.Text && button2.Text == button3.Text && button3.Text != "")
            {
                return WinCheck(button1.Text);
            }
            else if (button4.Text == button5.Text && button5.Text == button6.Text && button6.Text != "")
            {
                return WinCheck(button4.Text);
            }
            else if (button7.Text == button8.Text && button8.Text == button9.Text && button9.Text != "")
            {
                return WinCheck(button7.Text);
            }

            //

            else if (button1.Text == button4.Text && button4.Text == button7.Text && button7.Text != "")
            {
                return WinCheck(button1.Text);
            }
            else if (button2.Text == button5.Text && button5.Text == button8.Text && button8.Text != "")
            {
                return WinCheck(button2.Text);
            }
            else if (button3.Text == button6.Text && button6.Text == button9.Text && button9.Text != "")
            {
                return WinCheck(button3.Text);
            }

            //Диагонали
            else  if (button1.Text == button5.Text && button5.Text == button9.Text && button9.Text != "")
            {
                return WinCheck(button1.Text);
            }
            else if (button3.Text == button5.Text && button5.Text == button7.Text && button7.Text != "")
            {
                return WinCheck(button3.Text);
            }

            else if (button1.Text != "" && button2.Text != "" && button3.Text != "" && button4.Text != "" && button5.Text != "" && button5.Text != "" && button6.Text != "" && button7.Text != "" && button8.Text != "" && button9.Text != "")
            {
                LableState.Text = "Ничья!";
                MessageBox.Show("Ничья!");
                FreezeBoard();
                this.Close();
                MessageReciever.WorkerSupportsCancellation = true;
                BotClick.WorkerSupportsCancellation = true;
                MessageReciever.CancelAsync();
                BotClick.CancelAsync();
                if (server != null)
                    server.Stop();
                BotCheck = false;
                return true;
            }

            return false;
        }
        private bool WinCheck(string x)
        {
            if (x[0] == PlayerChar)
            {
                LableState.Text = "Вы выиграли!";
                MessageBox.Show("Вы выиграли!");
            }
            else
            {
                LableState.Text = "Вы проиграли";
                MessageBox.Show("Вы проиграли");
            }
            FreezeBoard();
            this.Close();
            MessageReciever.WorkerSupportsCancellation = true;
            BotClick.WorkerSupportsCancellation = true;
            MessageReciever.CancelAsync();
            BotClick.CancelAsync();
            if (server != null)
                server.Stop();
            BotCheck = false;
            return true;
        }

        private void FreezeBoard()
        {
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
            button5.Enabled = false;
            button6.Enabled = false;
            button7.Enabled = false;
            button8.Enabled = false;
            button9.Enabled = false;
        }

        private void UnFreezeBoard()
        {
            if (button1.Text == "")
                button1.Enabled = true;
            if (button2.Text == "")
                button2.Enabled = true;
            if (button3.Text == "")
                button3.Enabled = true;
            if (button4.Text == "")
                button4.Enabled = true;
            if (button5.Text == "")
                button5.Enabled = true;
            if (button6.Text == "")
                button6.Enabled = true;
            if (button7.Text == "")
                button7.Enabled = true;
            if (button8.Text == "")
                button8.Enabled = true;
            if (button9.Text == "")
                button9.Enabled = true;
        }

        private void GetMove()
        {
            byte[] buffer = new byte[1];
            socket.Receive(buffer); //Ожидание хода противника
            if (buffer[0] == 1)
                button1.Text = OpponentChar.ToString();
            if (buffer[0] == 2)
                button2.Text = OpponentChar.ToString();
            if (buffer[0] == 3)
                button3.Text = OpponentChar.ToString();
            if (buffer[0] == 4)
                button4.Text = OpponentChar.ToString();
            if (buffer[0] == 5)
                button5.Text = OpponentChar.ToString();
            if (buffer[0] == 6)
                button6.Text = OpponentChar.ToString();
            if (buffer[0] == 7)
                button7.Text = OpponentChar.ToString();
            if (buffer[0] == 8)
                button8.Text = OpponentChar.ToString();
            if (buffer[0] == 9)
                button9.Text = OpponentChar.ToString();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            byte[] num = { 1 };
            socket.Send(num);
            button1.Text = PlayerChar.ToString();
            MessageReciever.RunWorkerAsync();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            byte[] num = { 2 };
            socket.Send(num);
            button2.Text = PlayerChar.ToString();
            MessageReciever.RunWorkerAsync();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            byte[] num = { 3 };
            socket.Send(num);
            button3.Text = PlayerChar.ToString();
            MessageReciever.RunWorkerAsync();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            byte[] num = { 4 };
            socket.Send(num);
            button4.Text = PlayerChar.ToString();
            MessageReciever.RunWorkerAsync();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            byte[] num = { 5 };
            socket.Send(num);
            button5.Text = PlayerChar.ToString();
            MessageReciever.RunWorkerAsync();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            byte[] num = { 6 };
            socket.Send(num);
            button6.Text = PlayerChar.ToString();
            MessageReciever.RunWorkerAsync();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            byte[] num = { 7 };
            socket.Send(num);
            button7.Text = PlayerChar.ToString();
            MessageReciever.RunWorkerAsync();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            byte[] num = { 8 };
            socket.Send(num);
            button8.Text = PlayerChar.ToString();
            MessageReciever.RunWorkerAsync();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            byte[] num = { 9 };
            socket.Send(num);
            button9.Text = PlayerChar.ToString();
            MessageReciever.RunWorkerAsync();
        }

        private void GameWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            MessageReciever.WorkerSupportsCancellation = true;
            BotClick.WorkerSupportsCancellation = true;
            MessageReciever.CancelAsync();
            BotClick.CancelAsync();
            if (server != null)
                server.Stop();
        }

        private void BotLogic()
        {
            if (mvCount == 1)
                rnMV1();
            else if (mvCount == 2)
                rnMV2();
            else if (mvCount > 2)
            {
                Random rnd = new Random();
                bool check = true;
                while (check)
                {
                    rndMove = rnd.Next(1, 10);
                    switch (rndMove)
                    {
                        case 1: if (button1.Text == "") { button1_Click(button1, null); check = false; } break;
                        case 2: if (button2.Text == "") { button2_Click(button2, null); check = false; } break;
                        case 3: if (button3.Text == "") { button3_Click(button3, null); check = false; } break;
                        case 4: if (button4.Text == "") { button4_Click(button4, null); check = false; } break;
                        case 5: if (button5.Text == "") { button5_Click(button5, null); check = false; } break;
                        case 6: if (button6.Text == "") { button6_Click(button6, null); check = false; } break;
                        case 7: if (button7.Text == "") { button7_Click(button7, null); check = false; } break;
                        case 8: if (button8.Text == "") { button8_Click(button8, null); check = false; } break;
                        case 9: if (button9.Text == "") { button9_Click(button9, null); check = false; } break;
                    }
                }
            }
        }

        private void rnMV1()
        {
            switch (rndMove)
            {
                case 1: button1_Click(button1, null); break;
                case 2: button2_Click(button2, null); break;
                case 3: button3_Click(button3, null); break;
                case 4: button4_Click(button4, null); break;
                case 5: button5_Click(button5, null); break;
                case 6: button6_Click(button6, null); break;
                case 7: button7_Click(button7, null); break;
                case 8: button8_Click(button8, null); break;
                case 9: button9_Click(button9, null); break;
            }
        }

        private void rnMV2()
        {
            if (button5.Text == "")
                button5_Click(button5, null);
            else if (button5.Text == "O") //Если на втором ходе нуль по середине
            {
                // X на первом ходу пришел на угол
                if (button1.Text == "" && button9.Text == "X")
                    button1_Click(button1, null);
                else if (button3.Text == "" && button7.Text == "X")
                    button3_Click(button3, null);
                else if (button7.Text == "" && button3.Text == "X")
                    button7_Click(button7, null);
                else if (button9.Text == "" && button1.Text == "X")
                    button9_Click(button9, null);

                // X на первом ходу пришел на сторону
                else if (button2.Text == "X")
                {
                    rndMove = random_except_list(new int[] { 2, 5 });
                    switch (rndMove)
                    {
                        case 1: button1_Click(button1, null); break;
                        case 3: button3_Click(button3, null); break;
                        case 4: button4_Click(button4, null); break;
                        case 6: button6_Click(button6, null); break;
                        case 7: button7_Click(button7, null); break;
                        case 8: button8_Click(button8, null); break;
                        case 9: button9_Click(button9, null); break;
                    }
                }
                else if (button4.Text == "X")
                {
                    rndMove = random_except_list(new int[] { 4, 5 });
                    switch (rndMove)
                    {
                        case 1: button1_Click(button1, null); break;
                        case 2: button2_Click(button2, null); break;
                        case 3: button3_Click(button3, null); break;
                        case 6: button6_Click(button6, null); break;
                        case 7: button7_Click(button7, null); break;
                        case 8: button8_Click(button8, null); break;
                        case 9: button9_Click(button9, null); break;
                    }
                }
                else if (button6.Text == "X")
                {
                    rndMove = random_except_list(new int[] { 5, 6 });
                    switch (rndMove)
                    {
                        case 1: button1_Click(button1, null); break;
                        case 2: button2_Click(button2, null); break;
                        case 3: button3_Click(button3, null); break;
                        case 4: button4_Click(button4, null); break;
                        case 7: button7_Click(button7, null); break;
                        case 8: button8_Click(button8, null); break;
                        case 9: button9_Click(button9, null); break;
                    }
                }
                else if (button8.Text == "X")
                {
                    rndMove = random_except_list(new int[] { 5, 8 });
                    switch (rndMove)
                    {
                        case 1: button1_Click(button1, null); break;
                        case 2: button2_Click(button2, null); break;
                        case 3: button3_Click(button3, null); break;
                        case 4: button4_Click(button4, null); break;
                        case 6: button6_Click(button6, null); break;
                        case 7: button7_Click(button7, null); break;
                        case 9: button9_Click(button9, null); break;
                    }
                }
            }
            else //Если Х по середине с первого хода
            {
                // O на первом ходу пришел на угол
                if (button1.Text == "O")
                    button3_Click(button3, null);
                else if (button3.Text == "O")
                    button1_Click(button1, null);
                else if (button7.Text == "O")
                    button9_Click(button9, null);
                else if (button9.Text == "O")
                    button7_Click(button7, null);

                // O на первом ходу пришел на сторону
                else if (button2.Text == "O")
                {
                    rndMove = random_except_list(new int[] { 1, 2, 3, 4, 5, 6, 8 });
                    switch (rndMove)
                    {
                        case 7: button7_Click(button7, null); break;
                        case 9: button9_Click(button9, null); break;
                    }
                }
                else if (button4.Text == "O")
                {
                    rndMove = random_except_list(new int[] { 1, 2, 4, 5, 6, 7, 8 });
                    switch (rndMove)
                    {
                        case 3: button3_Click(button3, null); break;
                        case 9: button9_Click(button9, null); break;
                    }
                }
                else if (button6.Text == "O")
                {
                    rndMove = random_except_list(new int[] { 2, 3, 4, 5, 6, 8, 9 });
                    switch (rndMove)
                    {
                        case 1: button1_Click(button1, null); break;
                        case 7: button7_Click(button7, null); break;
                    }
                }
                else if (button8.Text == "O")
                {
                    rndMove = random_except_list(new int[] { 2, 4, 5, 6, 7, 8, 9 });
                    switch (rndMove)
                    {
                        case 1: button1_Click(button1, null); break;
                        case 3: button9_Click(button3, null); break;
                    }
                }
            }
        }
        public static int random_except_list(int[] x)
        {
            var range = Enumerable.Range(1, 9).Where(i => !x.Contains(i));

            var rand = new System.Random();
            int index = rand.Next(0, 9 - x.Length);
            return range.ElementAt(index);
        }
    }
}

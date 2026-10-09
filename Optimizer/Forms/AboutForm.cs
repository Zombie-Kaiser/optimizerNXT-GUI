using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Optimizer
{
    public sealed partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();
            OptionsHelper.ApplyTheme(this);

            pictureBox1.BackColor = OptionsHelper.CurrentOptions.Theme;
        }

        private void About_Load(object sender, EventArgs e)
        {
            t1.Interval = 50;
            t2.Interval = 50;

            t1.Start();
        }

        private void t1_Tick(object sender, EventArgs e)
        {
            string s0 = "";
            string s1 = "O";
            string s2 = "Op";
            string s3 = "Opt";
            string s4 = "Opti";
            string s5 = "Optim";
            string s6 = "Optimi";
            string s7 = "Optimiz";
            string s8 = "Optimize";
            string s9 = "Optimizer";
            string s10 = "OptimizerN";
            string s11 = "OptimizerNX";
            string s12 = "OptimizerNXT";

            switch (l1.Text)
            {
                case "":
                    l1.Text = s1;
                    break;
                case "O":
                    l1.Text = s2;
                    break;
                case "Op":
                    l1.Text = s3;
                    break;
                case "Opt":
                    l1.Text = s4;
                    break;
                case "Opti":
                    l1.Text = s5;
                    break;
                case "Optim":
                    l1.Text = s6;
                    break;
                case "Optimi":
                    l1.Text = s7;
                    break;
                case "Optimiz":
                    l1.Text = s8;
                    break;
                case "Optimize":
                    l1.Text = s9;
                    break;
                case "Optimizer":
                    l1.Text = s10;
                    break;
                case "OptimizerN":
                    l1.Text = s11;
                    break;
                case "OptimizerNX":
                    l1.Text = s12;
                    t1.Stop();
                    t2.Start();
                    break;
                case "OptimizerNXT":
                    l1.Text = s0;
                    break;
            }
        }

        private void t2_Tick(object sender, EventArgs e)
        {
            string s0 = "";
            string s1 = "Z";
            string s2 = "Zo";
            string s3 = "Zom";
            string s4 = "Zomb";
            string s5 = "Zombi";
            string s6 = "Zombie";
            string s7 = "Zombie-";
            string s8 = "Zombie-K";
            string s9 = "Zombie-Ka";
            string s10 = "Zombie-Kai";
            string s11 = "Zombie-Kais";
            string s12 = "Zombie-Kaise";
            string s13 = "Zombie-Kaiser";

            switch (l2.Text)
            {
                case "":
                    l2.Text = s1;
                    break;
                case "Z":
                    l2.Text = s2;
                    break;
                case "Zo":
                    l2.Text = s3;
                    break;
                case "Zom":
                    l2.Text = s4;
                    break;
                case "Zomb":
                    l2.Text = s5;
                    break;
                case "Zombi":
                    l2.Text = s6;
                    break;
                case "Zombie":
                    l2.Text = s7;
                    break;
                case "Zombie-":
                    l2.Text = s8;
                    break;
                case "Zombie-K":
                    l2.Text = s9;
                    break;
                case "Zombie-Ka":
                    l2.Text = s10;
                    break;
                case "Zombie-Kai":
                    l2.Text = s11;
                    break;
                case "Zombie-Kais":
                    l2.Text = s12;
                    break;
                case "Zombie-Kaise":
                    l2.Text = s13;
                    t2.Stop();
                    break;
                case "Zombie-Kaiser":
                    l2.Text = s0;
                    break;
            }
        }

        private void l2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/Zombie-Kaiser/optimizerNXT-GUI");
        }
    }
}

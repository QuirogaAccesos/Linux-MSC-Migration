using System;
using System.Windows.Forms;

namespace AA.Pango.TestWinApp
{

    public partial class PlateInput : UserControl
    {

        public event EventHandler<PlateEnteredEventArgs> PlateEntered;

        protected virtual void OnPlateEntered(PlateEnteredEventArgs e)
        {
            EventHandler<PlateEnteredEventArgs> handler = PlateEntered;
            handler?.Invoke(this, e);
        }

        public PlateInput()
        {
            InitializeComponent();
        }

        private void TypeInput(string input)
        {
            if (this.txtInput.Text.Length > 0)
            {
                this.txtInput.Text = this.txtInput.Text + input;
            }
            else
            {
                this.txtInput.Text = input;
            }
        }

        private void button23_Click(object sender, EventArgs e)
        {
            this.txtInput.Clear();
            this.txtInput.Focus();
        }

        private void button22_Click(object sender, EventArgs e)
        {

            if (this.txtInput.Text == null)
            {
                this.txtInput.Text = "";
                return;
            }

            if (this.txtInput.Text == "")
            {
                return;
            }

            this.txtInput.Text = this.txtInput.Text.Substring(0, this.txtInput.Text.Length - 1);

        }

        private void PlateInput_Load(object sender, EventArgs e)
        {
            this.txtInput.Focus();
        }

        private void btnProcess_Click(object sender, EventArgs e)
        {
            OnPlateEntered(new PlateEnteredEventArgs
            {
                Plate = this.txtInput.Text,
                Date = DateTime.Now
            });

        }

        private void button1_Click(object sender, EventArgs e)
        {
            TypeInput("1");
        }


        private void button2_Click(object sender, EventArgs e)
        {
            TypeInput("2");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            TypeInput("3");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            TypeInput("4");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            TypeInput("5");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            TypeInput("6");
        }

        private void button7_Click(object sender, EventArgs e)
        {
            TypeInput("7");
        }

        private void button8_Click(object sender, EventArgs e)
        {
            TypeInput("8");
        }

        private void button9_Click(object sender, EventArgs e)
        {
            TypeInput("9");
        }

        private void button0_Click(object sender, EventArgs e)
        {
            TypeInput("0");
        }

        private void btnA_Click(object sender, EventArgs e)
        {
            TypeInput("A");
        }

        private void btnB_Click(object sender, EventArgs e)
        {
            TypeInput("B");
        }

        private void btnC_Click(object sender, EventArgs e)
        {
            TypeInput("C");
        }

        private void btnD_Click(object sender, EventArgs e)
        {
            TypeInput("D");
        }

        private void btnE_Click(object sender, EventArgs e)
        {
            TypeInput("E");
        }

        private void btnF_Click(object sender, EventArgs e)
        {
            TypeInput("F");
        }

        private void btnG_Click(object sender, EventArgs e)
        {
            TypeInput("G");
        }

        private void btnH_Click(object sender, EventArgs e)
        {
            TypeInput("H");
        }

        private void btnI_Click(object sender, EventArgs e)
        {
            TypeInput("I");
        }

        private void btnJ_Click(object sender, EventArgs e)
        {
            TypeInput("J");
        }

        private void btnK_Click(object sender, EventArgs e)
        {
            TypeInput("K");
        }

        private void btnL_Click(object sender, EventArgs e)
        {
            TypeInput("L");
        }

        private void btnM_Click(object sender, EventArgs e)
        {
            TypeInput("M");
        }

        private void btnN_Click(object sender, EventArgs e)
        {
            TypeInput("N");
        }

        private void btnO_Click(object sender, EventArgs e)
        {
            TypeInput("O");
        }

        private void btnP_Click(object sender, EventArgs e)
        {
            TypeInput("P");
        }

        private void btnQ_Click(object sender, EventArgs e)
        {
            TypeInput("Q");
        }

        private void btnR_Click(object sender, EventArgs e)
        {
            TypeInput("R");
        }

        private void btnS_Click(object sender, EventArgs e)
        {
            TypeInput("S");
        }

        private void btnT_Click(object sender, EventArgs e)
        {
            TypeInput("T");
        }

        private void btnU_Click(object sender, EventArgs e)
        {
            TypeInput("U");
        }

        private void btnV_Click(object sender, EventArgs e)
        {
            TypeInput("V");
        }

        private void btnW_Click(object sender, EventArgs e)
        {
            TypeInput("W");
        }

        private void btnX_Click(object sender, EventArgs e)
        {
            TypeInput("X");
        }

        private void btnY_Click(object sender, EventArgs e)
        {
            TypeInput("Y");
        }

        private void btnZ_Click(object sender, EventArgs e)
        {
            TypeInput("Z");
        }
    }
}

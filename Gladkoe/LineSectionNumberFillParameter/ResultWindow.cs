﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gladkoe.LineSectionNumberFillParameter
{
    public partial class ResultWindow : Form
    {
        public ResultWindow(StringBuilder sb, int i)
        {
            this.InitializeComponent();
            this.textBoxResults.Text = sb.ToString();
            this.ParametersFilledLabel.Text = $@"Параметров заполнено: {i}";
            this.textBoxResults.DeselectAll();
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace lab1_graphics
{
    public partial class Form1 : Form
    {
        Bitmap image;
        public Form1()
        {
            InitializeComponent();
            
        }

        private void toolStripMenuItem7_Click(object sender, EventArgs e)
        {
            Filters filter = new BlurFilter();
            backgroundWorker2.RunWorkerAsync(filter);
        }

        private void toolStripMenuItem8_Click(object sender, EventArgs e)
        {
            Filters filter = new GaussianFilter();
            backgroundWorker2.RunWorkerAsync(filter);
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            //InvertFilter filter = new InvertFilter();

            Filters filter = new InvertFilter();
            backgroundWorker2.RunWorkerAsync(filter);
            //Bitmap resultImage = filter.processImage(image, backgroundWorker2);
            //pictureBox2.Image = resultImage;
            //pictureBox2.Refresh();
        }

        private void серыйМирToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image files|*.png;*.jpg;*.bmp|All files(*.*)|*.*";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                image = new Bitmap(dialog.FileName);
                pictureBox2.Image = image;
                pictureBox2.Refresh();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            backgroundWorker2.CancelAsync();
        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            Bitmap newImage = ((Filters)e.Argument).processImage(image, backgroundWorker2);
            if (backgroundWorker2.CancellationPending != true)
                image = newImage;
        }

        private void backgroundWorker2_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar2.Value = e.ProgressPercentage;
        }

        private void backgroundWorker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                pictureBox2.Image = image;
                pictureBox2.Refresh();
            }
            progressBar2.Value = 0;
        }
    }
}

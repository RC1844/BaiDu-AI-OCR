using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace BaiDu_AI
{
    public partial class Form1 : Form
    {



        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            OpenFileDialog fdlg = new OpenFileDialog();
            fdlg.Title = "选择需要识别的图片（只支持PNG、JPG、JPEG、BMP）";
            fdlg.InitialDirectory = @"";   //@是取消转义字符的意思
            fdlg.Filter = "图片(PNG,JPG,JPEG,BMP)|*.png;*.jpg;*.jpeg;*.bmp|全部文件(*.*)|*.* ";
            /*
             * FilterIndex 属性用于选择了何种文件类型,缺省设置为0,系统取Filter属性设置第一项
             * ,相当于FilterIndex 属性设置为1.如果你编了3个文件类型，当FilterIndex ＝2时是指第2个.
             */
            fdlg.FilterIndex = 0;
            /*
             *如果值为false，那么下一次选择文件的初始目录是上一次你选择的那个目录，
             *不固定；如果值为true，每次打开这个对话框初始目录不随你的选择而改变，是固定的  
             */
            fdlg.RestoreDirectory = false;
            if (fdlg.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = Path.GetFullPath(fdlg.FileName);
                pictureBox1.Image = Image.FromFile(textBox1.Text);
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            var APP_ID = "15264733";
            var API_KEY = "agCqSFsY7OtRTGx3TaykmvIq";
            var SECRET_KEY = "SWZqUnLpGXCUmZ6fPmxtcR8y4f6rI3Cy";
            var client = new Baidu.Aip.Ocr.Ocr(API_KEY, SECRET_KEY);
            var result = "操作不OK";
            if (radioButton1.Checked == true)
            {
                if (textBox1.Text != "")
                {
                    var image = File.ReadAllBytes(@textBox1.Text);
                    // 调用通用文字识别, 图片参数为本地图片，可能会抛出网络等异常，请使用try/catch捕获
                    result = "" + client.GeneralBasic(image);
                }
            }
            else if (textBox6.Text != "")
            {
                pictureBox1.ImageLocation = @textBox6.Text;
                string url = textBox6.Text;
                // 调用通用文字识别, 图片参数为远程url图片，可能会抛出网络等异常，请使用try/catch捕获
                result = "" + client.GeneralBasicUrl(url);
            }
            textBox2.Text = result;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked == true)
            {
                if (textBox1.Text != "")
                    pictureBox1.Image = Image.FromFile(textBox1.Text);
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked == true)
            {
                if (textBox6.Text != "")
                    pictureBox1.ImageLocation = @textBox6.Text;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();

            //设置保存文件对话框的标题
            sfd.Title = "请选择要保存的文件路径";

            //初始化保存目录，默认exe文件目录
            sfd.InitialDirectory = Application.StartupPath;

            //设置保存文件的类型
            sfd.Filter = "文本文件|*.txt|所有文件|*.*";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                //获得保存文件的路径
                string filePath = sfd.FileName;
                textBox3.Text = filePath;
                //保存
                using (FileStream fsWrite = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    byte[] buffer = Encoding.Default.GetBytes(textBox2.Text);
                    fsWrite.Write(buffer, 0, buffer.Length);
                }
            }
        }
    }
}

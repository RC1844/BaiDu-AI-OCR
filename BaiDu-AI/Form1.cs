using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Xml;

namespace BaiDu_AI
{
    public partial class Form1 : Form
    {
        string XmlPath = "Seetings.xml";
        string API_KEY = "agCqSFsY7OtRTGx3TaykmvIq";
        string SECRET_KEY = "SWZqUnLpGXCUmZ6fPmxtcR8y4f6rI3Cy";
        public Form1()
        {
            InitializeComponent();
            Form_Init();
        }
        private void Form_Init()
        {
            //判断文件是否存在
            if (File.Exists(XmlPath))
            {
                //该文件存在
                //加载文件
                XmlDocument XmlDoc = new XmlDocument();
                XmlDoc.Load(XmlPath);
                //获得根节点
                XmlElement Root = XmlDoc.DocumentElement;
                //匹配XmlNode
                XmlNode Settings = Root.SelectSingleNode("Setting");
                //根据XmlNode内容设置各项组件的属性
                //还原键值
                checkBox3.Checked = Convert.ToBoolean(Settings.Attributes["Setting"].Value);
                bool Enabled = Convert.ToBoolean(Settings.Attributes["Enabled"].Value);
                checkBox1.Checked = Enabled;
                //还原密匙
                textBox4.Text = Settings.Attributes["API_KEY"].Value;
                textBox5.Text = Settings.Attributes["SECRET_KEY"].Value;
                //检查是否启用自定义密匙
                if (Enabled)
                {
                    //使用自定义密匙
                    API_KEY = textBox4.Text;
                    SECRET_KEY = textBox5.Text;
                }
                XmlNode FilePath = Root.SelectSingleNode("ImagePath");
                XmlNodeList PathList = FilePath.ChildNodes;
                for (int i = 0; i < 1; i++)
                {
                    //还原上次使用过的路径
                    textBox1.Text = PathList[i].Attributes["FilePath"].Value;
                    textBox6.Text = PathList[i].Attributes["UrlPath"].Value;
                    textBox3.Text = PathList[i].Attributes["SavePath"].Value;
                }
            }
            //报告初始化完成
            toolStripStatusLabel1.Text = "就绪";
        }
        public static bool WebRequestTest()
        {
            string url = "https://aip.baidubce.com/rest/2.0/ocr/v1/general";
            try
            {
                System.Net.WebRequest myRequest = System.Net.WebRequest.Create(url);
                //设置请求超时为6000ms
                myRequest.Timeout = 1000;
                System.Net.WebResponse myResponse = myRequest.GetResponse();
            }
            catch (System.Net.WebException)
            {
                return false;
            }

            return true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //显示一个标准对话框
            OpenFileDialog O_File = new OpenFileDialog
            {
                Title = "选择需要识别的图片（只支持PNG、JPG、JPEG、BMP）",
                InitialDirectory = @"",   //@是取消转义字符的意思
                Filter = "全部图片|*.png;*.jpg;*.jpeg;*.bmp|JPG&JPEG图片|*.jpg;*.jpeg|BMP图片|*.bmp|PNG图片|*.png|所有文件|*.* ",
                /*
                 * FilterIndex 属性用于选择了何种文件类型,缺省设置为0,系统取Filter属性设置第一项
                 * ,相当于FilterIndex 属性设置为1.如果你编了3个文件类型，当FilterIndex ＝2时是指第2个.
                 */
                FilterIndex = 1,
                /*
                 *如果值为false，那么下一次选择文件的初始目录是上一次你选择的那个目录，
                 *不固定；如果值为true，每次打开这个对话框初始目录不随你的选择而改变，是固定的  
                 */
                RestoreDirectory = false
            };
            if (O_File.ShowDialog() == DialogResult.OK)
            {
                //获取用户选择的图片路径
                textBox1.Text = Path.GetFullPath(O_File.FileName);
                //在pictureBox1中打开图片
                pictureBox1.Image = Image.FromFile(@textBox1.Text);
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            //禁用button3防止多次点击
            button3.Enabled = false;
            if (WebRequestTest()/*检查网络的联通性*/)
            {
                var client = new Baidu.Aip.Ocr.Ocr(API_KEY, SECRET_KEY);
                var result = new JObject();
                if (radioButton1.Checked)
                {
                    if (File.Exists(@textBox1.Text))
                    {
                        pictureBox1.Image = Image.FromFile(@textBox1.Text);
                        //将图片内容读入一个字节数组
                        var image = File.ReadAllBytes(@textBox1.Text);
                        // 调用通用文字识别, 图片参数为本地图片，可能会抛出网络等异常，请使用try/catch捕获
                        result = client.GeneralBasic(image);
                    }
                    else toolStripStatusLabel2.Text = "文件不存在！";

                }
                else if (textBox6.Text != "")
                {
                    pictureBox1.ImageLocation = @textBox6.Text;
                    string url = textBox6.Text;
                    // 调用通用文字识别, 图片参数为远程url图片，可能会抛出网络等异常，请使用try/catch捕获
                    result = client.GeneralBasicUrl(url);
                }
                else toolStripStatusLabel2.Text = "网址为空！";

                //判断words_result是否存在
                if (result.ContainsKey("words_result"))
                {
                    //清空textbox2
                    textBox2.Text = "";
                    //数组生成
                    int[] arr = Enumerable.Range(0, Convert.ToInt32(result["words_result_num"])).ToArray();
                    //遍历数组字典
                    foreach (int x in arr)
                    {
                        //将识别内容写入textBox2
                        textBox2.Text += result["words_result"][x]["words"] + "\r\n";
                    }
                    toolStripStatusLabel2.Text = "完成";
                }
                else if (result.ContainsKey("error_code"))
                {
                    //索检错误代码
                    Error_mes(result);
                    toolStripStatusLabel2.Text = "存在错误";
                }
                else toolStripStatusLabel2.Text = "存在未知错误";
            }
            else toolStripStatusLabel2.Text = "无法链接服务器";
            button3.Enabled = true;
        }
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked == true)
            {
                if (textBox1.Text != "")
                    pictureBox1.Image = Image.FromFile(@textBox1.Text);
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
            SaveFileDialog S_File = new SaveFileDialog
            {

                //设置保存文件对话框的标题
                Title = "请选择要保存的文件路径",

                //初始化保存目录，默认exe文件目录
                InitialDirectory = Application.StartupPath,

                //设置保存文件的类型
                Filter = "TXT文本|*.txt|RTF文本|*.rtf|所有文件|*.*"
            };

            if (S_File.ShowDialog() == DialogResult.OK)
            {
                //获得保存文件的路径
                textBox3.Text = S_File.FileName;
                //保存

                Save_file();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Save_file();
        }
        public void Save_file()
        {
            if (textBox3.Text != "")
            {
                string txt = textBox1.Text;
                if (radioButton2.Checked)
                {
                    txt = textBox6.Text;
                }
                //创建或打开用于写UTF-8编码文本的文件
                StreamWriter W_File = File.CreateText(@textBox3.Text);
                //写入内容
                W_File.WriteLine("图片来自：" + txt);
                W_File.WriteLine(textBox2.Text);
                W_File.Flush();     //清理缓冲区
                W_File.Close();     //关闭文件
                toolStripStatusLabel2.Text = "保存成功";
                //FileStream fsWrite = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write)
                //{
                //    byte[] buffer = Encoding.Default.GetBytes(textBox2.Text);
                //    fsWrite.Write(buffer, 0, buffer.Length);
                //}
            }
            else toolStripStatusLabel2.Text = "缺少保存路径，保存失败";
        }
        public void Error_mes(JObject result)
        {
            Dictionary<JToken, string> Error_message = new Dictionary<JToken, string>
            {
                //{错误码,"描述"},
                { 4, "集群超限额" },
                { 14, "IAM鉴权失败，建议用户参照文档自查生成sign的方式是否正确，或换用控制台中ak sk的方式调用" },
                { 17, "每天流量超限额" },
                { 18, "QPS超限额" },
                { 19, " 请求总量超限额" },
                { 100, "无效参数" },
                { 110, "Access Token失效" },
                { 111, "Access token过期" },
                { 282000, "服务器内部错误，如果您使用的是高精度接口，报这个错误码的原因可能是您上传的图片中文字过多，识别超时导致的，建议您对图片进行切割后再识别，其他情况请再次请求， 如果持续出现此类错误，请通过QQ群（631977213）或工单联系技术支持团队。" },
                { 216100, " 请求中包含非法参数，请检查后重新尝试" },
                { 216101, "缺少必须的参数，请检查参数是否有遗漏" },
                { 216102, "请求了不支持的服务，请检查调用的url" },
                { 216103, " 请求中某些参数过长，请检查后重新尝试" },
                { 216110, " appid不存在，请重新核对信息是否为后台应用列表中的appid" },
                { 216200, " 图片为空，请检查后重新尝试" },
                { 216201, "上传的图片格式错误，现阶段我们支持的图片格式为：PNG、JPG、JPEG、BMP，请进行转码或更换图片" },
                { 216202, "上传的图片大小错误，现阶段我们支持的图片大小为：base64编码后小于4M，分辨率不高于4096*4096，请重新上传图片" },
                { 216630, "识别错误，请再次请求，如果持续出现此类错误，请通过QQ群（631977213）或工单联系技术支持团队。" },
                { 216631, " 识别银行卡错误，出现此问题的原因一般为：您上传的图片非银行卡正面，上传了异形卡的图片或上传的银行卡正品图片不完整" },
                { 216633, "识别身份证错误，出现此问题的原因一般为：您上传了非身份证图片或您上传的身份证图片不完整" },
                { 216634, "  检测错误，请再次请求，如果持续出现此类错误，请通过QQ群（631977213）或工单联系技术支持团队。" },
                { 282003, "请求参数缺失" },
                { 282005, "处理批量任务时发生部分或全部错误，请根据具体错误码排查" },
                { 282006, "批量任务处理数量超出限制，请将任务数量减少到10或10以下" },
                { 282110, "URL参数不存在，请核对URL后再次提交" },
                { 282111, "URL格式非法，请检查url格式是否符合相应接口的入参要求" },
                { 282112, "url下载超时，请检查url对应的图床/图片无法下载或链路状况不好，您可以重新尝试一下，如果多次尝试后仍不行，建议更换图片地址" },
                { 282113, "URL返回无效参数" },
                { 282114, "URL长度超过1024字节或为0" },
                { 282808, "request id xxxxx 不存在" },
                { 282809, "返回结果请求错误（不属于excel或json）" },
                { 282810, "图像识别错误" }
            };
            MessageBox.Show(Error_message[result["error_code"]] + "\r\n" + result["error_msg"], "错误提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(@"C:\WINDOWS\system32\mspaint.exe");
        }

        private void toolStripSplitButton1_ButtonClick(object sender, EventArgs e)
        {
            if (File.Exists(@textBox1.Text))
            {
                System.Diagnostics.Process.Start(@"C:\WINDOWS\system32\mspaint.exe", @textBox1.Text);
            }
            else toolStripStatusLabel2.Text = "文件不存在！";
        }

        private void button5_Click(object sender, EventArgs e)
        {
            textBox2.Text = "";
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (textBox2.Text != "")
            {
                Clipboard.SetDataObject(textBox2.Text);
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Save_Xml();
        }
        public void Save_Xml()
        {  //创XML建对象
            XmlDocument XmlDoc = new XmlDocument();
            //声明根节点
            XmlElement Root, settings;
            //判断文件是否存在
            if (File.Exists(XmlPath))
            {
                //该文件存在
                //加载文件
                XmlDoc.Load(XmlPath);
                //获得根节点
                Root = XmlDoc.DocumentElement;
                settings = (XmlElement)Root.SelectSingleNode("Setting");
                //匹配XmlNode
                XmlNode FilePath = Root.SelectSingleNode("ImagePath");
                //获取XmlNode列表
                XmlNodeList PathList = FilePath.ChildNodes;
                for (int i = 0; i < 1; i++)
                {
                    //遍历XmlNode列表
                    //保存本地图片路径
                    PathList[i].Attributes["FilePath"].Value = textBox1.Text + "";
                    //保存在线图片地址
                    PathList[i].Attributes["UrlPath"].Value = textBox6.Text + "";
                    //保存txt文件保存地址
                    PathList[i].Attributes["SavePath"].Value = textBox3.Text + "";
                }
            }
            else//该文件不存在
            {
                //创建声明
                XmlDeclaration dec = XmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
                XmlDoc.AppendChild(dec);
                //创建根节点
                Root = XmlDoc.CreateElement("root");
                //将节点包含到xml对象
                XmlDoc.AppendChild(Root);
                //创建新元素
                settings = XmlDoc.CreateElement("Setting");
                XmlElement FilePath = XmlDoc.CreateElement("ImagePath");
                Root.AppendChild(FilePath);
                for (int i = 0; i < 1; i++)
                {
                    XmlElement Path = XmlDoc.CreateElement("Path");
                    //设置串连值
                    Path.InnerText = i + "";
                    Path.SetAttribute("FilePath", textBox1.Text + "");
                    Path.SetAttribute("UrlPath", textBox6.Text + "");
                    Path.SetAttribute("SavePath", textBox3.Text + "");
                    FilePath.AppendChild(Path);
                }
            }
            //设置节点的属性
            //保存自动保存使用记录设置
            settings.SetAttribute("Setting", checkBox3.Checked + "");
            //保存自定义密匙设置
            settings.SetAttribute("Enabled", checkBox1.Checked + "");
            //settings.SetAttribute("AppID", "string");
            //保存API_KEY
            settings.SetAttribute("API_KEY", textBox4.Text + "");
            //保存SECRET_KEY
            settings.SetAttribute("SECRET_KEY", textBox5.Text + "");
            Root.AppendChild(settings);
            //保存xml文件
            XmlDoc.Save(XmlPath);
        }
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked) textBox5.PasswordChar = '*';
            else textBox5.PasswordChar = '\0';
        }

        private void button8_Click(object sender, EventArgs e)
        {
            //判断是否启用自定义密匙
            if (checkBox1.Checked)
            {
                //启用自定义密匙
                API_KEY = textBox4.Text + "";
                SECRET_KEY = textBox5.Text + "";
            }
            else
            {
                //还原默认密匙
                API_KEY = "agCqSFsY7OtRTGx3TaykmvIq";
                SECRET_KEY = "SWZqUnLpGXCUmZ6fPmxtcR8y4f6rI3Cy";
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (checkBox3.Checked)
            {
                Save_Xml();
            }
        }
    }
}
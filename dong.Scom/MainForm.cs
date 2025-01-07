
using System.Diagnostics;
using System.IO.Ports;
using System.Windows.Forms;
using AntdUI;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Timer = System.Windows.Forms.Timer;

namespace dong.Scom
{
    public partial class MainForm : AntdUI.Window
    {
        private SerialPort serialPort;
        private Timer timer;

        public MainForm()
        {
            InitializeComponent();
            LoadSerialPortToSelect();
            selectDataBits.SelectedIndex = 0; // 默认数据位 8
            selectParity.SelectedIndex = 0; // 默认校验位 None
            selectStopBits.SelectedIndex = 1; // 默认停止位 1
            InitializeTimeLabel();
            AntdUI.Config.ShowInWindow = true;

        }

        private void InitializeTimeLabel()
        {
            timer = new Timer();
            timer.Interval = 1000; // 每秒更新一次
            timer.Tick += Timer_Tick; // 触发定时器事件
            timer.Start();
        }
        private void Timer_Tick(object? sender, EventArgs e)
        {
            // 更新时间格式为 当前时间 2024/01/07 13:11:00
            labelTime.Text = $"当前时间 {DateTime.Now:yyyy/MM/dd HH:mm:ss}";
        }
        private void LoadSerialPortToSelect()
        {
            // 获取所有可用的串口名称
            string[] ports = SerialPort.GetPortNames();

            // 将串口添加到 ComboBox 中
            selectSerialPorts.Items.Clear();
            foreach (var port in ports)
            {
                selectSerialPorts.Items.Add(port);
            }

            // 如果有串口，默认选择第一个
            if (ports.Length > 0)
            {
                selectSerialPorts.SelectedIndex = 0;
            }
            else
            {

            }
        }


        private void btnSaveWindow_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                // 设置文件过滤器，限制只能保存为 txt 文件
                saveFileDialog.Filter = "文本文件 (*.txt)|*.txt|所有文件 (*.*)|*.*";
                saveFileDialog.DefaultExt = "txt";  // 设置默认扩展名
                saveFileDialog.AddExtension = true; // 如果用户没有添加扩展名，自动加上 .txt

                // 显示保存文件对话框
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // 使用 StreamWriter 保存文件
                        using (StreamWriter writer = new StreamWriter(saveFileDialog.FileName))
                        {
                            writer.Write(inputReceive.Text);  // 将 RichTextBox 中的文本写入文件
                        }

                        MessageBox.Show("文件保存成功！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"保存文件失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        private void btnOpenPort_Click(object sender, EventArgs e)
        {
            try
            {
                if (serialPort != null && serialPort.IsOpen)
                {
                    serialPort.Close();
                    btnOpenPort.Text = "打开串口";
                    AntdUI.Notification.error(this, "错误", "串口已经打开", autoClose: 3, align: TAlignFrom.TR);
                    return;
                }
                if (selectSerialPorts.Text.Trim() == "")
                {

                    AntdUI.Notification.error(this, "错误", "串口信息为空", autoClose: 3, align: TAlignFrom.TR);
                    return;
                }
                if (selectBaudRate.Text.Trim() == "")
                {

                    AntdUI.Message.success(this, "This is a success message", autoClose: 3);
                    AntdUI.Notification.error(this, "错误", "串口比特率设置为空", autoClose: 3, align: TAlignFrom.TR);
                    return;
                }
                if (!int.TryParse(selectBaudRate.SelectedValue.ToString(), out int result))
                {
                    AntdUI.Notification.error(this, "错误", "串口比特率设置不为数字", autoClose: 3, align: TAlignFrom.TR);
                    return;
                }
                if (!int.TryParse(selectDataBits.SelectedValue.ToString(), out result))
                {
                    AntdUI.Notification.error(this, "", "串口比特率设置不为数字", autoClose: 3, align: TAlignFrom.TR);
                    return;
                }
                // 获取选择的串口配置
                string selectedPort = selectSerialPorts.SelectedValue.ToString();
                int baudRate = int.Parse(selectBaudRate.SelectedValue.ToString());
                int dataBits = int.Parse(selectDataBits.SelectedValue.ToString());


                Parity parity = (Parity)selectParity.SelectedIndex;
                StopBits stopBits = (StopBits)selectStopBits.SelectedIndex;

                // 初始化串口
                serialPort = new SerialPort(selectedPort, baudRate, parity, dataBits, stopBits);
                serialPort.Open();
                serialPort.DataReceived += SerialPort_DataReceived;

                btnOpenPort.Text = "关闭串口";
                AntdUI.Notification.success(this, "成功", $"串口 {selectedPort} 已成功打开！", autoClose: 3, align: TAlignFrom.TR);
            }
            catch (Exception ex)
            {
                AntdUI.Notification.error(this, "错误", $"打开串口失败: {ex.Message}", autoClose: 3, align: TAlignFrom.TR);

            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // 从串口读取数据
            string data = serialPort.ReadExisting(); // 读取所有可用的数据

            // 使用 Invoke 确保在 UI 线程中更新 RichTextBox
            Invoke(new Action(() =>
            {
                inputReceive.AppendText(data);  // 将接收到的数据添加到 RichTextBox 中
                inputReceive.ScrollToCaret();   // 滚动到文本框的末尾
            }));
        }

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {
            string url = "https://github.com/baitianbt/dong.Scom";
            try
            {
                // 打开默认浏览器并访问网址
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"无法打开网址: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void 计算器ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // 启动计算器
                Process.Start("calc.exe");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"无法打开计算器: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnCleanWindow_Click(object sender, EventArgs e)
        {
            inputReceive.Clear();
        }

        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                // 设置文件过滤器，只允许选择 .txt 文件
                openFileDialog.Filter = "文本文件 (*.txt)|*.txt|所有文件 (*.*)|*.*";
                openFileDialog.Title = "选择要打开的文件";

                // 显示打开文件对话框
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // 使用 StreamReader 读取文件内容
                        string fileContent = File.ReadAllText(openFileDialog.FileName);

                        // 将文件内容显示在 RichTextBox 中
                        inputSend.Text = fileContent;

                        MessageBox.Show("文件导入成功！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"读取文件失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                // 获取 RichTextBox 中的文本
                string dataToSend = inputSend.Text;

                // 检查串口是否已打开
                if (serialPort.IsOpen)
                {
                    // 发送数据到串口
                    serialPort.Write(dataToSend);

                    MessageBox.Show("数据发送成功", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("串口未打开", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"发送数据失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

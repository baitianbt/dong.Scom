
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
            selectDataBits.SelectedIndex = 0; // Ĭ������λ 8
            selectParity.SelectedIndex = 0; // Ĭ��У��λ None
            selectStopBits.SelectedIndex = 1; // Ĭ��ֹͣλ 1
            InitializeTimeLabel();
            AntdUI.Config.ShowInWindow = true;

        }

        private void InitializeTimeLabel()
        {
            timer = new Timer();
            timer.Interval = 1000; // ÿ�����һ��
            timer.Tick += Timer_Tick; // ������ʱ���¼�
            timer.Start();
        }
        private void Timer_Tick(object? sender, EventArgs e)
        {
            // ����ʱ���ʽΪ ��ǰʱ�� 2024/01/07 13:11:00
            labelTime.Text = $"��ǰʱ�� {DateTime.Now:yyyy/MM/dd HH:mm:ss}";
        }
        private void LoadSerialPortToSelect()
        {
            // ��ȡ���п��õĴ�������
            string[] ports = SerialPort.GetPortNames();

            // ��������ӵ� ComboBox ��
            selectSerialPorts.Items.Clear();
            foreach (var port in ports)
            {
                selectSerialPorts.Items.Add(port);
            }

            // ����д��ڣ�Ĭ��ѡ���һ��
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
                // �����ļ�������������ֻ�ܱ���Ϊ txt �ļ�
                saveFileDialog.Filter = "�ı��ļ� (*.txt)|*.txt|�����ļ� (*.*)|*.*";
                saveFileDialog.DefaultExt = "txt";  // ����Ĭ����չ��
                saveFileDialog.AddExtension = true; // ����û�û�������չ�����Զ����� .txt

                // ��ʾ�����ļ��Ի���
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // ʹ�� StreamWriter �����ļ�
                        using (StreamWriter writer = new StreamWriter(saveFileDialog.FileName))
                        {
                            writer.Write(inputReceive.Text);  // �� RichTextBox �е��ı�д���ļ�
                        }

                        MessageBox.Show("�ļ�����ɹ���", "�ɹ�", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"�����ļ�ʧ��: {ex.Message}", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    btnOpenPort.Text = "�򿪴���";
                    AntdUI.Notification.error(this, "����", "�����Ѿ���", autoClose: 3, align: TAlignFrom.TR);
                    return;
                }
                if (selectSerialPorts.Text.Trim() == "")
                {

                    AntdUI.Notification.error(this, "����", "������ϢΪ��", autoClose: 3, align: TAlignFrom.TR);
                    return;
                }
                if (selectBaudRate.Text.Trim() == "")
                {

                    AntdUI.Message.success(this, "This is a success message", autoClose: 3);
                    AntdUI.Notification.error(this, "����", "���ڱ���������Ϊ��", autoClose: 3, align: TAlignFrom.TR);
                    return;
                }
                if (!int.TryParse(selectBaudRate.SelectedValue.ToString(), out int result))
                {
                    AntdUI.Notification.error(this, "����", "���ڱ��������ò�Ϊ����", autoClose: 3, align: TAlignFrom.TR);
                    return;
                }
                if (!int.TryParse(selectDataBits.SelectedValue.ToString(), out result))
                {
                    AntdUI.Notification.error(this, "", "���ڱ��������ò�Ϊ����", autoClose: 3, align: TAlignFrom.TR);
                    return;
                }
                // ��ȡѡ��Ĵ�������
                string selectedPort = selectSerialPorts.SelectedValue.ToString();
                int baudRate = int.Parse(selectBaudRate.SelectedValue.ToString());
                int dataBits = int.Parse(selectDataBits.SelectedValue.ToString());


                Parity parity = (Parity)selectParity.SelectedIndex;
                StopBits stopBits = (StopBits)selectStopBits.SelectedIndex;

                // ��ʼ������
                serialPort = new SerialPort(selectedPort, baudRate, parity, dataBits, stopBits);
                serialPort.Open();
                serialPort.DataReceived += SerialPort_DataReceived;

                btnOpenPort.Text = "�رմ���";
                AntdUI.Notification.success(this, "�ɹ�", $"���� {selectedPort} �ѳɹ��򿪣�", autoClose: 3, align: TAlignFrom.TR);
            }
            catch (Exception ex)
            {
                AntdUI.Notification.error(this, "����", $"�򿪴���ʧ��: {ex.Message}", autoClose: 3, align: TAlignFrom.TR);

            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // �Ӵ��ڶ�ȡ����
            string data = serialPort.ReadExisting(); // ��ȡ���п��õ�����

            // ʹ�� Invoke ȷ���� UI �߳��и��� RichTextBox
            Invoke(new Action(() =>
            {
                inputReceive.AppendText(data);  // �����յ���������ӵ� RichTextBox ��
                inputReceive.ScrollToCaret();   // �������ı����ĩβ
            }));
        }

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {
            string url = "https://github.com/baitianbt/dong.Scom";
            try
            {
                // ��Ĭ���������������ַ
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"�޷�����ַ: {ex.Message}", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ������ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // ����������
                Process.Start("calc.exe");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"�޷��򿪼�����: {ex.Message}", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void �˳�ToolStripMenuItem_Click(object sender, EventArgs e)
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
                // �����ļ���������ֻ����ѡ�� .txt �ļ�
                openFileDialog.Filter = "�ı��ļ� (*.txt)|*.txt|�����ļ� (*.*)|*.*";
                openFileDialog.Title = "ѡ��Ҫ�򿪵��ļ�";

                // ��ʾ���ļ��Ի���
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // ʹ�� StreamReader ��ȡ�ļ�����
                        string fileContent = File.ReadAllText(openFileDialog.FileName);

                        // ���ļ�������ʾ�� RichTextBox ��
                        inputSend.Text = fileContent;

                        MessageBox.Show("�ļ�����ɹ���", "�ɹ�", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"��ȡ�ļ�ʧ��: {ex.Message}", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                // ��ȡ RichTextBox �е��ı�
                string dataToSend = inputSend.Text;

                // ��鴮���Ƿ��Ѵ�
                if (serialPort.IsOpen)
                {
                    // �������ݵ�����
                    serialPort.Write(dataToSend);

                    MessageBox.Show("���ݷ��ͳɹ�", "�ɹ�", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("����δ��", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"��������ʧ��: {ex.Message}", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

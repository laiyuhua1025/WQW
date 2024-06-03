using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace WindowsFormsApp3
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            InitializeFontComboBox();
            InitializeFontSizeComboBox();
            InitializeFontStyleComboBox();
        }
        private void InitializeFontComboBox()
        {
            foreach (FontFamily font in FontFamily.Families)
            {
                comboBoxFont.Items.Add(font.Name);
            }
            comboBoxFont.SelectedIndex = 0;
    }
        private void InitializeFontSizeComboBox()
        {
            for(int i = 8;  i <= 72; i += 2)
            {
                comboBoxSize.Items.Add(i);
            }
            comboBoxSize.SelectedIndex = 2;
        }
        private void InitializeFontStyleComboBox()
        {
            comboBoxStyle.Items.Add(FontStyle.Regular.ToString());
            comboBoxStyle.Items.Add(FontStyle.Bold.ToString());
            comboBoxStyle.Items.Add(FontStyle.Italic.ToString());
            comboBoxStyle.Items.Add(FontStyle.Underline.ToString());
            comboBoxStyle.Items.Add(FontStyle.Strikeout.ToString());
            comboBoxStyle.SelectedIndex = 0;
        }

        private int selectionStart = 0;
        private int selectionLength = 0;

        private void comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectionStart = rtbText.SelectionStart;
            selectionLength = rtbText.SelectionLength;
            if (rtbText.SelectionFont != null)
            {
                string selectedFont = comboBoxFont.SelectedItem?.ToString();
                string selectedSizeStr = comboBoxSize.SelectedItem?.ToString();
                string selectedStyleStr = comboBoxStyle.SelectedItem?.ToString();
                if (selectedFont != null && selectedSizeStr != null && selectedStyleStr != null)
                {
                    float selectedSize = float.Parse(selectedSizeStr);
                    FontStyle selectedStyle = (FontStyle)Enum.Parse(typeof(FontStyle), selectedStyleStr);
                    Font currentFont = rtbText.SelectionFont;
                    FontStyle newStyle = currentFont.Style;
                    if (comboBoxStyle.SelectedItem.ToString() == FontStyle.Bold.ToString())
                        newStyle = FontStyle.Bold;
                    else if (comboBoxStyle.SelectedItem.ToString() == FontStyle.Italic.ToString())
                        newStyle = FontStyle.Italic;
                    else if (comboBoxStyle.SelectedItem.ToString() == FontStyle.Underline.ToString())
                        newStyle = FontStyle.Underline;
                    else if (comboBoxStyle.SelectedItem.ToString() == FontStyle.Strikeout.ToString())
                        newStyle = FontStyle.Strikeout;
                    else
                        newStyle = FontStyle.Regular;
                    Font newFont = new Font(selectedFont, selectedSize, newStyle);
                    rtbText.SelectionFont = newFont;
                }
            }
            rtbText.Focus();
            rtbText.Select(selectionStart, selectionLength);
        }


            private void btnSave_Click(object sender, EventArgs e)
        {

            saveFileDialog1.Title = "儲存檔案";

            saveFileDialog1.Filter = "文字檔案 (*.txt)|*.txt|所有檔案 (*.*)|*.*";

            saveFileDialog1.FilterIndex = 1;

            saveFileDialog1.InitialDirectory = "C:\\";


            DialogResult result = saveFileDialog1.ShowDialog();


            FileStream fileStream = null;


            if (result == DialogResult.OK)
            {
                try
                {

                    string saveFileName = saveFileDialog1.FileName;


                    fileStream = new FileStream(saveFileName, FileMode.Create, FileAccess.Write);

                    byte[] data = Encoding.UTF8.GetBytes(rtbText.Text);
                    fileStream.Write(data, 0, data.Length);



                    MessageBox.Show("檔案儲存成功。", "訊息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {

                    MessageBox.Show("儲存檔案時發生錯誤: " + ex.Message, "錯誤訊息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                finally
                {

                    fileStream.Close();
                }
            }
            else
            {
                MessageBox.Show("使用者取消了儲存檔案操作。", "訊息", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
            }
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {

            openFileDialog1.Title = "選擇檔案";

            openFileDialog1.Filter = "文字檔案 (*.txt)|*.txt|所有檔案 (*.*)|*.*";

            openFileDialog1.FilterIndex = 1;

            openFileDialog1.InitialDirectory = "C:\\";

            openFileDialog1.Multiselect = true;


            DialogResult result = openFileDialog1.ShowDialog();


            if (result == DialogResult.OK)
            {
                try
                {

                    string selectedFileName = openFileDialog1.FileName;

                    using (FileStream fileStream = new FileStream(selectedFileName, FileMode.Open, FileAccess.Read))
                    {

                        using (StreamReader streamReader = new StreamReader(fileStream, Encoding.UTF8))
                        {

                            rtbText.Text = streamReader.ReadToEnd();
                        }
                    }


                }
                catch (Exception ex)
                {

                    MessageBox.Show("讀取檔案時發生錯誤: " + ex.Message, "錯誤訊息", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("使用者取消了選擇檔案操作。", "訊息", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
            }

            openFileDialog1.Title = "選擇檔案";
            openFileDialog1.Filter = "RTF格式檔案 (*.rtf)|*.rtf|文字檔案 (*.txt)|*.txt|所有檔案 (*.txt*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.Multiselect= true;
            DialogResult result1 = openFileDialog1.ShowDialog();
            if(result == DialogResult.OK)
            {
                try
                {
                    string selectedFileName = openFileDialog1.FileName;
                    string fileExtension = Path.GetExtension(selectedFileName).ToLowerInvariant();
                    if (fileExtension == ".txt")
                    {
                        using (FileStream fileStream = FileStream(selectedFileName, FileMode.Open, FileAccess.Read))
                        using (StreamReader streamReader = new StreamReader(fileStream, Encoding.UTF8))
                            rtbText.Text = streamReader.ReadToEnd();
                    }
                }
             }
            else if (fileExtension == ".rtf")
            {
                rtbText.LoadFile(selectedFileName, RichTextBoxStreamType.RichText);
            }
            
        
        }
        private bool isUndoRedo = false;
        private Stack<string> UndoStack = new Stack<string>();
        private Stack<string> redoStack = new Stack<string>();
        private const int MaxtHistoryCount = 10;

        public int MaxHistoryCount { get; private set; }

        private void rtbText_TextChanged(object sender, EventArgs e)
        {

            if (isUndoRedo == false)
            {
                UndoStack.Push(rtbText.Text);
                redoStack.Clear();
            }                     
            if (UndoStack.Count > MaxHistoryCount)
            {
               
                Stack<string> tempStack = new Stack<string>();
                for (int i = 0; i < MaxHistoryCount; i++)
                {
                    tempStack.Push(UndoStack.Pop());
                }
                UndoStack.Clear();
                
                foreach (string item in tempStack)
                {
                    UndoStack.Push(item);
                }
            }
            UpdateListBox();          
        }
        void UpdateListBox()
        {
            listUndo.Items.Clear(); 

            
            foreach (string item in UndoStack)
            {
                listUndo.Items.Add(item);
            }
        }
        
        
        private void btnRedo_Click(object sender, EventArgs e)
            
        {
            if (redoStack.Count > 0)
            {

                isUndoRedo = true;
                UndoStack.Push(redoStack.Pop());
                rtbText.Text = UndoStack.Peek();
                UpdateListBox();
                isUndoRedo = false;

            }

        }

        private void btnUndo_Click(object sender, EventArgs e)
        {
            if (UndoStack.Count > 1)
            {

                isUndoRedo = true;
                redoStack.Push(UndoStack.Pop());
                rtbText.Text = UndoStack.Peek();
                UpdateListBox();
                isUndoRedo = false;

            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBoxSize_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBoxStyle_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
    }

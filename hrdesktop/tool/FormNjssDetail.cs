﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.IO;
using System.Diagnostics;

namespace highwayns
{
    public partial class FormNjssDetail : Form
    {
        private string CompanyName="";
        public FormNjssDetail(string CompanyName)
        {
            this.CompanyName = CompanyName;
            InitializeComponent();
        }
        Hashtable bid = new Hashtable();
        /// <summary>
        /// load from Csv
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLoadCsv_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                using (StreamReader sr = new StreamReader(dlg.FileName, Encoding.UTF8))
                {
                    dgvData.Rows.Clear();
                    bid.Clear();
                    string line = sr.ReadLine();
                    while (line != null)
                    {
                        line = line.Trim();
                        //line += sr.ReadLine().Trim();
                        //line += sr.ReadLine().Trim();
                        string[] temp = line.Split(',');
                        if (temp[0] == CompanyName)
                        {
                            string[] rows = new string[9];
                            rows[0] = dgvData.Rows.Count.ToString();
                            rows[1] = temp[0];//名称
                            rows[2] = temp[1];//プロジェクト
                            rows[3] = temp[2].Replace("都道府県", "");//区域                    
                            rows[4] = temp[3].Replace("入札形式", "");//入札形式
                            rows[5] = temp[4].Replace("公示日", "");//公示日
                            rows[6] = temp[5].Substring(2);//住所
                            rows[7] = temp[6];//web
                            rows[8] = "";//other
                            dgvData.Rows.Add(rows);
                            bid[rows[0]] = rows;
                        }
                        line = sr.ReadLine();
                    }
                }
                MessageBox.Show("Load Csv Over!\r\n there are " + bid.Keys.Count.ToString() + " record!");
            }
        }
        /// <summary>
        /// load csv
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormNjssDetail_Load(object sender, EventArgs e)
        {
            btnLoadCsv_Click(null, null);
        }
        /// <summary>
        /// DoubleClick
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvData_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > -1)
            {
                string url = dgvData.Rows[e.RowIndex].Cells[7].Value.ToString();
                ExecuteCommand(@"C:\Program Files\Internet Explorer\iexplore.exe", url, "", false);
            }

        }
        /// <summary>
        /// 应用程序启动
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="localworkpath"></param>
        /// <returns></returns>
        private static int ExecuteCommand(string cmd, string para, string localworkpath, bool userShell)
        {
            int exitCode = -1;

            try
            {
                Process process = new Process();
                process.StartInfo.UseShellExecute = userShell;
                process.StartInfo.FileName = cmd;
                process.StartInfo.WorkingDirectory = localworkpath;
                process.StartInfo.Arguments = para;
                process.Start();
                //process.WaitForExit();
                exitCode = process.ExitCode;
                process.Close();
            }
            catch (Exception er)
            {
                //MessageBox.Show("Exception=" + er.ToString());
                exitCode = -1;
            }
            return exitCode;
        }
    }
}

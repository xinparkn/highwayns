﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using NC.HPS.Lib;
using System.IO;
using System.Collections;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
//using Com.Seezt.Skins;

namespace HPSBid
{
    public partial class FormMain : Form
    {

        private const string SYSTEM_ID = "HPSBid";
        private const string SQL_FILE = "bid.sql";

        private string strDataSource = null;
        private string strDbName = null;
        private string strUserName = null;
        private string strPassword = null;
        /// <summary>
        /// 取得实例
        /// </summary>
        /// <param name="paramenter"></param>
        public void GetInstance(object[] paramenter)
        {
            CmWinServiceAPI db_ = null;
            if (paramenter.Length > 0) db_ = (CmWinServiceAPI)paramenter[0];
            if (paramenter.Length > 1)
            {
                string serialNo = (string)paramenter[1];
                if (!String.IsNullOrEmpty(serialNo))
                {
                    if (NCCryp.checkLic(serialNo, SYSTEM_ID))
                    {
                        FormMain form = new FormMain(db_);
                        form.ShowDialog();
                    }
                }
            }
        }
        /// <summary>
        /// 数据库
        /// </summary>
        private DB db;
        /// <summary>
        /// 主要函数入口
        /// </summary>
        /// <param name="db"></param>
        private FormMain(CmWinServiceAPI db)
        {
            this.db = new DB(db);
            InitializeComponent();
        }
        public FormMain()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 画面初期化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormCustomer_Load(object sender, EventArgs e)
        {
            string scriptFile = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), SQL_FILE);
            if (File.Exists(scriptFile))
            {
                if (GetConnectionString())
                {
                    executeCjwScript(scriptFile);
                }
            }
            initKind();
            initFormat();
            init("","","");
        }
        /// <summary>
        /// 初期化
        /// </summary>
        private void init(string kind, string format, string subscripted)
        {
            DataSet ds = new DataSet();
            string wheresql = "1=1";
            if (kind != "") wheresql += " and kind='" + kind + "'";
            if (format != "") wheresql += " and format='" + format + "'";
            if (subscripted != "") wheresql += " and subscripted='" + subscripted + "'";

            if (db.GetGovement(0, 0, "*", wheresql, "", ref ds))
            {
                dataGridView1.DataSource = ds.Tables[0];
                lblRecordNum.Text = "("+ds.Tables[0].Rows.Count.ToString() + ")";
            }
        }
        /// <summary>
        /// 初期化行业
        /// </summary>
        private void initKind()
        {
            DataSet ds = new DataSet();
            if (db.GetGovement(0, 0, "distinct kind", "", "", ref ds))
            {
                cmbKinds.DataSource = ds.Tables[0];
                cmbKinds.DisplayMember = "kind";
            }
            DataSet ds2 = new DataSet();
            if (db.GetGovement(0, 0, "distinct kind", "", "", ref ds2))
            {
                cmbKind.DataSource = ds2.Tables[0];
                cmbKind.DisplayMember = "kind";
            }
            cmbKinds.Text = "";
        }
        /// <summary>
        /// 初期化分类
        /// </summary>
        private void initFormat()
        {
            DataSet ds = new DataSet();
            if (db.GetGovement(0, 0, "distinct format", "", "", ref ds))
            {
                cmbFormat.DataSource = ds.Tables[0];
                cmbFormat.DisplayMember = "format";
            }
            DataSet ds2 = new DataSet();
            if (db.GetGovement(0, 0, "distinct format", "", "", ref ds2))
            {
                cmbFormats.DataSource = ds2.Tables[0];
                cmbFormats.DisplayMember = "format";
            }
            cmbFormats.Text = "";
        }
        /// <summary>
        /// 送信用構造体
        /// </summary>
        struct MailPara
        {
            public string sendid;
            public string name;
            public string subject;
            public string body;
            public string htmlbody;
            public string picfile;
            public string address;
            public string user;
            public string password;
            public string from;
            public string to;
            public string servertype;
            public string attachement;
            public string isHtml;
        }

        /// <summary>
        /// メールサーバ情報取得
        /// </summary>
        private Hashtable GetMailServer()
        {
            DataSet ds = new DataSet();
            if (db.db.GetMailServer(0, 0, "编号,名称,地址,端口,用户,密码,送信人地址,服务器类型,添付文件,HTML", "", "", ref ds))
            {
                Hashtable ht = new Hashtable();
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    MailPara mp = new MailPara();
                    mp.name = dr["名称"].ToString();
                    mp.address = dr["地址"].ToString();
                    //mp.port = dr["端口"].ToString();
                    mp.user = dr["用户"].ToString();
                    mp.password = dr["密码"].ToString();
                    mp.from = dr["送信人地址"].ToString();
                    mp.servertype = dr["服务器类型"].ToString().Trim();
                    mp.attachement = dr["添付文件"].ToString();
                    mp.isHtml = dr["HTML"].ToString();
                    ht[mp.servertype] = mp;
                }
                return ht;
            }
            return null;
        }
        /// <summary>
        /// executeCjwScriptを実行する。
        /// </summary>
        private bool executeCjwScript(string scriptFile)
        {
            string strConnectionString = string.Format(
                "Data Source={0};Initial Catalog={1};User ID={2};Password={3}",
                strDataSource, strDbName, strUserName, strPassword);
            SqlConnection conn = new SqlConnection(strConnectionString);

            try
            {
                FileInfo file = new FileInfo(scriptFile);
                using (StreamReader sr = file.OpenText())
                {
                    string script = sr.ReadToEnd();

                    conn.Open();
                    IEnumerable<string> commands = Regex.Split(script, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                    foreach (string command in commands)
                    {
                        if (command.Trim() != "")
                            new SqlCommand(command, conn).ExecuteNonQuery();
                    }
                    Application.DoEvents();
                    conn.Close();
                }
                //file.Delete();
            }
            catch (Exception ex)
            {
                NCLogger.GetInstance().WriteExceptionLog(ex);
                Application.DoEvents();
                conn.Close();
                return false;
            }
            return true;

        }
        /// <summary>
        /// 取得数据库连接
        /// </summary>
        /// <returns></returns>
        protected bool GetConnectionString()
        {
            NdnXmlConfig xmlConfig;
            xmlConfig = new NdnXmlConfig(NCConst.CONFIG_FILE_DIR + NCUtility.GetAppConfig());
            string strConnectionString = null;
            string str = "ConnectionString";
            if (xmlConfig.ReadXmlData("database", str, ref strConnectionString))
            {
                string[] temp = strConnectionString.Split(';');
                if (temp.Length > 0)
                {
                    for (int idx = 0; idx < temp.Length; idx++)
                    {
                        if (temp[idx].IndexOf("Password=") > -1)
                        {
                            temp[idx] = temp[idx].Replace("Password=", "");
                            strPassword = NCCryp.Decrypto(temp[idx]);
                        }
                        else if (temp[idx].IndexOf("Data Source=") > -1)
                        {
                            temp[idx] = temp[idx].Replace("Data Source=", "");
                            strDataSource = temp[idx];
                        }
                        else if (temp[idx].IndexOf("Initial Catalog=") > -1)
                        {
                            temp[idx] = temp[idx].Replace("Initial Catalog=", "");
                            strDbName = temp[idx];
                        }
                        else if (temp[idx].IndexOf("User ID=") > -1)
                        {
                            temp[idx] = temp[idx].Replace("User ID=", "");
                            strUserName = temp[idx];
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 选择行变化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                txtId.Text = dataGridView1.SelectedRows[0].Cells[0].Value.ToString();
                txtCname.Text = dataGridView1.SelectedRows[0].Cells[1].Value.ToString();
                txtName.Text = dataGridView1.SelectedRows[0].Cells[2].Value.ToString();
                txtPostCode.Text = dataGridView1.SelectedRows[0].Cells[3].Value.ToString();
                txtAddress.Text = dataGridView1.SelectedRows[0].Cells[4].Value.ToString();
                txtTel.Text = dataGridView1.SelectedRows[0].Cells[5].Value.ToString();
                txtFax.Text = dataGridView1.SelectedRows[0].Cells[6].Value.ToString();
                cmbKind.Text = dataGridView1.SelectedRows[0].Cells[7].Value.ToString();
                cmbFormat.Text = dataGridView1.SelectedRows[0].Cells[8].Value.ToString();
                txtScale.Text = dataGridView1.SelectedRows[0].Cells[9].Value.ToString();
                txtCYMD.Text = dataGridView1.SelectedRows[0].Cells[10].Value.ToString();
                txtOther.Text = dataGridView1.SelectedRows[0].Cells[11].Value.ToString();
                txtMail.Text = dataGridView1.SelectedRows[0].Cells[12].Value.ToString();
                txtWeb.Text = dataGridView1.SelectedRows[0].Cells[13].Value.ToString();
                txtJCname.Text = dataGridView1.SelectedRows[0].Cells[14].Value.ToString();
                txtCreateTime.Text = dataGridView1.SelectedRows[0].Cells[15].Value.ToString();
                cmbSubscript.Text = dataGridView1.SelectedRows[0].Cells[16].Value.ToString();
            }
        }
        /// <summary>
        /// 增加機構
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAdd_Click(object sender, EventArgs e)
        {
            int id = 0;
            String valueList = "'" + txtCname.Text + "','" + txtName.Text + "','" + txtPostCode.Text + "','" + txtAddress.Text + "','" + txtTel.Text
                + "','" + txtFax.Text + "','" + cmbKind.Text + "','" + cmbFormat.Text + "','" + txtScale.Text + "','" + txtCYMD.Text + "','" + txtOther.Text
                + "','" + txtMail.Text + "','" + txtWeb.Text + "','" + txtJCname.Text + "','" + System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "','" + cmbSubscript.Text + "','" + db.db.UserID + "'";
            if (db.SetGovement(0, 0, "Cname,name,postcode,address,tel,fax,kind,format,scale,CYMD,other,mail,web,jCNAME,createtime,subscripted,UserID",
                                 "", valueList, out id))
            {
                string msg = NCMessage.GetInstance(db.db.Language).GetMessageById("CM0001I", db.db.Language);
                MessageBox.Show(msg);
                init("","","");
            }
            else
            {
                string msg = NCMessage.GetInstance(db.db.Language).GetMessageById("CM0002I", db.db.Language);
                MessageBox.Show(msg);
            }

        }
        /// <summary>
        /// 更新機構
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {

                int id = 0;
                String valueList = "Cname='" + txtCname.Text + "',name='" + txtName.Text + "',postcode='" + txtPostCode.Text + "',address='" + txtAddress.Text + "',tel='" + txtTel.Text
                    + "',fax='" + txtFax.Text + "',kind='" + cmbKind.Text + "',format='" + cmbFormat.Text + "',scale='" + txtScale.Text + "',CYMD='" + txtCYMD.Text + "',other='" + txtOther.Text
                    + "',mail='" + txtMail.Text + "',web='" + txtWeb.Text + "',jCName='" + txtJCname.Text + "',createtime='" + txtCreateTime.Text + "',subscripted='" + cmbSubscript.Text + "'";
                if (db.SetGovement(0, 1, "", "id=" + txtId.Text, valueList, out id) && id == 1)
                {
                    string msg = NCMessage.GetInstance(db.db.Language).GetMessageById("CM0003I", db.db.Language);
                    MessageBox.Show(msg);
                    init("","","");
                }
                else
                {
                    string msg = NCMessage.GetInstance(db.db.Language).GetMessageById("CM0004I", db.db.Language);
                    MessageBox.Show(msg);
                }
            }
            else
            {
                string msg = NCMessage.GetInstance(db.db.Language).GetMessageById("CM0000I", db.db.Language);
                MessageBox.Show(msg);
            }
        }
        /// <summary>
        /// Create　Publish
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPublish_Click(object sender, EventArgs e)
        {
            Hashtable ht = null; 
            DataSet ds = new DataSet();
            NdnPublicFunction function = new NdnPublicFunction();
            int newid = 0;
            if (db.GetGovement(0, 0, "*", "", "", ref ds))
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    String id = row["id"].ToString();
                    String mail = row["mail"].ToString();
                    String subscripted = row["subscripted"].ToString().Trim();
                    if (!function.IsMail(mail, false) || ht[mail]!=null)
                    {
                        db.SetGovement(0, 1, "", "id=" + id, "subscripted='N'", out newid);
                    }
                    else
                    {
                        if (subscripted != "T")
                        {
                            db.SetGovement(0, 1, "", "id=" + id, "subscripted='Y'", out newid);
                        }
                    }
                }
                string msg = NCMessage.GetInstance(db.db.Language).GetMessageById("CM0007I", db.db.Language);
                MessageBox.Show(msg);
                init("", "", "");
            }
            else
            {
                string msg = NCMessage.GetInstance(db.db.Language).GetMessageById("CM0008I", db.db.Language);
                MessageBox.Show(msg);
            }

        }
        /// <summary>
        /// 删除機構数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int id = 0;
                if (db.SetGovement(0, 2, "", "id=" + txtId.Text, "", out id) && id == 2)
                {
                    string msg = NCMessage.GetInstance(db.db.Language).GetMessageById("CM0005I", db.db.Language);
                    MessageBox.Show(msg);
                    init("", "", "");
                }
                else
                {
                    string msg = NCMessage.GetInstance(db.db.Language).GetMessageById("CM0006I", db.db.Language);
                    MessageBox.Show(msg);
                }
            }
            else
            {
                string msg = NCMessage.GetInstance(db.db.Language).GetMessageById("CM0000I", db.db.Language);
                MessageBox.Show(msg);
            }
        }
        /// <summary>
        /// 数据检索
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, EventArgs e)
        {
            init(cmbKinds.Text, cmbFormats.Text, cmbSubscripts.Text);
        }
        /// <summary>
        /// 機構导入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnGet_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Text File (*.csv)|*.csv|All File (*.*)|*.*";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                importGovementFile(dlg.FileName);
                string msg = NCMessage.GetInstance(db.db.Language).GetMessageById("CM0001I", db.db.Language);
                MessageBox.Show(msg);
            }


        }
        /// <summary>
        /// 機構导入
        /// </summary>
        /// <param name="csvFile"></param>
        private void importGovementFile(String fileName)
        {
            NdnPublicFunction func = new NdnPublicFunction();
            using (StreamReader reader =
                new StreamReader(fileName, Encoding.GetEncoding("UTF-8")))
            {
                String line = reader.ReadLine();
                while (line != null)
                {
                    string[] temp = line.Split(',');
                    string name = temp[0];//名称
                    string bunrui = temp[1];//分類
                    string area = temp[2];//区域                    
                    string num1 = temp[3].Replace("入札可能案件", "");//入札可能案件
                    string num2 = temp[4].Replace("案件登録数", "");//案件登録数
                    string num3 = temp[5].Replace("入札結果数", "");//入札結果数
                    string url = temp[6];//web

                    int id = 0;
                    String valueList = "'" 
                        + name //名称
                        + "','" 
                        + "" //管理者
                        + "','','" //postcode
                        + "" //address
                        + "','','','"//tel,fax
                        +bunrui+"','" //kind
                        + area //formart
                        + "','" 
                        + "" //scale
                        + "','2017/06/15','" //CYMD
                        + ""//other
                        + "','" 
                        + "" //mail
                        + "','"
                        + url//web
                        + "','" 
                        + ""//jCName 
                        + "','" 
                        + System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") 
                        + "','Y','1',"+num1+","+num2+","+num3;
                    db.SetGovement(0, 0, "Cname,name,postcode,[address],tel,fax,kind,[format],scale,CYMD,other,mail,web,jCNAME,createtime,subscripted,UserID,入札可能案件,案件登録数,入札結果数",
                                            "", valueList, out id);
                    line = reader.ReadLine();
                }
            }

        }


        /// <summary>
        /// 機構データ導出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnExport_Click(object sender, EventArgs e)
        {

        }
        /// <summary>
        /// メール送信
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnMailSend_Click(object sender, EventArgs e)
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > -1)
            {
                string name = dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString();
                (new FormProject(db,name)).ShowDialog();
            }

        }

    }
}

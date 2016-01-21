using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
//using SevenZSharp;
using System.Runtime.InteropServices;
using System.Net;
using System.Diagnostics;
using System.Threading;
using System.Security.Permissions;
using Microsoft.Win32;



namespace Eterm_CS
{
    public partial class FrmMain : Form
    {
        bool isopen;
        public static Guid IID_IAuthenticate = new Guid("79eac9d0-baf9-11ce-8c82-00aa004ba90b");
        public const int INET_E_DEFAULT_ACTION = unchecked((int)0x800C0011);
        public const int S_OK = unchecked((int)0x00000000);
        string path = string.Empty;
        string dir = string.Empty;//�ļ�·��
        string name = string.Empty;//�ļ���
        DataTable dt2 = new DataTable();//ȡ����ָ��
        DataTable dt1 = new DataTable();//ȡ�ļ������ļ�·��
        string ftp_type = string.Empty, id_code = string.Empty, iss_co = string.Empty;
        WriteFile writeFile = new WriteFile();
        string ls_status = string.Empty;
        string ls_flag = string.Empty;
        string str_content = string.Empty;
        int time_out = 300;
        int il_loop_count = 0;

        [DefaultValue(false)]
        public bool IsOpen
        {
            get { return isopen; }
            set { isopen = value; }
        }
        public FrmMain()
        {
            InitializeComponent();
        }
        string ls_id_code = string.Empty, ls_file_type = string.Empty;
        private void FrmMain_Load(object sender, EventArgs e)
        {
            string ls_message = string.Empty;
            string time_str = ConfigurationManager.AppSettings["TimeOut"];
            ls_id_code = ConfigurationManager.AppSettings["ID_CODE"];
            ls_file_type = ConfigurationManager.AppSettings["FILE_TYPE"];
            if (!string.IsNullOrEmpty(time_str))
            {
                time_out = Int32.Parse(time_str);
            }
            this.startcheck.Checked = isstartup();
        }

        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        /// <summary>
        /// ��ȡ�ƻ���
        /// </summary>
        private void InitPlan()
        {
            try
            {
                var sql = "select plan_time,act_code,days,last_time,next_time from p_background_plan where act_code='GDS' and job='" + bga.gl_job + "'";
                bga.backgroup_plan = bga.db.ExecuteDataSet(sql).Tables[0];
            }
            catch (Exception ex)
            {
                writeFile.Write("��ȡִ�мƻ������!" + ex.Message);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            InitPlan();
            DateTime lt_date, lt_plan_time, lt_next_time, lt_date1;
            DateTime date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59);

            int ll_days = 0;
            string ls_act_code;

            if (bga.backgroup_plan == null)
            {
                textBox_status.Text = "ϵͳδ����" + bga.gl_job + "���Զ����мƻ���";
                return;
            }

            lt_date = DateTime.Now;
            #region
            for (int i = 0; i < bga.backgroup_plan.Rows.Count; i++)
            {
                lt_plan_time = Convert.ToDateTime(bga.backgroup_plan.Rows[i]["plan_time"].ToString());
                if (bga.backgroup_plan.Rows[i]["next_time"].ToString() != string.Empty)
                {
                    lt_next_time = Convert.ToDateTime(bga.backgroup_plan.Rows[i]["next_time"].ToString());
                }
                else
                    lt_next_time = Convert.ToDateTime("2000-01-01");
                ls_act_code = bga.backgroup_plan.Rows[i]["act_code"].ToString();
                if (bga.backgroup_plan.Rows[i]["days"].ToString() != string.Empty)
                {
                    ll_days = Convert.ToInt32(bga.backgroup_plan.Rows[i]["days"].ToString());
                }
                lt_date = DateTime.Now;
                //�жϵ�ǰʱ���Ƿ�����´�ִ��ʱ�䣬����ִ��
                if (lt_date > lt_next_time)
                {
                    bga.backgroup_plan.Rows[i]["last_time"] = lt_date;
                    lt_date1 = Convert.ToDateTime(DateTime.Now.AddDays(ll_days).ToString("yyyy-MM-dd") + " " + lt_plan_time.ToString("HH:mm:ss"));
                    bga.backgroup_plan.Rows[i]["next_time"] = lt_date1;
                    var sql = "update p_background_plan set last_time=to_date('" + lt_date.ToString() + "','yyyy-mm-dd hh24:mi:ss') , next_time=to_date('" + lt_date1.ToString() + "','yyyy-mm-dd hh24:mi:ss') where act_code='" + ls_act_code + "' and plan_time=to_date('" + bga.backgroup_plan.Rows[i]["plan_time"].ToString() + "','yyyy-mm-dd hh24:mi:ss')" +
                        "  and  job='" + bga.gl_job + "' ";
                    try
                    {
                        bga.db.ExecuteNonQuery(sql);
                    }
                    catch (Exception ex)
                    {
                        writeFile.Write("���º�ִ̨�мƻ�ʱ�����" + ex.Message);
                        return;
                    }
                    if (ls_act_code == "GDS")
                    {
                        button1_Click(sender, e);
                    }
                }
            }
            #endregion
        }

        private void StreamWriter(String dir, string name, string str)
        {
            try
            {
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                FileStream fsFile = new FileStream(dir + "\\" + name, FileMode.OpenOrCreate);
                StreamWriter swWriter = new StreamWriter(fsFile, Encoding.Default);
                //д������
                swWriter.WriteLine(str);

                swWriter.Close();

            }
            catch (Exception e)
            {
                writeFile.Write("д�ļ�����" + e.Message);
            }
        }

        int num_2 = 0;
        /// <summary>
        /// �����ļ�����ִ�еĶ���ָ������
        /// </summary>
        int num_1 = 0;
        /// <summary>
        /// �����ļ��µĶ���ָ������
        /// </summary>
        int count_1 = 0;
        /// <summary>
        /// �Ѿ��������GDS�ļ�����
        /// </summary>
        int num_3 = 0;
        int num_4 = 0;
        /// <summary>
        /// �˴���Ҫ�����GDS�ļ�����
        /// </summary>
        int count_2 = 0;
        bool ib_flag = true;
        string cmd = string.Empty;
        int page_count = 0;
        DateTime time = DateTime.Now;
        int pn_num = 0;
        private void button1_Click(object sender, EventArgs e)
        {
            if (ib_flag == false)
                return;
            try
            {
                string content = string.Empty, sql1 = string.Empty;//д����ļ�����             
                sql1 = "select a.ftp_type,a.id_code,a.iss_co, a.d_filename,b.down_file_dir from p_ftp_deal_record a,p_ftp_down_config b where a.ftp_type=b.ftp_type and a.id_code=b.id_code and a.iss_co=b.iss_co AND  a.ftp_type LIKE 'GDS%' AND a.load_flag='B' AND a.d_fileName>=b.bef_str||to_char(sysdate-7,'yyyymmdd') ";
                if (!string.IsNullOrEmpty(ls_id_code))
                    sql1 = sql1 + "  and  instr('" + ls_id_code + "',a.id_code)>0 ";
                if (!string.IsNullOrEmpty(ls_file_type))
                    sql1 = sql1 + "  and  instr('" + ls_file_type + "',a.ftp_type)>0 ";
                sql1 = sql1 + " order by a.d_filename asc";
                dt1 = bga.db.ExecuteDataSet(sql1).Tables[0];//ȡ�ļ������ļ�·��
                num_3 = 0;
                num_4 = -1;
                count_2 = dt1.Rows.Count;
                if (count_2 > 0)
                {
                    ib_flag = false;
                    timer1.Enabled = false;//��ʱ�Ƚ�timer1�رա�����δ�����ļ�������֮����������             
                    timer4.Enabled = true;//���ʱ��timer4������
                }
                else
                {
                    textBox_status.Text = "��ʱû����Ҫִ�еļƻ���";
                    timer1.Enabled = true;
                    timer4.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                writeFile.Write(ex.Message);
            }
        }

        /// <summary>
        /// д�ļ�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer4_Tick(object sender, EventArgs e)
        {
            if (num_3 == count_2 && count_2 > 0)
            {
                timer4.Enabled = false;
                timer1.Enabled = true;
                ib_flag = true;
                textBox_status.Text = "����ʱ��,�رն�������";
                eterm_bga.ib_connect_status = false;
                eterm_bga.ib_disconnect = true;
            }
            if (num_3 < count_2 && num_3 != num_4)
            {
                #region  ���ݶ���ָ��ȡ�ļ�
                dir = dt1.Rows[num_3]["down_file_dir"].ToString();
                name = dt1.Rows[num_3]["d_filename"].ToString();
                ftp_type = dt1.Rows[num_3]["ftp_type"].ToString();
                id_code = dt1.Rows[num_3]["id_code"].ToString();
                iss_co = dt1.Rows[num_3]["iss_co"].ToString();
                string sql2 = "select CODE_STR from table(select a.ARR_STR from p_ftp_deal_record a,p_ftp_down_config b where a.ftp_type=b.ftp_type and a.id_code=b.id_code and a.iss_co=b.iss_co  AND a.load_flag='B' AND a.ftp_type='" + ftp_type + "' AND a.id_code='" + id_code + "' AND a.iss_co='" + iss_co + "' AND a.d_filename='" + name + "')";
                try
                {
                    dt2 = bga.db.ExecuteDataSet(sql2).Tables[0];//��ȡһ��p_deal_record�еĶ���ָ���
                }
                catch (Exception ex)
                {
                    writeFile.Write(ex.Message);
                }
                count_1 = dt2.Rows.Count;
                num_1 = 0;
                num_2 = -1;
                if (count_1 > 0)//�����ε�����Ҫ����ģ�������timer3������
                {
                    this.textBox1.Text += DateTime.Now.ToString() + "(" + name.ToUpper() + ") ��ʼȡ����ָ��  " + System.Environment.NewLine;
                    num_4 = num_3;
                    str_content = "";
                    timer3.Enabled = true;
                    timer4.Enabled = false;
                }
                else//���û�У�������p_deal_record�ƻ��ı�־����һ��
                {
                    #region �޸ı�־
                    string sql3 = "update p_ftp_deal_record set DOWN_TIME=sysdate,FILE_LENGTH=0, load_flag='U',file_status='FINISH' where ftp_type='" + ftp_type + "' AND id_code='" + id_code + "' AND iss_co='" + iss_co + "' AND d_filename='" + name + "'";
                    try
                    {
                        bga.db.ExecuteNonQuery(sql3);
                    }
                    catch (Exception ex)
                    {
                        writeFile.Write("�޸��ļ���ʾ����" + ex.Message);
                    }

                    #endregion
                    num_3 = num_3 + 1;
                }

                #endregion
            }

        }

        /// <summary>
        /// ����������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer3_Tick(object sender, EventArgs e)
        {
            #region  ����ȡ����
            if (num_1 == count_1 && count_1 > 0)
            {
                long size = 0;
                this.textBox1.Text += DateTime.Now.ToString() + "   ����ָ��ȡ��" + System.Environment.NewLine;
                this.textBox1.Text += DateTime.Now.ToString() + "   ��ʼд���ļ�" + System.Environment.NewLine;
                StreamWriter(dir, name, str_content);//д����                            
                this.textBox1.Text += DateTime.Now.ToString() + "   �ļ�д��" + System.Environment.NewLine;
                str_content = string.Empty;//�������
                FileInfo file_info = new FileInfo(dir + "\\" + name);
                size = file_info.Length;
                #region �޸ı�ʾ
                string sql3 = "update p_ftp_deal_record set DOWN_TIME=sysdate,FILE_LENGTH=" + size + ", load_flag='U',file_status='FINISH' where ftp_type='" + ftp_type + "' AND id_code='" + id_code + "' AND iss_co='" + iss_co + "' AND d_filename='" + name + "'";
                try
                {
                    bga.db.ExecuteNonQuery(sql3);
                }
                catch (Exception ex)
                {
                    writeFile.Write("�޸��ļ���ʾ����" + ex.Message);
                }
                #endregion
                if (!string.IsNullOrEmpty(textBox1.Text))
                {
                    if (textBox1.Text.Length > 30000)
                        textBox1.Text = string.Empty;//���ȳ���30000�����
                }
                timer3.Enabled = false;
                num_1 = 0;
                num_2 = 0;
                num_3 += 1;
                timer4.Enabled = true;
            }

            if (num_1 < count_1 && (num_2 != num_1 || getDiffSend(DateTime.Now, time) > 20))
            {

                #region ѭ��ȡ����

                string com = string.Empty;
                cmd = dt2.Rows[num_1]["CODE_STR"].ToString();//ȡ��ÿ��ָ��
                timer2.Enabled = false;
                num_2 = num_1;
                pn_num = 0;
                if (time_out > 0)
                    Thread.Sleep(time_out);
                time = DateTime.Now;
                il_loop_count = il_loop_count + 1;
                if (il_loop_count >= 10)
                {
                    textBox_status.Text = cmd;
                    if (il_loop_count >= 20)
                        il_loop_count = 0;
                }
                eterm_bga.ib_dataflag = false;
                timer3.Enabled = false;
                com = eterm_fun.Eterm_comman(cmd);
                if (com != "Ok")//���ָ��û����ִ�У�������֮������
                {
                    timer3.Enabled = true;
                    eterm_bga.il_retry_count += 1;
                    if (eterm_bga.il_retry_count >= 3)
                    {
                        this.textBox1.Text += DateTime.Now.ToString() + "   �������ξ�δ��ȡ���ӳɹ�����������Eterm���ã�" + System.Environment.NewLine;
                        this.textBox1.Text += DateTime.Now.ToString() + "   " + cmd + ":" + com + System.Environment.NewLine;
                        eterm_bga.il_retry_count = 0;//����Ϊ0
                        timer3.Enabled = false;
                        timer1.Enabled = true;
                    }
                }
                else  //ָ������ִ�У�����ת��timer2�н������
                {
                    eterm_bga.il_retry_count = 0;
                    timer2.Enabled = true;
                }
                #endregion
            }

            #endregion
        }


        /// <summary>
        /// ��ȡ����ָ����ַ���
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer2_Tick(object sender, EventArgs e)
        {
            try
            {
                string ls_show_txt = string.Empty, con_str = string.Empty;
                if (num_2 == num_1 && count_1 > 0 && getDiffSend(DateTime.Now, time) > 20 && timer3.Enabled == false)
                {
                    timer3.Enabled = true;
                }
                if (eterm_bga.ib_flag)
                {
                    eterm_bga.ib_flag = false;
                }
                if (((eterm_bga.is_eterm_status != "Start") && (eterm_bga.is_eterm_status != "disconnect")) || eterm_bga.is_eterm_result.Length > 0)
                {
                    if ((eterm_bga.is_eterm_status != "connection successed") && (eterm_bga.is_eterm_status != "Start"))
                    {
                        eterm_bga.il_error_count = eterm_bga.il_error_count + 1;
                        if (eterm_bga.il_error_count <= 2)
                        {
                            con_str = con_str + eterm_bga.is_eterm_status + "\r\n";

                        }
                        eterm_bga.is_eterm_result = "";
                        if (eterm_bga.ib_connect_status == true)//��ǰ���ڴ򿪣���ϵͳ����������ùرմ��ڣ�Ȼ�����¼���
                        {
                            eterm_bga.il_retry_count = eterm_bga.il_retry_count + 1;
                            if (eterm_bga.il_retry_count <= 2) //�����ιرմ������´򿪣��������û�취�������Ӿͷ�����
                            {
                                textBox_status.Text = "�رն�������";
                                eterm_bga.ib_connect_status = false;
                                eterm_bga.ib_disconnect = true;
                                num_1 -= 1;
                            }
                        }
                    }
                    if (eterm_bga.ib_dataflag)
                    {

                        if (eterm_bga.is_eterm_result.IndexOf("ET PASSENGER DATA NOT FOUND") > -1 && cmd.IndexOf(",P") < 0)
                        {
                            cmd = cmd + ",P";
                            eterm_bga.is_eterm_status = "Start";
                            eterm_bga.is_eterm_result = "";
                            eterm_bga.ib_dataflag = false;
                            eterm_fun.Eterm_comman(cmd);
                            return;
                        }

                        if (eterm_bga.is_eterm_result.IndexOf("SI") > -1 && eterm_bga.is_eterm_result.Trim().Length <= 4)
                        {
                            textBox_status.Text = "�رն�������";
                            eterm_bga.ib_connect_status = false;
                            eterm_bga.ib_disconnect = true;
                            num_1 -= 1;
                        }

                        str_content = str_content + eterm_bga.is_Command_str + eterm_bga.is_eterm_result.Trim();

                        if (il_loop_count >= 10)
                        {
                            textBox_status.Text = eterm_bga.is_eterm_result.Trim();
                            il_loop_count = 0;
                        }
                        if (eterm_bga.is_eterm_result.IndexOf("PAGE ") > -1 && cmd != "PN")
                        {
                            if (eterm_bga.is_eterm_result.LastIndexOf("/") > -1)
                            {
                                int length1 = eterm_bga.is_eterm_result.LastIndexOf("/") - eterm_bga.is_eterm_result.LastIndexOf("PAGE ") - 4;
                                //string sss = com.Substring(com.LastIndexOf("PAGE") + 4, length1);
                                int pages = Convert.ToInt32(eterm_bga.is_eterm_result.Substring(eterm_bga.is_eterm_result.LastIndexOf("PAGE ") + 4, length1));//��ǰҳ��
                                int b = eterm_bga.is_eterm_result.LastIndexOf("/") + 1;
                                string aaa = eterm_bga.is_eterm_result.Substring(b, 3);
                                int page1 = Convert.ToInt32(aaa);//��ҳ��
                                if (pages != page1)//�з�ҳ
                                {
                                    page_count = page1 - pages;
                                }
                            }
                        }
                        if (eterm_bga.is_eterm_result.IndexOf("PAGE ") > -1 && pn_num < page_count)
                        {
                            cmd = "PN";
                            eterm_bga.is_eterm_status = "Start";
                            eterm_bga.is_eterm_result = "";
                            eterm_bga.ib_dataflag = false;
                            eterm_fun.Eterm_comman(cmd);
                            pn_num += 1;
                            return;
                        }
                        else
                        {
                            page_count = 0;
                            if (pn_num <= 3 && ((eterm_bga.is_eterm_result.IndexOf(" +") >= 0) || (eterm_bga.is_eterm_result.IndexOf("+ ") >= 0)) && (eterm_bga.is_eterm_result.IndexOf(" + ") < 0))
                            {
                                eterm_bga.is_eterm_status = "Start";
                                eterm_bga.is_eterm_result = "";
                                eterm_bga.ib_dataflag = false;
                                eterm_fun.Eterm_comman("PN");
                                pn_num += 1;
                                return;
                            }
                        }
                        num_1 += 1;
                        eterm_bga.is_eterm_status = "Start";
                        eterm_bga.is_eterm_result = "";
                        eterm_bga.ib_dataflag = false;
                        timer2.Enabled = false;
                        timer3.Enabled = true;
                        return;
                    }

                }
            }
            catch (Exception)
            {


            }
        }

        /// <summary>
        /// ����ʱ��������
        /// </summary>
        /// <param name="t1">��ǰʱ��</param>
        /// <param name="t2">����ʱ��</param>
        /// <returns></returns>
        private int getDiffSend(DateTime t1, DateTime t2)
        {
            TimeSpan ts1 = new TimeSpan(t1.Ticks);
            TimeSpan ts2 = new TimeSpan(t2.Ticks);
            TimeSpan ts = ts1.Subtract(ts2).Duration();
            return ts.Seconds;
        }

        /// <summary>
        /// �Ƿ񿪻�����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void startcheck_CheckedChanged(object sender, EventArgs e)
        {
            if (this.startcheck.Checked)
            {
                startup(true);
            }
            else
            {
                startup(false);
            }
        }

        /// <summary>
        /// ��ӻ��Ƴ�ע�����
        /// </summary>
        /// <param name="add"></param>
        [RegistryPermissionAttribute(SecurityAction.LinkDemand, Write = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run")]
        private void startup(bool add)
        {
            try
            {
                string reg_name = ConfigurationManager.AppSettings["RegName"];
                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                if (add)
                {
                    if (key.GetValue(reg_name) == null)
                    {
                        key.SetValue(reg_name, "\"" + Application.ExecutablePath + "\"");
                    }
                }
                else
                {
                    key.DeleteValue(reg_name);
                }
                key.Close();
            }
            catch (Exception ex)
            {
                writeFile.Write("ע�����������ӻ�ɾ��ʧ�ܣ�" + ex.Message);
                return;
            }
        }

        private bool isstartup()
        {
            bool result = false;
            try
            {
                string reg_name = ConfigurationManager.AppSettings["RegName"];
                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                result = key.GetValue(reg_name) != null;
                key.Close();
            }
            catch (Exception ex)
            {
                writeFile.Write("��ȡע���ʧ�ܣ�" + ex.Message);
                //MessageBox.Show("��ȡע���ʧ�ܣ�" + ex.Message, "��ʾ");
            }
            return result;
        }
    }

}
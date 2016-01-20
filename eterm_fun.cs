﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using eterm_base;

namespace Eterm_CS
{
    class eterm_fun
    {
        public static eterm_bases frm_eterm;

        public static string Eterm_connect()  //ETERM连接
        {
            int ll_num = 0;
            if (frm_eterm == null)
            {
                frm_eterm = new eterm_bases();
                frm_eterm.Show();
                frm_eterm.Hide();
                for (int i = 0; i < 3000; i++)
                {
                    System.Windows.Forms.Application.DoEvents();
                    System.Windows.Forms.Application.DoEvents();
                    System.Windows.Forms.Application.DoEvents();
                    System.Windows.Forms.Application.DoEvents();
                    System.Windows.Forms.Application.DoEvents();
                    System.Windows.Forms.Application.DoEvents();
                    System.Windows.Forms.Application.DoEvents();
                    System.Windows.Forms.Application.DoEvents();
                    System.Windows.Forms.Application.DoEvents();
                    System.Windows.Forms.Application.DoEvents();
                    System.Windows.Forms.Application.DoEvents();
                    System.Windows.Forms.Application.DoEvents();
                    System.Windows.Forms.Application.DoEvents();
                    System.Windows.Forms.Application.DoEvents();
                    System.Threading.Thread.Sleep(10);
                    if ((eterm_bga.is_eterm_status == "connection successed") || eterm_bga.is_eterm_status.IndexOf("connection error") >= 0)
                    {
                        ll_num = 1;
                        eterm_bga.ib_connect_status = true;
                        break;
                    }
                }

                if ((ll_num == 1) && (frm_eterm.Eterm_first_comm.Length > 5) && (eterm_bga.is_eterm_status == "connection successed"))
                {
                    ll_num = 0;

                    frm_eterm.command_exe(frm_eterm.Eterm_first_comm);
                    for (int i = 0; i < 2000; i++)
                    {
                        System.Windows.Forms.Application.DoEvents();
                        System.Windows.Forms.Application.DoEvents();
                        System.Windows.Forms.Application.DoEvents();
                        System.Windows.Forms.Application.DoEvents();
                        System.Windows.Forms.Application.DoEvents();
                        System.Windows.Forms.Application.DoEvents();
                        System.Windows.Forms.Application.DoEvents();
                        System.Windows.Forms.Application.DoEvents();
                        System.Windows.Forms.Application.DoEvents();
                        System.Windows.Forms.Application.DoEvents();
                        System.Threading.Thread.Sleep(10);
                        if (eterm_bga.is_eterm_result.Length > 2)
                        {
                            ll_num = 1;
                            break;
                        }
                    }
                }

                if (ll_num == 0)
                {
                    eterm_bga.is_eterm_status = "connect timeout";
                }
                eterm_bga.is_eterm_result = "";
            }
            return eterm_bga.is_eterm_status;
        }
        public static string Eterm_comman(string Command_str)
        {

            string ls_temp;

            if (frm_eterm != null)
            {
                if (eterm_bga.ib_connect_status == false && (eterm_bga.is_eterm_status == "disconnect" || eterm_bga.is_eterm_status == "Host system down"))
                {
                    frm_eterm.Dispose();
                    frm_eterm = null;
                }
            }

            if (frm_eterm != null)
            {

                eterm_bga.is_eterm_result = "";
            }
            else
            {
                ls_temp = Eterm_connect();
                if (ls_temp != "connection successed")
                {
                    return eterm_bga.is_eterm_result;
                }
            }

            eterm_bga.is_Command_str = "<" + Command_str.ToUpper() + ">" + "\r\n";
            eterm_bga.is_eterm_result = "";
            eterm_bga.is_eterm_status = "Start";
            frm_eterm.command_exe(Command_str);
            return "";


        }
    }
}

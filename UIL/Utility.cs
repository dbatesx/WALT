using System;
using System.Data;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Reflection;
using System.Diagnostics;

namespace WALT.UIL
{
    public class Utility
    {
        // Do not change, 128 bit encyption    
        private static readonly byte[] Key = { 18, 19, 8, 24, 36, 22, 4, 22, 17, 5, 11, 9, 13, 15, 06, 23 };
        private static readonly byte[] IV = { 14, 2, 16, 7, 5, 9, 17, 8, 4, 47, 16, 12, 1, 32, 25, 18 };

        public static void DisplayInfoMessage(string message)
        {
            ((Site1)HttpContext.Current.Session["master_page"]).DisplayInfoMessage(message);
        }

        public static void DisplayWarningMessage(string message)
        {
            ((Site1)HttpContext.Current.Session["master_page"]).DisplayWarningMessage(message);
        }

        public static void DisplayErrorMessage(string message)
        {           
            ((Site1)HttpContext.Current.Session["master_page"]).DisplayErrorMessage(message);
        }

        public static void DisplayException(Exception e)
        {
            DisplayErrorMessage(e.Message);
        }


        public static string GetPostBackControlId(Page page)
        {
            if (!page.IsPostBack)
                return string.Empty;
            Control control = null;

            string controlName = page.Request.Params["__EVENTTARGET"];

            if (!String.IsNullOrEmpty(controlName))
            {
                control = page.FindControl(controlName);
            }
            else
            {

                string controlId; Control foundControl;

                foreach (string ctl in page.Request.Form)
                {

                    if (ctl.EndsWith(".x") || ctl.EndsWith(".y"))
                    {
                        controlId = ctl.Substring(0, ctl.Length - 2); foundControl = page.FindControl(controlId);
                    }
                    else { foundControl = page.FindControl(ctl); }
                    if (!(foundControl is Button || foundControl is ImageButton)) continue; control = foundControl;
                    break;
                }

            }

            return control == null ? String.Empty : control.ID;
        }


        //public static Control GetPostBackControl(Page page)
        //{
        //    Control postbackControlInstance = null;

        //    string postbackControlName = page.Request.Params.Get("__EVENTTARGET");

        //    if (postbackControlName != null && postbackControlName != string.Empty)
        //    {
        //        postbackControlInstance = page.FindControl(postbackControlName);
        //    }

        //    else
        //    {
        //        // find button control postbacks
        //        for (int i = 0; i < page.Request.Form.Keys.Count; i++)
        //        {
        //            postbackControlInstance = page.FindControl(page.Request.Form.Keys[i]);

        //            if (postbackControlInstance is System.Web.UI.WebControls.Button)
        //            {
        //                return postbackControlInstance;
        //            }
        //        }
        //    }

        //    // find the ImageButton postbacks
        //    if (postbackControlInstance == null)
        //    {
        //        for (int i = 0; i < page.Request.Form.Count; i++)
        //        {

        //            if ((page.Request.Form.Keys[i].EndsWith(".x")) || (page.Request.Form.Keys[i].EndsWith(".y")))
        //            {
        //                postbackControlInstance = page.FindControl(page.Request.Form.Keys[i].Substring(0, page.Request.Form.Keys[i].Length - 2));
        //                return postbackControlInstance;
        //            }

        //        }
        //    }
        //    return postbackControlInstance;
        //}

        public static string Encrypt(string unencryptedString)
        {
            byte[] bytIn = ASCIIEncoding.ASCII.GetBytes(unencryptedString);
                        
            MemoryStream ms = new MemoryStream();

            RijndaelManaged _cryptoProvider = new RijndaelManaged();
            _cryptoProvider.Mode = CipherMode.CBC;
            _cryptoProvider.Padding = PaddingMode.PKCS7;

            // encrypt
            CryptoStream cs = new CryptoStream(ms, _cryptoProvider.CreateEncryptor(Key, IV), CryptoStreamMode.Write);
                       
            cs.Write(bytIn, 0, bytIn.Length);
            cs.FlushFinalBlock();

            byte[] bytOut = ms.ToArray();
            return Convert.ToBase64String(bytOut);
        }

        public static string Decrypt(string encryptedString)
        {
            if (encryptedString.Trim().Length != 0)
            {
                // convert to binary
                byte[] bytIn = Convert.FromBase64String(encryptedString);

                // create a stream
                MemoryStream ms = new MemoryStream(bytIn, 0, bytIn.Length);

                RijndaelManaged _cryptoProvider = new RijndaelManaged();
                _cryptoProvider.Mode = CipherMode.CBC;
                _cryptoProvider.Padding = PaddingMode.PKCS7;

                // decrypt the data
                CryptoStream cs = new CryptoStream(ms, _cryptoProvider.CreateDecryptor(Key, IV), CryptoStreamMode.Read);
                              
                StreamReader sr = new StreamReader(cs);

                return sr.ReadToEnd();
            }
            else
            {
                return "";
            }
        }

        public static string[] UppercaseFirst(string[] array)
        {
            if (array==null || array.Count() == 0)
            {
                return array;
            }

           for(int i=0; i< array.Count(); i++)
            {
                array[i] = char.ToUpper(array[i][0]) + array[i].Substring(1).ToLower();
            }
           return array;
        }

        public static string UppercaseFirst(string str)
        {
            if (string.IsNullOrEmpty(str) || str.Equals(DTO.Task.StatusEnum.OBE.ToString()))
            {
                return str;
            }

            str = char.ToUpper(str[0]) + str.Substring(1).ToLower();
            return str;
        }

        public static Control FindControlIterative(Control control, string id)
        {
            Control ctl = control;
            LinkedList<Control> controls = new LinkedList<Control>();
            while (ctl != null)
            {
                if (ctl.ID == id)
                {
                    return ctl;
                }

                foreach (Control child in ctl.Controls)
                {
                    if (child.ID == id) { return child; }
                    if (child.HasControls()) { controls.AddLast(child); }
                }
                ctl = controls.First.Value; controls.Remove(ctl);

            } return null;
        }

        public static Control FindControlRecursive(Control ctrl, string controlID)
        {
            if (string.Compare(ctrl.ID, controlID, true) == 0)
            {  
                return ctrl;
            }
            else
            {   
                foreach (Control child in ctrl.Controls)
                {
                    Control lookFor = FindControlRecursive(child, controlID);
                    if (lookFor != null) return lookFor; 
                }  
                return null;
            }
        }

        public static string GetVersion()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fvi.FileVersion;
        }       

        public static DateTime ConvertToLocal(DateTime dt)
        {
            if (HttpContext.Current.Session["timeDiff"] != null)
            {
                return dt.AddHours((int)HttpContext.Current.Session["timeDiff"]);
            }

            return dt;
        }

        public static string RoundDoubleToString(double value)
        {
            return RoundDoubleToString(value, 1);
        }

        public static string RoundDoubleToString(double value, int places)
        {
            return decimal.Round(Convert.ToDecimal(value), places).ToString();
        }
    }
}

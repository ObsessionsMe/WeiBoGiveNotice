using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WeiBoGiveNotice
{
    public partial class LoginFrm : Form
    {

        public string UniqueCode = MachineCode.GetShortMachineCodeString();

        public LoginFrm()
        {
            InitializeComponent();
        }

        private void LoginFrm_Load(object sender, EventArgs e)
        {
            tbxUniqueCode.Text = UniqueCode;
            //string PrivateKey, PublicKey;
            //RSACrypto.RSA.CreateKeys(out PrivateKey,out PublicKey);
        }

        public static bool VerifyCheckCode(string uniqueCode, string checkCode)
        {
            bool flag = false;
            var PrivateKey = "<RSAKeyValue><Modulus>tD97/jZbc93tSLiyphGfBZnLuuy5dm7q5FuaSXSqjdSWH/aPDUOPnQeDh3X6YkNaoL19IHW+QugCuj2pGYDmMEsAGGG8TbsPKdlCxA8aTEFF/sFXRdQ/Kb3BdjmbI9LZ1LS4/WYX3t0nlsrYxDXyadIJojeURhNZjRSuayLZonk=</Modulus><Exponent>AQAB</Exponent><P>7FkB1oH3bSMVQwJGgtKvKwUhAkiCKxdEgYgX37Ya7ohIkrDDHFN57fcxoszNoo68rcIe4mGt2PSZ9X8u5ZhXbw==</P><Q>wzxQ8imPg231NtBiZCDP0h3yPkW7KCaxQ5oLKLEednTsMOywuXhBLWkuafeJSHauiUjyX8vjwkVhOA+SyVrwlw==</Q><DP>IkVZxd/8n7+pkpje3pNsQQGyYqFe9p6eGWZBh/fg+dubImItOItGL+JbOS8XVk36P/vY/JyLV91IAdgaVcJ8Uw==</DP><DQ>LPQMZ1Xud4Kv/YkJvqhXcbf3eSMxLtOJ6VjkzN/LddcCM1msb2gjCcO4LioS8B5znaSwOiKGNMso0XG0iKxpzQ==</DQ><InverseQ>pjqJ5gjhptiwz3UbRprgDEVdzqRGJ6n3pbQf1EohxnkFYDrX7MtNBQRbG7WZ1kNkhLnaKAit8HR80bCGPZ8vDg==</InverseQ><D>paH8z+KlcTjLNls7Fy30IlJsi03DM5jDUqr2gW+q1oA2oBKS685cpyDjddQtDKT2H+VgmTZ5asQG1ZuZy2zOmJ+Kox9/8hzbfvrVaq8f0No++Ig0tBLPxzE9P1ZkbZUc+wC1nFi0Dtufs6dB4BjM5T4Klaxb/5Su9gUb7Dwrvo0=</D></RSAKeyValue>";
            try
            {
                if (RSACrypto.RSA.Decrypt(PrivateKey, checkCode) == uniqueCode)
                {
                    flag = true;
                }
            }
            catch (Exception ex)
            {

            }

            return flag;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (!VerifyCheckCode(tbxUniqueCode.Text.Trim(), tbxCheckCode.Text.Trim()))
            {
                MessageBox.Show("校验失败!");
            }
            else
            {
                this.DialogResult = DialogResult.OK;
                CfgMgr.SaveValue("CheckCode", tbxCheckCode.Text.Trim());
            }
        }

        private void btnNo_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.No;
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            Clipboard.SetDataObject(tbxUniqueCode.Text);
            MessageBox.Show("已复制到剪贴板！");
        }
    }
}

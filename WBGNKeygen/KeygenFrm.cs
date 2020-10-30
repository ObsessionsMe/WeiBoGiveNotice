using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WBGNKeygen
{
    public partial class KeygenFrm : Form
    {
        public KeygenFrm()
        {
            InitializeComponent();
        }

        private void btnGetCheckCode_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(tbxUniqueCode.Text.Trim()))
            {
                MessageBox.Show("请输入机械码!");
            }
            else
            {
                var PublicKey = "<RSAKeyValue><Modulus>tD97/jZbc93tSLiyphGfBZnLuuy5dm7q5FuaSXSqjdSWH/aPDUOPnQeDh3X6YkNaoL19IHW+QugCuj2pGYDmMEsAGGG8TbsPKdlCxA8aTEFF/sFXRdQ/Kb3BdjmbI9LZ1LS4/WYX3t0nlsrYxDXyadIJojeURhNZjRSuayLZonk=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
                rtbxCheckCode.Text = RSACrypto.RSA.Encrypt(PublicKey, tbxUniqueCode.Text.Trim());
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


public class RSACrypto
{
    #region 构造器
    /// <summary>
    /// 无参构造器
    /// </summary>
    public RSACrypto()
    {
        this.encoding = Encoding.Default;
    }
    #endregion

    #region 属性
    /// <summary>
    /// 编码
    /// </summary>
    public Encoding encoding { get; set; }
    /// <summary>
    /// 公钥
    /// </summary>
    public string PublicKey { get; set; }
    /// <summary>
    /// 私钥
    /// </summary>
    public string PrivateKey { get; set; }
    /// <summary>
    /// 静态方法
    /// </summary>
    public static RSACrypto RSA { get { return new RSACrypto(); } }
    #endregion

    #region 方法

    #region 产生 密钥
    /// <summary>
    /// RSA产生密钥
    /// </summary>
    /// <param name="PrivateKey">私钥</param>
    /// <param name="PublicKey">公钥</param>
    public void CreateKeys(out string PrivateKey, out string PublicKey)
    {
        try
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            PrivateKey = rsa.ToXmlString(true);
            PublicKey = rsa.ToXmlString(false);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    #endregion

    #region 加密函数
    /// <summary>
    /// 加密函数
    /// </summary>
    /// <param name="publicKey">公钥</param>
    /// <param name="data">待加密的字符串</param>
    /// <returns></returns>
    public string Encrypt(string publicKey, string data)
    {
        try
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(publicKey);
            var _data = rsa.Encrypt(data.GetBytes(this.encoding), false);
            return Convert.ToBase64String(_data); ;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    /// <summary>
    /// 加密函数
    /// </summary>
    /// <param name="data">明文</param>
    /// <returns></returns>
    public string Encrypt(string data)
    {
        var encryptBytes = new RSACryptoServiceProvider(new CspParameters()).Encrypt(data.GetBytes(this.encoding), false);
        return Convert.ToBase64String(encryptBytes);
    }
    #endregion

    #region 解密函数        
    /// <summary>
    /// 解密函数
    /// </summary>
    /// <param name="privateKey">私钥</param>
    /// <param name="data">待解密的字符串</param>
    /// <returns></returns>
    public string Decrypt(string privateKey, string data)
    {
        try
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(privateKey);
            var _data = rsa.Decrypt(Convert.FromBase64String(data.Replace(" ", "+")), false);
            return _data.GetString(this.encoding);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    /// <summary>
    /// 解密函数
    /// </summary>
    /// <param name="data">密文</param>
    /// <returns></returns>
    public string Decrypt(string data)
    {
        try
        {
            var DecryptBytes = new RSACryptoServiceProvider(new CspParameters()).Decrypt(Convert.FromBase64String(data), false);
            return DecryptBytes.GetString(this.encoding);
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }
    #endregion

    #endregion
}

public static class StringExtension
{
    #region 字符串字节互转
    /// <summary>
    /// 字符串转字节
    /// </summary>
    /// <param name="_">字符串</param>
    /// <param name="encoding">编码</param>
    /// <returns></returns>
    public static byte[] GetBytes(this String _, Encoding encoding = null)
    {
        return (encoding ?? Encoding.UTF8).GetBytes(_);
    }
    /// <summary>
    /// 字符串转字节
    /// </summary>
    /// <param name="_">字符串</param>
    /// <param name="encoding">编码</param>
    /// <returns></returns>
    public static byte[] GetBytes(this String _, string encoding)
    {
        return Encoding.GetEncoding(encoding ?? "UTF-8").GetBytes(_);
    }
    /// <summary>
    /// 字节转字符串
    /// </summary>
    /// <param name="_">字节</param>
    /// <param name="encoding">编码</param>
    /// <param name="index">开始位置</param>
    /// <param name="count">长度</param>
    /// <returns></returns>
    public static string GetString(this byte[] _, Encoding encoding = null, int index = 0, int count = 0)
    {
        return (encoding ?? Encoding.UTF8).GetString(_, index, count == 0 ? _.Length : count);
    }
    /// <summary>
    /// 字节转字符串
    /// </summary>
    /// <param name="_">字节</param>
    /// <param name="encoding">编码</param>
    /// <param name="index">开始位置</param>
    /// <param name="count">长度</param>
    /// <returns></returns>
    public static string GetString(this byte[] _, string encoding = null, int index = 0, int count = 0)
    {
        return Encoding.GetEncoding(encoding ?? "UTF-8").GetString(_, index, count == 0 ? _.Length : count);
    }
    #endregion


}


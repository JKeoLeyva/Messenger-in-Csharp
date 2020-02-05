using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using Newtonsoft.Json;

/*
 * This program is used to implement the RSA encryption and decryption methods on given encrypted messages
 */
namespace Messenger
{
    public class RSAEncryptionandDecryption
    {
        public Message encodeMessage(String email, String plaintext){
            if (!File.Exists(email + ".key"))
            {
                keyErrorMessage(email);
            }
            String userKey = File.ReadAllText(email + ".key");

            byte[] b64StringKey = Convert.FromBase64String(userKey);

            byte[] sizeofE = b64StringKey[..4];
            
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(sizeofE);
            }
            
            int lengthOfE = BitConverter.ToInt32(sizeofE);

            byte[] E_value_Bytes = b64StringKey[4..(lengthOfE + 4)];

            byte[] sizeofN = b64StringKey[(lengthOfE + 4)..(lengthOfE + 8)];
            
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(sizeofN);
            }

            int lenghtofN = BitConverter.ToInt32(sizeofN);

            byte[] N_value_Bytes = b64StringKey[(lengthOfE + 8)..(lengthOfE + 8 + lenghtofN)];

            BigInteger e_value = new BigInteger(E_value_Bytes);
            BigInteger n_value = new BigInteger(N_value_Bytes);

            byte[] plaintext_bytes = Encoding.Default.GetBytes(plaintext);
            BigInteger plaintext_bigInt = new BigInteger(plaintext_bytes);
            BigInteger ciphertext_bigInt = BigInteger.ModPow(plaintext_bigInt, e_value, n_value);
            byte[] ciphertext_bytes = ciphertext_bigInt.ToByteArray();
            String base64encryptedstring = Convert.ToBase64String(ciphertext_bytes);

            Message encryptedMessage = new Message();
            encryptedMessage.email = email;
            encryptedMessage.content = base64encryptedstring;
            
            return encryptedMessage;
        }
        
        public void decodeMessage(String ciphertext, String email)
        {
            PrivateKey job = JsonConvert.DeserializeObject<PrivateKey>(File.ReadAllText("private.key"));
            if (!job.emails.Contains(email))
            {
                Console.WriteLine("Message cannot be decoded");
                Environment.Exit(0);
            }

            byte[] b64StringKey = Convert.FromBase64String(job.key);

            byte[] sizeofD = b64StringKey[..4];

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(sizeofD);
            }

            int lengthOfD = BitConverter.ToInt32(sizeofD);

            byte[] D_value_bytes = b64StringKey[4..(lengthOfD + 4)];
            
            byte[] sizeofN = b64StringKey[(lengthOfD + 4)..(lengthOfD + 8)];
            
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(sizeofN);
            }

            int lenghtofN = BitConverter.ToInt32(sizeofN);

            byte[] N_value_Bytes = b64StringKey[(lengthOfD + 8)..(lengthOfD + 8 + lenghtofN)];

            BigInteger d_value = new BigInteger(D_value_bytes);
            BigInteger n_value = new BigInteger(N_value_Bytes);

            byte[] ciphertext_Bytes = Convert.FromBase64String(ciphertext);
            BigInteger ciphertext_BigInt = new BigInteger(ciphertext_Bytes);

            BigInteger decoded_BigInt = BigInteger.ModPow(ciphertext_BigInt, d_value, n_value);
            byte[] decoded_Bytes = decoded_BigInt.ToByteArray();

            Console.WriteLine(Encoding.Default.GetString(decoded_Bytes));
        }
        
        public void keyErrorMessage(String email)
        {
            Console.WriteLine("Do not have key for" + email + "on file, please download key");
            Environment.Exit(0);
        }
    }
}
using System;
using System.IO;
using System.Numerics;
using Newtonsoft.Json;


/*
 * This program implements the RSA algorithm to create two keys, a public and a private key and stores them on the\
 * local system
 */
namespace Messenger
{
    public class RSAGenerator
    {
        private BigInteger e_val = new BigInteger(65537);
        private BigInteger p_val = BigInteger.Zero;
        private BigInteger q_val = BigInteger.Zero;
        private BigInteger n_val = BigInteger.Zero;

        public RSAGenerator(BigInteger p_val, BigInteger q_val)
        {
            this.p_val = p_val;
            this.q_val = q_val;
        }
        
        public static BigInteger modInverse(BigInteger a, BigInteger n)
        {
            BigInteger i = n, v = 0, d = 1;
            while (a>0) {
                BigInteger t = i/a, x = a;
                a = i % x;
                i = x;
                x = d;
                d = v - t*x;
                v = x;
            }
            v %= n;
            if (v<0) v = (v+n)%n;
            return v;
        }

        public void createKeyPair()
        {
            BigInteger one = BigInteger.One;
            n_val = BigInteger.Multiply(p_val, q_val);
            BigInteger r = BigInteger.Multiply(BigInteger.Subtract(p_val, one), BigInteger.Subtract(q_val, one));
            BigInteger privateKey = modInverse(e_val, r);

            byte[] privKey = privateKey.ToByteArray();
            byte[] pubKey = e_val.ToByteArray();
            byte[] n_value = n_val.ToByteArray();

            byte[] sizeOfEVal = BitConverter.GetBytes(pubKey.Length);
            byte[] sizeOfNVal = BitConverter.GetBytes(n_value.Length);
            byte[] sizeOfDVal = BitConverter.GetBytes(privKey.Length);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(sizeOfEVal);
                Array.Reverse(sizeOfNVal);
                Array.Reverse(sizeOfDVal);
            }
            
            byte[] fullPrivateKey = new byte[sizeOfDVal.Length + privKey.Length + sizeOfNVal.Length + n_value.Length];
            byte[] fullPublicKey = new byte[sizeOfEVal.Length + pubKey.Length + sizeOfNVal.Length + n_value.Length];
            
            sizeOfDVal.CopyTo(fullPrivateKey, 0);
            privKey.CopyTo(fullPrivateKey, sizeOfDVal.Length);
            sizeOfNVal.CopyTo(fullPrivateKey, sizeOfDVal.Length + privKey.Length);
            n_value.CopyTo(fullPrivateKey, sizeOfDVal.Length + privKey.Length + sizeOfNVal.Length);
            
            sizeOfEVal.CopyTo(fullPublicKey, 0);
            pubKey.CopyTo(fullPublicKey, sizeOfEVal.Length);
            sizeOfNVal.CopyTo(fullPublicKey, sizeOfEVal.Length + pubKey.Length);
            n_value.CopyTo(fullPublicKey, sizeOfEVal.Length + pubKey.Length + sizeOfNVal.Length);

            String b64FullPrivateKey = Convert.ToBase64String(fullPrivateKey);
            String b64FullPublicKey = Convert.ToBase64String(fullPublicKey);

            PrivateKey myPrivateKey = new PrivateKey();
            myPrivateKey.emails = null;
            myPrivateKey.key = b64FullPrivateKey;
            
            PublicKey myPublicKey = new PublicKey();
            myPublicKey.email = null;
            myPublicKey.key = b64FullPublicKey;

            var jsonPrivateKey = JsonConvert.SerializeObject(myPrivateKey);
            var jsonPublicKey = JsonConvert.SerializeObject(myPublicKey);

            File.WriteAllText("private.key", jsonPrivateKey);
            File.WriteAllText("public.key", jsonPublicKey);

            Console.WriteLine("Key pair generated!");
        }
    }
}
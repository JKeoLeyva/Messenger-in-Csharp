﻿using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/*
 * Author: Jorge Leyva
 *
 * Secure Messaging Project
 * Professor Jeremy Brown
 * COPADS
 *
 * This project utilizes the RSA encryption algorithm to generate a public and private key pair to store on the local
 * system, send and receive public keys to a centralized server, and to send and receive encrypted messages encoded in
 * base64.
 * 
 */
namespace Messenger
{
    /*
     * a public class utilized to more easily store the public key generated by RSA.
     */
    public class PublicKey
    {
        public String email { get; set; }
        public String key { get; set; }
    }
    
    /*
     * a public class utilized to more easily store the private key generated by RSA.  Utilizes a string array to hold
     * all potential emails associated with this private key.
     */
    public class PrivateKey
    {
        public String[] emails { get; set; }
        public String key { get; set; }
    }

    /*
     * a public class utilized to more easily send messages to the server, as these objects will be serialized to
     * a JSON string to be sent to the server
     */
    public class Message
    {
        public string email { get; set; }
        public string content { get; set; }
    }
    
    class Program
    {
        public static int Keysize;
        public static int Count = 2;
        public static String Email = null;
        public static String Plaintext = null;

        static HttpClient client = new HttpClient();
        static RSAEncryptionandDecryption rsaed = new RSAEncryptionandDecryption();
        
        /*
         * asynchronous method to send the users public key to the server.  Checks to ensure that a public/private key
         * pair exists on the machine before attempting to send the key.  Reads in the public key from local system,
         * serializes it as a JSON string and attempts to send it to the server, checking for a success status code
         */
        static async Task PutKeyAsync(String email)
        {
            if (!File.Exists("private.key") && !File.Exists("public.key"))
            {
                Console.WriteLine("No public or private key on local system.  Please generate public/private key pair");
                errorMessage();
            }
            Console.WriteLine("Sending Key...");
            
            var PrivateKey = File.ReadAllText("private.key");
            PrivateKey PrivateKeyObj = JsonConvert.DeserializeObject<PrivateKey>(PrivateKey);
            PrivateKeyObj.emails = new string[10];
            PrivateKeyObj.emails[0] = email;
            var privJson = JsonConvert.SerializeObject(PrivateKeyObj);
            File.WriteAllText("private.key", privJson);

            var json = File.ReadAllText("public.Key");
            PublicKey job = JsonConvert.DeserializeObject<PublicKey>(json);
            
            job.email = email;
            var json2 = JsonConvert.SerializeObject(job);

            var content = new StringContent(json2, Encoding.UTF8, "application/json");
            var url = "http://kayrun.cs.rit.edu:5000/Key/" + email;

            try
            {
                var response = await client.PutAsync(url, content);
                Console.WriteLine(await response.Content.ReadAsStringAsync());
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Key not sent!");
                }
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Key sent!");
                }
            }
            catch(Exception exception)
            {
                System.Diagnostics.Debug.WriteLine("CAUGHT EXCEPTION:");
                System.Diagnostics.Debug.WriteLine(exception);
            }
        }

        /*
         * asynchronous method to attempt to retrieve a public key from a user on the server
         */
        static async Task GetKeyAsync(String email)
        {
            Console.WriteLine("Getting key...");
            var url = "http://kayrun.cs.rit.edu:5000/Key/" + email;

            try
            {
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var jo = JObject.Parse(content);
                    var key = jo["key"].ToString();
                    File.WriteAllText(email + ".key", key);
                    Console.WriteLine("Key received!");
                }
                else
                {
                    Console.WriteLine("Unable to retrieve public key for " + email);
                    Environment.Exit(0);
                }
            }
            catch(Exception exception)
            {
                System.Diagnostics.Debug.WriteLine("CAUGHT EXCEPTION:");
                System.Diagnostics.Debug.WriteLine(exception);
            }
        }

        /*
         * Asynchronous method that attempts to retrieve a message for a given user from the server
         */
        static async Task<String> GetMessageAsync(String email)
        {
            var url = "http://kayrun.cs.rit.edu:5000/Message/" + email;
            String messageContent = null;

            try
            {
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var jo = JObject.Parse(content);
                    messageContent = jo["content"].ToString();
                }
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine("CAUGHT EXCEPTION:");
                System.Diagnostics.Debug.WriteLine(exception);
            }

            return messageContent;
        }

        /*
         * Asynchronous method that attempts to send an encrypted message to a specific user on the server.  If the
         * public key for the desired recipient of the message is not on the system, it will let the user know the
         * public key was not found for that user and terminate
         */
        static async Task PutMessageAsync(String email, Message message)
        {
            if (!File.Exists(email + ".key"))
            {
                Console.WriteLine("Public Key does not exist for " + email);
                Environment.Exit(0);
            }
            Console.WriteLine("Sending message to " + email);
            var json = JsonConvert.SerializeObject(message);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var url = "http://kayrun.cs.rit.edu:5000/Message/" + email;

            try
            {
                var response = await client.PutAsync(url, content);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Message not sent!");
                }
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Message sent!");
                }
            }
            catch(Exception exception)
            {
                System.Diagnostics.Debug.WriteLine("CAUGHT EXCEPTION:");
                System.Diagnostics.Debug.WriteLine(exception);
            }
        }
        
        public static void errorMessage()
        {
            Console.WriteLine("Usage: dotnet run <option> <other arguments>");
            Console.WriteLine("Options: " + "keyGen <keysize>, " + "sendKey <email>, " + "getKey <email>, " +
                              "sendMsg <email> <plaintext>, " + "getMsg <email>");
            Environment.Exit(0);
        }
        
        static async Task Main(string[] args)
        {
            if (args.Length < 1 || args.Length > 3)
            {
                errorMessage();
            }

            if (args[0].Equals("keyGen") & args.Length == 2)
            {
                Keysize = int.Parse(args[1]);

                if (Keysize % 8 != 0)
                {
                    errorMessage();
                }
                
                Random r = new Random();
                int divide = r.Next(Keysize / 4, Keysize);
                
                var png = new PrimeNumberGenerator(Keysize, Count, divide);
                
                Console.WriteLine("Generating key pair...");
                
                BigInteger[] Primes = png.run();

                var rsag = new RSAGenerator(Primes[0], Primes[1]);
                rsag.createKeyPair();
                
                Environment.Exit(0);
            }
            
            else if (args[0].Equals("sendKey") && args.Length == 2)
            {
                await PutKeyAsync(args[1]);
                Environment.Exit(0);
            }
            
            else if (args[0].Equals("getKey") && args.Length == 2)
            {
                await GetKeyAsync(args[1]);
                Environment.Exit(0);
            }
            
            else if (args[0].Equals("sendMsg") && args.Length == 3)
            {
                Message encodedMessage = rsaed.encodeMessage(args[1], args[2]);
                await PutMessageAsync(args[1], encodedMessage);
                Environment.Exit(0);
            }
            
            else if (args[0].Equals("getMsg") && args.Length == 2)
            {
                String privateKeysJson = File.ReadAllText("private.key");
                PrivateKey PrivateKeys = JsonConvert.DeserializeObject<PrivateKey>(privateKeysJson);
                if (!PrivateKeys.emails.Contains(args[1]))
                {
                    Console.WriteLine("Message can not be decoded");
                    Environment.Exit(0);
                }
                var ciphertext = await GetMessageAsync(args[1]);
                rsaed.decodeMessage(ciphertext, args[1]);
                Environment.Exit(0);
            }
            else
            {
                errorMessage();
            }
        }
    }
}
using System;
using System.Numerics;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Messenger
{
    using Extensions;
    
    public class PrimeNumberGenerator
    {
        private static object myLock = new Object();
        private RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
        private int bits;
        private int count;
        private int divide;
        private int counter;

        /*
         * Constructor of PrimeNumGen object
         */
        public PrimeNumberGenerator(int bits, int count, int divide)
        {
            this.bits = bits;
            this.count = count;
            this.divide = divide;
        }

        /*
         * the makeAPrimeNumber method uses the bit count given to it by the PrimeNumGen object to create a potentially
         * prime number 
         */
        public BigInteger makeAPotentialPrimeNumnber(int numOfBits)
        {
            byte[] randNum = new byte[numOfBits/8];
            rng.GetBytes(randNum);
            BigInteger primeNum = new BigInteger(randNum);
            return primeNum;
        }
        
        /*
         * this run method utilizes the C# parallel library to create a number of prime numbers in parallel.
         */
        public BigInteger[] run()
        {
            BigInteger[] result = new BigInteger[2];
            
            Parallel.For((long) 0, Int32.MaxValue, (i, state) =>
            {
                if (counter == count)
                {
                    state.Break();
                }

                BigInteger potentialPrimeNum = BigInteger.Zero;

                if (counter == 0)
                {
                    int first_bit_num = bits - divide;
                    potentialPrimeNum = makeAPotentialPrimeNumnber(first_bit_num);
                    //Console.WriteLine("This is the bit size for first prime p {0}", first_bit_num);
                }
                else
                {
                    potentialPrimeNum = makeAPotentialPrimeNumnber(divide);
                    //Console.WriteLine("This is the bit number for the second prime q {0}", divide);
                }

                //potentialPrimeNum = makeAPotentialPrimeNumnber(bits);
                
                lock (myLock)
                {
                    if (potentialPrimeNum.IsProbablyPrime())
                    {
                            BigInteger zero = BigInteger.Zero;
                            BigInteger two = new BigInteger(2);
                            int comparison = potentialPrimeNum.CompareTo(zero);
                            if (counter != count & comparison >= 0 & !BigInteger.Remainder(potentialPrimeNum, two).IsZero)
                            {
                                result[counter] = potentialPrimeNum;
                                counter++;
                                //Console.WriteLine("{0}: {1}", counter, potentialPrimeNum);
                                //Console.WriteLine();
                            }
                    } 
                }
            });
            return result;
        }
    }
}
using System;
using NBitcoin;

namespace LkeServices.BitcoinHelpers
{
    public class BitcoinAddressHelper
    {
        public static bool IsBitcoinColoredAddress(string base58, Network network)
        {
            try
            {
                var notUsed = new BitcoinColoredAddress(base58, network);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool IsBitcoinPubKeyAddress(string base58, Network network)
        {
            try
            {
                var notUsed = new BitcoinPubKeyAddress(base58, network);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool IsBitcoinScriptAddress(string base58, Network network)
        {
            try
            {
                var notUsed = new BitcoinScriptAddress(base58, network);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        public static bool IsAddress(string base58, Network network)
        {
            return IsBitcoinColoredAddress(base58, network) || 
                IsBitcoinPubKeyAddress(base58, network) ||
                IsBitcoinScriptAddress(base58, network);
        }

        public static IDestination GetAddress(string key, Network network)
        {
            if (IsBitcoinColoredAddress(key, network))
            {
                return new BitcoinColoredAddress(key, network);
            }

            if (IsBitcoinPubKeyAddress(key, network))
            {
                return new BitcoinPubKeyAddress(key, network);
            }

            if (IsBitcoinScriptAddress(key, network))
            {
                return new BitcoinScriptAddress(key, network);
            }


            throw new FormatException("Invalid base58 type");
        }
    }
}

namespace DotSmog.service;

using System;
using System.Numerics;
using System.Threading.Tasks;
using Nethereum.Hex.HexTypes;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using static Nethereum.Util.UnitConversion;
using Nethereum.Contracts;
using DotNetEnv;

// public static async Task Main(string[] args)
// {
//     var tk = new TokenService();
//     string name = await tk.GetTokenName();
//     string symbol = await tk.GetTokenSymbol();
// 	BigInteger supply = await tk.GetTokenTotalSupply();
//
//     Console.WriteLine($"Token Name: {name}");
//     Console.WriteLine($"Token Symbol: {symbol}");
// 	Console.WriteLine($"Token Supply: {supply}");
// }

public class TokenService
{
    private static string infuraUrl = "https://holesky.infura.io/v3/ca497ee51f654018a387e2a04966f11c";
    private static string? privateKey;
    private static string contractAddress = "0xefE3024ed92309139f2551c638f404CfcbD3dbC0"; // Adres kontraktu tokenu

    Account account;
    Web3 web3;
    Contract contract;

    public TokenService()
    {
        Env.Load();
        privateKey =
            Environment.GetEnvironmentVariable("PRIVATE_KEY"); // Klucz prywatny do konta w MetaMask dla sieci Holesky
        if (String.IsNullOrEmpty(privateKey))
        {
            throw new Exception(".env file missing");
        }

        // Tworzenie konta Nethereum
        account = new Account(privateKey);
        web3 = new Web3(account, infuraUrl);

        // ABI kontraktu ERC-20
        string abiFilePath = "ContractABI.json";
        string abi = File.ReadAllText(abiFilePath);
        if (String.IsNullOrEmpty(abi))
        {
            throw new Exception("ContractABI.json file missing");
        }

        contract = web3.Eth.GetContract(abi, contractAddress);
    }

    public async Task<string> GetTokenName()
    {
        var nameFunction = contract.GetFunction("name");
        return await nameFunction.CallAsync<string>();
    }

    public async Task<string> GetTokenSymbol()
    {
        var symbolFunction = contract.GetFunction("symbol");
        return await symbolFunction.CallAsync<string>();
    }

    public async Task<int> GetTokenDecimals()
    {
        var decimalsFunction = contract.GetFunction("decimals");
        return await decimalsFunction.CallAsync<int>();
    }

    public async Task<BigInteger> GetTokenTotalSupply()
    {
        var totalSupplyFunction = contract.GetFunction("totalSupply");
        var totalSupply = await totalSupplyFunction.CallAsync<BigInteger>();

        int decimals = await GetTokenDecimals();
        return totalSupply / BigInteger.Pow(10, decimals);
    }

    public async Task<BigInteger> GetBalance(string addressToCheck)
    {
        var balanceOfFunction = contract.GetFunction("balanceOf");
        var balance = await balanceOfFunction.CallAsync<BigInteger>(addressToCheck);

        int decimals = await GetTokenDecimals();
        return balance / BigInteger.Pow(10, decimals); // Divide by 10^6 to adjust for decimal
    }

    public async void TransferTo(string toAddress)
    {
        if (!string.IsNullOrEmpty(toAddress))
        {
            var transferFunction = contract.GetFunction("transfer");
            int decimals = await GetTokenDecimals();

            // The smallest transferable value is 1 base unit (smallest unit of the token)
            // To convert 1 base unit to Wei, divide by 10^decimals.
            var amountInWei = Web3.Convert.ToWei(1, decimals); // Convert 1 token to the smallest unit
            // This converts 1 token to base units

            var gasPrice = Web3.Convert.ToWei(3, EthUnit.Gwei);
            var gasLimit = 100000; // Limit gazu dla funkcji transfer

            // execute transaction
            var transactionReceipt = await transferFunction.SendTransactionAndWaitForReceiptAsync(
                from: account.Address,
                gas: new HexBigInteger(gasLimit),
                gasPrice: new HexBigInteger(gasPrice),
                value: null,
                functionInput: new object[] { toAddress, amountInWei }); // Send 1 base unit
        }
    }
}
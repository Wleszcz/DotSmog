using System.Numerics;
using System;
using System.Numerics;
using System.Threading.Tasks;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;

namespace DotSmog.service;

public class TokenService {
    private static string infuraUrl = "https://holesky.infura.io/v3/ca497ee51f654018a387e2a04966f11c";

    private static string
        privateKey =
            "29dcc97e65e5e8af1e13e5f31b0a3e5d4cfca679342331a88874ee31aefb943a"; // Klucz prywatny do konta w MetaMask dla sieci Holesky

    private static string contractAddress = "0xefE3024ed92309139f2551c638f404CfcbD3dbC0"; // Adres kontraktu tokenu

    static async Task Main(string[] args)
    {
        // Tworzenie konta Nethereum
        var account = new Account(privateKey);
        var web3 = new Web3(account, infuraUrl);

        // ABI kontraktu ERC-20
        var abi = @"[
	{
		""inputs"": [],
		""stateMutability"": ""nonpayable"",
		""type"": ""constructor""
	},
	{
		""anonymous"": false,
		""inputs"": [
			{
				""indexed"": true,
				""internalType"": ""address"",
				""name"": ""owner"",
				""type"": ""address""
			},
			{
				""indexed"": true,
				""internalType"": ""address"",
				""name"": ""spender"",
				""type"": ""address""
			},
			{
				""indexed"": false,
				""internalType"": ""uint256"",
				""name"": ""value"",
				""type"": ""uint256""
			}
		],
		""name"": ""Approval"",
		""type"": ""event""
	},
	{
		""inputs"": [
			{
				""internalType"": ""address"",
				""name"": ""spender"",
				""type"": ""address""
			},
			{
				""internalType"": ""uint256"",
				""name"": ""amount"",
				""type"": ""uint256""
			}
		],
		""name"": ""approve"",
		""outputs"": [
			{
				""internalType"": ""bool"",
				""name"": ""success"",
				""type"": ""bool""
			}
		],
		""stateMutability"": ""nonpayable"",
		""type"": ""function""
	},
	{
		""inputs"": [
			{
				""internalType"": ""address"",
				""name"": ""recipient"",
				""type"": ""address""
			},
			{
				""internalType"": ""uint256"",
				""name"": ""amount"",
				""type"": ""uint256""
			}
		],
		""name"": ""transfer"",
		""outputs"": [
			{
				""internalType"": ""bool"",
				""name"": ""success"",
				""type"": ""bool""
			}
		],
		""stateMutability"": ""nonpayable"",
		""type"": ""function""
	},
	{
		""anonymous"": false,
		""inputs"": [
			{
				""indexed"": true,
				""internalType"": ""address"",
				""name"": ""from"",
				""type"": ""address""
			},
			{
				""indexed"": true,
				""internalType"": ""address"",
				""name"": ""to"",
				""type"": ""address""
			},
			{
				""indexed"": false,
				""internalType"": ""uint256"",
				""name"": ""value"",
				""type"": ""uint256""
			}
		],
		""name"": ""Transfer"",
		""type"": ""event""
	},
	{
		""inputs"": [
			{
				""internalType"": ""address"",
				""name"": ""from"",
				""type"": ""address""
			},
			{
				""internalType"": ""address"",
				""name"": ""to"",
				""type"": ""address""
			},
			{
				""internalType"": ""uint256"",
				""name"": ""amount"",
				""type"": ""uint256""
			}
		],
		""name"": ""transferFrom"",
		""outputs"": [
			{
				""internalType"": ""bool"",
				""name"": ""success"",
				""type"": ""bool""
			}
		],
		""stateMutability"": ""nonpayable"",
		""type"": ""function""
	},
	{
		""inputs"": [],
		""name"": ""_totalSupply"",
		""outputs"": [
			{
				""internalType"": ""uint256"",
				""name"": """",
				""type"": ""uint256""
			}
		],
		""stateMutability"": ""view"",
		""type"": ""function""
	},
	{
		""inputs"": [
			{
				""internalType"": ""address"",
				""name"": ""ownner"",
				""type"": ""address""
			},
			{
				""internalType"": ""address"",
				""name"": ""spender"",
				""type"": ""address""
			}
		],
		""name"": ""allowance"",
		""outputs"": [
			{
				""internalType"": ""uint256"",
				""name"": ""remaining"",
				""type"": ""uint256""
			}
		],
		""stateMutability"": ""view"",
		""type"": ""function""
	},
	{
		""inputs"": [
			{
				""internalType"": ""address"",
				""name"": ""account"",
				""type"": ""address""
			}
		],
		""name"": ""balanceOf"",
		""outputs"": [
			{
				""internalType"": ""uint256"",
				""name"": ""balance"",
				""type"": ""uint256""
			}
		],
		""stateMutability"": ""view"",
		""type"": ""function""
	},
	{
		""inputs"": [],
		""name"": ""decimals"",
		""outputs"": [
			{
				""internalType"": ""uint256"",
				""name"": """",
				""type"": ""uint256""
			}
		],
		""stateMutability"": ""view"",
		""type"": ""function""
	},
	{
		""inputs"": [],
		""name"": ""name"",
		""outputs"": [
			{
				""internalType"": ""string"",
				""name"": """",
				""type"": ""string""
			}
		],
		""stateMutability"": ""view"",
		""type"": ""function""
	},
	{
		""inputs"": [],
		""name"": ""symbol"",
		""outputs"": [
			{
				""internalType"": ""string"",
				""name"": """",
				""type"": ""string""
			}
		],
		""stateMutability"": ""view"",
		""type"": ""function""
	},
	{
		""inputs"": [],
		""name"": ""totalSupply"",
		""outputs"": [
			{
				""internalType"": ""uint256"",
				""name"": """",
				""type"": ""uint256""
			}
		],
		""stateMutability"": ""view"",
		""type"": ""function""
	}
]";

        // Tworzenie obiektu kontraktu
        var contract = web3.Eth.GetContract(abi, contractAddress);

        // Wywołanie funkcji "name"
        var nameFunction = contract.GetFunction("name");
        var tokenName = await nameFunction.CallAsync<string>();
        Console.WriteLine($"Token Name: {tokenName}");

        // Wywołanie funkcji "symbol"
        var symbolFunction = contract.GetFunction("symbol");
        var tokenSymbol = await symbolFunction.CallAsync<string>();
        Console.WriteLine($"Token Symbol: {tokenSymbol}");

        // Wywołanie funkcji "totalSupply"
        var totalSupplyFunction = contract.GetFunction("totalSupply");
        var totalSupply = await totalSupplyFunction.CallAsync<BigInteger>();
        Console.WriteLine($"Total Supply: {Web3.Convert.FromWei(totalSupply)}");

        // Sprawdzenie salda tokenów dla konkretnego adresu
        var balanceOfFunction = contract.GetFunction("balanceOf");
        var addressToCheck = "ADDRESS_TO_CHECK"; // Adres portfela, którego saldo chcesz sprawdzić
        var balance = await balanceOfFunction.CallAsync<BigInteger>(addressToCheck);
        Console.WriteLine($"Balance of account {addressToCheck}: {Web3.Convert.FromWei(balance)} {tokenSymbol}");
        
        var transferFunction = contract.GetFunction("transfer");
        string toAddress = "0x37C298fE359B4EccFd6d69C4f51Da0Ef9b484Aa4";
        // Przeliczenie tokenów na wartość w Wei (jeśli token ma 18 miejsc po przecinku)
        var decimals = 6; // Zmień, jeśli Twój token używa innej liczby miejsc
        var amountInWei = Web3.Convert.ToWei(0.1m, decimals);

        // Wywołanie funkcji "transfer"
        var gasPrice = Web3.Convert.ToWei(3, UnitConversion.EthUnit.Gwei); // Ustawienie ceny gazu
        var gasLimit = 30000; // Limit gazu dla funkcji transfer

        var transactionReceipt = await transferFunction.SendTransactionAndWaitForReceiptAsync(
	        from: account.Address,
	        gas: new HexBigInteger(gasLimit),
	        gasPrice: new HexBigInteger(gasPrice),
	        value: null,
	        functionInput: new object[] { toAddress, amountInWei });
    }
}
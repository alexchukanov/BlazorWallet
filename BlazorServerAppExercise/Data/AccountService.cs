using Nethereum.Web3;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Policy;
using System.Text;

namespace BlazorServerAppExercise.Data
{
    public class AccountService
    {
        readonly string filePath = "wallets.sql";
        readonly int rowsNum = 0; // 0 = show all records
        string baseUrl = $"https://mainnet.infura.io/v3/";
        string infuraKey = "";   //your PK for Infura
        Web3 web3;

        public AccountService()
        {
            string infuraPath = $"{baseUrl + infuraKey}";
            web3 = new Web3(infuraPath);
        }

        async Task<List<Account>> ReadAccountListAsync(int rows, string filePath)
        {   
            int i = 0;

            List<Account> accList = new List<Account>();

            try
            {
                if (File.Exists(filePath) != false)
                {
                    var lines = File.ReadLinesAsync(filePath);

                    await foreach(string line in lines)
                    {
                        if (line.StartsWith("INSERT")) 
                        {
                            string[] arrStr = line.Split(" ");
                                                        
                            string idStr = arrStr[6].TrimStart('(').TrimEnd(',');

                            int id = 0;
                            int.TryParse(idStr, out id);

                            string adress = arrStr[7].TrimStart('\'').TrimEnd('\'',')',';');

                            Account acc = new Account {Id=id, Address=adress };

                            accList.Add(acc);

                            if (++i >= rows && rows != 0) break;
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"file not found: {filePath}");
                }                
            }
            catch (Exception ex) 
            {
                ex.ToString();
            }
            
            return accList;
        }
                
        public async Task FillAccountBalanceAsync(List<Account> accList)
        {   
            await Task.WhenAll(from acc in accList select DownloadBalanceTaskAsync(acc));
        }

        public async Task DownloadBalanceTaskAsync(Account acc)
        {
            try
            {
                var balance = await web3.Eth.GetBalance.SendRequestAsync(acc.Address);
                acc.Balance = Web3.Convert.FromWei(balance.Value);
            }
            catch (Exception ex)
            {
                acc.Balance = null;
            }
        }

        public async Task<List<Account>> GetAccountListAsync()
        {
            List<Account> accList = await ReadAccountListAsync(rowsNum, filePath);
            await FillAccountBalanceAsync(accList);

            return accList;
        }
    }
}


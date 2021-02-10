using QuickRestClient.Test.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;

namespace QuickRestClient.Test
{
    public class Program
    {
        public static void Main(string[] args)
        {
            using var client = new HttpClient();
            const string host = "https://jsonplaceholder.typicode.com/";
            RestClientsFactory clientsFactory = new RestClientsFactory(client, new Uri(host));
            IUsersService usersService = clientsFactory.CreateClient<IUsersService>();
            string response = usersService.GetUsers();
            Console.Write(response);
            //var users = usersService.GetUsers();
            
            //foreach(var user in users)
            //{
            //    Console.WriteLine(user.UserName);
            //}
        }
    }

    public interface IUsersService
    {
        [Endpoint("users")]
        public string GetUsers();
    }
}

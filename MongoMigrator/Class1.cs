using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using MongoDB.Driver;
using SharedLibrary;
using Newtonsoft.Json;
using MongoDB.Bson.IO;

namespace MigrationTool
{
    class Program
    {
        static void Main()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var db = client.GetDatabase("EthernetDictionaryDB");

            var termsCollection = db.GetCollection<Term>("terms");
            var userDataCollection = db.GetCollection<UserData>("userdata");

            
            // --- 2. Перенос терминов из JSON ---
            var termsJson = File.ReadAllText("data2.json");
            var termList = Newtonsoft.Json.JsonConvert.DeserializeObject<TermList>(termsJson);
            foreach (var term in termList.terms)
                termsCollection.InsertOne(term);

            // --- 3. Перенос файлов пользователей (UserData/*.json) ---
            // --- 3. Перенос всех пользователей из UserData.json ---
            var usersJson = File.ReadAllText("UserData.json");
            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<UserDataList>(usersJson);

            foreach (var user in data.userData)
            {
                userDataCollection.InsertOne(user);
            }



            Console.WriteLine("✅ Миграция завершена успешно!");
        }
    }
}

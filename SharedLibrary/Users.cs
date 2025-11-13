//using MongoDB.Bson.Serialization.Attributes;
//using MongoDB.Bson;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace SharedLibrary
//{
//    public class Users
//    {
//        [BsonId] // Основной идентификатор
//        [BsonRepresentation(BsonType.ObjectId)]
//        public string Id { get; set; }
//        public string personality { get; set; }
//        public string login { get; set; }
//        public string password { get; set; }

//        public static List<Users> LoadUsersFromFile(string filePath)
//        {
//            var users = new List<Users>();
//            try
//            {
//                using (var reader = new StreamReader(filePath))
//                {
//                    string line;
//                    while ((line = reader.ReadLine()) != null)
//                    {
//                        var fields = line.Split('\t').Select(f => f.Trim()).ToArray();

//                        if (fields.Length == 3)
//                        {
//                            users.Add(new Users
//                            {
//                                personality = fields[0],
//                                login = fields[1],
//                                password = fields[2]
//                            });
//                        }
//                        else
//                        {
//                            Console.WriteLine($"Ошибка считывания в строке: {line}");
//                        }
//                    }
//                }
//            }
//            catch (FileNotFoundException)
//            {
//                throw new Exception("Файл users.txt не найден.");
//            }
//            catch (Exception ex)
//            {
//                throw new Exception("Ошибка при чтении файла пользователей: " + ex.Message);
//            }

//            return users;
//        }

//        public static void AppendUserToFile(string filePath, Users user)
//        {
//            try
//            {
//                string line = $"{user.personality}\t{user.login}\t{user.password}";
//                File.AppendAllText(filePath, line + Environment.NewLine);
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine("Ошибка при добавлении пользователя: " + ex.Message);
//            }
//        }
//    }
//}

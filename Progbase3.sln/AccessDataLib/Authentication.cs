/*Додати у бібліотеку класів модуль аутентифікації, 
у якому описати функції реєстрації та логіну. 
Реєстрація перевіряє унікальність імені 
користувача створює новий запис у таблиці користувачів.
 Хешувати паролі користувачів за допомогою алгоритму SHA256. 
Функція логіну перевіряє надане ім’я користувача та 
пароль і повертає об’єкт користувача на основі запису з БД. 
*/
using System;
using System.Security.Cryptography;
using System.Text;
//using Terminal.Gui;
namespace AccessDataLib
{
    public static class Authentication
    {
        public static User SignUp(UserReposytory userReposytory, User user)
        {
            if(userReposytory.GetByUsername(user.username) != null)
            {
                // user exists
                //MessageBox.ErrorQuery("Error","User with such a username already exists, please try again","Ok");
                return null;
            }
            user.password = GetHashPassword(user.password);
            user.id = userReposytory.Insert(user);
            //user.id = userId;
            return user;
        }
        public static User LogIn(UserReposytory userReposytory, string username, string password)
        {
            if(username == "" || password == "")
            {
                // MessageBox.ErrorQuery("Error","Enter data","Ok");
                return null;
            }
            User user = userReposytory.GetByUsername(username);
            if(user == null)
            {
                return null;
            }
            password = GetHashPassword(password);
            if(user.password != password)
            {
                return null;
            }
            return user;
        }
        private static string GetHashPassword(string source)
        {
            SHA256 sha256Hash = SHA256.Create();
            string hash = GetHash(sha256Hash, source);

            // Console.WriteLine($"The SHA256 hash of {source} is: {hash}.");

            // Console.WriteLine("Verifying the hash...");

            // if (VerifyHash(sha256Hash, source, hash))
            // {
            //     //Console.WriteLine("The hashes are the same.");
            // }
            // else
            // {
            //     //Console.WriteLine("The hashes are not same.");
            // }
    
            // sha256Hash.Dispose();
            return hash;
        }
        private static string GetHash(HashAlgorithm hashAlgorithm, string input)
        {
            byte[] data = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));
            var sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }
        private static bool VerifyHash(HashAlgorithm hashAlgorithm, string input, string hash)
        {
            var hashOfInput = GetHash(hashAlgorithm, input);
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;
        
            return comparer.Compare(hashOfInput, hash) == 0;
        }
    }
}
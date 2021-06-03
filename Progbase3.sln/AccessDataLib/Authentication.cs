using System.Security.Cryptography;
using System.Text;
namespace AccessDataLib
{
    public static class Authentication
    {
        public static User SignUp(UserRepository userReposytory, User user)
        {
            if(userReposytory.GetByUsername(user.username) != null)
            {
                return null;
            }
            user.password = GetHashPassword(user.password);
            user.id = userReposytory.Insert(user);
            return user;
        }
        public static User LogIn(UserRepository userReposytory, string username, string password)
        {
            if(username == "" || password == "")
            {
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
    }
}
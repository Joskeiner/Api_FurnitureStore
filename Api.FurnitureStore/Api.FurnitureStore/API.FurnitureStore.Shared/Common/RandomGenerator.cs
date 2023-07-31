using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.FurnitureStore.Shared.Common
{
    public static class RandomGenerator
    {
        public static string GenerateRandomString(int size)
        {
            var random = new Random();

            var chars = "ABCDEFGHIJKLMNOPRSTUVWXYZabcdefghijklmnopqrstuvwxyz!@#$%&*|?";

            return new string(Enumerable.Repeat(chars, size).Select(p => p[random.Next(p.Length)]).ToArray());
          
        }
    }
}

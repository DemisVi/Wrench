using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Wrench.Services
{
    internal class Validator
    {
        private static readonly byte[] _pass = new byte[32] { 0x52, 0xA6, 0xEB, 0x68, 0x7C, 0xD2, 0x2E, 0x80, 0xD3, 0x34, 0x2E, 0xAC, 0x6F, 0xCC, 0x7F, 0x2E, 0x19, 0x20, 0x9E, 0x8F, 0x83, 0xEB, 0x9B, 0x82, 0xE8, 0x1C, 0x6F, 0x3E, 0x6F, 0x30, 0x74, 0x3B };

        public bool IsValidationPassed(string pass)
        {
            using SHA256 sHA = SHA256.Create();

            var bytes = sHA.ComputeHash(Encoding.ASCII.GetBytes(pass));

            return bytes.SequenceEqual(_pass);
        }
    }
}

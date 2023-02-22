using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace Wrench.Services
{
    public abstract class AccessType
    {
        public virtual byte[]? Key { get; }
    }
    public class Regular : AccessType
    {
        public override byte[] Key => new byte[] { 0x52, 0xA6, 0xEB, 0x68, 0x7C, 0xD2, 0x2E, 0x80, 0xD3, 0x34, 0x2E, 0xAC, 0x6F, 0xCC, 0x7F, 0x2E, 0x19, 0x20, 0x9E, 0x8F, 0x83, 0xEB, 0x9B, 0x82, 0xE8, 0x1C, 0x6F, 0x3E, 0x6F, 0x30, 0x74, 0x3B };
    }
    public class Transceiver : AccessType
    {
        public override byte[] Key => new byte[] { 0x0D, 0x0C, 0x9B, 0xC3, 0x7A, 0xE9, 0x55, 0xB2, 0x6C, 0x8B, 0xFE, 0xCC, 0x22, 0xFC, 0xD0, 0x72, 0xC4, 0xEA, 0x5C, 0xE9, 0x59, 0x47, 0xA5, 0x05, 0x1B, 0x5E, 0xD7, 0x39, 0x9B, 0xFF, 0x4F, 0x2E };
    }

    internal class Validator
    {
        private readonly byte[] _pass = new byte[] { 0x52, 0xA6, 0xEB, 0x68, 0x7C, 0xD2, 0x2E, 0x80, 0xD3, 0x34, 0x2E, 0xAC, 0x6F, 0xCC, 0x7F, 0x2E, 0x19, 0x20, 0x9E, 0x8F, 0x83, 0xEB, 0x9B, 0x82, 0xE8, 0x1C, 0x6F, 0x3E, 0x6F, 0x30, 0x74, 0x3B };

        private readonly List<AccessType> _valid = new() { new Regular(), new Transceiver() };

        public bool IsValidationPassed(string? pass)
        {
            using SHA256 sHA = SHA256.Create();

            var bytes = sHA.ComputeHash(Encoding.ASCII.GetBytes(pass ?? string.Empty));

            return bytes.SequenceEqual(_pass);
        }

        public AccessType? GetAccessType(string? request)
        {
            using SHA256 sHA = SHA256.Create();

            var bytes = sHA.ComputeHash(Encoding.ASCII.GetBytes(request ?? string.Empty));

            foreach (var type in _valid)
            {
                if (type is not null && bytes.SequenceEqual(type.Key!))
                {
                    return type;
                }
            }
            return null;
        }
    }
}

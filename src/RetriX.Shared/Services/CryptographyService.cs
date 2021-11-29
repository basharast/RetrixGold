﻿using Plugin.FileSystem.Abstractions;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace RetriX.Shared.Services
{
    public class CryptographyService : ICryptographyService
    {
        private IPlatformService PlatformService { get; }
        public async Task<string> ComputeMD5Async(IFileInfo file)
        {
            try
            {
                using (var inputStream = await file.OpenAsync(FileAccess.Read))
                using (var hasher = IncrementalHash.CreateHash(HashAlgorithmName.MD5))
                {
                    var buffer = new byte[1024 * 1024];
                    while (inputStream.Position < inputStream.Length)
                    {
                        var bytesRead = await inputStream.ReadAsync(buffer, 0, buffer.Length);
                        hasher.AppendData(buffer, 0, bytesRead);
                    }

                    var hashBytes = hasher.GetHashAndReset();
                    var hashString = BitConverter.ToString(hashBytes);
                    return hashString.Replace("-", string.Empty);
                }
            }
            catch (Exception e)
            {
                if (PlatformService != null)
                {
                    PlatformService.ShowErrorMessage(e);
                }
                return null;
            }
        }

        public static async Task<string> ComputeMD5AsyncDirect(IFileInfo file)
        {
            try
            {
                using (var inputStream = await file.OpenAsync(FileAccess.Read))
                using (var hasher = IncrementalHash.CreateHash(HashAlgorithmName.MD5))
                {
                    var buffer = new byte[1024 * 1024];
                    while (inputStream.Position < inputStream.Length)
                    {
                        var bytesRead = await inputStream.ReadAsync(buffer, 0, buffer.Length);
                        hasher.AppendData(buffer, 0, bytesRead);
                    }

                    var hashBytes = hasher.GetHashAndReset();
                    var hashString = BitConverter.ToString(hashBytes);
                    return hashString.Replace("-", string.Empty);
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }




    }

    public class FileSizeFormatProvider : IFormatProvider, ICustomFormatter
    {
        public object GetFormat(Type formatType)
        {
            if (formatType == typeof(ICustomFormatter)) return this;
            return null;
        }

        private const string fileSizeFormat = "fs";
        private const Decimal OneKiloByte = 1024M;
        private const Decimal OneMegaByte = OneKiloByte * 1024M;
        private const Decimal OneGigaByte = OneMegaByte * 1024M;

        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (format == null || !format.StartsWith(fileSizeFormat))
            {
                return defaultFormat(format, arg, formatProvider);
            }

            if (arg is string)
            {
                return defaultFormat(format, arg, formatProvider);
            }

            Decimal size;

            try
            {
                size = Convert.ToDecimal(arg);
            }
            catch (Exception e)
            {
                return defaultFormat(format, arg, formatProvider);
            }

            string suffix;
            if (size > OneGigaByte)
            {
                size /= OneGigaByte;
                suffix = " GB";
            }
            else if (size > OneMegaByte)
            {
                size /= OneMegaByte;
                suffix = " MB";
            }
            else if (size > OneKiloByte)
            {
                size /= OneKiloByte;
                suffix = " KB";
            }
            else
            {
                suffix = " B";
            }

            string precision = format.Substring(2);
            if (String.IsNullOrEmpty(precision)) precision = "2";
            return String.Format("{0:N" + precision + "}{1}", size, suffix);

        }

        private static string defaultFormat(string format, object arg, IFormatProvider formatProvider)
        {
            IFormattable formattableArg = arg as IFormattable;
            if (formattableArg != null)
            {
                return formattableArg.ToString(format, formatProvider);
            }
            return arg.ToString();
        }

    }
}

// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.Security.Cryptography.X509Certificates
{
    internal static class X509CertificateExtensions
    {
        // Returns the SHA1 hash value for the X.509v3 certificate as a hexadecimal string.
        internal static string GetCertHashString(this X509Certificate cert)
        {
            return BitConverter.ToString(cert.GetCertHash()).Replace("-", String.Empty);
        }
    }

    internal static class X509StoreExtensions
    {
        // Placeholder for the X509Store(IntPtr) ctor.
        // Tracking with DevDiv2 bug# 1071357.
        internal static X509Store CreateFromNativeHandle(IntPtr storeHandle)
        {
            return new X509Store(StoreName.My, StoreLocation.CurrentUser);
        }
    }
}
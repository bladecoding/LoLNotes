/*
copyright (C) 2011-2012 by high828@gmail.com

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using NotMissing.Logging;

namespace LoLNotes.Util
{
    public class CertificateInstaller
    {
        public X509Certificate2[] Certificates { get; set; }

        public CertificateInstaller(X509Certificate2[] certs)
        {
            Certificates = certs;
        }

        public bool Install()
        {
            try
            {
                var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
                store.Open(OpenFlags.MaxAllowed);
                var names = GetCommonNames(store.Certificates);
                foreach (var cert in Certificates)
                {
                    if (names.Contains(GetCommonName(cert)))
                        continue;
                    store.Add(cert);
                }
                store.Close();

                return true;
            }
            catch (SecurityException se)
            {
                StaticLogger.Warning(se);
            }
            catch (Exception e)
            {
                StaticLogger.Error("Failed to install " + e);
            }
            return false;
        }

        string GetCommonName(X509Certificate2 cert)
        {
            return cert.GetNameInfo(X509NameType.SimpleName, false);
        }

        IList<string> GetCommonNames(X509Certificate2Collection collection)
        {
            return (from X509Certificate2 cert in collection select GetCommonName(cert)).ToList();
        }

        X509Certificate2 FindCertificateByCommonName(X509Certificate2Collection collection, X509Certificate2 find)
        {
            var str = GetCommonName(find);
            return (from X509Certificate2 cert in collection
                    where GetCommonName(cert) == str
                    select cert).FirstOrDefault();
        }

        public bool IsInstalled
        {
            get
            {
                try
                {
                    var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
                    store.Open(OpenFlags.MaxAllowed);
                    var names = GetCommonNames(store.Certificates);
                    foreach (var cert in Certificates)
                    {
                        if (!names.Contains(GetCommonName(cert)))
                            return false;
                    }
                    store.Close();
                    return true;
                }
                catch (SecurityException se)
                {
                    StaticLogger.Warning(se);
                    return false;
                }
            }
        }
        public bool Uninstall()
        {
            try
            {
                var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
                store.Open(OpenFlags.MaxAllowed);
                foreach (var cert in Certificates)
                {
                    X509Certificate2 storeCert;
                    while ((storeCert = FindCertificateByCommonName(store.Certificates, cert)) != null)
                        store.Remove(storeCert);
                }
                store.Close();

                return true;
            }
            catch (SecurityException se)
            {
                StaticLogger.Warning(se);
            }
            catch (Exception e)
            {
                StaticLogger.Error("Failed to uninstall " + e);
            }

            return false;
        }
    }
}

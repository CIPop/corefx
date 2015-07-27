// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using Microsoft.Win32;

namespace System.Net
{
    /// <devdoc>
    ///    <para>Provides credentials for password-based
    ///       authentication schemes such as basic, digest, NTLM and Kerberos.</para>
    /// </devdoc>
    public class NetworkCredential : ICredentials, ICredentialsByHost
    {
        private static readonly object s_lockingObject = new object();
        private string _domain;
        private string _userName;
        private SecureString _password;

        public NetworkCredential()
        : this(string.Empty, string.Empty, string.Empty)
        {
        }

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Net.NetworkCredential'/>
        ///       class with name and password set as specified.
        ///    </para>
        /// </devdoc>
        public NetworkCredential(string userName, string password)
        : this(userName, password, string.Empty)
        {
        }

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Net.NetworkCredential'/>
        ///       class with name and password set as specified.
        ///    </para>
        /// </devdoc>
        public NetworkCredential(string userName, SecureString password)
        : this(userName, password, string.Empty)
        {
        }

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Net.NetworkCredential'/>
        ///       class with name and password set as specified.
        ///    </para>
        /// </devdoc>
        public NetworkCredential(string userName, string password, string domain)
        {
            UserName = userName;
            Password = password;
            Domain = domain;
        }

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Net.NetworkCredential'/>
        ///       class with name and password set as specified.
        ///    </para>
        /// </devdoc>
        public NetworkCredential(string userName, SecureString password, string domain)
        {
            UserName = userName;
            SecurePassword = password;
            Domain = domain;
        }

        /// <devdoc>
        ///    <para>
        ///       The user name associated with this credential.
        ///    </para>
        /// </devdoc>
        public string UserName
        {
            get
            {
                return InternalGetUserName();
            }
            set
            {
                if (value == null)
                    _userName = String.Empty;
                else
                    _userName = value;
                // GlobalLog.Print("NetworkCredential::set_UserName: m_userName: \"" + m_userName + "\"" );
            }
        }

        /// <devdoc>
        ///    <para>
        ///       The password for the user name.
        ///    </para>
        /// </devdoc>
        public string Password
        {
            get
            {
                return InternalGetPassword();
            }
            set
            {
                _password = SecureStringHelpers.CreateSecureString(value);
                //                GlobalLog.Print("NetworkCredential::set_Password: value = " + value);
                //                GlobalLog.Print("NetworkCredential::set_Password: m_password:");
                //                GlobalLog.Dump(m_password);
            }
        }

#if !FEATURE_PAL
        /// <devdoc>
        ///    <para>
        ///       The password for the user name.
        ///    </para>
        /// </devdoc>
        public SecureString SecurePassword
        {
            get
            {
                return InternalGetSecurePassword().Copy();
            }
            set
            {
                if (value == null)
                    _password = new SecureString(); // makes 0 length string
                else
                    _password = value.Copy();
            }
        }
#endif //!FEATURE_PAL

        /// <devdoc>
        ///    <para>
        ///       The machine name that verifies
        ///       the credentials. Usually this is the host machine.
        ///    </para>
        /// </devdoc>
        public string Domain
        {
            get
            {
                return InternalGetDomain();
            }
            set
            {
                if (value == null)
                    _domain = String.Empty;
                else
                    _domain = value;
                //                GlobalLog.Print("NetworkCredential::set_Domain: m_domain: \"" + m_domain + "\"" );
            }
        }

        internal string InternalGetUserName()
        {
            // GlobalLog.Print("NetworkCredential::get_UserName: returning \"" + m_userName + "\"");
            return _userName;
        }

        internal string InternalGetPassword()
        {
            string decryptedString = SecureStringHelpers.GetPlaintext(_password);

            // GlobalLog.Print("NetworkCredential::get_Password: returning \"" + decryptedString + "\"");
            return decryptedString;
        }

        internal SecureString InternalGetSecurePassword()
        {
            return _password;
        }

        internal string InternalGetDomain()
        {
            // GlobalLog.Print("NetworkCredential::get_Domain: returning \"" + m_domain + "\"");
            return _domain;
        }

        internal string InternalGetDomainUserName()
        {
            string domainUserName = InternalGetDomain();
            if (domainUserName.Length != 0)
                domainUserName += "\\";
            domainUserName += InternalGetUserName();
            return domainUserName;
        }

        /// <devdoc>
        ///    <para>
        ///       Returns an instance of the NetworkCredential class for a Uri and
        ///       authentication type.
        ///    </para>
        /// </devdoc>
        public NetworkCredential GetCredential(Uri uri, String authType)
        {
            return this;
        }

        public NetworkCredential GetCredential(string host, int port, String authenticationType)
        {
            return this;
        }

#if DEBUG
        // this method is only called as part of an assert
        internal bool IsEqualTo(object compObject)
        {
            if ((object)compObject == null)
                return false;
            if ((object)this == (object)compObject)
                return true;
            NetworkCredential compCred = compObject as NetworkCredential;
            if ((object)compCred == null)
                return false;
            return (InternalGetUserName() == compCred.InternalGetUserName() &&
                    InternalGetDomain() == compCred.InternalGetDomain() &&
                    SecureStringHelpers.AreEqualValues(InternalGetSecurePassword(),
                                                       compCred.InternalGetSecurePassword()));
        }
#endif //DEBUG
    } // class NetworkCredential
} // namespace System.Net

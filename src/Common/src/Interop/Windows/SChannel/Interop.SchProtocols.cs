// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

internal static partial class Interop
{
    internal static partial class SChannel
    {
        // Most constants below are taken from schannel.h; those that are not are
        // called out explicitly.

        public const int SP_PROT_PCT1_SERVER = 0x00000001;
        public const int SP_PROT_PCT1_CLIENT = 0x00000002;
        public const int SP_PROT_PCT1 = (SP_PROT_PCT1_SERVER | SP_PROT_PCT1_CLIENT);

        public const int SP_PROT_SSL2_SERVER = 0x00000004;
        public const int SP_PROT_SSL2_CLIENT = 0x00000008;
        public const int SP_PROT_SSL2 = (SP_PROT_SSL2_SERVER | SP_PROT_SSL2_CLIENT);

        public const int SP_PROT_SSL3_SERVER = 0x00000010;
        public const int SP_PROT_SSL3_CLIENT = 0x00000020;
        public const int SP_PROT_SSL3 = (SP_PROT_SSL3_SERVER | SP_PROT_SSL3_CLIENT);

        public const int SP_PROT_TLS1_0_SERVER = 0x00000040;
        public const int SP_PROT_TLS1_0_CLIENT = 0x00000080;
        public const int SP_PROT_TLS1_0 = (SP_PROT_TLS1_0_SERVER | SP_PROT_TLS1_0_CLIENT);

        public const int SP_PROT_TLS1_1_SERVER = 0x00000100;
        public const int SP_PROT_TLS1_1_CLIENT = 0x00000200;
        public const int SP_PROT_TLS1_1 = (SP_PROT_TLS1_1_SERVER | SP_PROT_TLS1_1_CLIENT);

        public const int SP_PROT_TLS1_2_SERVER = 0x00000400;
        public const int SP_PROT_TLS1_2_CLIENT = 0x00000800;
        public const int SP_PROT_TLS1_2 = (SP_PROT_TLS1_2_SERVER | SP_PROT_TLS1_2_CLIENT);

        public const int SP_PROT_SSL3TLS1_CLIENTS = (SP_PROT_TLS1_0_CLIENT | SP_PROT_SSL3_CLIENT);
        public const int SP_PROT_SSL3TLS1_SERVERS = (SP_PROT_TLS1_0_SERVER | SP_PROT_SSL3_SERVER);
        public const int SP_PROT_SSL3TLS1 = (SP_PROT_SSL3 | SP_PROT_TLS1_0);

        public const int SP_PROT_UNI_SERVER = 0x40000000;
        public const int SP_PROT_UNI_CLIENT = unchecked((int)0x80000000);
        public const int SP_PROT_UNI = (SP_PROT_UNI_SERVER | SP_PROT_UNI_CLIENT);

        public const int SP_PROT_NONE = 0;

        // These two constants are not taken from schannel.h. 
        public const int ClientProtocolMask = (SP_PROT_PCT1_CLIENT | SP_PROT_SSL2_CLIENT | SP_PROT_SSL3_CLIENT | SP_PROT_TLS1_0_CLIENT | SP_PROT_TLS1_1_CLIENT | SP_PROT_TLS1_2_CLIENT | SP_PROT_UNI_CLIENT);
        public const int ServerProtocolMask = (SP_PROT_PCT1_SERVER | SP_PROT_SSL2_SERVER | SP_PROT_SSL3_SERVER | SP_PROT_TLS1_0_SERVER | SP_PROT_TLS1_1_SERVER | SP_PROT_TLS1_2_SERVER | SP_PROT_UNI_SERVER);
    }
}

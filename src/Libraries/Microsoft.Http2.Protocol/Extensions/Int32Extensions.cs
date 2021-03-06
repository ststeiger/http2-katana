﻿// Copyright © Microsoft Open Technologies, Inc.
// All Rights Reserved       
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0

// THIS CODE IS PROVIDED ON AN *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR NON-INFRINGEMENT.

// See the Apache 2 License for the specific language governing permissions and limitations under the License.
using System;
using System.Diagnostics.Contracts;
using System.IO;

namespace Microsoft.Http2.Protocol.Extensions
{
    //Algorithm is described in
    //https://github.com/yoavnir/compression-spec/blob/7f67f0dbecdbe65bc22f3e3b57e2d5adefeb08dd/compression-spec.txt
    //item 4.2.1
    public static class Int32Extensions
    {
        private const byte Divider = 128;

        public static byte[] ToUVarInt(this Int32 number, byte prefix)
        {
            Contract.Assert(prefix <= 7);
            int prefixMaxValue = (1 << prefix) - 1;

            if (number < prefixMaxValue)
            {
                return new[] { (byte)number };
            }

            using (var binaryStream = new MemoryStream())
            {
                int integralPart = 1;
                number -= prefixMaxValue;

                binaryStream.WriteByte((byte)prefixMaxValue);

                while (integralPart > 0)
                {
                    integralPart = number / Divider;
                    byte fractionalPart = (byte) (number % Divider);

                    if (integralPart > 0)
                    {
                        //Set to one highest bit
                        fractionalPart |= 0x80;
                    }

                    binaryStream.WriteByte(fractionalPart);

                    number = integralPart;
                }

                var result = new byte[binaryStream.Position];
                Buffer.BlockCopy(binaryStream.GetBuffer(), 0, result, 0, result.Length);
                return result;
            }
        }

        public static Int32 FromUVarInt(byte[] binary)
        {
            int currentIntegral = 0;

            for (int i = binary.Length - 1; i >= 1; i--)
            {
                //Zero highest bit
                byte fractional = (byte)(binary[i] & 0x7f);
                currentIntegral *= Divider;
                currentIntegral += fractional;
            }

            return currentIntegral + binary[0];
        }
    }
}

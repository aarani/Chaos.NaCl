﻿using System;
using System.Text;
using Chaos.NaCl.Internal.Ed25519Ref10;

namespace Chaos.NaCl
{
    public static class Ed25519
    {
        public static readonly int PublicKeySizeInBytes = 32;
        public static readonly int SignatureSizeInBytes = 64;
        public static readonly int ExpandedPrivateKeySizeInBytes = 32 * 2;
        public static readonly int PrivateKeySeedSizeInBytes = 32;
        public static readonly int SharedKeySizeInBytes = 32;

        public static bool Verify(ArraySegment<byte> signature, ArraySegment<byte> message, ArraySegment<byte> publicKey)
        {
            if (signature.Count != SignatureSizeInBytes)
                throw new ArgumentException(string.Format("Signature size must be {0}", SignatureSizeInBytes), "signature.Count");
            if (publicKey.Count != PublicKeySizeInBytes)
                throw new ArgumentException(string.Format("Public key size must be {0}", PublicKeySizeInBytes), "publicKey.Count");
            return Ed25519Operations.crypto_sign_verify(signature.Array, signature.Offset, message.Array, message.Offset, message.Count, publicKey.Array, publicKey.Offset);
        }

        public static bool Verify(byte[] signature, byte[] message, byte[] publicKey)
        {
            if (signature == null)
                throw new ArgumentNullException("signature");
            if (message == null)
                throw new ArgumentNullException("message");
            if (publicKey == null)
                throw new ArgumentNullException("publicKey");
            if (signature.Length != SignatureSizeInBytes)
                throw new ArgumentException(string.Format("Signature size must be {0}", SignatureSizeInBytes), "signature.Length");
            if (publicKey.Length != PublicKeySizeInBytes)
                throw new ArgumentException(string.Format("Public key size must be {0}", PublicKeySizeInBytes), "publicKey.Length");
            return Ed25519Operations.crypto_sign_verify(signature, 0, message, 0, message.Length, publicKey, 0);
        }

        public static void Sign(ArraySegment<byte> signature, ArraySegment<byte> message, ArraySegment<byte> expandedPrivateKey)
        {
            if (signature.Array == null)
                throw new ArgumentNullException("signature.Array");
            if (signature.Count != SignatureSizeInBytes)
                throw new ArgumentException("signature.Count");
            if (expandedPrivateKey.Array == null)
                throw new ArgumentNullException("expandedPrivateKey.Array");
            if (expandedPrivateKey.Count != ExpandedPrivateKeySizeInBytes)
                throw new ArgumentException("expandedPrivateKey.Count");
            if (message.Array == null)
                throw new ArgumentNullException("message.Array");
            Ed25519Operations.crypto_sign2(signature.Array, signature.Offset, message.Array, message.Offset, message.Count, expandedPrivateKey.Array, expandedPrivateKey.Offset);
        }

        public static void SignWithPrehashedPrivateKey(ArraySegment<byte> signature, ArraySegment<byte> message, ArraySegment<byte> expandedPrivateKey, ArraySegment<byte> publicKey)
        {
            if (signature.Array == null)
                throw new ArgumentNullException("signature.Array");
            if (signature.Count != SignatureSizeInBytes)
                throw new ArgumentException("signature.Count");
            if (expandedPrivateKey.Array == null)
                throw new ArgumentNullException("expandedPrivateKey.Array");
            if (expandedPrivateKey.Count != ExpandedPrivateKeySizeInBytes)
                throw new ArgumentException("expandedPrivateKey.Count");
            if (message.Array == null)
                throw new ArgumentNullException("message.Array");
            Ed25519Operations.crypto_sign_with_preexpanded_secret_key(signature.Array, signature.Offset, message.Array, message.Offset, message.Count, expandedPrivateKey.Array, expandedPrivateKey.Offset, publicKey.Array, publicKey.Offset);
        }

        public static byte[] Sign(byte[] message, byte[] expandedPrivateKey)
        {
            var signature = new byte[SignatureSizeInBytes];
            Sign(new ArraySegment<byte>(signature), new ArraySegment<byte>(message), new ArraySegment<byte>(expandedPrivateKey));
            return signature;
        }

        public static byte[] PublicKeyFromSeed(byte[] privateKeySeed)
        {
            byte[] privateKey;
            byte[] publicKey;
            KeyPairFromSeed(out publicKey, out privateKey, privateKeySeed);
            CryptoBytes.Wipe(privateKey);
            return publicKey;
        }

        public static byte[] ExpandedPrivateKeyFromSeed(byte[] privateKeySeed)
        {
            byte[] privateKey;
            byte[] publicKey;
            KeyPairFromSeed(out publicKey, out privateKey, privateKeySeed);
            CryptoBytes.Wipe(publicKey);
            return privateKey;
        }

        public static void KeyPairFromSeed(out byte[] publicKey, out byte[] expandedPrivateKey, byte[] privateKeySeed)
        {
            if (privateKeySeed == null)
                throw new ArgumentNullException("privateKeySeed");
            if (privateKeySeed.Length != PrivateKeySeedSizeInBytes)
                throw new ArgumentException("privateKeySeed");
            var pk = new byte[PublicKeySizeInBytes];
            var sk = new byte[ExpandedPrivateKeySizeInBytes];
            Ed25519Operations.crypto_sign_keypair(pk, 0, sk, 0, privateKeySeed, 0);
            publicKey = pk;
            expandedPrivateKey = sk;
        }

        public static void KeyPairFromSeed(ArraySegment<byte> publicKey, ArraySegment<byte> expandedPrivateKey, ArraySegment<byte> privateKeySeed)
        {
            if (publicKey.Array == null)
                throw new ArgumentNullException("publicKey.Array");
            if (expandedPrivateKey.Array == null)
                throw new ArgumentNullException("expandedPrivateKey.Array");
            if (privateKeySeed.Array == null)
                throw new ArgumentNullException("privateKeySeed.Array");
            if (publicKey.Count != PublicKeySizeInBytes)
                throw new ArgumentException("publicKey.Count");
            if (expandedPrivateKey.Count != ExpandedPrivateKeySizeInBytes)
                throw new ArgumentException("expandedPrivateKey.Count");
            if (privateKeySeed.Count != PrivateKeySeedSizeInBytes)
                throw new ArgumentException("privateKeySeed.Count");
            Ed25519Operations.crypto_sign_keypair(
                publicKey.Array, publicKey.Offset,
                expandedPrivateKey.Array, expandedPrivateKey.Offset,
                privateKeySeed.Array, privateKeySeed.Offset);
        }

        [Obsolete("Needs more testing")]
        public static byte[] KeyExchange(byte[] publicKey, byte[] privateKey)
        {
            var sharedKey = new byte[SharedKeySizeInBytes];
            KeyExchange(new ArraySegment<byte>(sharedKey), new ArraySegment<byte>(publicKey), new ArraySegment<byte>(privateKey));
            return sharedKey;
        }

        [Obsolete("Needs more testing")]
        public static void KeyExchange(ArraySegment<byte> sharedKey, ArraySegment<byte> publicKey, ArraySegment<byte> privateKey)
        {
            if (sharedKey.Array == null)
                throw new ArgumentNullException("sharedKey.Array");
            if (publicKey.Array == null)
                throw new ArgumentNullException("publicKey.Array");
            if (privateKey.Array == null)
                throw new ArgumentNullException("privateKey");
            if (sharedKey.Count != 32)
                throw new ArgumentException("sharedKey.Count != 32");
            if (publicKey.Count != 32)
                throw new ArgumentException("publicKey.Count != 32");
            if (privateKey.Count != 64)
                throw new ArgumentException("privateKey.Count != 64");

            FieldElement montgomeryX, edwardsY, edwardsZ, sharedMontgomeryX;
            FieldOperations.fe_frombytes(out edwardsY, publicKey.Array, publicKey.Offset);
            FieldOperations.fe_1(out edwardsZ);
            MontgomeryCurve25519.EdwardsToMontgomeryX(out montgomeryX, ref edwardsY, ref edwardsZ);
            byte[] h = Sha512.Hash(privateKey.Array, privateKey.Offset, 32);//ToDo: Remove alloc
            ScalarOperations.sc_clamp(h, 0);
            MontgomeryOperations.scalarmult(out sharedMontgomeryX, h, 0, ref montgomeryX);
            CryptoBytes.Wipe(h);
            FieldOperations.fe_tobytes(sharedKey.Array, sharedKey.Offset, ref sharedMontgomeryX);
            MontgomeryCurve25519.KeyExchangeOutputHashNaCl(sharedKey.Array, sharedKey.Offset);
        }

        public static bool CalculateBlindedPublicKey(byte[] publicKey, byte[] blindingFator, out byte[] output)
        {
            if (publicKey is null)
                throw new ArgumentNullException("publicKey.Array");
            if (publicKey.Length != PublicKeySizeInBytes)
                throw new ArgumentException("publicKey.Count != 32");

            output = new byte[PublicKeySizeInBytes];

            byte[] zeros = new byte[PublicKeySizeInBytes];
            byte[] pkCopy = new byte[PublicKeySizeInBytes];
            Array.Copy(publicKey, pkCopy, PublicKeySizeInBytes);
            pkCopy[31] ^= (1 << 7);

            if (GroupOperations.ge_frombytes_negate_vartime(out var A, pkCopy, 0) != 0)
                return false;

            /* There isn't a regular ge_scalarmult -- we have to do tweak*A + zero*B. */
            GroupOperations.ge_double_scalarmult_vartime(out var Aprime, blindingFator, ref A, zeros);
            GroupOperations.ge_tobytes(output,0, ref Aprime);

            return true;
        }

        public static bool CalculateBlindedPrivateKey(byte[] expandedPrivateKey, byte[] blindingFator, string prefixMsg, out byte[] output)
        {
            if (expandedPrivateKey is null)
                throw new ArgumentNullException("publicKey.Array");
            if (expandedPrivateKey.Length != 64)
                throw new ArgumentException("expandedPrivateKey.Count != 64");

            byte[] prefixMsgInBytes = Encoding.ASCII.GetBytes(prefixMsg);

            output = new byte[64];

            byte[] zeros = new byte[32];
            byte[] tweak = new byte[64];
            Array.Copy(blindingFator, 0, tweak, 0, blindingFator.Length);

            ScalarOperations.sc_muladd(output, expandedPrivateKey, tweak, zeros);

            var hasher = new Sha512();
            hasher.Update(prefixMsgInBytes, 0, prefixMsgInBytes.Length);
            hasher.Update(expandedPrivateKey, 32, 32);
            byte[] newRH = hasher.Finish();

            Array.Copy(newRH, 0, output, 32, 32);

            return true;
        }

        public static bool Ed25519PublicKeyFromCurve25519(byte[] publicKey, bool signbit, out byte[] output)
        {
            output = new byte[32];
            
            FieldElement u;
            FieldElement one;
            FieldElement y;
            FieldElement uplus1;
            FieldElement uminus1;
            FieldElement inv_uplus1;

            FieldOperations.fe_frombytes(out u, publicKey, 0);
            FieldOperations.fe_1(out one);
            FieldOperations.fe_sub(out uminus1, ref u, ref one);
            FieldOperations.fe_add(out uplus1, ref u, ref one);
            FieldOperations.fe_invert(out inv_uplus1, ref uplus1);
            FieldOperations.fe_mul(out y, ref uminus1, ref inv_uplus1);

            FieldOperations.fe_tobytes(output, 0, ref y);

            /* propagate sign. */
            output[31] |= (byte)(Convert.ToInt32(!!signbit) << 7);

            return true;
        }
    }
}
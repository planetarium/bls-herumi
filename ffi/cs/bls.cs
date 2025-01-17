/**
	@file
	@brief C# interface of BLS signature
	@author MITSUNARI Shigeo(@herumi)
	@license modified new BSD license
	http://opensource.org/licenses/BSD-3-Clause
    @note
    use bls384_256 built by `mklib dll eth` to use Ethereum mode
*/

using System;
using System.Linq;
using Planetarium.Cryptography.BLS12_381.NativeImport;

namespace Planetarium.Cryptography.BLS12_381
{
    /// <summary>
    /// The constants and static field class for BLS library.
    /// </summary>
    public class BLS
    {
        public const int BN254 = 0;
        public const int BLS12_381 = 5;

        /// <summary>
        /// A constant value of the whether BLS library is initialized in eth mode.
        /// </summary>
        /// <remarks>Currently, this binding only supports eth mode.
        /// </remarks>
        public const bool isETH = true;

        const int IoEcComp = 512; // fixed byte representation
        public const int FR_UNIT_SIZE = 4;
        public const int FP_UNIT_SIZE = 6;
        public const int BLS_COMPILER_TIME_VAR_ADJ = isETH ? 200 : 0;
        public const int COMPILED_TIME_VAR =
            FR_UNIT_SIZE * 10 + FP_UNIT_SIZE + BLS_COMPILER_TIME_VAR_ADJ;

        public const int ID_UNIT_SIZE = FR_UNIT_SIZE;
        public const int SECRETKEY_UNIT_SIZE = FR_UNIT_SIZE;
        public const int PUBLICKEY_UNIT_SIZE = FP_UNIT_SIZE * 3 * (isETH ? 1 : 2);
        public const int SIGNATURE_UNIT_SIZE = FP_UNIT_SIZE * 3 * (isETH ? 2 : 1);

        /// <summary>
        /// A constant value of the <see cref="Id"/> size in <see cref="byte"/>.
        /// </summary>
        public const int ID_SERIALIZE_SIZE = ID_UNIT_SIZE * 8;

        /// <summary>
        /// A constant value of the <see cref="SecretKey"/> size in <see cref="byte"/>.
        /// </summary>
        public const int SECRETKEY_SERIALIZE_SIZE = 32;

        /// <summary>
        /// A constant value of the <see cref="PublicKey"/> size in <see cref="byte"/>.
        /// </summary>
        public const int PUBLICKEY_SERIALIZE_SIZE = isETH ? 48 : 96;

        /// <summary>
        /// A constant value of the <see cref="Signature"/> size in <see cref="byte"/>.
        /// </summary>
        public const int SIGNATURE_SERIALIZE_SIZE = isETH ? 96 : 48;

        /// <summary>
        /// A constant value of the <see cref="Msg"/> size in <see cref="byte"/>.
        /// </summary>
        public const int MSG_SIZE = 32;

        /// <summary>
        /// Verifies multiple signatures with messages and public keys. The number of signatures,
        /// messages and pubic key must be equal, and the same index of each array should have its
        /// pair signature and message, and public key.
        /// </summary>
        /// <param name="sigVec">A signature arrays to verify.</param>
        /// <param name="pubVec">A public key arrays used for signing <paramref name="msgVec"/>.
        /// </param>
        /// <param name="msgVec">A message arrays used in signing.</param>
        /// <returns>Returns <see langword="true"/> if the given values are all true, otherwise
        /// returns <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown if the number of signatures, messages and
        /// public key is not equal.
        /// </exception>
        public static bool MultiVerify(
            in Signature[] sigVec, in PublicKey[] pubVec, in Msg[] msgVec)
        {
            if (pubVec.Length != msgVec.Length)
            {
                throw new ArgumentException("different length of pubVec and msgVec");
            }
            if (pubVec.Length != sigVec.Length)
            {
                throw new ArgumentException("different length of pubVec and sigVec");
            }

            ulong n = (ulong)pubVec.Length;
            if (n == 0)
            {
                throw new ArgumentException("pubVec is empty");
            }

            SecretKey[] randVec = new SecretKey[pubVec.Length];
            for (int i = 0; i < randVec.Length; ++i)
            {
                var otherRandVec = randVec.Except(new[] { randVec[i] }).ToArray();
                randVec[i].SetByCSPRNG();

                // Make sure random values are unique.
                while (otherRandVec.Any(x => x.IsEqual(randVec[i])))
                {
                    randVec[i].SetByCSPRNG();
                }
            }

            return Native.Instance.blsMultiVerify(
                ref sigVec[0],
                ref pubVec[0],
                ref msgVec[0],
                MSG_SIZE,
                ref randVec[0],
                n,
                n,
                4) == 1;
        }
    }
}

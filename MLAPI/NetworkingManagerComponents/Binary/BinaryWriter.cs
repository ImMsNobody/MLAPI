﻿#define ARRAY_WRITE_PERMISSIVE  // Allow attempt to write "packed" byte array (calls WriteByteArray())
#define ARRAY_RESOLVE_IMPLICIT  // Include WriteArray() method with automatic type resolution
#define ARRAY_WRITE_PREMAP      // Create a prefixed array diff mapping
#define ARRAY_DIFF_ALLOW_RESIZE // Whether or not to permit writing diffs of differently sized arrays

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MLAPI.NetworkingManagerComponents.Binary
{
    // Improved version of BitWriter
    public class BinaryWriter
    {
        protected readonly Stream sink;
        protected readonly BitStream bitSink;

        public BinaryWriter(Stream sink)
        {
            this.sink = sink;
            bitSink = sink as BitStream;
        }





        /// <summary>
        /// Write single-precision floating point value to the stream
        /// </summary>
        /// <param name="value">Value to write</param>
        public void WriteSingle(float value)
        {
            WriteUInt32(new UIntFloat
            {
                floatValue = value
            }.uintValue);
        }

        /// <summary>
        /// Write double-precision floating point value to the stream
        /// </summary>
        /// <param name="value">Value to write</param>
        public void WriteDouble(double value)
        {
            WriteUInt64(new UIntFloat
            {
                doubleValue = value
            }.ulongValue);
        }

        /// <summary>
        /// Write single-precision floating point value to the stream as a varint
        /// </summary>
        /// <param name="value">Value to write</param>
        public void WriteSinglePacked(float value)
        {
            WriteUInt32Packed(new UIntFloat
            {
                floatValue = value
            }.uintValue);
        }

        /// <summary>
        /// Write double-precision floating point value to the stream as a varint
        /// </summary>
        /// <param name="value">Value to write</param>
        public void WriteDoublePacked(double value)
        {
            WriteUInt64Packed(new UIntFloat
            {
                doubleValue = value
            }.ulongValue);
        }

        /// <summary>
        /// Convenience method that writes two non-packed Vector3 from the ray to the stream
        /// </summary>
        /// <param name="ray">Ray to write</param>
        public void WriteRay(Ray ray)
        {
            WriteVector3(ray.origin);
            WriteVector3(ray.direction);
        }

        /// <summary>
        /// Convenience method that writes two packed Vector3 from the ray to the stream
        /// </summary>
        /// <param name="ray">Ray to write</param>
        public void WriteRayPacked(Ray ray)
        {
            WriteVector3Packed(ray.origin);
            WriteVector3Packed(ray.direction);
        }

        /// <summary>
        /// Convenience method that writes four non-varint floats from the color to the stream
        /// </summary>
        /// <param name="color">Color to write</param>
        public void WriteColor(Color color)
        {
            WriteSingle(color.r);
            WriteSingle(color.g);
            WriteSingle(color.b);
            WriteSingle(color.a);
        }

        /// <summary>
        /// Convenience method that writes four varint floats from the color to the stream
        /// </summary>
        /// <param name="color">Color to write</param>
        public void WriteColorPacked(Color color)
        {
            WriteSinglePacked(color.r);
            WriteSinglePacked(color.g);
            WriteSinglePacked(color.b);
            WriteSinglePacked(color.a);
        }

        /// <summary>
        /// Convenience method that writes four non-varint floats from the color to the stream
        /// </summary>
        /// <param name="color32">Color32 to write</param>
        public void WriteColor32(Color32 color32)
        {
            WriteSingle(color32.r);
            WriteSingle(color32.g);
            WriteSingle(color32.b);
            WriteSingle(color32.a);
        }

        /// <summary>
        /// Convenience method that writes two non-varint floats from the vector to the stream
        /// </summary>
        /// <param name="vector2">Vector to write</param>
        public void WriteVector2(Vector2 vector2)
        {
            WriteSingle(vector2.x);
            WriteSingle(vector2.y);
        }

        /// <summary>
        /// Convenience method that writes two varint floats from the vector to the stream
        /// </summary>
        /// <param name="vector2">Vector to write</param>
        public void WriteVector2Packed(Vector2 vector2)
        {
            WriteSinglePacked(vector2.x);
            WriteSinglePacked(vector2.y);
        }

        /// <summary>
        /// Convenience method that writes three non-varint floats from the vector to the stream
        /// </summary>
        /// <param name="vector3">Vector to write</param>
        public void WriteVector3(Vector3 vector3)
        {
            WriteSingle(vector3.x);
            WriteSingle(vector3.y);
            WriteSingle(vector3.z);
        }

        /// <summary>
        /// Convenience method that writes three varint floats from the vector to the stream
        /// </summary>
        /// <param name="vector3">Vector to write</param>
        public void WriteVector3Packed(Vector3 vector3)
        {
            WriteSinglePacked(vector3.x);
            WriteSinglePacked(vector3.y);
            WriteSinglePacked(vector3.z);
        }

        /// <summary>
        /// Convenience method that writes four non-varint floats from the vector to the stream
        /// </summary>
        /// <param name="vector4">Vector to write</param>
        public void WriteVector4(Vector4 vector4)
        {
            WriteSingle(vector4.x);
            WriteSingle(vector4.y);
            WriteSingle(vector4.z);
            WriteSingle(vector4.w);
        }

        /// <summary>
        /// Convenience method that writes four varint floats from the vector to the stream
        /// </summary>
        /// <param name="vector4">Vector to write</param>
        public void WriteVector4Packed(Vector4 vector4)
        {
            WriteSinglePacked(vector4.x);
            WriteSinglePacked(vector4.y);
            WriteSinglePacked(vector4.z);
            WriteSinglePacked(vector4.w);
        }

        /// <summary>
        /// Write a single-precision floating point value to the stream. The value is between (inclusive) the minValue and maxValue.
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="minValue">Minimum value that this value could be</param>
        /// <param name="maxValue">Maximum possible value that this could be</param>
        /// <param name="bytes">How many bytes the compressed result should occupy. Must be between 1 and 4 (inclusive)</param>
        public void WriteRangedSingle(float value, float minValue, float maxValue, int bytes)
        {
            if (bytes < 1 || bytes > 4) throw new ArgumentOutOfRangeException("Result must occupy between 1 and 4 bytes!");
            if (value < minValue || value > maxValue) throw new ArgumentOutOfRangeException("Given value does not match the given constraints!");
            uint result = (uint)(((value + minValue) / (maxValue + minValue)) * ((0x100 * bytes) - 1));
            for (int i = 0; i < bytes; ++i) sink.WriteByte((byte)(result >> (i << 3)));
        }

        /// <summary>
        /// Write a double-precision floating point value to the stream. The value is between (inclusive) the minValue and maxValue.
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="minValue">Minimum value that this value could be</param>
        /// <param name="maxValue">Maximum possible value that this could be</param>
        /// <param name="bytes">How many bytes the compressed result should occupy. Must be between 1 and 8 (inclusive)</param>
        public void WriteRangedDouble(double value, double minValue, double maxValue, int bytes)
        {
            if (bytes < 1 || bytes > 8) throw new ArgumentOutOfRangeException("Result must occupy between 1 and 8 bytes!");
            if (value < minValue || value > maxValue) throw new ArgumentOutOfRangeException("Given value does not match the given constraints!");
            ulong result = (ulong)(((value + minValue) / (maxValue + minValue)) * ((0x100 * bytes) - 1));
            for (int i = 0; i < bytes; ++i) WriteByte((byte)(result >> (i << 3)));
        }

        /// <summary>
        /// Write a rotation to the stream.
        /// </summary>
        /// <param name="rotation">Rotation to write</param>
        /// <param name="bytesPerAngle">How many bytes each written angle should occupy. Must be between 1 and 4 (inclusive)</param>
        public void WriteRotation(Quaternion rotation, int bytesPerAngle)
        {
            if (bytesPerAngle < 1 || bytesPerAngle > 4) throw new ArgumentOutOfRangeException("Bytes per angle must be at least 1 byte and at most 4 bytes!");
            if (bytesPerAngle == 4) WriteVector3(rotation.eulerAngles);
            else
            {
                Vector3 rot = rotation.eulerAngles;
                WriteRangedSingle(rot.x, 0f, 360f, bytesPerAngle);
                WriteRangedSingle(rot.y, 0f, 360f, bytesPerAngle);
                WriteRangedSingle(rot.z, 0f, 360f, bytesPerAngle);
            }
        }

        public void WriteBit(bool bit) => bitSink.WriteBit(bit);

        /// <summary>
        /// Write the lower half (lower nibble) of a byte.
        /// </summary>
        /// <param name="value">Value containing nibble to write.</param>
        public void WriteNibble(byte value) => WriteBits(value, 4);
        /// <summary>
        /// Write either the upper or lower nibble of a byte to the stream.
        /// </summary>
        /// <param name="value">Value holding the nibble</param>
        /// <param name="upper">Whether or not the upper nibble should be written. True to write the four high bits, else writes the four low bits.</param>
        public void WriteNibble(byte value, bool upper) => WriteNibble((byte)(value >> (upper ? 4 : 0)));

        /// <summary>
        /// Write s certain amount of bits to the stream.
        /// </summary>
        /// <param name="value">Value to get bits from.</param>
        /// <param name="bitCount">Amount of bits to write</param>
        public void WriteBits(ulong value, int bitCount)
        {
            if (bitCount > 64) throw new ArgumentOutOfRangeException("Cannot read more than 64 bits from a 64-bit value!");
            if (bitCount < 0) throw new ArgumentOutOfRangeException("Cannot read fewer than 0 bits!");
            int count = 0;
            for (; count + 8 < bitCount; count += 8) bitSink.WriteByte((byte)(value >> count));
            for (; count < bitCount; ++count) bitSink.WriteBit((value & (1UL << count))!=0);
        }


        /// <summary>
        /// Write bits to stream.
        /// </summary>
        /// <param name="value">Value to get bits from.</param>
        /// <param name="bitCount">Amount of bits to write.</param>
        public void WriteBits(byte value, int bitCount) => WriteBits(value, bitCount);

        /// <summary>
        /// Write a signed byte to the stream.
        /// </summary>
        /// <param name="value">Value to write</param>
        public void WriteSByte(sbyte value) => WriteByte((byte)value);

        /// <summary>
        /// Write a single character to the stream.
        /// </summary>
        /// <param name="c">Character to write</param>
        public void WriteChar(char c) => WriteUInt16(c);

        /// <summary>
        /// Write an unsigned short (UInt16) to the stream.
        /// </summary>
        /// <param name="value">Value to write</param>
        public void WriteUInt16(ushort value)
        {
            sink.WriteByte((byte)value);
            sink.WriteByte((byte)(value >> 8));
        }
        /// <summary>
        /// Write a signed short (Int16) to the stream.
        /// </summary>
        /// <param name="value">Value to write</param>
        public void WriteInt16(short value) => WriteUInt16((ushort)value);
        /// <summary>
        /// Write an unsigned int (UInt32) to the stream.
        /// </summary>
        /// <param name="value">Value to write</param>
        public void WriteUInt32(uint value)
        {
            sink.WriteByte((byte)value);
            sink.WriteByte((byte)(value >> 8));
            sink.WriteByte((byte)(value >> 16));
            sink.WriteByte((byte)(value >> 24));
        }
        /// <summary>
        /// Write a signed int (Int32) to the stream.
        /// </summary>
        /// <param name="value">Value to write</param>
        public void WriteInt32(int value) => WriteUInt32((uint)value);
        /// <summary>
        /// Write an unsigned long (UInt64) to the stream.
        /// </summary>
        /// <param name="value">Value to write</param>
        public void WriteUInt64(ulong value)
        {
            sink.WriteByte((byte)value);
            sink.WriteByte((byte)(value >> 8));
            sink.WriteByte((byte)(value >> 16));
            sink.WriteByte((byte)(value >> 24));
            sink.WriteByte((byte)(value >> 32));
            sink.WriteByte((byte)(value >> 40));
            sink.WriteByte((byte)(value >> 48));
            sink.WriteByte((byte)(value >> 56));
        }
        /// <summary>
        /// Write a signed long (Int64) to the stream.
        /// </summary>
        /// <param name="value">Value to write</param>
        public void WriteInt64(long value) => WriteUInt64((ulong)value);

        /// <summary>
        /// Write a signed short (Int16) as a ZigZag encoded varint to the stream.
        /// </summary>
        /// <param name="value">Value to write</param>
        public void WriteInt16Packed(short value) => WriteInt64Packed(value);
        /// <summary>
        /// Write an unsigned short (UInt16) as a varint to the stream.
        /// </summary>
        /// <param name="value">Value to write</param>
        public void WriteUInt16Packed(ushort value) => WriteUInt64Packed(value);
        /// <summary>
        /// Write a two-byte character as a varint to the stream.
        /// </summary>
        /// <param name="c">Value to write</param>
        public void WriteCharPacked(char c) => WriteUInt16Packed(c);
        /// <summary>
        /// Write a signed int (Int32) as a ZigZag encoded varint to the stream.
        /// </summary>
        /// <param name="value">Value to write</param>
        public void WriteInt32Packed(int value) => WriteInt64Packed(value);
        /// <summary>
        /// Write an unsigned int (UInt32) as a varint to the stream.
        /// </summary>
        /// <param name="value">Value to write</param>
        public void WriteUInt32Packed(uint value) => WriteUInt64Packed(value);
        /// <summary>
        /// Write a signed long (Int64) as a ZigZag encoded varint to the stream.
        /// </summary>
        /// <param name="value">Value to write</param>
        public void WriteInt64Packed(long value) => WriteUInt64Packed(Arithmetic.ZigZagEncode(value));
        /// <summary>
        /// Write an unsigned long (UInt64) as a varint to the stream.
        /// </summary>
        /// <param name="value">Value to write</param>
        public void WriteUInt64Packed(ulong value)
        {
            if (value <= 240) WriteULongByte(value);
            else if (value <= 2287)
            {
                WriteULongByte(((value - 240) >> 8) + 241);
                WriteULongByte(value - 240);
            }
            else if (value <= 67823)
            {
                WriteULongByte(249);
                WriteULongByte((value - 2288) >> 8);
                WriteULongByte(value - 2288);
            }
            else
            {
                ulong header = 255;
                ulong match = 0x00FF_FFFF_FFFF_FFFFUL;
                while (value <= match)
                {
                    --header;
                    match >>= 8;
                }
                WriteULongByte(header);
                int max = (int)(header - 247);
                for (int i = 0; i < max; ++i) WriteULongByte(value >> (i << 3));
            }
        }

        /// <summary>
        /// Write a byte (in an int format) to the stream.
        /// </summary>
        /// <param name="value">Value to write</param>
        private void WriteIntByte(int value) => WriteByte((byte)value);
        /// <summary>
        /// Write a byte (in a ulong format) to the stream.
        /// </summary>
        /// <param name="byteValue">Value to write</param>
        private void WriteULongByte(ulong byteValue) => WriteByte((byte)byteValue);
        /// <summary>
        /// Write a byte to the stream.
        /// </summary>
        /// <param name="value">Value to write</param>
        public void WriteByte(byte value)
        {
            sink.WriteByte(value);
        }

        // As it turns out, strings cannot be treated as char arrays, since strings use pointers to store data rather than C# arrays
        public void WriteString(string s, bool oneByteChars = false)
        {
            WriteUInt32Packed((uint)s.Length);
            int target = s.Length;
            for (int i = 0; i < target; ++i)
                if (oneByteChars) WriteByte((byte)s[i]);
                else WriteChar(s[i]);
        }

        public void WriteStringPacked(string s)
        {
            WriteUInt32Packed((uint)s.Length);
            int target = s.Length;
            for (int i = 0; i < target; ++i) WriteCharPacked(s[i]);
        }

        public void WriteStringDiff(string write, string compare, bool oneByteChars = false)
        {
#if !ARRAY_DIFF_ALLOW_RESIZE
            if (write.Length != compare.Length) throw new ArgumentException("Mismatched string lengths");
#endif
            WriteUInt32Packed((uint)write.Length);

            // Premapping
            int target;
#if ARRAY_WRITE_PREMAP
#if ARRAY_DIFF_ALLOW_RESIZE
            target = Math.Min(write.Length, compare.Length);
#else
            target = a1.Length;
#endif
            for (int i = 0; i < target; ++i) WriteBit(write[i] != compare[i]);
#else
            target = write.Length;
#endif
            for (int i = 0; i < target; ++i)
            {

                bool b = write[i] != compare[i];
#if !ARRAY_WRITE_PREMAP
                WriteBit(!b);
#endif
                if (b)
                {
                    if (oneByteChars) WriteByte((byte)write[i]);
                    else WriteChar(write[i]);
                }
            }
        }

        public void WriteStringPackedDiff(string write, string compare)
        {

#if !ARRAY_DIFF_ALLOW_RESIZE
            if (write.Length != compare.Length) throw new ArgumentException("Mismatched string lengths");
#endif
            WriteUInt32Packed((uint)write.Length);

            // Premapping
            int target;
#if ARRAY_WRITE_PREMAP
#if ARRAY_DIFF_ALLOW_RESIZE
            target = Math.Min(write.Length, compare.Length);
#else
            target = a1.Length;
#endif
            for (int i = 0; i < target; ++i) WriteBit(write[i] != compare[i]);
#else
            target = write.Length;
#endif
            for (int i = 0; i < target; ++i)
            {

                bool b = write[i] != compare[i];
#if !ARRAY_WRITE_PREMAP
                WriteBit(!b);
#endif
                if (b) WriteCharPacked(write[i]);
            }
        }

        private void CheckLengths(Array a1, Array a2)
        {
        }
        [Conditional("ARRAY_WRITE_PREMAP")]
        private void WritePremap(Array a1, Array a2)
        {
            long target;
            target = Math.Min(a1.LongLength, a2.LongLength);
            for (long i = 0; i < target; ++i) WriteBit(!a1.GetValue(i).Equals(a2.GetValue(i)));
            // TODO: Byte-align here
        }
        private ulong WriteArraySize(Array a1, Array a2, long length)
        {
            ulong write = (ulong)(length >= 0 ? length : a1.LongLength);
            if (length < 0)
            {
                if (length > a1.LongLength) throw new IndexOutOfRangeException("Cannot write more data than is available");
                WriteUInt64Packed(write);
            }
            return write;
        }


        public void WriteByteArray(byte[] b, long count = -1)
        {
            ulong target = WriteArraySize(b, null, count);
            for (ulong i = 0; i < target; ++i) sink.WriteByte(b[i]);
        }

        public void WriteByteArrayDiff(byte[] write, byte[] compare, long count = -1)
        {
            CheckLengths(write, compare);
            long target = (long)WriteArraySize(write, compare, count);
            WritePremap(write, compare);
            for (long i = 0; i < target; ++i)
            {
                bool b = i >= compare.LongLength || write[i] != compare[i];
#if !ARRAY_WRITE_PREMAP
                WriteBit(b);
#endif
                if (b) WriteByte(write[i]);
            }
        }

        public void WriteShortArray(short[] b, long count = -1)
        {
            ulong target = WriteArraySize(b, null, count);
            for (ulong i = 0; i < target; ++i) WriteInt16(b[i]);
        }

        public void WriteShortArrayDiff(short[] write, short[] compare, long count = -1)
        {
            CheckLengths(write, compare);
            long target = (long)WriteArraySize(write, compare, count);
            WritePremap(write, compare);
            for (long i = 0; i < target; ++i)
            {
                bool b = i >= compare.LongLength || write[i] != compare[i];
#if !ARRAY_WRITE_PREMAP
                WriteBit(!b);
#endif
                if (b) WriteInt16(write[i]);
            }
        }

        public void WriteUShortArray(ushort[] b, long count = -1)
        {
            ulong target = WriteArraySize(b, null, count);
            for (ulong i = 0; i < target; ++i) WriteUInt16(b[i]);
        }

        public void WriteUShortArrayDiff(ushort[] write, ushort[] compare, long count = -1)
        {
            CheckLengths(write, compare);
            long target = (long)WriteArraySize(write, compare, count);
            WritePremap(write, compare);
            for (long i = 0; i < target; ++i)
            {
                bool b = i >= compare.LongLength || write[i] != compare[i];
#if !ARRAY_WRITE_PREMAP
                WriteBit(!b);
#endif
                if (b) WriteUInt16(write[i]);
            }
        }

        public void WriteCharArray(char[] b, long count = -1)
        {
            ulong target = WriteArraySize(b, null, count);
            for (ulong i = 0; i < target; ++i) WriteChar(b[i]);
        }

        public void WriteCharArrayDiff(char[] write, char[] compare, long count = -1)
        {
            CheckLengths(write, compare);
            long target = (long)WriteArraySize(write, compare, count);
            WritePremap(write, compare);
            for (long i = 0; i < target; ++i)
            {
                bool b = i >= compare.LongLength || write[i] != compare[i];
#if !ARRAY_WRITE_PREMAP
                WriteBit(!b);
#endif
                if (b) WriteChar(write[i]);
            }
        }

        public void WriteIntArray(int[] b, long count = -1)
        {
            ulong target = WriteArraySize(b, null, count);
            for (ulong i = 0; i < target; ++i) WriteInt32(b[i]);
        }

        public void WriteIntArrayDiff(int[] write, int[] compare, long count = -1)
        {
            CheckLengths(write, compare);
            long target = (long)WriteArraySize(write, compare, count);
            WritePremap(write, compare);
            for (long i = 0; i < target; ++i)
            {
                bool b = i >= compare.LongLength || write[i] != compare[i];
#if !ARRAY_WRITE_PREMAP
                WriteBit(!b);
#endif
                if (b) WriteInt32(write[i]);
            }
        }

        public void WriteUIntArray(uint[] b, long count = -1)
        {
            ulong target = WriteArraySize(b, null, count);
            for (ulong i = 0; i < target; ++i) WriteUInt32(b[i]);
        }

        public void WriteUIntArrayDiff(uint[] write, uint[] compare, long count = -1)
        {
            CheckLengths(write, compare);
            long target = (long)WriteArraySize(write, compare, count);
            WritePremap(write, compare);
            for (long i = 0; i < target; ++i)
            {
                bool b = i >= compare.LongLength || write[i] != compare[i];
#if !ARRAY_WRITE_PREMAP
                WriteBit(!b);
#endif
                if (b) WriteUInt32(write[i]);
            }
        }

        public void WriteLongArray(long[] b, long count = -1)
        {
            ulong target = WriteArraySize(b, null, count);
            for (ulong i = 0; i < target; ++i) WriteInt64(b[i]);
        }

        public void WriteLongArrayDiff(long[] write, long[] compare, long count = -1)
        {
            CheckLengths(write, compare);
            long target = (long)WriteArraySize(write, compare, count);
            WritePremap(write, compare);
            for (long i = 0; i < target; ++i)
            {
                bool b = i >= compare.LongLength || write[i] != compare[i];
#if !ARRAY_WRITE_PREMAP
                WriteBit(!b);
#endif
                if (b) WriteInt64(write[i]);
            }
        }

        public void WriteULongArray(ulong[] b, long count = -1)
        {
            ulong target = WriteArraySize(b, null, count);
            for (ulong i = 0; i < target; ++i) WriteUInt64(b[i]);
        }

        public void WriteULongArrayDiff(ulong[] write, ulong[] compare, long count = -1)
        {
            CheckLengths(write, compare);
            long target = (long)WriteArraySize(write, compare, count);
            WritePremap(write, compare);
            for (long i = 0; i < target; ++i)
            {
                bool b = i >= compare.LongLength || write[i] != compare[i];
#if !ARRAY_WRITE_PREMAP
                WriteBit(!b);
#endif
                if (b) WriteUInt64(write[i]);
            }
        }

        public void WriteFloatArray(float[] b, long count = -1)
        {
            ulong target = WriteArraySize(b, null, count);
            for (ulong i = 0; i < target; ++i) WriteSingle(b[i]);
        }

        public void WriteFloatArrayDiff(float[] write, float[] compare, long count = -1)
        {
            CheckLengths(write, compare);
            long target = (long)WriteArraySize(write, compare, count);
            WritePremap(write, compare);
            for (long i = 0; i < target; ++i)
            {
                bool b = i >= compare.LongLength || write[i] != compare[i];
#if !ARRAY_WRITE_PREMAP
                WriteBit(!b);
#endif
                if (b) WriteSingle(write[i]);
            }
        }

        public void WriteDoubleArray(double[] b, long count = -1)
        {
            ulong target = WriteArraySize(b, null, count);
            for (ulong i = 0; i < target; ++i) WriteDouble(b[i]);
        }

        public void WriteDoubleArrayDiff(double[] write, double[] compare, long count = -1)
        {
            CheckLengths(write, compare);
            long target = (long)WriteArraySize(write, compare, count);
            WritePremap(write, compare);
            for (long i = 0; i < target; ++i)
            {
                bool b = i >= compare.LongLength || write[i] != compare[i];
#if !ARRAY_WRITE_PREMAP
                WriteBit(!b);
#endif
                if (b) WriteDouble(write[i]);
            }
        }





        // Packed arrays
#if ARRAY_RESOLVE_IMPLICIT
        public void WriteArrayPacked(Array a, long count = -1)
        {
            Type arrayType = a.GetType();


#if ARRAY_WRITE_PERMISSIVE
            if (arrayType == typeof(byte[])) WriteByteArray(a as byte[], count);
            else
#endif
            if (arrayType == typeof(short[])) WriteShortArrayPacked(a as short[], count);
            else if (arrayType == typeof(ushort[])) WriteUShortArrayPacked(a as ushort[], count);
            else if (arrayType == typeof(char[])) WriteCharArrayPacked(a as char[], count);
            else if (arrayType == typeof(int[])) WriteIntArrayPacked(a as int[], count);
            else if (arrayType == typeof(uint[])) WriteUIntArrayPacked(a as uint[], count);
            else if (arrayType == typeof(long[])) WriteLongArrayPacked(a as long[], count);
            else if (arrayType == typeof(ulong[])) WriteULongArrayPacked(a as ulong[], count);
            else if (arrayType == typeof(float[])) WriteFloatArrayPacked(a as float[], count);
            else if (arrayType == typeof(double[])) WriteDoubleArrayPacked(a as double[], count);
            else throw new InvalidDataException("Unknown array type! Please serialize manually!");
        }

        public void WriteArrayPackedDiff(Array write, Array compare, long count = -1)
        {
            Type arrayType = write.GetType();
            if (arrayType != compare.GetType()) throw new ArrayTypeMismatchException("Cannot write diff of two differing array types");

#if ARRAY_WRITE_PERMISSIVE
            if (arrayType == typeof(byte[])) WriteByteArrayDiff(write as byte[], compare as byte[], count);
            else
#endif
            if (arrayType == typeof(short[])) WriteShortArrayPackedDiff(write as short[], compare as short[], count);
            else if (arrayType == typeof(ushort[])) WriteUShortArrayPackedDiff(write as ushort[], compare as ushort[], count);
            else if (arrayType == typeof(char[])) WriteCharArrayPackedDiff(write as char[], compare as char[], count);
            else if (arrayType == typeof(int[])) WriteIntArrayPackedDiff(write as int[], compare as int[], count);
            else if (arrayType == typeof(uint[])) WriteUIntArrayPackedDiff(write as uint[], compare as uint[], count);
            else if (arrayType == typeof(long[])) WriteLongArrayPackedDiff(write as long[], compare as long[], count);
            else if (arrayType == typeof(ulong[])) WriteULongArrayPackedDiff(write as ulong[], compare as ulong[], count);
            else if (arrayType == typeof(float[])) WriteFloatArrayPackedDiff(write as float[], compare as float[], count);
            else if (arrayType == typeof(double[])) WriteDoubleArrayPackedDiff(write as double[], compare as double[], count);
            else throw new InvalidDataException("Unknown array type! Please serialize manually!");
        }
#endif

        public void WriteShortArrayPacked(short[] b, long count = -1)
        {
            ulong target = WriteArraySize(b, null, count);
            for (ulong i = 0; i < target; ++i) WriteInt16Packed(b[i]);
        }

        public void WriteShortArrayPackedDiff(short[] write, short[] compare, long count = -1)
        {
            CheckLengths(write, compare);
            long target = (long)WriteArraySize(write, compare, count);
            WritePremap(write, compare);
            for (long i = 0; i < target; ++i)
            {
                bool b = i >= compare.LongLength || write[i] != compare[i];
#if !ARRAY_WRITE_PREMAP
                WriteBit(!b);
#endif
                if (b) WriteInt16Packed(write[i]);
            }
        }

        public void WriteUShortArrayPacked(ushort[] b, long count = -1)
        {
            ulong target = WriteArraySize(b, null, count);
            for (ulong i = 0; i < target; ++i) WriteUInt16Packed(b[i]);
        }

        public void WriteUShortArrayPackedDiff(ushort[] write, ushort[] compare, long count = -1)
        {
            CheckLengths(write, compare);
            long target = (long)WriteArraySize(write, compare, count);
            WritePremap(write, compare);
            for (long i = 0; i < target; ++i)
            {
                bool b = i >= compare.LongLength || write[i] != compare[i];
#if !ARRAY_WRITE_PREMAP
                WriteBit(!b);
#endif
                if (b) WriteUInt16Packed(write[i]);
            }
        }

        public void WriteCharArrayPacked(char[] b, long count = -1)
        {
            ulong target = WriteArraySize(b, null, count);
            for (ulong i = 0; i < target; ++i) WriteCharPacked(b[i]);
        }

        public void WriteCharArrayPackedDiff(char[] write, char[] compare, long count = -1)
        {
            CheckLengths(write, compare);
            long target = (long)WriteArraySize(write, compare, count);
            WritePremap(write, compare);
            for (long i = 0; i < target; ++i)
            {
                bool b = i >= compare.LongLength || write[i] != compare[i];
#if !ARRAY_WRITE_PREMAP
                WriteBit(!b);
#endif
                if (b) WriteCharPacked(write[i]);
            }
        }

        public void WriteIntArrayPacked(int[] b, long count = -1)
        {
            ulong target = WriteArraySize(b, null, count);
            for (ulong i = 0; i < target; ++i) WriteInt32Packed(b[i]);
        }

        public void WriteIntArrayPackedDiff(int[] write, int[] compare, long count = -1)
        {
            CheckLengths(write, compare);
            long target = (long)WriteArraySize(write, compare, count);
            WritePremap(write, compare);
            for (long i = 0; i < target; ++i)
            {
                bool b = i >= compare.LongLength || write[i] != compare[i];
#if !ARRAY_WRITE_PREMAP
                WriteBit(!b);
#endif
                if (b) WriteInt32Packed(write[i]);
            }
        }

        public void WriteUIntArrayPacked(uint[] b, long count = -1)
        {
            ulong target = WriteArraySize(b, null, count);
            for (ulong i = 0; i < target; ++i) WriteUInt32Packed(b[i]);
        }

        public void WriteUIntArrayPackedDiff(uint[] write, uint[] compare, long count = -1)
        {
            CheckLengths(write, compare);
            long target = (long)WriteArraySize(write, compare, count);
            WritePremap(write, compare);
            for (long i = 0; i < target; ++i)
            {
                bool b = i >= compare.LongLength || write[i] != compare[i];
#if !ARRAY_WRITE_PREMAP
                WriteBit(!b);
#endif
                if (b) WriteUInt32Packed(write[i]);
            }
        }

        public void WriteLongArrayPacked(long[] b, long count = -1)
        {
            ulong target = WriteArraySize(b, null, count);
            for (ulong i = 0; i < target; ++i) WriteInt64Packed(b[i]);
        }

        public void WriteLongArrayPackedDiff(long[] write, long[] compare, long count = -1)
        {
            CheckLengths(write, compare);
            long target = (long)WriteArraySize(write, compare, count);
            WritePremap(write, compare);
            for (long i = 0; i < target; ++i)
            {
                bool b = i >= compare.LongLength || write[i] != compare[i];
#if !ARRAY_WRITE_PREMAP
                WriteBit(!b);
#endif
                if (b) WriteInt64Packed(write[i]);
            }
        }

        public void WriteULongArrayPacked(ulong[] b, long count = -1)
        {
            ulong target = WriteArraySize(b, null, count);
            for (ulong i = 0; i < target; ++i) WriteUInt64Packed(b[i]);
        }

        public void WriteULongArrayPackedDiff(ulong[] write, ulong[] compare, long count = -1)
        {
            CheckLengths(write, compare);
            long target = (long)WriteArraySize(write, compare, count);
            WritePremap(write, compare);
            for (long i = 0; i < target; ++i)
            {
                bool b = i >= compare.LongLength || write[i] != compare[i];
#if !ARRAY_WRITE_PREMAP
                WriteBit(!b);
#endif
                if (b) WriteUInt64Packed(write[i]);
            }
        }

        public void WriteFloatArrayPacked(float[] b, long count = -1)
        {
            ulong target = WriteArraySize(b, null, count);
            for (ulong i = 0; i < target; ++i) WriteSinglePacked(b[i]);
        }

        public void WriteFloatArrayPackedDiff(float[] write, float[] compare, long count = -1)
        {
            CheckLengths(write, compare);
            long target = (long)WriteArraySize(write, compare, count);
            WritePremap(write, compare);
            for (long i = 0; i < target; ++i)
            {
                bool b = i >= compare.LongLength || write[i] != compare[i];
#if !ARRAY_WRITE_PREMAP
                WriteBit(!b);
#endif
                if (b) WriteSinglePacked(write[i]);
            }
        }

        public void WriteDoubleArrayPacked(double[] b, long count = -1)
        {
            ulong target = WriteArraySize(b, null, count);
            for (ulong i = 0; i < target; ++i) WriteDoublePacked(b[i]);
        }

        public void WriteDoubleArrayPackedDiff(double[] write, double[] compare, long count = -1)
        {
            CheckLengths(write, compare);
            long target = (long)WriteArraySize(write, compare, count);
            WritePremap(write, compare);
            for (long i = 0; i < target; ++i)
            {
                bool b = i >= compare.LongLength || write[i] != compare[i];
#if !ARRAY_WRITE_PREMAP
                WriteBit(!b);
#endif
                if (b) WriteDoublePacked(write[i]);
            }
        }
    }
}
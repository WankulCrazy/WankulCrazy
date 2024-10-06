using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace WankulCrazyPlugin.utils.obj
{
    public class CharWordReader
    {
        public char[] word;
        public int wordSize;
        public bool endReached;
        private StreamReader reader;
        private int bufferSize;
        private char[] buffer;
        public char currentChar;
        private int currentPosition = 0;
        private int maxPosition = 0;

        public CharWordReader(StreamReader reader, int bufferSize)
        {
            this.reader = reader;
            this.bufferSize = bufferSize;
            buffer = new char[this.bufferSize];
            word = new char[this.bufferSize];
            MoveNext();
        }

        public void SkipWhitespaces()
        {
            while (char.IsWhiteSpace(currentChar))
                MoveNext();
        }

        public void SkipWhitespaces(out bool newLinePassed)
        {
            newLinePassed = false;
            while (char.IsWhiteSpace(currentChar))
            {
                if (currentChar == '\r' || currentChar == '\n')
                    newLinePassed = true;
                MoveNext();
            }
        }

        public void SkipUntilNewLine()
        {
            while (currentChar != char.MinValue && currentChar != '\n' && currentChar != '\r')
                MoveNext();
            SkipNewLineSymbols();
        }

        public void ReadUntilWhiteSpace()
        {
            wordSize = 0;
            while (currentChar != char.MinValue && !char.IsWhiteSpace(currentChar))
            {
                word[wordSize] = currentChar;
                ++wordSize;
                MoveNext();
            }
        }

        public void ReadUntilNewLine()
        {
            wordSize = 0;
            while (currentChar != char.MinValue && currentChar != '\n' && currentChar != '\r')
            {
                word[wordSize] = currentChar;
                ++wordSize;
                MoveNext();
            }
            SkipNewLineSymbols();
        }

        public bool Is(string other)
        {
            if (other.Length != wordSize)
                return false;
            for (int index = 0; index < wordSize; ++index)
            {
                if (word[index] != other[index])
                    return false;
            }
            return true;
        }

        public string GetString(int startIndex = 0)
        {
            return startIndex >= wordSize - 1 ? string.Empty : new string(word, startIndex, wordSize - startIndex);
        }

        public Vector3 ReadVector()
        {
            SkipWhitespaces();
            float x = ReadFloat();
            SkipWhitespaces();
            float y = ReadFloat();
            bool newLinePassed;
            SkipWhitespaces(out newLinePassed);
            float z = 0.0f;
            if (!newLinePassed)
                z = ReadFloat();
            return new Vector3(x, y, z);
        }

        public int ReadInt()
        {
            int num1 = 0;
            bool flag = currentChar == '-';
            if (flag)
                MoveNext();
            while (currentChar >= '0' && currentChar <= '9')
            {
                int num2 = currentChar - 48;
                num1 = num1 * 10 + num2;
                MoveNext();
            }
            return flag ? -num1 : num1;
        }

        public float ReadFloat()
        {
            bool flag = currentChar == '-';
            if (flag)
                MoveNext();
            float num = ReadInt();
            if (currentChar == '.' || currentChar == ',')
            {
                MoveNext();
                num += ReadFloatEnd();
                if (currentChar == 'e' || currentChar == 'E')
                {
                    MoveNext();
                    int p = ReadInt();
                    num *= Mathf.Pow(10f, p);
                }
            }
            if (flag)
                num = -num;
            return num;
        }

        private float ReadFloatEnd()
        {
            float num1 = 0.0f;
            float num2 = 0.1f;
            while (currentChar >= '0' && currentChar <= '9')
            {
                int num3 = currentChar - 48;
                num1 += num3 * num2;
                num2 *= 0.1f;
                MoveNext();
            }
            return num1;
        }

        private void SkipNewLineSymbols()
        {
            while (currentChar == '\n' || currentChar == '\r')
                MoveNext();
        }

        public void MoveNext()
        {
            ++currentPosition;
            if (currentPosition >= maxPosition)
            {
                if (reader.EndOfStream)
                {
                    currentChar = char.MinValue;
                    endReached = true;
                    return;
                }
                currentPosition = 0;
                maxPosition = reader.Read(buffer, 0, bufferSize);
            }
            currentChar = buffer[currentPosition];
        }
    }
}

using System;

namespace CLGameToolkit.Attributes
{
    public static class EnumExtentions
    {
        public static bool HasMultipleFlags(this Enum enumValue)
        {
            return Math.Log(Convert.ToInt32(enumValue), 2) % 1 != 0;
        }

        public static int FlagIndex(this Enum enumValue)
        {
            return (int)Math.Log(Convert.ToInt32(enumValue), 2);
        }

        public static int FlagCount(this Enum enumValue)
        {
            int count = 0;
            int value = Convert.ToInt32(enumValue);
            while (value != 0)
            {
                if ((value & 1) == 1)
                    count++;
                value >>= 1;
            }
            return count;
        }

        public static int FlagCountFast(this Enum enumValue)
        {
            // https://stackoverflow.com/a/677359
            int v = Convert.ToInt32(enumValue);
            v = v - ((v >> 1) & 0x55555555); // reuse input as temporary
            v = (v & 0x33333333) + ((v >> 2) & 0x33333333); // temp
            int c = ((v + (v >> 4) & 0xF0F0F0F) * 0x1010101) >> 24; // count
            return c;
        }

        public static T RandomFlag<T>(this T enumValue) where T : Enum
        {
            int value = Convert.ToInt32(enumValue);
            if (value == 0)
                return enumValue;

            int bitCount = 0;
            int temp = value;
            while (temp != 0)
            {
                if ((temp & 1) != 0)
                    bitCount++;
                temp >>= 1;
            }

            int target = UnityEngine.Random.Range(0, bitCount);

            int mask = 1;
            while (true)
            {
                if ((value & mask) != 0)
                {
                    if (target-- == 0)
                        break;
                }
                mask <<= 1;
            }

            return (T)Enum.ToObject(typeof(T), mask);
        }


    }
}

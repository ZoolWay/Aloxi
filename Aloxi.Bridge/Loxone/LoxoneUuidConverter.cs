using System;
using System.ComponentModel;
using System.Globalization;

namespace ZoolWay.Aloxi.Bridge.Loxone
{
    public class LoxoneUuidConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string)) return true;
            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                return LoxoneUuid.Parse(value as string);
            }
            return base.ConvertFrom(context, culture, value);
        }
    }
}

using System;

namespace MyWalletLib
{
    public class CacheResultAttribute:Attribute
    {
        public int Duration { get; set; }
    }
}
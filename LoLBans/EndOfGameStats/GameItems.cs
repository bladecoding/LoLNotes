using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LoLBans.EndOfGameStats
{
    public class GameItems : List<int>
    {
        protected readonly FlashObject Base;

        public GameItems(FlashObject body)
        {
            if (body == null)
                throw new ArgumentNullException("body");

            Base = body;

            var array = Base["list"]["source"];
            foreach (var field in array.Fields)
                Add(Parse.Int(field.Value));
        }
    }
}

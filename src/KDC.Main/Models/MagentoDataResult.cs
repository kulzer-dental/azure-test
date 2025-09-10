using System.Collections.Generic;

namespace KDC.Main.Models
{
    public class MagentoDataResult<T> : MagentoRequestResultBase
    {
        public IReadOnlyList<T>? Data { get; set; }
    }
}

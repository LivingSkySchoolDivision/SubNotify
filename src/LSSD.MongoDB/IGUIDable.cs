using System;
using System.Collections.Generic;
using System.Text;

namespace LSSD.MongoDB
{
    public interface IGUIDable
    {
        Guid Id { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Text;

namespace SubNotify.Core;

public interface IGUIDable
{
    Guid Id { get; set; }
}

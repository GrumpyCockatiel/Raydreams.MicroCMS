using System;
using System.ComponentModel;

namespace Raydreams.MicroCMS
{
    /// <summary>Enumerates the possible types of environments</summary>
    public enum EnvironmentType
    {
        [Description( "unk" )]
        Unknown = 0,
        [Description( "local" )]
        Local = 1,
        [Description( "dev" )]
        Development = 2,
        [Description( "test" )]
        Testing = 3,
        [Description( "stg" )]
        Staging = 4,
        [Description( "train" )]
        Training = 5,
        [Description( "prod" )]
        Production = 10
    }
}


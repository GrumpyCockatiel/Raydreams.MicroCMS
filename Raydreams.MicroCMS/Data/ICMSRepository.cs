using System;
using System.Collections.Generic;

namespace Raydreams.MicroCMS
{
    /// <summary></summary>
    public interface ICMSRepository
    {
        PageDetails GetTextFile(string shareName, string fileName);

        RawFileWrapper GetRawFile(string shareName, string fileName);

        List<string> ListFiles(string shareName, string pattern = null);

        string UploadFile(RawFileWrapper file, string shareName, string sharePath);
    }
}


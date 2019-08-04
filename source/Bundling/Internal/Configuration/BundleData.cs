﻿using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Karambolo.AspNetCore.Bundling.Internal.Configuration
{
    public class BundleData
    {
        [JsonPropertyName("outputFileName")]
        public string OutputFileName { get; set; }

        [JsonPropertyName("inputFiles")]
        public List<string> InputFiles { get; } = new List<string>();

        [JsonPropertyName("minify")]
        public Dictionary<string, object> Minify { get; } = new Dictionary<string, object> { ["enabled"] = true };

        [JsonPropertyName("includeInProject")]
        public bool IncludeInProject { get; set; } = true;

        [JsonPropertyName("sourceMap")]
        public bool SourceMap { get; set; }

        [JsonPropertyName("sourceMapRootPath")]
        public string SourceMapRootPath { get; set; }
    }
}

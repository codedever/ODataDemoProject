using Newtonsoft.Json;
using ODataDemoProject.Models;
using System.Collections;

namespace ODataDemoProject
{
    /// <summary>
    /// 适用于显式的 odata 参数查询的返回值
    /// </summary>
    public class ODataActionResult<TEntitySet> where TEntitySet : EntitySet
    {
        [JsonProperty("@odata.context")]
        public string Context { get; set; }
        [JsonProperty("@odata.count")]
        public int Count { get; set; }
        [JsonProperty("value")]
        public IEnumerable Value { get; set; }
    }
}

using System;

namespace ODataDemoProject.Models
{
    /// <summary>
    /// key 生成器的参数构造器
    /// 基准机器码的长度和可以支持的分布式机器数量成正比, 和可以产生的时间范围成反比, 经过计算, 
    /// 当基准机器码长度为 12 位时, 支持的机器数量为 4096 台, 可以产生的时间范围为大约   15 年
    /// 当基准机器码长度为 10 位时, 支持的机器数量为 1024 台, 可以产生的时间范围为大约   58 年
    /// 当基准机器码长度为  8 位时, 支持的机器数量为  256 台, 可以产生的时间范围为大约  229 年
    /// 当基准机器码长度为  4 位时, 支持的机器数量为   16 台, 可以产生的时间范围为大约 3654 年
    /// 经测试, 基准值和机器码的设置值基本不会影响 key 生成的速度, 仅会影响时间范围和机器数量, 因此, 
    /// 如设置此类属性, 请使用正确的基准值和机器码组合; 如不设置此类的属性, 将使用以下默认值
    /// BenchmarkDateTime: 今年初
    /// BenchmarkSuffix  : 0xFF
    /// MachineCode      : 0x00
    /// </summary>
    public class KeyGeneratorOptions
    {
        /// <summary>
        /// 初始化默认值
        /// </summary>
        public KeyGeneratorOptions()
        {
            BenchmarkDateTime = new DateTime(DateTime.Now.Year, 1, 1);
            BenchmarkMachineCode = 0xFF;
            MachineCode = 0x00;
        }

        /// <summary>
        /// 基准时间, 使用基准时间和当前时间的时间差来产生不重复的连续 key;
        /// 一个时间, 时间范围 60 年内, ;
        /// 默认值 0x0F, 4 位二进制;
        /// </summary>
        public DateTime BenchmarkDateTime { get; set; }
        /// <summary>
        /// 基准机器码, 即为最大节点数量, 用于决定最多可以有多少个分布式节点, 推荐值 2 的 N 次方;
        /// 不要超过 12 位二进制, 否则产生的速度极大下降;
        /// 默认值 0xFF, 8 位二进制;
        /// </summary>
        public int BenchmarkMachineCode { get; set; }
        /// <summary>
        /// 机器码, 用于产生固定机器码后缀的 key 值, 不能超过基准后缀码的范围;
        /// 默认值 0x00; 即第 0 号机器节点;
        /// </summary>
        public int MachineCode { get; set; }
    }

    /// <summary>
    /// key 生成器;
    /// 需要注入基准码和机器码搭配使用, 机器码的值;
    /// </summary>
    public static class KeyGenerator
    {
        /// <summary>
        /// 生成器参数
        /// </summary>
        private static readonly KeyGeneratorOptions keyGeneratorOption;
        /// <summary>
        /// 基准时间 ticks 值
        /// </summary>
        private static long benchmarkTicks;
        /// <summary>
        /// 从传入的基准后缀码计算出用于产生 key 的按位与预算值
        /// </summary>
        private static int benchmarkMachineCode;
        /// <summary>
        /// 最后一次产生的 key
        /// </summary>
        private static long last = 0;

        /// <summary>
        /// 静态初始化 key 生成器
        /// </summary>
        static KeyGenerator()
        {
            keyGeneratorOption = new KeyGeneratorOptions();
            InitOptions();
        }

        /// <summary>
        /// 产生不重复的基于时间的 long 类型的 key
        /// </summary>
        /// <returns></returns>
        public static long GetKey()
        {
            long next;
            lock (keyGeneratorOption)
            {
                next = DateTime.Now.Ticks - benchmarkTicks;
                if (next == last)
                {
                    next++;
                }

                last = next;
                next = (next << benchmarkMachineCode) + keyGeneratorOption.MachineCode;
            }

            return next;
        }

        /// <summary>
        /// 使用依赖注入方式在项目启动时注入生成器所需的参数值;
        /// 常规委托注入方式;
        /// </summary>
        /// <param name="action">生成器的参数构造器委托</param>
        public static void InitOptions(Action<KeyGeneratorOptions> action)
        {
            action?.Invoke(keyGeneratorOption);
            InitOptions();
        }

        /// <summary>
        /// 校验和初始化生成器参数
        /// </summary>
        private static void InitOptions()
        {
            if (keyGeneratorOption.MachineCode > keyGeneratorOption.BenchmarkMachineCode)
            {
                throw new ArgumentOutOfRangeException(nameof(keyGeneratorOption.MachineCode), $"MachineCode must be in the range of 0x00 to {keyGeneratorOption.BenchmarkMachineCode.ToString("X")}!");
            }

            if (keyGeneratorOption.BenchmarkDateTime == DateTime.MinValue)
            {
                keyGeneratorOption.BenchmarkDateTime = new DateTime(DateTime.Now.Year, 1, 1);
            }

            if (keyGeneratorOption.BenchmarkMachineCode == 0)
            {
                keyGeneratorOption.BenchmarkMachineCode = 0xFF;
            }

            benchmarkTicks = keyGeneratorOption.BenchmarkDateTime.Ticks;
            benchmarkMachineCode = Convert.ToString(keyGeneratorOption.BenchmarkMachineCode, 2).Length;
        }
    }
}
